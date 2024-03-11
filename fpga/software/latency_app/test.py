"""
Uses the NIOS II terminal to collect data from the FPGA to simulate a game.

Some controller processing done in the FPGA, and some done in the Python
program. 
"""

import sys
import subprocess
import select
import intel_jtag_uart
import time

RTT = []
iterations = 100


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


    # GRTT calculation
    for i in range(iterations):
        # Send RECEIVED to through UART to calculate RTT
        start_RTT = time.perf_counter()
        ju.write("start\n".encode())
        

        # Handle UART input
        ju.read()
        end_RTT = time.perf_counter()

        RTT.append(end_RTT - start_RTT)
    

    print("Average RTT: ", 1000*sum(RTT)/len(RTT), "ms")





if __name__ == "__main__":
    main()
