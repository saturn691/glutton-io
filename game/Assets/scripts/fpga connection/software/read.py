import matplotlib.pyplot as plt
import random
import select
import subprocess

NIOS_CMD_SHELL_BAT = "D:/intelFPGA_lite/23.1std/nios2eds/Nios II Command Shell.bat"


plt.style.use("ggplot")
plt.rcParams['font.family'] = 'serif'
plt.rcParams['font.serif'] = ['Ubuntu', 'Calibri']
plt.rcParams['font.size'] = 10
plt.rcParams['axes.labelsize'] = 10
plt.rcParams['axes.labelweight'] = 'bold'
plt.rcParams['xtick.labelsize'] = 8
plt.rcParams['ytick.labelsize'] = 8
plt.rcParams['legend.fontsize'] = 10
plt.rcParams['figure.titlesize'] = 12


previous_data = 0


def plot_data(length):
    process = subprocess.Popen(["nios2-terminal.exe"], stdout=subprocess.PIPE)
    for _ in range(10):
        process.stdout.readline()

    plt.ion()   # Turn on interactive mode
    plt.clf()   # Clear the current figure
    ax = plt.gca()  # Get the current axes
    x_data = [0]
    y_data = [get_accelerometer_data(process)]

    line, = ax.plot(x_data, y_data)  # Plot the initial data

    ax.set_title("Accelerometer Data over Time")
    ax.set_xlabel("Reading [n]")
    ax.set_ylabel("Accelerometer Reading")

    for _ in range(length):
        # Pause for a short interval to create real-time effect
        plt.pause(0.01)

        # Get new data and append it to the existing data
        new_y = get_accelerometer_data(process)
        x_data.append(x_data[-1] + 1)
        y_data.append(new_y)

        # Set the x-axis limits to start at 0
        ax.set_xlim(0, len(x_data) - 1)

        # Update the plot with new data
        line.set_data(x_data, y_data)
        ax.relim()
        ax.autoscale_view()

    plt.show()


def get_accelerometer_data(process):
    global previous_data
    line = ''
    while select.select([process.stdout], [], [], 0.0)[0]:
        try:
            line = process.stdout.readline().decode("utf-8").strip()
        except subprocess.TimeoutExpired:
            process.terminate()
    
    previous_data = int(line) if line else previous_data

    return previous_data


def main():
    
    #subprocess.Popen([NIOS_CMD_SHELL_BAT], shell=True)

    while True:
        cmd = input()

        if cmd.startswith("plot"):
            plot_data(int(cmd.split(" ")[1]))
        elif cmd == "exit":
            break
        else:
            print("Invalid command")
    


if __name__ == "__main__":
    main()
