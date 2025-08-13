using HostInputHandler.LinuxImpl;

namespace HostInputHandler;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var keyboardHandler = KeyboardHandler.GetInstance();
        keyboardHandler.Type("Test", 500); 
        keyboardHandler.Shutdown();
    }
}