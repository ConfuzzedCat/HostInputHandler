using HostInputHandler.Enums;

namespace HostInputHandler.Interfaces;

public interface IInputHandler
{
    // TODO: rewrite it to be async
    #region Keyboard
    bool Type(string value, uint delay = 0);
    bool Press(char key, uint duration = 0, uint delay = 0);
    #endregion
    
    #region Mouse
    #region Buttons
    bool Click(MouseButton mouseButton, uint delay = 0);
    bool DoubleClick(MouseButton mouseButton, uint delay = 0);
    bool MultipleClicks(MouseButton mouseButton, uint times, uint delay = 0);
    bool Hold(MouseButton mouseButton, uint duration, uint delay = 0);
    #endregion

    #region Move
    bool MoveRelative(int dx, int dy, uint delay = 0);
    bool MoveAbsolute(int x, int y, uint delay = 0);
    #endregion
    #endregion
}
