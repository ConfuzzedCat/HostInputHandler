using System.Text;
using CliWrap;
using HostInputHandler.Enums;
using HostInputHandler.Interfaces;

namespace HostInputHandler.Linux;

public sealed class KeyboardHandler : IKeyboardHandler<LinuxKeys>
{
    // TODO: rewrite it to be async
    private readonly YdotoolHandler _ydotoolHandler = YdotoolHandler.GetInstance();

    public bool Type(string value, uint delay = 0)
    {
        if (_ydotoolHandler.IsDaemonRunning == false)
        {
            throw new Exception("You are not running ydotool.");
        }

        value = value.Replace("'", "");
        value = value.Replace("\"", "");

        Console.WriteLine($"Typing: {value}");
        var result = new StringBuilder();

        Thread.Sleep((int)delay);

        var cmd = Cli.Wrap("ydotool")
            .WithArguments($"type {value}")
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(result))
            .ExecuteAsync().GetAwaiter().GetResult();

        Console.WriteLine($"result: {result} - {cmd.IsSuccess}");
        return cmd.IsSuccess;
    }

    public bool Press(LinuxKeys key, uint duration = 0, uint delay = 0)
    {
        if (duration == 0)
        {
            duration = 1;
        }

        if (_ydotoolHandler.IsDaemonRunning == false)
        {
            throw new Exception("You are not running ydotool.");
        }


        Thread.Sleep((int)delay);
        var result = new StringBuilder();

        var cmd = Cli.Wrap("ydotool")
            .WithArguments($"key -d {duration} {(ushort)key}:1 {(ushort)key}:0")
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(result))
            .ExecuteAsync().GetAwaiter().GetResult();


        Console.WriteLine($"result: {result} - {cmd.IsSuccess}");
        return cmd.IsSuccess;
    }
}
    