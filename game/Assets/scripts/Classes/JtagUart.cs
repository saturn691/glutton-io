using System;
using System.IO;
using System.Runtime.InteropServices;


// Must be enabled as some functions can return null
#nullable enable

/// <summary>
/// This class is a wrapper for the JTAG Atlantic library written in C++.
/// It provides a simple interface for reading data from the JTAG UART.
/// 
/// Note that the QUARTUS_ROOTDIR environment variable must be set to the
/// Quartus installation directory.
/// </summary>
public class JtagUart
{  
    //=========================================================================
    // Constants
    //=========================================================================

    private const string libjtagAtlantic = "jtag_atlantic.dll";

    private static string jtagAtlanticPath
    {
        get
        {
            string? quartusRootDir = 
                Environment.GetEnvironmentVariable("QUARTUS_ROOTDIR");

            if (quartusRootDir == null)
            {
                throw new Exception("QUARTUS_ROOTDIR environment variable not set");
            }

            // Must be Windows
            return quartusRootDir + "/bin64/jtag_atlantic.dll";
        }
    }

    //=========================================================================
    // Wrappers for external functions from DLLs (C/C++ libraries)
    //=========================================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct JTAGATLANTIC { }    

    // Windows DLL required to find the JTAG Atlantic library
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetDllDirectory(string lpPathName);
    
    [DllImport(
        libjtagAtlantic, 
        EntryPoint = "?jtagatlantic_open@@YAPEAUJTAGATLANTIC@@PEBDHH0@Z", 
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern IntPtr 
    jtagatlantic_open(string cable, int device, int instance, string progname);

    [DllImport(
        libjtagAtlantic,
        EntryPoint = "?jtagatlantic_get_info@@YAXPEAUJTAGATLANTIC@@PEAPEBDPEAH2@Z",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern void 
    jtagatlantic_get_info(IntPtr atlantic, ref string info, ref IntPtr device, ref IntPtr instance);

    [DllImport(
        libjtagAtlantic,
        EntryPoint = "?jtagatlantic_close@@YAXPEAUJTAGATLANTIC@@@Z",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int 
    jtagatlantic_close(IntPtr jtagatlantic);

    [DllImport(
        libjtagAtlantic,
        EntryPoint = "?jtagatlantic_read@@YAHPEAUJTAGATLANTIC@@PEADI@Z",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int 
    jtagatlantic_read(IntPtr jtagatlantic, [Out] byte[] data, uint len);

    [DllImport(
        libjtagAtlantic,
        EntryPoint = "?jtagatlantic_bytes_available@@YAHPEAUJTAGATLANTIC@@@Z",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int
    jtagatlantic_bytes_available(IntPtr jtagatlantic);

    //=========================================================================
    // Fields
    //=========================================================================

    private IntPtr atlantic;

    //=========================================================================
    // Public methods
    //=========================================================================

    public JtagUart()
    {
        // Call the Windows API to find the JTAG library
        string? dirName = Path.GetDirectoryName(jtagAtlanticPath);
        if (dirName == null)
        {
            throw new Exception("Failed to get directory name of JTAG Atlantic library");
        }

        bool result = SetDllDirectory(dirName);
        if (!result)
        {
            throw new Exception("Failed to set DLL directory");
        }

        atlantic = jtagatlantic_open("", -1, -1, "");
        if (atlantic == IntPtr.Zero)
        {
            throw new Exception("Failed to open JTAG Atlantic");
        }
        else
        {
            Console.WriteLine("JTAG Atlantic opened");
        }
    }

    ~JtagUart()
    {
        jtagatlantic_close(atlantic);
    }

    public int BytesAvailable()
    {
        return jtagatlantic_bytes_available(atlantic);
    }

    /// <summary>
    /// Reads as much data from UART as possible.
    /// </summary>
    /// <returns>A bytes object of the data read.</returns>
    /// <exception cref="Exception">If the UART stops working</exception>
    public byte[] Read()
    {
        int buf_len = BytesAvailable();
        byte[] buf = new byte[buf_len];

        int bytes_read = jtagatlantic_read(atlantic, buf, (uint)buf.Length);

        if (bytes_read == -1)
        {
            throw new Exception("Failed to read from JTAG Atlantic");
        }

        return buf;
    }


    /// <summary>
    /// Closes the JTAG Atlantic connection. This should be called when the
    /// program is finished with the JTAG UART, otherwise the connection will
    /// be left hanging.
    /// </summary>
    public void Close()
    {
        jtagatlantic_close(atlantic);
    }
}
