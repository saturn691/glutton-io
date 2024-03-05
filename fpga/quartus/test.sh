#!/bin/bash

# Runs the testbench

# Constants
TEST_FOLDER=$(realpath "tb")
RTL_FOLDER=$(realpath "rtl")
NAME="debounce"

# Cleanup
rm -rf obj_dir

# Translate the Verilog to C++
verilator   -Wall --trace -Wno-MODDUP \
            -cc ${RTL_FOLDER}/${NAME}.v \
            --exe ${TEST_FOLDER}/${NAME}_tb.cpp \
            -y ${RTL_FOLDER} \
            --prefix "Vdut" \
            -o Vdut \
            -CFLAGS "-fprofile-generate -fprofile-correction" \
            -LDFLAGS "-lgtest -lpthread -fprofile-generate"

# Build C++ project with automatically generated Makefile
make -j -C obj_dir/ -f Vdut.mk

# Run executable simulaton file
./obj_dir/Vdut
