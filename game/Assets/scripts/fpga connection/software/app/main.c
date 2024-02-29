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

#define OFFSET 		-32
#define PWM_PERIOD 	16
#define TIMER_BASE 	TIMER_0_BASE
#define TIMER_IRQ	TIMER_0_IRQ

alt_8 pwm = 0;
alt_u8 led;

const float simple_fir[] = {0.2, 0.2, 0.2, 0.2, 0.2};
const float real_fir[] = {
    0.0046, 0.0074, -0.0024, -0.0071, 0.0033, 0.0001,
    -0.0094, 0.0040, 0.0044, -0.0133, 0.0030, 0.0114,
    -0.0179, -0.0011, 0.0223, -0.0225, -0.0109, 0.0396,
    -0.0263, -0.0338, 0.0752, -0.0289, -0.1204, 0.2879,
    0.6369, 0.2879, -0.1204, -0.0289, 0.0752, -0.0338,
    -0.0263, 0.0396, -0.0109, -0.0225, 0.0223, -0.0011,
    -0.0179, 0.0114, 0.0030, -0.0133, 0.0044, 0.0040,
    -0.0094, 0.0001, 0.0033, -0.0071, -0.0024, 0.0074,
    0.0046
};

#define fir 			real_fir
#define fir_arr_size 	49
#define CONVERT_BACK 	1024


int buffer[fir_arr_size];
int level;

void convert_coeffs_to_int(const float *coeffs, int *converted_coeffs, int size) 
{
    for (int i = 0; i < size; ++i) 
    {
        converted_coeffs[i] = coeffs[i] * 1024;
    }
}


int fir_filter(int new_input, int* input, 
               const float* fir_coeffs, int fir_size)
{
    float result = 0;

    for (int i = fir_size - 1; i > 0; --i)
    {
        input[i] = input[i - 1];
    }
    input[0] = new_input;
    
    
    for (int i = 0; i < fir_size; ++i)
    {
        result += input[i] * fir_coeffs[i];
    }


    return (int) result;
}


int fir_filter_optimized(int new_input, int* input, 
                        const int* fir_coeffs, int fir_size)
{
    int result = 0;

    for (int i = fir_size - 1; i > 0; --i)
    {
        input[i] = input[i - 1];
    }
    input[0] = new_input;
        
    for (int i = 0; i < fir_size; ++i)
    {
        result += input[i] * fir_coeffs[i];
    }

    // The fir_coeffs are scaled by CONVERT_BACK, so return the true value
    return result / CONVERT_BACK;
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
    // Non-blocking control 
    fcntl(0, F_SETFL, O_NONBLOCK);
    alt_32 x_read;
    alt_32 y_read;
    alt_up_accelerometer_spi_dev *acc_dev;
    acc_dev = alt_up_accelerometer_spi_open_dev("/dev/accelerometer_spi");

    if (acc_dev == NULL) 
    { 
        // if return 1, check if the spi ip name is "accelerometer_spi"
        return 1;
    }

    int fir_int[fir_arr_size];
    convert_coeffs_to_int(fir, fir_int, fir_arr_size);

    timer_init(sys_timer_isr);

    // Set the sampling rate to 3.2kHz
    alt_up_accelerometer_spi_write(acc_dev, 0x2c, 0b1111);
    
    while (1) 
    {
        alt_up_accelerometer_spi_read_x_axis(acc_dev, &x_read);
        alt_up_accelerometer_spi_read_y_axis(acc_dev, &y_read);

        // Read from the button
        int button_datain = ~IORD_ALTERA_AVALON_PIO_DATA(BUTTON_BASE);
        bool key0 = button_datain & 0b1;
        bool key1 = button_datain & 0b10;

        // Read from the switch
        int switch_datain = IORD_ALTERA_AVALON_PIO_DATA(SWITCH_BASE);
        switch_datain &= 0x3ff;
        
        // Print the encoded data to the console
        uint32_t encoded = (
            (x_read & 0x3FF) << 22) | (
            (y_read & 0x3FF) << 12) | (
            (switch_datain & 0x3FF) << 2) | (
            (key0 & 0x1) << 1) | (key1 & 0x1);

        // Use the faster function to print to the console
        alt_printf("%x\n", encoded);

        convert_read(x_read, &level, &led);
    }

    return 0;
}
