"""
This file is responsible for the data analysis that is used to choose the FIR 
coefficients

How it works
    1. The user simulates playing the game with the accelerometer by making 
    random movements on a real FPGA. Raw data is outputted.
    2. This data is then collected and placed into a file called "data.txt"
    3. The file is then read and the data is analyzed to find the FIR 
    coefficients by taking an FFT. This graph will be outputted.

This file has two modes
    - Collect data
    - Measure sample rate
    - Analyse data
"""

import matplotlib.pyplot as plt
import numpy as np
import argparse
import subprocess
import time
from pathlib import Path
from fir_tester import FPGA

SCRIPT_LOCATION = Path(__file__).resolve().parent
DATA_LOCATION = SCRIPT_LOCATION / "data"
DATA_FILE = DATA_LOCATION / "data.txt"

NIOS_CMD_SHELL_BAT = "D:/intelFPGA_lite/23.1std/nios2eds/Nios II Command Shell.bat"
NIOS_CMD_SHELL_SH = "/mnt/d/intelFPGA_lite/23.1std/nios2eds/nios2_command_shell.sh"
NIOS_TERMINAL = "nios2-terminal.exe"


def collect_data(runtime=10):
    """
    Attempts to open the NIOS2 terminal and collect random motion from the FPGA.
    Outputs this data to a file.
    """
    process = subprocess.Popen([NIOS_TERMINAL],
                               stdout=subprocess.PIPE,
                               stderr=subprocess.DEVNULL)

    # Skip first 100 lines (e.g. "nios2-terminal: connected to hardware target")
    for _ in range(100):
        process.stdout.readline()

    with open(DATA_FILE, "w") as file:
        start_time = time.time()

        while time.time() - start_time < runtime:
            line = process.stdout.readline().decode('utf-8').strip()
            file.write(line + "\n")

    print("Data collection complete!")

    process.terminate()
    return


def measure_sample_rate(runtime=10):
    """
    Measures the sample rate of the FPGA by taking how many samples are taken
    in 10 seconds.

    More acccurate than data collection as it doesn't have to write to a file.
    """
    process = subprocess.Popen([NIOS_TERMINAL],
                               stdout=subprocess.PIPE)

    # Skip first 100 lines (e.g. "nios2-terminal: connected to hardware target")
    for _ in range(100):
        process.stdout.readline()

    start_time = time.time()
    sample_count = 0
    while time.time() - start_time < runtime:
        line = process.stdout.readline()
        if line:
            sample_count += 1

    print("Measured sample rate:", sample_count / runtime, "Hz")

    process.terminate()
    return


def analyse_data(runtime=10):
    """
    Opens the data file and outputs the data in time and frequency domain
    using FFT.
    """
    MAX_FREQ_SHOWN = 100

    # Open the file and put the data into an array
    with open(DATA_FILE, "r") as file:
        data = file.readlines()

    sampling_rate = len(data) / runtime

    # Convert the data from strings to numbers
    data_x = [float(FPGA.uart_decode(d)["accel_x"]) for d in data]
    data_y = [float(FPGA.uart_decode(d)["accel_y"]) for d in data]

    # Take the FFT of the data
    fft = abs(np.fft.fft(data_x))

    n = len(data_x)

    sample_points = np.arange(n)
    frequencies = sample_points * sampling_rate / n
    times = sample_points / sampling_rate

    # Plot the data in the time domain
    plt.figure(1)
    plt.plot(times, data_x)
    # Uncomment to plot the y data (to compare raw/filtered readings)
    # plt.plot(times, data_y)
    plt.title("Time Domain")
    plt.xlabel("Time (s)")
    plt.ylabel("Amplitude")
    plt.xlim(0, max(times) + 0.1)  # Constrain x-axis to start from 0

    plt.figure(2)
    # Only care about the first half of the data (second half is a mirror)
    plt.plot(frequencies[:n // 2], abs(fft)[:n // 2] / n)
    plt.title("Frequency Domain")
    plt.xlabel("Frequency (Hz)")
    plt.ylabel("Amplitude")
    # Constrain x-axis to start from 0
    plt.xlim(0, min(max(frequencies[:n // 2]) / 2, MAX_FREQ_SHOWN))
    plt.ylim(0, max(abs(fft)[:n // 2]) / n)  # Constrain y-axis to start from 0

    # Create the "graphs" directory if it doesn't exist
    graphs_directory = SCRIPT_LOCATION / "graphs"
    if not graphs_directory.exists():
        graphs_directory.mkdir()

    # Save the figures as high resolution images in the "graphs" directory
    figure1_path = graphs_directory / "accel_time.png"
    figure2_path = graphs_directory / "accel_frequency.png"
    plt.figure(1)
    plt.savefig(figure1_path, dpi=600)
    plt.figure(2)
    plt.savefig(figure2_path, dpi=600)

    plt.show()

    return


def main():
    parser = argparse.ArgumentParser(description="FIR Data Analysis")

    # Create a mutually exclusive group for the three functions
    group = parser.add_mutually_exclusive_group()
    group.add_argument('-c', '--collect', action='store_true',
                       help='Collect data')
    group.add_argument('-m', '--measure', action='store_true',
                       help='Measure sample rate')
    group.add_argument('-a', '--analyse', action='store_true',
                       help='Analyse data')

    args = parser.parse_args()

    # Make data directory if it doesn't exist
    if not DATA_LOCATION.exists():
        DATA_LOCATION.mkdir()

    if args.collect:
        collect_data()
    elif args.measure:
        measure_sample_rate()
    elif args.analyse:
        analyse_data()
    else:
        parser.print_help()


if __name__ == "__main__":
    main()
