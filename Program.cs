using HostInputHandler.Enums;
using HostInputHandler.Linux;

namespace HostInputHandler;

class Program
{
    static OperatingSystem OS = Environment.OSVersion;
    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (o, e) => Shutdown();
        Console.CancelKeyPress += (o, e) => Shutdown();
        Console.WriteLine("Starting.");
        var keyboardHandler = new KeyboardHandler();
        //keyboardHandler.Type("Test", 2000);
        keyboardHandler.Press(LinuxKeys.KEY_A, 5000, 5000);
    }

    static void Shutdown()
    {
        Console.WriteLine("Shutting down...");
        if (OS.Platform == PlatformID.Unix)
        {
            YdotoolHandler.Shutdown();
        }
    }
}