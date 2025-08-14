using HostInputHandler.Enums;

namespace HostInputHandler.Interfaces;

public interface IKeyboardHandler<in T> where T : Enum
{
    bool Type(string value, uint delay = 0);
    bool Press(T key, uint duration = 0, uint delay = 0);
}