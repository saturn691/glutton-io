/*
 *  Base testbench class
 *  Author: William Huynh <wh1022@ic.ac.uk>
*/


#include "Vdut.h"
#include "verilated.h"
#include "verilated_vcd_c.h"
#include "gtest/gtest.h"


#define MAX_SIM_CYCLES  10000

class Testbench : public ::testing::Test
{
protected:
    Vdut* top;
    VerilatedVcdC* tfp;

    void SetUp() override
    {
        // Init top verilog instance
        top = new Vdut;

        // Init trace dump
        Verilated::traceEverOn(true);
        tfp = new VerilatedVcdC;
        top->trace(tfp, 99);
        tfp->open("waveform.vcd");

        initializeInputs();
    }

    void TearDown() override
    {
        top->final();
        tfp->close();

        delete top;
        delete tfp;
    }

    void runSimulation(int num_cycles = 1)
    {
        // Run simuation for many clock cycles
        for (int i = 0; i < num_cycles; ++i)
        {
            // dump variables into VCD file and toggle clock
            for (int clk = 0; clk < 2; ++clk)
            {
                top->eval();
                tfp->dump(2*ticks + clk);    // picoseconds
                top->clk = !top->clk;
            }

            ticks++;

            if (Verilated::gotFinish())
            {
                exit(0);
            }
        }
    }

    virtual void initializeInputs() = 0;

private:
    int ticks = 0;
};
