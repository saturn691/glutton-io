"""
Tester for FIR filters - outputs graphs of the raw and filtered data over time.

"""

import random
import json
import math
import matplotlib.pyplot as plt


# Assume a sampling rate of 500 Hz for the descriptions of the FIR filters.

# Frequency response characterised by the sinc(x) function, with a null at
# 1/5, 2/5 of the sampling frequency (100Hz, 200Hz)
# Note that the rest has been omitted as digital frequencies do not exceed
# the Nyquist frequency (250Hz). See Signals and Systems for more information.
FIR1 = [0.2] * 5

# Specifications:
#   FrequencyResponse: 'lowpass'
#     ImpulseResponse: 'fir'
#          SampleRate: 500
#      PassbandRipple: 10
# StopbandAttenuation: 65
#   PassbandFrequency: 10
#   StopbandFrequency: 15
#        DesignMethod: 'equiripple'
FIR2 = [
    0.0004, 0.0003, 0.0004, 0.0005, 0.0006, 0.0008,
    0.0009, 0.0011, 0.0013, 0.0016, 0.0018, 0.0021,
    0.0023, 0.0026, 0.0028, 0.0031, 0.0034, 0.0036,
    0.0038, 0.0041, 0.0042, 0.0044, 0.0044, 0.0045,
    0.0044, 0.0044, 0.0042, 0.0040, 0.0037, 0.0033,
    0.0028, 0.0022, 0.0016, 0.0009, 0.0001, -0.0008,
    -0.0017, -0.0026, -0.0036, -0.0047, -0.0057, -0.0068,
    -0.0078, -0.0088, -0.0097, -0.0106, -0.0114, -0.0121,
    -0.0126, -0.0130, -0.0133, -0.0133, -0.0132, -0.0129,
    -0.0124, -0.0117, -0.0107, -0.0095, -0.0082, -0.0066,
    -0.0048, -0.0028, -0.0006, 0.0017, 0.0042, 0.0068,
    0.0095, 0.0123, 0.0151, 0.0179, 0.0207, 0.0234,
    0.0261, 0.0287, 0.0311, 0.0333, 0.0354, 0.0372,
    0.0388, 0.0401, 0.0412, 0.0419, 0.0424, 0.0425,
    0.0424, 0.0419, 0.0412, 0.0401, 0.0388, 0.0372,
    0.0354, 0.0333, 0.0311, 0.0287, 0.0261, 0.0234,
    0.0207, 0.0179, 0.0151, 0.0123, 0.0095, 0.0068,
    0.0042, 0.0017, -0.0006, -0.0028, -0.0048, -0.0066,
    -0.0082, -0.0095, -0.0107, -0.0117, -0.0124, -0.0129,
    -0.0132, -0.0133, -0.0133, -0.0130, -0.0126, -0.0121,
    -0.0114, -0.0106, -0.0097, -0.0088, -0.0078, -0.0068,
    -0.0057, -0.0047, -0.0036, -0.0026, -0.0017, -0.0008,
    0.0001, 0.0009, 0.0016, 0.0022, 0.0028, 0.0033,
    0.0037, 0.0040, 0.0042, 0.0044, 0.0044, 0.0045,
    0.0044, 0.0044, 0.0042, 0.0041, 0.0038, 0.0036,
    0.0034, 0.0031, 0.0028, 0.0026, 0.0023, 0.0021,
    0.0018, 0.0016, 0.0013, 0.0011, 0.0009, 0.0008,
    0.0006, 0.0005, 0.0004, 0.0003, 0.0004,
]

# Specifications:
#   FrequencyResponse: 'lowpass'
#     ImpulseResponse: 'fir'
#          SampleRate: 500
#      PassbandRipple: 10
# StopbandAttenuation: 65
#   PassbandFrequency: 10
#   StopbandFrequency: 50
#        DesignMethod: 'kaiserwin'

FIR3 = [
    0.0000, 0.0006, 0.0020, 0.0032, 0.0019, -0.0043,
    -0.0147, -0.0238, -0.0217, 0.0016, 0.0496, 0.1141,
    0.1765, 0.2150,
    0.2150, 0.1765, 0.1141, 0.0496, 0.0016, -0.0217, -0.0238,
    -0.0147, -0.0043, 0.0019, 0.0032, 0.0020, 0.0006, 0.0000,
]


class FPGA():
    def __init__(self) -> None:
        self.FIXED_POINT_MULTIPLIER = 1024

        # Choose a FIR to test
        coeffs_float = FIR3

        self.inputs_x = [0] * len(coeffs_float)
        self.inputs_y = [0] * len(coeffs_float)

        # Use the MATLAB coeffs to get fixed point values
        self.coeffs = [coeffs * self.FIXED_POINT_MULTIPLIER
                       for coeffs in coeffs_float]

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


def plot(data, filtered_data, sampling_rate=500):
    # Adjust the x-axis to be in seconds
    time = [i / sampling_rate for i in range(len(data))]

    plt.figure()
    plt.xlabel("Time (s)")
    plt.ylabel("Amplitude")
    plt.plot(time, data, color="blue", label="raw data")
    plt.plot(time, filtered_data, color="red", label="filtered data")
    plt.xlim(0, max(time) + 0.1)  # Constrain x-axis to start from 0
    plt.legend()
    plt.show()


def main():
    # Ensure the random controller is deterministic
    random.seed(0)

    controller = Controller()
    fpga = FPGA()

    movement_x = []
    filtered_x = []
    movement_y = []
    filtered_y = []

    for _ in range(1000):
        movement = controller.make_random_movement()

        movement_x.append(movement[0])
        movement_y.append(movement[1])

        key0 = controller.get_key()
        key1 = controller.get_key()

        output = fpga.output(movement, 0, key0, key1)

        output = json.loads(output)
        filtered_x.append(output["accel_x"])
        filtered_y.append(output["accel_y"])

    plot(movement_x, filtered_x)
    plot(movement_y, filtered_y)


if __name__ == '__main__':
    main()
