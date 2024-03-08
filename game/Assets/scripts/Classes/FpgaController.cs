using System;


/// <summary>
/// Every function required to read data from the FPGA. Uses the JTAG UART
/// interface to communicate with the FPGA.
/// </summary>
public class FpgaController
{
    //=========================================================================
    // Private methods
    //=========================================================================

    /// <summary>
    /// Decodes a valid message received from the FPGA. Stores the values in 
    /// the class fields.
    /// </summary>
    /// <param name="input">the string input</param>
    private void decode(string input)
    {
        // Input string is hexadecimal
        // Not the most efficient, but PCs are powerful so it's a non-issue
        int dataIn = Convert.ToInt32(input, 16);

        int readingX = (dataIn >> 22) & 0x3FF;
        if ((readingX & 0x200) != 0)
        {
            readingX = -((~readingX & 0x3FF) + 1);
        }

        int readingY = (dataIn >> 12) & 0x3FF;
        if ((readingY & 0x200) != 0)
        {
            readingY = -((~readingY & 0x3FF) + 1);
        }

        int switchValue = (dataIn >> 2) & 0x3FF;
        int key0 = (dataIn >> 1) & 0x1;
        int key1 = dataIn & 0x1;

        // Update the class fields
        this.readingX = readingX;
        this.readingY = readingY;
        this.switchValue = switchValue;
        this.key0 = key0;
        this.key1 = key1;

        return;
    }

    /// <summary>
    /// Maps the velocity to a value between -1 and 1
    /// Removes stick drift
    /// </summary>
    /// <param name="value">The accelerometer reading</param>
    /// <returns>The normalised reading</returns>
    private float MapVelocity(int value)
    {
        int MAX_V = 255;
        double DEADZONE = 0.1;

        if (Math.Abs(value) < DEADZONE * MAX_V)
        {
            return 0;
        }
        else if (Math.Abs(value) > MAX_V)
        {
            return value > 0 ? 1 : -1;
        }
        else
        {
            // Linear mapping
            // IMPORTANT: BOTH AXIS ARE INVERTED
            return (float)-value / MAX_V;
        }
    }

    //=========================================================================
    // Fields
    //=========================================================================

    private int readingX;
    private int readingY;
    private int switchValue;  
    private int key0;
    private int key1;
    private int previousKey0;
    private int previousKey1;

    // The JTAG UART object (pretty much library) to communicate with the FPGA
    private JtagUart jtagUart;

    // The previous data received from the FPGA in case nothing comes through
    private string previousData = string.Empty;

    //=========================================================================
    // Public methods
    //=========================================================================

    public FpgaController()
    {
        jtagUart = new JtagUart();
        
        // Set the fields to a default value
        readingX = 0;
        readingY = 0;
        switchValue = 0;
        key0 = 0;
        key1 = 0;
    }

    public float GetReadingX()
    {
        return MapVelocity(readingX);
    }

    public float GetReadingY()
    {
        return MapVelocity(readingY);
    }

    public int GetSwitchValue()
    {
        return switchValue;
    }

    public int GetKey0()
    {
        // Check for a rising edge
        bool risingEdge = key0 == 1 && previousKey0 == 0;

        // Update the previous state
        previousKey0 = key0;

        return risingEdge ? 1 : 0;
    }

    public int GetKey1()
    {
        // Check for a rising edge
        bool risingEdge = key1 == 1 && previousKey1 == 0;

        // Update the previous state
        previousKey1 = key1;

        return risingEdge ? 1 : 0;
    }

    /// <summary>
    /// Reads the data from the FPGA and updates the class fields.
    /// Must be called every frame.
    /// </summary>    
    public void UpdateData()
    {
        byte[] read = jtagUart.Read();
        string s = System.Text.Encoding.UTF8.GetString(read);
        string[] lines = s.Split('\n');
        
        string line = string.Empty;
        
        // Second to last line NOT last line because the last line could be 
        // incomplete due to the read buffer
        if (lines.Length > 2)
        {
            line = lines[^2];
        }

        // Sometimes we don't get any data so we use the previous data
        // This time we just update the previous data
        if (!string.IsNullOrEmpty(line))
        {
            previousData = line;
        }
        
        if (!string.IsNullOrEmpty(previousData))
        {
            decode(previousData);
        }

        return;
    }
}