#include "system.h"
#include "altera_up_avalon_accelerometer_spi.h"
#include "altera_avalon_timer_regs.h"
#include "altera_avalon_timer.h"
#include "altera_avalon_pio_regs.h"
#include "sys/alt_irq.h"
#include "alt_types.h"
#include <time.h>
#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <stdbool.h>
#include <stdint.h>
#include "sys/alt_stdio.h" 


#define OFFSET 		-32
#define PWM_PERIOD 	16
#define TIMER_BASE 	TIMER_0_BASE
#define TIMER_IRQ	TIMER_0_IRQ

alt_8 pwm = 0;
alt_u8 led;

int level;

typedef enum rtt_phase {
    SEND,
    RECEIVE
}  rtt_phase_t;


void convert_coeffs_to_int(const float *coeffs, int *converted_coeffs, int size) 
{
    for (int i = 0; i < size; ++i) 
    {
        converted_coeffs[i] = coeffs[i] * 1024;
    }
}


void led_write(alt_u8 led_pattern)
{
    IOWR(LED_BASE, 0, led_pattern);
}


/**
 * Converts the x_value to led and level pair
 *
 * @param acc_read- a 9-bit signed integer representing
 * 					the direction of accleration
 */
void convert_read(alt_32 acc_read, int *level, alt_u8 * led)
{
    // Something to do with calibration
	acc_read += OFFSET;

    // val takes the 3 MSBs of acc_read. Returns a number between 0-7
    alt_u8 val = (acc_read >> 6) & 0x07;

    // We have 8 LEDs. There is some overflow but this maps val 0 to LED 3
    // Subsequent vals are bit shifted to the RIGHT
    *led = (8 >> val) | (8 << (8 - val));

	// Considers 5 bits starting from bit 1
    // (this is because val takes the 3 MSBs)
    *level = (acc_read >> 1) & 0x1f;
}


void sys_timer_isr()
{
    IOWR_ALTERA_AVALON_TIMER_STATUS(TIMER_BASE, 0);
    int val = level;

    // Uses a timer to determine how often to write the adjacent LED
    if (pwm < val) {
        if (val < 0) {
        	// If bit 5 = 1, the val is actually higher -> shift LEFT
            led_write(led << 1);
        } else {
        	// default option- just render the current LED
            led_write(led >> 1);
        }

    } else {
        led_write(led);
    }

    // Internal timer
    if (pwm > PWM_PERIOD) {
        pwm = 0;
    } else {
        pwm++;
    }
}


void timer_init(void * isr)
{
    IOWR_ALTERA_AVALON_TIMER_CONTROL(TIMER_BASE, 0x0003);
    IOWR_ALTERA_AVALON_TIMER_STATUS(TIMER_BASE, 0);
    IOWR_ALTERA_AVALON_TIMER_PERIODL(TIMER_BASE, 0x0900);
    IOWR_ALTERA_AVALON_TIMER_PERIODH(TIMER_BASE, 0x0000);
    alt_irq_register(TIMER_IRQ, 0, isr);
    IOWR_ALTERA_AVALON_TIMER_CONTROL(TIMER_BASE, 0x0007);

}


int main()
{
    // track RTT stage
    rtt_phase_t rtt_phase = RECEIVE;
    
    // Non-blocking control 
    // fcntl(0, F_SETFL, O_NONBLOCK);
    alt_32 x_read;
    alt_32 y_read;
    alt_up_accelerometer_spi_dev *acc_dev;
    acc_dev = alt_up_accelerometer_spi_open_dev("/dev/accelerometer_spi");

    alt_u32 time_elapsed;

    if (acc_dev == NULL) 
    { 
        // if return 1, check if the spi ip name is "accelerometer_spi"
        return 1;
    }

    timer_init(sys_timer_isr);

    // Set the sampling rate to 3.2kHz
    alt_up_accelerometer_spi_write(acc_dev, 0x2c, 0b1111);

    while (1) 
    {
        switch (rtt_phase){

        case SEND:
            alt_up_accelerometer_spi_read_x_axis(acc_dev, &x_read);
            alt_up_accelerometer_spi_read_y_axis(acc_dev, &y_read);

            // Read from the button (active low)
            int button_datain = ~IORD_ALTERA_AVALON_PIO_DATA(BUTTON_BASE);
            bool key0 = button_datain & 0b1;
            bool key1 = button_datain & 0b10;

            // Read from the switch
            int switch_datain = IORD_ALTERA_AVALON_PIO_DATA(SWITCH_BASE);
            switch_datain &= 0x3ff;

            // Output raw data to the FIR filter
            IOWR_ALTERA_AVALON_PIO_DATA(RAW_X_BASE, x_read);
            IOWR_ALTERA_AVALON_PIO_DATA(RAW_Y_BASE, y_read);

            // Read the output of the FIR filter
            int x_filtered = IORD_ALTERA_AVALON_PIO_DATA(FILTER_X_BASE);
            int y_filtered = IORD_ALTERA_AVALON_PIO_DATA(FILTER_Y_BASE);

            /*
                To compare the filtered data with the raw data, output both
                (x_read and x_filtered) OR (y_read and y_filtered), then use the 
                Python script fir_data.py
            */

            // Print the encoded data to the console
            uint32_t encoded = (
                (x_filtered & 0x3FF) << 22) | (
                (y_filtered & 0x3FF) << 12) | (
                (switch_datain & 0x3FF) << 2) | (
                (key0 & 0x1) << 1) | (key1 & 0x1);

            // Use the faster function to print to the console
            alt_printf("%x\n", encoded);

            convert_read(x_read, &level, &led);

            // if (alt_timestamp_start() < 0) {
            //     printf ("No timestamp device available\n");
            // }

            rtt_phase = RECEIVE;
            break;
        
        case RECEIVE:
            // Read the encoded data from the console
            alt_getchar();
            
            // // get timestamp
            // time_elapsed = alt_timestamp();
            // printf("RTT: %lu\n", time_elapsed);

            rtt_phase = SEND;
            break;

        }

    }
    return 0;
}