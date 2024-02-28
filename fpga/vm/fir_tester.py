"""
Tester for FIR filters - outputs graphs of the raw and filtered data over time.

"""

import random
import json
import math
import matplotlib.pyplot as plt

SAMPLING_RATE = 3200

# Assume a sampling rate of 3200 Hz for the descriptions of the FIR filters.

# Frequency response characterised by the sinc(x) function, with a null at
# 1/32, 2/32 of the sampling frequency (100Hz, 200Hz)
# Note that the rest has been omitted as digital frequencies do not exceed
# the Nyquist frequency (1/2 Ts). See Signals and Systems for more information.
FIR1 = [1/32] * 32

# Specifications:
#   FrequencyResponse: 'lowpass'
#     ImpulseResponse: 'fir'
#          SampleRate: 3200
#      PassbandRipple: 1
# StopbandAttenuation: 30
#   PassbandFrequency: 10
#   StopbandFrequency: 100
#        DesignMethod: 'kaiserwin'
FIR2 = [
    0.0005, 0.0011, 0.0018, 0.0027, 0.0036, 0.0046, 0.0058, 0.0071, 0.0085,
    0.0099, 0.0115, 0.0131, 0.0147, 0.0164, 0.0181, 0.0198, 0.0215, 0.0231,
    0.0247, 0.0262, 0.0276, 0.0289, 0.0301, 0.0310, 0.0319, 0.0325, 0.0330,
    0.0333, 0.0334, 0.0333, 0.0330, 0.0325, 0.0319, 0.0310, 0.0301, 0.0289,
    0.0276, 0.0262, 0.0247, 0.0231, 0.0215, 0.0198, 0.0181, 0.0164, 0.0147,
    0.0131, 0.0115, 0.0099, 0.0085, 0.0071, 0.0058, 0.0046, 0.0036, 0.0027,
    0.0018, 0.0011, 0.0005
]

# Specifications:
#   FrequencyResponse: 'lowpass'
#     ImpulseResponse: 'fir'
#          SampleRate: 3200
#      PassbandRipple: 1
# StopbandAttenuation: 20
#   PassbandFrequency: 10
#   StopbandFrequency: 70
#        DesignMethod: 'kaiserwin'
FIR3 = [
    0.0041, 0.0048, 0.0054, 0.0062, 0.0069, 0.0077, 0.0085, 0.0093, 0.0102,
    0.0110, 0.0119, 0.0127, 0.0136, 0.0145, 0.0153, 0.0161, 0.0169, 0.0177,
    0.0185, 0.0192, 0.0199, 0.0205, 0.0211, 0.0217, 0.0222, 0.0226, 0.0230,
    0.0233, 0.0236, 0.0238, 0.0239, 0.0240, 0.0240, 0.0239, 0.0238, 0.0236,
    0.0233, 0.0230, 0.0226, 0.0222, 0.0217, 0.0211, 0.0205, 0.0199, 0.0192,
    0.0185, 0.0177, 0.0169, 0.0161, 0.0153, 0.0145, 0.0136, 0.0127, 0.0119,
    0.0110, 0.0102, 0.0093, 0.0085, 0.0077, 0.0069, 0.0062, 0.0054, 0.0048,
    0.0041
]


