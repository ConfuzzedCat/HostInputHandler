using System.Text;
using CliWrap;

namespace HostInputHandler.Linux;

class YdotoolHandler : IDisposable
{
    private YdotoolHandler()
    {
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
    }

    private static volatile YdotoolHandler? _instance;

    private static readonly Lock Lock = new Lock();

    public static YdotoolHandler GetInstance()
    {
        if (_instance != null) return _instance;
        lock (Lock)
        {
            if (_instance == null)
            {
                _instance = new YdotoolHandler();
            }
        }

        return _instance;
    }

    public static void Shutdown()
    {
        if(_instance == null) return;
        _instance.Dispose();
        _instance = null;
    }

    public bool IsDaemonRunning { get; set; }
    
    private (string username, uint groupid, uint userid) _userInfo;
    private CommandTask<CommandResult>? _ydotoolDaemonTask;
    private CancellationTokenSource? _cts;
    
    CommandTask<CommandResult> LaunchYdotoolDaemon()
    {
        //ydotoold -p /run/user/{uid}/.ydotool_socket -o {uid}:{gid}
        uint uid = _userInfo.userid;
        uint gid = _userInfo.groupid;
        var task = Cli.Wrap("pkexec")
            .WithArguments(
                $"ydotoold -p /run/user/{uid}/.ydotool_socket -o {uid}:{gid}"
            )
            .ExecuteAsync(_cts.Token);


        while (DidSocketSart(uid) == false)
        {
            Thread.Sleep(500);
            Console.WriteLine("Waiting for ydotool socket...");
        }

        //Console.WriteLine("Press any key when you have typed you password.");
        //Console.ReadKey(false);
        
        IsDaemonRunning = true;
        Console.WriteLine($"pid: {task.ProcessId}");
        //task.GetAwaiter().GetResult();

        return task;

        bool DidSocketSart(uint uid)
        {
            var cmd = Cli.Wrap("ls")
                .WithArguments($"/run/user/{uid}/.ydotool_socket")
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStream(Stream.Null))
                .ExecuteAsync().GetAwaiter().GetResult();
            return cmd.IsSuccess;
        }
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
            {
                var uid = _userInfo.userid;
                Cli.Wrap("rm")
                    .WithArguments($"/run/user/{uid}/.ydotool_socket")
                    .ExecuteAsync().GetAwaiter().GetResult();
            }
        }
        IsDaemonRunning = false;
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


    public void Dispose()
    {
        StopYdotoolDaemon();
        _instance = null;
    }
}