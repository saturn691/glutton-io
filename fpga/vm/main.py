import random
import json
import argparse


class FPGA():
    def __init__(self) -> None:
        self.FIXED_POINT_MULTIPLIER = 1024

        self.inputs_x = [0] * 49
        self.inputs_y = [0] * 49
        coeffs_float = [
            0.0046, 0.0074, -0.0024, -0.0071, 0.0033, 0.0001,
            -0.0094, 0.0040, 0.0044, -0.0133, 0.0030, 0.0114,
            -0.0179, -0.0011, 0.0223, -0.0225, -0.0109, 0.0396,
            -0.0263, -0.0338, 0.0752, -0.0289, -0.1204, 0.2879,
            0.6369, 0.2879, -0.1204, -0.0289, 0.0752, -0.0338,
            -0.0263, 0.0396, -0.0109, -0.0225, 0.0223, -0.0011,
            -0.0179, 0.0114, 0.0030, -0.0133, 0.0044, 0.0040,
            -0.0094, 0.0001, 0.0033, -0.0071, -0.0024, 0.0074,
            0.0046
        ]

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
        
        # Remove the oldest reading
        self.inputs_x.pop(0)
        self.inputs_x.append(input_x)
        self.inputs_y.pop(0)
        self.inputs_y.append(input_y)

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
    def __init__(self, seed: int) -> None:
        self.key_probability = 0.01
        
        # Maximum tilt per turn
        self.max_tilt = 10
        
        self.seed = seed
        self.tilt_x = 0
        self.tilt_y = 0

    def make_movement(self):
        tilt_x = self.tilt_x + random.randint(-self.max_tilt, self.max_tilt)
        tilt_y = self.tilt_y + random.randint(-self.max_tilt, self.max_tilt)
        
        # Helper function for later
        def constrain(x):
            if x > 255:
                return x - 512
            elif x < -256:
                return x + 512
            else:
                return x

        # Make sure that they are within the range [-256, 255]
        self.tilt_x = constrain(tilt_x)
        self.tilt_y = constrain(tilt_y)
        
        return (self.tilt_x, self.tilt_y)


    def get_key(self) -> bool:
        rng = random.randint(1, 1/self.key_probability)

        return (rng == 1)


def main():
    controller = Controller(0)
    fpga = FPGA()
    for _ in range(100):
        movement = controller.make_movement()
        print(movement)
        key0 = controller.get_key()
        key1 = controller.get_key()

        output = fpga.output(movement, 0, key0, key1)
        print(output)


if __name__ == '__main__': 
    main()