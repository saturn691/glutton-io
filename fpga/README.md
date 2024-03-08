# FPGA - Guidance

## Getting started

1. Install dependencies if you haven't. In Powershell:

```
pip install matplotlib
pip install intel-jtag-uart
```

2. Using Quartus, open up *Tools > Programmer*, then flash the FPGA using the
[`top.sof`](quartus/top.sof) file attached.

3. Open up the NIOS II terminal. Run these commands:

```bash
# You may or may not need to run this command
export PATH=$PATH:$QUARTUS_ROOTDIR/../nios2eds/bin/gnu/H-x86_64-mingw32/bin/

cd fpga/software
./generate_app.sh
nios2-download -g app/main.elf
```

4. Using Powershell, open up [`demo.py`](vm/demo.py) to test that the NIOS II
hardware and software have been programmed correctly.

```bash
python fpga/vm/demo.py
```

## Documentation

### Quartus

Contains the RTL and testbench for hardware modules. The `.sopcinfo` file 
contains the information necessary to build the BSP (board support package) in
the software folder.

To use the testbench you need to install GTest and Verilator

```bash
sudo apt install libgtest-dev
```

To install Verilator, you need to compile it from source to get the latest 
version. Follow the instructions on the [GitHub](https://github.com/verilator).

### Software

Contains the apps and the BSP required. A script is used to generate the apps.
To create a new app, create a new app folder, and follow the bash script to 
generate the application makefile, which is used to generate the .elf used to
flash the board.

### VM

See [`vm/README.md`](vm/README.md)