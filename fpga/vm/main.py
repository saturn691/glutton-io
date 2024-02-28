"""
To test game inputs, run this program in a terminal and then try to
decode the input from reading a terminal.
"""

import time
from fir_tester import FPGA, Controller


def main():
    # The sampling rate of the FPGA accelerometer
    SAMPLING_RATE = 1000

    fpga = FPGA()
    controller = Controller()

    while True:
        output = fpga.output(
            controller.make_random_movement(),
            0,
            controller.get_key(),
            controller.get_key()
        )
        print(output)
        time.sleep(1.0 / SAMPLING_RATE)


if __name__ == '__main__':
    main()
