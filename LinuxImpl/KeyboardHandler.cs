using System.Diagnostics;
using System.Text;
using CliWrap;
using CliWrap.EventStream;
using HostInputHandler.Interfaces;

namespace HostInputHandler.LinuxImpl;

// This Singleton implementation uses double-check locking to ensure thread-safe lazy initialization.
public sealed class KeyboardHandler : IKeyboardHandler
{
    private KeyboardHandler()
    {
        Initialize();
    }

    private static KeyboardHandler? _instance;

    // Synchronization object for thread safety.
    private static readonly Lock Lock = new Lock();

    public static KeyboardHandler GetInstance()
    {
        if (_instance != null) return _instance;
        lock (Lock)
        {
            _instance ??= new KeyboardHandler();
        }

        return _instance;
    }

    private (string username, uint groupid, uint userid) _userInfo;
    private CommandTask<CommandResult> _ydotoolDaemonTask;
    private CancellationTokenSource _cts;
    private bool _isDaemonRunning;

    public bool Initialize()
    {
        if (_instance is not null)
        {
            return false;
        }
        
        StringBuilder commandResult = new StringBuilder();

        var command = Cli.Wrap("id")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(commandResult))
            .ExecuteAsync().GetAwaiter().GetResult();

        if (command.IsSuccess == false)
        {
            throw new Exception("Failed to get id of user.");
        }
        _userInfo = (
            GetUsername(commandResult.ToString()), 
            GetGroupId(commandResult.ToString()), 
            GetUserId(commandResult.ToString())
            );

        command = Cli.Wrap("pkexec")
            .WithArguments("--version")
            .ExecuteAsync().GetAwaiter().GetResult();
        if (command.IsSuccess == false)
        {
            throw new Exception("Failed to get version of pkexec. Is pkexec installed?");
        }
        command = Cli.Wrap("ydotoold")
            .WithArguments("--version")
            .ExecuteAsync().GetAwaiter().GetResult();
        if (command.IsSuccess == false)
        {
            throw new Exception("Failed to get version of ydotoold. Is ydotool installed?");
        }
        
        _cts = new CancellationTokenSource();

        _ydotoolDaemonTask = LaunchYdotoolDaemon();
        
        return true;
    }

    public void Shutdown()
    {
        if (_instance is null)
        {
            return;
        }
        StopYdotoolDaemon();
    }

    public bool Type(string value, uint delay = 0)
    {
        if (_isDaemonRunning == false)
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

    public bool Press(byte[] keys, uint duration = 0, uint delay = 0)
    {
        Thread.Sleep((int)delay);
        throw new NotImplementedException();
    }

    CommandTask<CommandResult> LaunchYdotoolDaemon()
    {
        //ydotoold -p /run/user/{uid}/.ydotool_socket -o {uid}:{gid}
        uint uid = _userInfo.userid;
        uint gid = _userInfo.groupid;
        var task = Cli.Wrap("pkexec")
            .WithArguments(
                $"ydotoold -p /run/user/{uid}/.ydotool_socket -o {uid}:{gid}; sleep 5; rm /run/user/{uid}/.ydotool_socket"
            )
            .ExecuteAsync(_cts.Token);
        // 
        //int msToWaitForPassword = 10000;
        //Thread.Sleep(msToWaitForPassword);
        Console.WriteLine("Press any key when you have typed you password.");
        Console.ReadKey(false);
        
        _isDaemonRunning = true;
        Console.WriteLine($"pid: {task.ProcessId}");
        //task.GetAwaiter().GetResult();

        return task;
    }

    void StopYdotoolDaemon()
    {
        _cts.Cancel();
        try
        {
            var commandResult = _ydotoolDaemonTask.GetAwaiter().GetResult();
            if (commandResult.IsSuccess == false)
            {
                throw new Exception("Failed to stop ydotool daemon.");
            }
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
        }
        finally
        {
            _cts.Dispose();
        }
        _isDaemonRunning = false;
    }

    uint GetUserId(string commandResult)
    {
        // Expected: uid=nnnn(username) gid=nnnn(username) groups=nnnn(username)
        // where nnnn is a number and username is a string.
        var startIndex = commandResult.IndexOf('=') + 1;
        var len = commandResult.IndexOf('(') - startIndex;
        string uid = commandResult.Substring(startIndex, len );
        return uint.Parse(uid);
    }

    uint GetGroupId(string commandResult)
    {
        // Expected: uid=nnnn(username) gid=nnnn(username) groups=nnnn(username)
        // where nnnn is a number and username is a string.
        var temp =  commandResult.Substring(commandResult.IndexOf(')')+2);
        var startIndex = temp.IndexOf('=') + 1;
        var len = temp.IndexOf('(') - startIndex;
        string gid = temp.Substring(startIndex, len );
        return uint.Parse(gid);
    }

    string GetUsername(string commandResult)
    {
        // Expected: uid=nnnn(username) gid=nnnn(username) groups=nnnn(username)
        // where nnnn is a number and username is a string.
        var startIndex = commandResult.IndexOf('(') + 1;
        var len = commandResult.IndexOf(')') - startIndex;
        return commandResult.Substring(startIndex, len);
    }
}