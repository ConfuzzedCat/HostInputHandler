namespace HostInputHandler.Interfaces;

public interface IKeyboardHandler : IDisposable
{
    bool Initialize();
    void Shutdown();

    bool Type(string value, uint delay = 0);
    bool Press(byte[] keys, uint duration = 0, uint delay = 0);

    void IDisposable.Dispose()
    {
        Shutdown();
    }
}