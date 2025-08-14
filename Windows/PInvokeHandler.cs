using System.Runtime.InteropServices;
using HostInputHandler.Enums;

namespace HostInputHandler.Windows;

public class PInvokeHandler
{
    // ---------- P/Invoke ----------
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, MapVirtualKeyMapTypes uMapType);

    // ---------- Types ----------
    private enum InputType : uint
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [Flags]
    private enum MouseEventF : uint
    {
        MOVE = 0x0001,
        LEFTDOWN = 0x0002,
        LEFTUP = 0x0004,
        RIGHTDOWN = 0x0008,
        RIGHTUP = 0x0010,
        MIDDLEDOWN = 0x0020,
        MIDDLEUP = 0x0040,
        XDOWN = 0x0080,
        XUP = 0x0100,
        WHEEL = 0x0800,
        HWHEEL = 0x01000,
        MOVE_NOCOALESCE = 0x2000,
        VIRTUALDESK = 0x4000,
        ABSOLUTE = 0x8000
    }

    [Flags]
    private enum KeyEventF : uint
    {
        KEYDOWN = 0x0000,
        EXTENDEDKEY = 0x0001,
        KEYUP = 0x0002,
        UNICODE = 0x0004,
        SCANCODE = 0x0008
    }

    private enum MapVirtualKeyMapTypes : uint
    {
        MAPVK_VK_TO_VSC = 0x0,
        MAPVK_VSC_TO_VK = 0x1,
        MAPVK_VK_TO_CHAR = 0x2,
        MAPVK_VSC_TO_VK_EX = 0x3,
        MAPVK_VK_TO_VSC_EX = 0x4
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
        public static int Size => Marshal.SizeOf<INPUT>();
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public WindowsKeys wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    // ---------- Public helpers ----------
    /// <summary>Presses and releases a virtual-key (e.g., VK codes like 0x41 for 'A').</summary>
    public static void SendKey(WindowsKeys vk)
    {
        ushort scan = (ushort)MapVirtualKey((uint)vk, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);

        var down = new INPUT
        {
            type = (uint)InputType.Keyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = vk,
                    wScan = scan,
                    dwFlags = (uint)KeyEventF.SCANCODE, // set SCANCODE for robustness
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        var up = down;
        up.U.ki.dwFlags = (uint)(KeyEventF.SCANCODE | KeyEventF.KEYUP);

        var inputs = new[] { down, up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }

    /// <summary>Types a Unicode character (e.g., 'é' or emoji) using KEYEVENTF.UNICODE.</summary>
    public static void SendChar(char ch)
    {
        var down = new INPUT
        {
            type = (uint)InputType.Keyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = ch,
                    dwFlags = (uint)KeyEventF.UNICODE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        var up = down;
        up.U.ki.dwFlags = (uint)(KeyEventF.UNICODE | KeyEventF.KEYUP);

        var inputs = new[] { down, up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }
    
    public static void SendCharDown(char ch)
    {
        var down = new INPUT
        {
            type = (uint)InputType.Keyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = ch,
                    dwFlags = (uint)KeyEventF.UNICODE | (uint)KeyEventF.KEYDOWN,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        
        var inputs = new[] { down};
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }
    
    public static void SendCharUp(char ch)
    {
        var up = new INPUT
        {
            type = (uint)InputType.Keyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = ch,
                    dwFlags = (uint)KeyEventF.UNICODE | (uint)KeyEventF.KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        var inputs = new[] { up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }

    /// <summary>Moves the mouse by relative deltas.</summary>
    public static void MoveMouseRelative(int dx, int dy)
    {
        var input = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    mouseData = 0,
                    dwFlags = (uint)MouseEventF.MOVE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        Check(SendInput(1, new[] { input }, INPUT.Size));
    }

    /// <summary>Moves the mouse to absolute x and y coordinates.</summary>
    public static void MoveMouseAbsolute(int x, int y)
    {
        var input = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = x,
                    dy = y,
                    mouseData = 0,
                    dwFlags = (uint)MouseEventF.ABSOLUTE | (uint)MouseEventF.MOVE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        Check(SendInput(1, new[] { input }, INPUT.Size));
    }
    
    /// <summary>Left-click at current cursor position.</summary>
    public static void LeftClick()
    {
        var down = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.LEFTDOWN }
            }
        };
        var up = down;
        up.U.mi.dwFlags = (uint)MouseEventF.LEFTUP;

        var inputs = new[] { down, up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }
    
    /// <summary>Right-click at current cursor position.</summary>
    public static void RightClick()
    {
        var down = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.RIGHTDOWN }
            }
        };
        var up = down;
        up.U.mi.dwFlags = (uint)MouseEventF.RIGHTUP;

        var inputs = new[] { down, up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }
    
    /// <summary>Right-click at current cursor position.</summary>
    public static void MiddleClick()
    {
        var down = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.MIDDLEDOWN }
            }
        };
        var up = down;
        up.U.mi.dwFlags = (uint)MouseEventF.MIDDLEUP;

        var inputs = new[] { down, up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }    
    /// <summary>Left-down at current cursor position.</summary>
    public static void LeftClickDown()
    {
        var down = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.LEFTDOWN }
            }
        };

        var inputs = new[] { down };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }
    
    /// <summary>Right-down at current cursor position.</summary>
    public static void RightClickDown()
    {
        var down = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.RIGHTDOWN }
            }
        };

        var inputs = new[] { down };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }
    
    /// <summary>Middle-down at current cursor position.</summary>
    public static void MiddleClickDown()
    {
        var down = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.MIDDLEDOWN }
            }
        };

        var inputs = new[] { down };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }    
    /// <summary>Left-up at current cursor position.</summary>
    public static void LeftClickUp()
    {
        var up = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.LEFTUP }
            }
        };

        var inputs = new[] { up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }
    
    /// <summary>Right-up at current cursor position.</summary>
    public static void RightClickUp()
    {
        var up = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.RIGHTUP }
            }
        };
        
        var inputs = new[] { up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }
    
    /// <summary>Middle-up at current cursor position.</summary>
    public static void MiddleClickUp()
    {
        var up = new INPUT
        {
            type = (uint)InputType.Mouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT { dwFlags = (uint)MouseEventF.MIDDLEUP }
            }
        };

        var inputs = new[] { up };
        Check(SendInput((uint)inputs.Length, inputs, INPUT.Size));
    }

    // ---------- Utility ----------
    private static void Check(uint sent)
    {
        if (sent == 0)
        {
            var err = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"SendInput failed. GetLastError={err}");
        }
    }
}