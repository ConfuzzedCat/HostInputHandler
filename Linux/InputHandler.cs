using System.Text;
using CliWrap;
using HostInputHandler.Enums;
using HostInputHandler.Interfaces;
using HostInputHandler.Utils;

namespace HostInputHandler.Linux;

public sealed class InputHandler : IInputHandler
{
    private readonly YdotoolDaemonHandler _ydotoolDaemonHandler = YdotoolDaemonHandler.GetInstance();
    private const string YdotoolCli = "ydotool";
    private const int BTN_LEFT       = 0xC0;
    private const int BTN_RIGHT      = 0xC1;
    private const int BTN_MIDDLE     = 0xC2;
    private const int BTN_LEFTDOWN   = 0x40;
    private const int BTN_RIGHTDOWN  = 0x41;
    private const int BTN_MIDDLEDOWN = 0x42;
    private const int BTN_LEFTUP     = 0x80;
    private const int BTN_RIGHTUP    = 0x81;
    private const int BTN_MIDDLEUP   = 0x82;

    public bool Type(string value, uint delay = 0)
    {
        AssertDaemonRunningAndWaitingForDelay(delay);

        value = value.Replace("'", "");
        value = value.Replace("\"", "");

        var result = new StringBuilder();


        var cmd = Cli.Wrap(YdotoolCli)
            .WithArguments($"type {value}")
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(result))
            .ExecuteAsync().GetAwaiter().GetResult();

        //DEBUG: Console.WriteLine($"result: {result} - {cmd.IsSuccess}");
        return cmd.IsSuccess;
    }

    public bool Press(char key, uint duration = 0, uint delay = 0)
    {
        if (duration == 0)
        {
            duration = 1;
        }
        AssertDaemonRunningAndWaitingForDelay(delay);

        int keyCode = KeyCodeConverter.CharToLinuxKey(key);

        var result = new StringBuilder();

        var cmd = Cli.Wrap(YdotoolCli)
            .WithArguments($"key --next-delay {duration} {keyCode}:1 {keyCode}:0")
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(result))
            .ExecuteAsync().GetAwaiter().GetResult();


        //DEBUG: Console.WriteLine($"result: {result} - {cmd.IsSuccess}");
        return cmd.IsSuccess;
    }

    public bool Click(MouseButton mouseButton, uint delay = 0)
    {
        AssertDaemonRunningAndWaitingForDelay(delay);

        int mb;
        switch (mouseButton)
        {
            case MouseButton.RightButton:
                mb = BTN_RIGHT;
                break;
            case MouseButton.LeftButton:
                mb = BTN_LEFT;
                break;
            case MouseButton.MiddleButton:
                mb = BTN_MIDDLE;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null);
        }

        Console.WriteLine($"click {mb:X}");
        var cmd = Cli.Wrap(YdotoolCli)
            .WithArguments($"click {mb:X}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync().GetAwaiter().GetResult();
        return cmd.IsSuccess;
    }

    public bool DoubleClick(MouseButton mouseButton, uint delay = 0)
    {
        AssertDaemonRunningAndWaitingForDelay(delay);

        return MultipleClicks(mouseButton, 2);
    }

    public bool MultipleClicks(MouseButton mouseButton, uint times, uint delay = 0)
    {
        AssertDaemonRunningAndWaitingForDelay(delay);

        int mb;
        switch (mouseButton)
        {
            case MouseButton.RightButton:
                mb = BTN_RIGHT;
                break;
            case MouseButton.LeftButton:
                mb = BTN_LEFT;
                break;
            case MouseButton.MiddleButton:
                mb = BTN_MIDDLE;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null);
        }

        var cmd = Cli.Wrap(YdotoolCli)
            .WithArguments($"click --repeat {times} {mb:X}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync().GetAwaiter().GetResult();
        return cmd.IsSuccess;
    }

    public bool Hold(MouseButton mouseButton, uint duration, uint delay = 0)
    {
        AssertDaemonRunningAndWaitingForDelay(delay);
        
        int mbDown, mbUp;
        switch (mouseButton)
        {
            case MouseButton.RightButton:
                mbDown = BTN_RIGHTDOWN;
                mbUp = BTN_RIGHTUP;
                break;
            case MouseButton.LeftButton:
                mbDown = BTN_LEFTDOWN;
                mbUp = BTN_LEFTUP;
                break;
            case MouseButton.MiddleButton:
                mbDown = BTN_MIDDLEDOWN;
                mbUp = BTN_MIDDLEUP;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null);
        }

        var cmd = Cli.Wrap(YdotoolCli)
            .WithArguments($"click --next-delay {duration} {mbDown:X}  {mbUp:X}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync().GetAwaiter().GetResult();
        return cmd.IsSuccess;
    }

    public bool MoveRelative(int dx, int dy, uint delay = 0)
    {
        AssertDaemonRunningAndWaitingForDelay(delay);
        var cmd = Cli.Wrap(YdotoolCli)
            .WithArguments($"mousemove -x {dx} -y {dy}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync().GetAwaiter().GetResult();

        return cmd.IsSuccess;
    }

    public bool MoveAbsolute(int x, int y, uint delay = 0)
    {
        AssertDaemonRunningAndWaitingForDelay(delay);
        var cmd = Cli.Wrap(YdotoolCli)
            .WithArguments($"mousemove --absolute -x {x} -y {y}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync().GetAwaiter().GetResult();

        return cmd.IsSuccess;
    }

    private void AssertDaemonRunningAndWaitingForDelay(uint delay)
    {
        if (_ydotoolDaemonHandler.IsDaemonRunning == false)
        {
            throw new Exception("Ydotool is not running.");
        }

        Thread.Sleep((int)delay);
    }
}
