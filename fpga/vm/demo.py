"""
Uses the NIOS II terminal to collect data from the FPGA to simulate a game.

Some controller processing done in the FPGA, and some done in the Python
program. 
"""

import pygame
import sys
import subprocess
import select
from fir_data import FPGA, NIOS_TERMINAL

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


def get_data(process):
    global previous_data
    line = ''
    while select.select([process.stdout], [], [], 0.0)[0]:
        try:
            line = process.stdout.readline().decode('utf-8').strip()
        except subprocess.TimeoutExpired:
            process.terminate()

    if line:
        previous_data = line

    if previous_data:
        return FPGA.uart_decode(previous_data)


def main():
    process = subprocess.Popen([NIOS_TERMINAL],
                               stdout=subprocess.PIPE,
                               stderr=subprocess.DEVNULL)

    # Initialize the game engine
    pygame.init()
    clock = pygame.time.Clock()

    # Set up the screen
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption(CAPTION)

    # Ball
    ball = Ball(WIDTH // 2, HEIGHT // 2, 50, 50, BALL_COLOR)
    # Skip first 100 lines (e.g. "nios2-terminal: connected to hardware target")
    for _ in range(100):
        process.stdout.readline()

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
        data = get_data(process)
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
