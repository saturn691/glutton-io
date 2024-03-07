"""
Uses the NIOS II terminal to collect data from the FPGA to simulate a game.

Some controller processing done in the FPGA, and some done in the Python
program. 
"""

import pygame
import sys
import subprocess
import select
import intel_jtag_uart
import time
from fir_data import FPGA

WIDTH = 800                 # px
HEIGHT = 600                # px
FPS = 240                   # Hz

BG_COLOR = (0, 0, 0)                # Black
GREEN = (0, 255, 0)
BLUE = (0, 0, 255)
BALL_COLOR = (255, 255, 255)        # White

CAPTION = "FPGA Demo"

previous_data = None


class Ball:
    def __init__(self, x_initial, y_initial, width, height, color):
        """
        Initial centre coordinates, width, height, and color of the ball.
        """
        self.x = x_initial
        self.y = y_initial
        self.width = width
        self.height = height
        self.color = color

        self.velocity_x = 0
        self.velocity_y = 0

    def map_velocity(self, v):
        """
        Similar to a controller input.
        """
        # Maxzone of 255
        MAX_V = 255
        # Deadzone of 10%
        DEADZONE = 0.1

        if abs(v) < DEADZONE * MAX_V:
            return 0
        elif abs(v) > MAX_V:
            return 1 if v > 0 else -1
        else:
            # Linear mapping
            return v / MAX_V

    def move(self):
        """
        Move the ball according to its velocity.
        """
        self.x += self.velocity_x
        self.y += self.velocity_y

        if self.x < self.width // 2:
            self.x = self.width // 2
        elif self.x > WIDTH - self.width // 2:
            self.x = WIDTH - self.width // 2

        if self.y < self.height // 2:
            self.y = self.height // 2
        elif self.y > HEIGHT - self.height // 2:
            self.y = HEIGHT - self.height // 2

        return

    def draw(self, screen):
        """
        Moves the ball and draws it on the screen.
        """
        self.move()
        rect = pygame.Rect(
            self.x - self.width // 2,
            self.y - self.height // 2,
            self.width,
            self.height
        )

        pygame.draw.ellipse(screen, self.color, rect)

        return


def get_data(ju):
    global previous_data
    line = ''
    counter = 0
    lines = ju.read().decode("utf-8").splitlines()

    if len(lines) > 2:
        line = lines[-2]

    if line:
        previous_data = line

    if previous_data:
        print(counter)
        return FPGA.uart_decode(previous_data)


def main():
    try:
        ju = intel_jtag_uart.intel_jtag_uart()
    except Exception as e:
        print(e)
        sys.exit(0)

    # Set up JTAG
    start = time.time()
    while (not (ju.is_setup_done())):
        pass
    end = time.time()

    # Statistics
    print("setup time      :  %.4fs" % (end-start))
    print("cable warning   : ", ju.cable_warning())
    print("info            : ", ju.get_info())
    print("setup done      : ", ju.is_setup_done())

    # Initialize the game engine
    pygame.init()
    clock = pygame.time.Clock()

    # Set up the screen
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption(CAPTION)

    # Ball
    ball = Ball(WIDTH // 2, HEIGHT // 2, 50, 50, BALL_COLOR)

    # Game loop
    while True:
        clock.tick(FPS)

        # Handle events
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                sys.exit()

            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_DOWN:
                    ball.velocity_y = 1
                if event.key == pygame.K_UP:
                    ball.velocity_y = -1
                if event.key == pygame.K_LEFT:
                    ball.velocity_x = -1
                if event.key == pygame.K_RIGHT:
                    ball.velocity_x = 1

            if event.type == pygame.KEYUP:
                if event.key == pygame.K_DOWN or event.key == pygame.K_UP:
                    ball.velocity_y = 0
                if event.key == pygame.K_LEFT or event.key == pygame.K_RIGHT:
                    ball.velocity_x = 0

        # Handle UART input
        data = get_data(ju)
        vx = 0
        vy = 0
        key0 = 0
        key1 = 0

        if data:
            vx = data["accel_x"]
            vy = data["accel_y"]
            key0 = data["key0"]
            key1 = data["key1"]

        # Invert vx only
        ball.velocity_x = ball.map_velocity(-vx)
        ball.velocity_y = ball.map_velocity(vy)

        # Draw the ball
        if key0:
            screen.fill(BLUE)
        elif key1:
            screen.fill(GREEN)
        else:
            screen.fill(BG_COLOR)
        ball.draw(screen)

        # Update the screen
        pygame.display.flip()


if __name__ == "__main__":
    main()
