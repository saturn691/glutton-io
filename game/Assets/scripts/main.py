"""
To test game inputs, run this program in a terminal and then try to
decode the input from reading a terminal.
"""

import time
import argparse
import subprocess
from fir_tester import FPGA, Controller

NIOS_TERMINAL = "nios2-terminal.exe"
SAMPLING_RATE = 1000


def simulate(sample_rate):

    
    # The sampling rate of the FPGA accelerometer

    fpga = FPGA()
    controller = Controller()
    #f = open("assets/scripts/input.txt", "a")

    while True:
        output = fpga.output(
            controller.make_random_movement(),
            0,
            controller.get_key(),
            controller.get_key()
        )
        f = open("assets/scripts/input.txt", "w")
        
        f.write(str(FPGA.uart_decode(output)) + "\n")
        f.close()
        time.sleep(0.1)
        print(FPGA.uart_decode(output))
        time.sleep(1.0 / sample_rate)


def real():
    f = open("input.txt", "a")
    process = subprocess.Popen([NIOS_TERMINAL],
                               stdout=subprocess.PIPE,
                               stderr=subprocess.DEVNULL)

    # Skip first 100 lines (e.g. "nios2-terminal: connected to hardware target")
    for _ in range(100):
        process.stdout.readline()

    while True:
        line = process.stdout.readline().decode('utf-8').strip()
        #print(FPGA.uart_decode(line))
        f.write(str(FPGA.uart_decode(output)))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "-r", "--real",
        action="store_true",
        help="Run the program with real inputs"
    )
    parser.add_argument(
        "-s", "--sample-rate",
        default=SAMPLING_RATE,
        type=int,
        help="Set the sample rate of the FPGA accelerometer (Hz)"
    )
    args = parser.parse_args()

    if args.real:
        real()
    else:
        simulate(args.sample_rate)


if __name__ == '__main__':
    main()