class FPGA():
    def __init__(self, fir: list[float]) -> None:
        self.FIXED_POINT_MULTIPLIER = 1024

        self.inputs_x = [0] * len(fir)
        self.inputs_y = [0] * len(fir)

        # Use the MATLAB coeffs to get fixed point values
        self.coeffs = [coeffs * self.FIXED_POINT_MULTIPLIER
                       for coeffs in fir]

    def __fir_filter(self, input: list[int]) -> int:
        """
        In the Nios II, this will be implemented in hardware
        """
        total = 0
        for i in range(len(input)):
            total += input[i] * self.coeffs[i]

        return int(total / self.FIXED_POINT_MULTIPLIER)

    def get_reading(self, input: tuple[int, int]) -> tuple[int, int]:
        # Expand the input
        input_x = input[0]
        input_y = input[1]

        # Move the new input to the front of the lists
        self.inputs_x.insert(0, input_x)
        self.inputs_y.insert(0, input_y)

        # Remove the oldest reading
        self.inputs_x.pop()
        self.inputs_y.pop()

        # Filter the input
        reading_x = self.__fir_filter(self.inputs_x)
        reading_y = self.__fir_filter(self.inputs_y)

        return (reading_x, reading_y)

    def output(
        self,
        accel: tuple[int, int],
        switch: int,
        key0: bool,
        key1: bool
    ) -> str:
        reading = self.get_reading(accel)

        reading_x = reading[0]
        reading_y = reading[1]

        data = {
            "accel_x": reading_x,
            "accel_y": reading_y,
            "switch": switch,
            "key0": key0,
            "key1": key1
        }

        json_data = json.dumps(data)

        return json_data


class Controller():
    """
    Simulates the user making random movements.
    """

    def __init__(self) -> None:
        self.tilt_x = 0
        self.tilt_y = 0

        self.sample_count = 0

    def make_random_movement(self):
        MAX_TILT = 25

        self.tilt_x += random.randint(-MAX_TILT, MAX_TILT)
        self.tilt_y += random.randint(-MAX_TILT, MAX_TILT)

        return (self.tilt_x, self.tilt_y)

    def make_sine_movement(self):
        NOISE_MULTIPLIER = 50
        AMPLITUDE = 256
        PERIOD = 100

        tilt_x = AMPLITUDE * math.sin(2 * math.pi * self.sample_count / PERIOD)
        tilt_y = AMPLITUDE * math.sin(2 * math.pi * self.sample_count / PERIOD)

        tilt_x += NOISE_MULTIPLIER * random.gauss(0, 1)
        tilt_y += NOISE_MULTIPLIER * random.gauss(0, 1)

        self.sample_count += 1
        self.sample_count %= PERIOD

        return (tilt_x, tilt_y)

    def get_key(self) -> bool:
        PROBABLITY = 0.01

        # Generate the random number
        rng = random.randint(1, 1/PROBABLITY)

        return (rng == 1)


def plot(data, sampling_rate, *filtered_data):
    # Adjust the x-axis to be in seconds
    time = [i / sampling_rate for i in range(len(data))]

    plt.figure()
    plt.xlabel("Time (s)")
    plt.ylabel("Amplitude")
    plt.plot(time, data, color="blue", label="raw data")

    for i, filtered in enumerate(filtered_data):
        plt.plot(time, filtered, label=f"filtered data {i+1}")

    plt.xlim(0, max(time) + 0.01)  # Constrain x-axis to start from 0
    plt.legend()
    plt.show()


def main():
    # Ensure the random controller is deterministic
    random.seed(0)
    controller = Controller()
    fpgas = []

    # Add the FIR filters for each FPGA
    fpgas.append(FPGA(FIR1))
    fpgas.append(FPGA(FIR2))
    fpgas.append(FPGA(FIR3))

    movement_x = []
    movement_y = []

    filtered_data_x = [[] for _ in range(len(fpgas))]
    filtered_data_y = [[] for _ in range(len(fpgas))]

    for _ in range(SAMPLING_RATE // 2):
        movement = controller.make_random_movement()

        movement_x.append(movement[0])
        movement_y.append(movement[1])

        key0 = controller.get_key()
        key1 = controller.get_key()

        for i, fpga in enumerate(fpgas):
            output = fpga.output(movement, 0, key0, key1)
            output = json.loads(output)

            filtered_data_x[i].append(output["accel_x"])
            filtered_data_y[i].append(output["accel_y"])

    plot(movement_x, SAMPLING_RATE, *filtered_data_x)
    plot(movement_y, SAMPLING_RATE, *filtered_data_y)


if __name__ == '__main__':
    main()
