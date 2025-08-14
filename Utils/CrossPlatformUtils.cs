using HostInputHandler.Interfaces;

namespace HostInputHandler.Utils;

public class CrossPlatformUtils
{
    public static IInputHandler GetInputHandler()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                return new Windows.InputHandler();
            case PlatformID.Unix:
                return new Linux.InputHandler();
            case PlatformID.MacOSX:
            case PlatformID.Other:
            default:
                break;
        }
        throw new PlatformNotSupportedException();
    }
}