using HostInputHandler.Enums;
using HostInputHandler.Linux;
using HostInputHandler.Utils;
using HostInputHandler.Windows;

namespace HostInputHandler;

class Program
{
    static OperatingSystem OS = Environment.OSVersion;
    private static CancellationTokenSource _cts = new CancellationTokenSource();
    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (o, e) => Shutdown();
        Console.CancelKeyPress += (o, e) => StopEventLoop();
        Console.WriteLine($"Starting event loop");
        

        EventLoop().GetAwaiter().GetResult();
        Console.WriteLine("End of event loop.");
    }

    static void Shutdown()
    {
        Console.WriteLine("Shutting down...");
        _cts.Dispose();
        if (OS.Platform == PlatformID.Unix)
        {
            YdotoolDaemonHandler.Shutdown();
        }
    }

    static void StopEventLoop()
    {
        _cts.Cancel();
        Shutdown();
    }

    static Task EventLoop()
    {
        var inputHandler = CrossPlatformUtils.GetInputHandler();
        while (_cts.IsCancellationRequested == false)
        {
            inputHandler.Click(MouseButton.LeftButton, 1000);
        }
        return Task.CompletedTask;
    }
}