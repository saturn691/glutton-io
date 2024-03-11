/*
 *  Testbench for the debounce module
 *  Author: William Huynh <wh1022@ic.ac.uk>
*/

#include "testbench.h"

class DebounceTestbench : public Testbench
{
protected:
    void initializeInputs() override
    {
        top->clk = 0;
        top->in = 0;
    }
};


TEST_F(DebounceTestbench, UnitStepResponse)
{
    // Test the unit step response of the debounce module
    top->in = 0;
    runSimulation(10);
    EXPECT_EQ(top->out, 0);

    top->in = 1;
    runSimulation(10);
    EXPECT_EQ(top->out, 1);

    /*
        Now we will test the debounce module with a 10ms pulse
        BOUNCE PERIOD
    */

    top->in = 0;
    runSimulation(100);
    // The output should still be 1 (bouncing around)
    EXPECT_EQ(top->out, 1);

    top->in = 1;
    runSimulation(100);
    // The output should still be 1 (bouncing around)
    EXPECT_EQ(top->out, 1);
   
    // END OF BOUNCE PERIOD
    top->in = 0;
    runSimulation(60000);
    EXPECT_EQ(top->out, 0);
}

int main(int argc, char **argv)
{
    Verilated::commandArgs(argc, argv);
    testing::InitGoogleTest(&argc, argv);
    Verilated::mkdir("logs");
    auto res = RUN_ALL_TESTS();

    return res;
}