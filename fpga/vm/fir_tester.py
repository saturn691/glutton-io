"""
Tester for FIR filters - outputs graphs of the raw and filtered data over time.

"""

import random
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
#    FrequencyResponse: 'lowpass'
#      ImpulseResponse: 'fir'
#           SampleRate: 3200
#       PassbandRipple: 1
#  StopbandAttenuation: 35
#    PassbandFrequency: 10
#    StopbandFrequency: 200
#         DesignMethod: 'kaiserwin'
FIR2 = [
    -0.0012, -0.0003, 0.0012, 0.0035, 0.0066, 0.0106, 0.0154, 0.0209,
    0.0269, 0.0332, 0.0397, 0.0459, 0.0517, 0.0567, 0.0607, 0.0635, 0.0650,
    0.0650, 0.0635, 0.0607, 0.0567, 0.0517, 0.0459, 0.0397, 0.0332, 0.0269,
    0.0209, 0.0154, 0.0106, 0.0066, 0.0035, 0.0012, -0.0003, -0.0012,
]


# Specifications:
#    FrequencyResponse: 'lowpass'
#      ImpulseResponse: 'fir'
#           SampleRate: 3200
#       PassbandRipple: 1
#  StopbandAttenuation: 20
#    PassbandFrequency: 10
#    StopbandFrequency: 100
#         DesignMethod: 'kaiserwin'
FIR3 = [
    0.0079, 0.0093, 0.0109, 0.0124, 0.0141, 0.0158, 0.0175, 0.0192, 0.0209,
    0.0226, 0.0242, 0.0257, 0.0272, 0.0286, 0.0298, 0.0309, 0.0319, 0.0327,
    0.0334, 0.0338, 0.0341, 0.0342, 0.0341, 0.0338, 0.0334, 0.0327, 0.0319,
    0.0309, 0.0298, 0.0286, 0.0272, 0.0257, 0.0242, 0.0226, 0.0209, 0.0192,
    0.0175, 0.0158, 0.0141, 0.0124, 0.0109, 0.0093, 0.0079
]


class FPGA():
    def __init__(self, fir: list[float] = FIR1) -> None:
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

    def __get_reading(self, input: tuple[int, int]) -> tuple[int, int]:
        """
        Simulates the FPGA reading the accelerometer and filtering the data.
        """
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

    def __uart_encode(self, accel_x, accel_y, switch, key0, key1) -> str:
        """
        Encodes the data into a hex string to be sent over UART.

        10 bits for accel_x, accel_y, switch
        1 bit for key0, key1
        Total = 32 bits
        """

        encoded = (
            (accel_x & 0x3FF) << 22) | (
            (accel_y & 0x3FF) << 12) | (
            (switch & 0x3FF) << 2) | (
            (key0 & 0x1) << 1) | (key1 & 0x1)

        return hex(encoded)[2:]

    @staticmethod
    def uart_decode(data: int) -> dict:
        """
        Decodes the data from the UART into a JSON-compatible dictionary.
        """
        data_in = int(data, 16)

        reading_x = (data_in >> 22) & 0x3FF
        if reading_x & 0x200:
            reading_x = -((~reading_x & 0x3FF) + 1)

        reading_y = (data_in >> 12) & 0x3FF
        if reading_y & 0x200:
            reading_y = -((~reading_y & 0x3FF) + 1)

        switch = (data_in >> 2) & 0x3FF
        key0 = (data_in >> 1) & 0x1
        key1 = data_in & 0x1

        data_object = {
            "accel_x": reading_x,
            "accel_y": reading_y,
            "switch": switch,
            "key0": key0,
            "key1": key1
        }

        return data_object

    def output(
        self,
        accel: tuple[int, int],
        switch: int,
        key0: bool,
        key1: bool
    ) -> str:
        """
        The data must be decoded on the other other end to be used.
        """
        reading = self.__get_reading(accel)

        reading_x = reading[0]
        reading_y = reading[1]

        data = self.__uart_encode(reading_x, reading_y, switch, key0, key1)

        return data


class Controller():
    """
    Simulates the user making random movements.
    """

    def __init__(self) -> None:
        self.tilt_x = 0
        self.tilt_y = 0

        self.sample_count = 0

    def make_random_movement(self):
        # Introduces clipping in simulation but the accelerometer cannot go
        # above this value
        MAX_AMPLITUDE = 500
        MAX_TILT = 15

        self.tilt_x += random.randint(-MAX_TILT, MAX_TILT)
        self.tilt_y += random.randint(-MAX_TILT, MAX_TILT)

        self.tilt_x = max(min(self.tilt_x, MAX_AMPLITUDE), -MAX_AMPLITUDE)
        self.tilt_y = max(min(self.tilt_y, MAX_AMPLITUDE), -MAX_AMPLITUDE)

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
        rng = random.randint(1, int(1/PROBABLITY))

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
            output = FPGA.uart_decode(
                fpga.output(movement, 0, key0, key1)
            )

            filtered_data_x[i].append(output["accel_x"])
            filtered_data_y[i].append(output["accel_y"])

    plot(movement_x, SAMPLING_RATE, *filtered_data_x)
    plot(movement_y, SAMPLING_RATE, *filtered_data_y)


if __name__ == '__main__':
    main()
