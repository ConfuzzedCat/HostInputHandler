namespace HostInputHandler.Utils;

public static class KeyCodeConverter
{
    // Linux evdev keycode to Windows Virtual Key mapping
    private static readonly Dictionary<int, int> LinuxToWindows = new()
    {
        // Alphabet
        { 30, 0x41 }, // KEY_A -> VK_A
        { 48, 0x42 }, // KEY_B -> VK_B
        { 46, 0x43 }, // KEY_C -> VK_C
        { 32, 0x44 }, // KEY_D -> VK_D
        { 18, 0x45 }, // KEY_E -> VK_E
        { 33, 0x46 }, // KEY_F -> VK_F
        { 34, 0x47 }, // KEY_G -> VK_G
        { 35, 0x48 }, // KEY_H -> VK_H
        { 23, 0x49 }, // KEY_I -> VK_I
        { 36, 0x4A }, // KEY_J -> VK_J
        { 37, 0x4B }, // KEY_K -> VK_K
        { 38, 0x4C }, // KEY_L -> VK_L
        { 50, 0x4D }, // KEY_M -> VK_M
        { 49, 0x4E }, // KEY_N -> VK_N
        { 24, 0x4F }, // KEY_O -> VK_O
        { 25, 0x50 }, // KEY_P -> VK_P
        { 16, 0x51 }, // KEY_Q -> VK_Q
        { 19, 0x52 }, // KEY_R -> VK_R
        { 31, 0x53 }, // KEY_S -> VK_S
        { 20, 0x54 }, // KEY_T -> VK_T
        { 22, 0x55 }, // KEY_U -> VK_U
        { 47, 0x56 }, // KEY_V -> VK_V
        { 17, 0x57 }, // KEY_W -> VK_W
        { 45, 0x58 }, // KEY_X -> VK_X
        { 21, 0x59 }, // KEY_Y -> VK_Y
        { 44, 0x5A }, // KEY_Z -> VK_Z

        // Numbers
        { 2, 0x31 }, // KEY_1 -> VK_1
        { 3, 0x32 }, // KEY_2 -> VK_2
        { 4, 0x33 }, // KEY_3 -> VK_3
        { 5, 0x34 }, // KEY_4 -> VK_4
        { 6, 0x35 }, // KEY_5 -> VK_5
        { 7, 0x36 }, // KEY_6 -> VK_6
        { 8, 0x37 }, // KEY_7 -> VK_7
        { 9, 0x38 }, // KEY_8 -> VK_8
        { 10, 0x39 }, // KEY_9 -> VK_9
        { 11, 0x30 }, // KEY_0 -> VK_0

        // Control keys
        { 1, 0x1B },  // KEY_ESC -> VK_ESCAPE
        { 28, 0x0D }, // KEY_ENTER -> VK_RETURN
        { 57, 0x20 }, // KEY_SPACE -> VK_SPACE
        { 14, 0x08 }, // KEY_BACKSPACE -> VK_BACK
        { 15, 0x09 }, // KEY_TAB -> VK_TAB
        { 42, 0xA0 }, // KEY_LEFTSHIFT -> VK_LSHIFT
        { 54, 0xA1 }, // KEY_RIGHTSHIFT -> VK_RSHIFT
        { 29, 0xA2 }, // KEY_LEFTCTRL -> VK_LCONTROL
        { 97, 0xA3 }, // KEY_RIGHTCTRL -> VK_RCONTROL
        { 56, 0xA4 }, // KEY_LEFTALT -> VK_LMENU
        { 100, 0xA5 }, // KEY_RIGHTALT -> VK_RMENU
    };

    // Reverse mapping
    private static readonly Dictionary<int, int> WindowsToLinux = LinuxToWindows
        .ToDictionary(kv => kv.Value, kv => kv.Key);

    /// <summary>
    /// Converts a Linux evdev keycode to a Windows Virtual Key code.
    /// </summary>
    public static int KeyToVK(int linuxKeyCode)
    {
        return LinuxToWindows.TryGetValue(linuxKeyCode, out var vk) ? vk : -1;
    }

    /// <summary>
    /// Converts a Windows Virtual Key code to a Linux evdev keycode.
    /// </summary>
    public static int VKToKey(int vkCode)
    {
        return WindowsToLinux.TryGetValue(vkCode, out var linuxKey) ? linuxKey : -1;
    }
    
    /// <summary>
    /// Converts a character to a Linux evdev keycode.
    /// Returns -1 if the character is not mapped.
    /// </summary>
    public static int CharToLinuxKey(char c)
    {
        c = char.ToUpperInvariant(c);
        int vk = c;
        return WindowsToLinux.TryGetValue(vk, out var linuxKey) ? linuxKey : -1;
    }

    /// <summary>
    /// Converts a character to a Windows Virtual Key code.
    /// Returns -1 if the character is not mapped.
    /// </summary>
    public static int CharToVK(char c)
    {
        c = char.ToUpperInvariant(c);
        return c;
    }

    /// <summary>
    /// Converts a Linux evdev keycode to its character representation.
    /// Returns '\0' if the key is not mapped or not a printable character.
    /// </summary>
    public static char LinuxKeyToChar(int linuxKeyCode)
    {
        if (LinuxToWindows.TryGetValue(linuxKeyCode, out var vk) && vk >= 0x20 && vk <= 0x7E)
            return (char)vk;
        return '\0';
    }

    /// <summary>
    /// Converts a Windows Virtual Key code to its character representation.
    /// Returns '\0' if the VK is not a printable character.
    /// </summary>
    public static char VKToChar(int vkCode)
    {
        if (vkCode >= 0x20 && vkCode <= 0x7E)
            return (char)vkCode;
        return '\0';
    }
}