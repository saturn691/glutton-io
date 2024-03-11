##################################################
#   Must be in a Quartus shell to run this script
#   This script generates the bsp and application
##################################################

export PATH=$PATH:$QUARTUS_ROOTDIR/../nios2eds/bin/gnu/H-x86_64-mingw32/bin/

# Generate the bsp
mkdir bsp
cd bsp
    nios2-bsp hal . ../../quartus/nios_accelerometer.sopcinfo \
        --set hal.max_file_descriptors 4 \
        --set hal.enable_small_c_library true \
        --set hal.enable_exit false \
        --set hal.enable_lightweight_device_driver_api true \
        --set hal.enable_clean_exit false \
        --set hal.enable_sim_optimize false \
        --set hal.enable_reduced_device_drivers true \
        --set hal.make.bsp_cflags_optimization '-Os'
cd ..

# Generate the application
cd app
    nios2-app-generate-makefile.exe \
        --bsp-dir ../bsp/ \
        --elf-name main.elf \
        --src-dir .
    make
cd ..


# Generate the application
cd latency_app
    nios2-app-generate-makefile.exe \
        --bsp-dir ../bsp/ \
        --elf-name main.elf \
        --src-dir .
    make
cd ..

echo ">> Now run 'nios2-download -g app/main.elf' to flash the FPGA"