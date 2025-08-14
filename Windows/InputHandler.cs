using HostInputHandler.Enums;
using HostInputHandler.Interfaces;

namespace HostInputHandler.Windows;

public class InputHandler :  IInputHandler
{
    public bool Type(string value, uint delay = 0)
    {
        Thread.Sleep((int)delay);
        foreach (var c in value)
        {
            PInvokeHandler.SendChar(c);
        }
        return true;
    }

    public bool Press(char key, uint duration = 0, uint delay = 0)
    {
        if (duration == 0)
        {
            duration++;
        }
        Thread.Sleep((int)delay);
        PInvokeHandler.SendCharDown(key);
        Thread.Sleep((int)duration);
        PInvokeHandler.SendCharUp(key);
        return true;
    }

    public bool Click(MouseButton mouseButton, uint delay = 0)
    {
        Thread.Sleep((int)delay);
        switch (mouseButton)
        {
            case MouseButton.RightButton:
                PInvokeHandler.RightClick();
                break;
            case MouseButton.LeftButton:
                PInvokeHandler.LeftClick();
                break;
            case MouseButton.MiddleButton:
                PInvokeHandler.MiddleClick();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null);
        }
        return true;
    }

    public bool DoubleClick(MouseButton mouseButton, uint delay = 0)
    {
        Thread.Sleep((int)delay);
        return MultipleClicks(mouseButton, 2);
    }

    public bool MultipleClicks(MouseButton mouseButton, uint times, uint delay = 0)
    {
        Thread.Sleep((int)delay);
        for (var i = 0; i < times; i++)
        {
            Click(mouseButton);
            //TODO: test if a delay between clicks is needed.
            Thread.Sleep(0);
        }
        return true;
    }

    public bool Hold(MouseButton mouseButton, uint duration, uint delay = 0)
    {
        Thread.Sleep((int)delay);
        switch (mouseButton)
        {
            case MouseButton.RightButton:
                PInvokeHandler.RightClickDown();
                Thread.Sleep((int)duration);
                PInvokeHandler.RightClickUp();
                break;
            case MouseButton.LeftButton:
                PInvokeHandler.LeftClickDown();
                Thread.Sleep((int)duration);
                PInvokeHandler.LeftClickUp();
                break;
            case MouseButton.MiddleButton:
                PInvokeHandler.MiddleClickDown();
                Thread.Sleep((int)duration);
                PInvokeHandler.MiddleClickUp();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null);
        }
        return true;
    }

    public bool MoveRelative(int dx, int dy, uint delay = 0)
    {
        Thread.Sleep((int)delay);
        PInvokeHandler.MoveMouseRelative(dx, dy);
        return true;
    }

    public bool MoveAbsolute(int x,int y, uint delay = 0)
    {
        Thread.Sleep((int)delay);
        PInvokeHandler.MoveMouseAbsolute(x,y);
        return true;
    }
}