namespace Pamella;

/// <summary>
/// Represents a input from hardware.
/// Equals to System.Windows.Forms.Keys.
/// </summary>
public enum Input
{
        //
        // Resumo:
        //     The bitmask to extract modifiers from a key value.
        Modifiers = -65536,
        //
        // Resumo:
        //     No key pressed.
        None = 0,
        //
        // Resumo:
        //     The left mouse button.
        LButton = 1,
        //
        // Resumo:
        //     The right mouse button.
        RButton = 2,
        //
        // Resumo:
        //     The CANCEL key.
        Cancel = 3,
        //
        // Resumo:
        //     The middle mouse button (three-button mouse).
        MButton = 4,
        //
        // Resumo:
        //     The first x mouse button (five-button mouse).
        XButton1 = 5,
        //
        // Resumo:
        //     The second x mouse button (five-button mouse).
        XButton2 = 6,
        //
        // Resumo:
        //     The BACKSPACE key.
        Back = 8,
        //
        // Resumo:
        //     The TAB key.
        Tab = 9,
        //
        // Resumo:
        //     The LINEFEED key.
        LineFeed = 10,
        //
        // Resumo:
        //     The CLEAR key.
        Clear = 12,
        //
        // Resumo:
        //     The RETURN key.
        Return = 13,
        //
        // Resumo:
        //     The ENTER key.
        Enter = 13,
        //
        // Resumo:
        //     The SHIFT key.
        ShiftKey = 16,
        //
        // Resumo:
        //     The CTRL key.
        ControlKey = 17,
        //
        // Resumo:
        //     The ALT key.
        Menu = 18,
        //
        // Resumo:
        //     The PAUSE key.
        Pause = 19,
        //
        // Resumo:
        //     The CAPS LOCK key.
        Capital = 20,
        //
        // Resumo:
        //     The CAPS LOCK key.
        CapsLock = 20,
        //
        // Resumo:
        //     The IME Kana mode key.
        KanaMode = 21,
        //
        // Resumo:
        //     The IME Hanguel mode key. (maintained for compatibility; use HangulMode)
        HanguelMode = 21,
        //
        // Resumo:
        //     The IME Hangul mode key.
        HangulMode = 21,
        //
        // Resumo:
        //     The IME Junja mode key.
        JunjaMode = 23,
        //
        // Resumo:
        //     The IME final mode key.
        FinalMode = 24,
        //
        // Resumo:
        //     The IME Hanja mode key.
        HanjaMode = 25,
        //
        // Resumo:
        //     The IME Kanji mode key.
        KanjiMode = 25,
        //
        // Resumo:
        //     The ESC key.
        Escape = 27,
        //
        // Resumo:
        //     The IME convert key.
        IMEConvert = 28,
        //
        // Resumo:
        //     The IME nonconvert key.
        IMENonconvert = 29,
        //
        // Resumo:
        //     The IME accept key, replaces System.Windows.Forms.Keys.IMEAceept.
        IMEAccept = 30,
        //
        // Resumo:
        //     The IME accept key. Obsolete, use System.Windows.Forms.Keys.IMEAccept instead.
        IMEAceept = 30,
        //
        // Resumo:
        //     The IME mode change key.
        IMEModeChange = 31,
        //
        // Resumo:
        //     The SPACEBAR key.
        Space = 32,
        //
        // Resumo:
        //     The PAGE UP key.
        Prior = 33,
        //
        // Resumo:
        //     The PAGE UP key.
        PageUp = 33,
        //
        // Resumo:
        //     The PAGE DOWN key.
        Next = 34,
        //
        // Resumo:
        //     The PAGE DOWN key.
        PageDown = 34,
        //
        // Resumo:
        //     The END key.
        End = 35,
        //
        // Resumo:
        //     The HOME key.
        Home = 36,
        //
        // Resumo:
        //     The LEFT ARROW key.
        Left = 37,
        //
        // Resumo:
        //     The UP ARROW key.
        Up = 38,
        //
        // Resumo:
        //     The RIGHT ARROW key.
        Right = 39,
        //
        // Resumo:
        //     The DOWN ARROW key.
        Down = 40,
        //
        // Resumo:
        //     The SELECT key.
        Select = 41,
        //
        // Resumo:
        //     The PRINT key.
        Print = 42,
        //
        // Resumo:
        //     The EXECUTE key.
        Execute = 43,
        //
        // Resumo:
        //     The PRINT SCREEN key.
        Snapshot = 44,
        //
        // Resumo:
        //     The PRINT SCREEN key.
        PrintScreen = 44,
        //
        // Resumo:
        //     The INS key.
        Insert = 45,
        //
        // Resumo:
        //     The DEL key.
        Delete = 46,
        //
        // Resumo:
        //     The HELP key.
        Help = 47,
        //
        // Resumo:
        //     The 0 key.
        D0 = 48,
        //
        // Resumo:
        //     The 1 key.
        D1 = 49,
        //
        // Resumo:
        //     The 2 key.
        D2 = 50,
        //
        // Resumo:
        //     The 3 key.
        D3 = 51,
        //
        // Resumo:
        //     The 4 key.
        D4 = 52,
        //
        // Resumo:
        //     The 5 key.
        D5 = 53,
        //
        // Resumo:
        //     The 6 key.
        D6 = 54,
        //
        // Resumo:
        //     The 7 key.
        D7 = 55,
        //
        // Resumo:
        //     The 8 key.
        D8 = 56,
        //
        // Resumo:
        //     The 9 key.
        D9 = 57,
        //
        // Resumo:
        //     The A key.
        A = 65,
        //
        // Resumo:
        //     The B key.
        B = 66,
        //
        // Resumo:
        //     The C key.
        C = 67,
        //
        // Resumo:
        //     The D key.
        D = 68,
        //
        // Resumo:
        //     The E key.
        E = 69,
        //
        // Resumo:
        //     The F key.
        F = 70,
        //
        // Resumo:
        //     The G key.
        G = 71,
        //
        // Resumo:
        //     The H key.
        H = 72,
        //
        // Resumo:
        //     The I key.
        I = 73,
        //
        // Resumo:
        //     The J key.
        J = 74,
        //
        // Resumo:
        //     The K key.
        K = 75,
        //
        // Resumo:
        //     The L key.
        L = 76,
        //
        // Resumo:
        //     The M key.
        M = 77,
        //
        // Resumo:
        //     The N key.
        N = 78,
        //
        // Resumo:
        //     The O key.
        O = 79,
        //
        // Resumo:
        //     The P key.
        P = 80,
        //
        // Resumo:
        //     The Q key.
        Q = 81,
        //
        // Resumo:
        //     The R key.
        R = 82,
        //
        // Resumo:
        //     The S key.
        S = 83,
        //
        // Resumo:
        //     The T key.
        T = 84,
        //
        // Resumo:
        //     The U key.
        U = 85,
        //
        // Resumo:
        //     The V key.
        V = 86,
        //
        // Resumo:
        //     The W key.
        W = 87,
        //
        // Resumo:
        //     The X key.
        X = 88,
        //
        // Resumo:
        //     The Y key.
        Y = 89,
        //
        // Resumo:
        //     The Z key.
        Z = 90,
        //
        // Resumo:
        //     The left Windows logo key (Microsoft Natural Keyboard).
        LWin = 91,
        //
        // Resumo:
        //     The right Windows logo key (Microsoft Natural Keyboard).
        RWin = 92,
        //
        // Resumo:
        //     The application key (Microsoft Natural Keyboard).
        Apps = 93,
        //
        // Resumo:
        //     The computer sleep key.
        Sleep = 95,
        //
        // Resumo:
        //     The 0 key on the numeric keypad.
        NumPad0 = 96,
        //
        // Resumo:
        //     The 1 key on the numeric keypad.
        NumPad1 = 97,
        //
        // Resumo:
        //     The 2 key on the numeric keypad.
        NumPad2 = 98,
        //
        // Resumo:
        //     The 3 key on the numeric keypad.
        NumPad3 = 99,
        //
        // Resumo:
        //     The 4 key on the numeric keypad.
        NumPad4 = 100,
        //
        // Resumo:
        //     The 5 key on the numeric keypad.
        NumPad5 = 101,
        //
        // Resumo:
        //     The 6 key on the numeric keypad.
        NumPad6 = 102,
        //
        // Resumo:
        //     The 7 key on the numeric keypad.
        NumPad7 = 103,
        //
        // Resumo:
        //     The 8 key on the numeric keypad.
        NumPad8 = 104,
        //
        // Resumo:
        //     The 9 key on the numeric keypad.
        NumPad9 = 105,
        //
        // Resumo:
        //     The multiply key.
        Multiply = 106,
        //
        // Resumo:
        //     The add key.
        Add = 107,
        //
        // Resumo:
        //     The separator key.
        Separator = 108,
        //
        // Resumo:
        //     The subtract key.
        Subtract = 109,
        //
        // Resumo:
        //     The decimal key.
        Decimal = 110,
        //
        // Resumo:
        //     The divide key.
        Divide = 111,
        //
        // Resumo:
        //     The F1 key.
        F1 = 112,
        //
        // Resumo:
        //     The F2 key.
        F2 = 113,
        //
        // Resumo:
        //     The F3 key.
        F3 = 114,
        //
        // Resumo:
        //     The F4 key.
        F4 = 115,
        //
        // Resumo:
        //     The F5 key.
        F5 = 116,
        //
        // Resumo:
        //     The F6 key.
        F6 = 117,
        //
        // Resumo:
        //     The F7 key.
        F7 = 118,
        //
        // Resumo:
        //     The F8 key.
        F8 = 119,
        //
        // Resumo:
        //     The F9 key.
        F9 = 120,
        //
        // Resumo:
        //     The F10 key.
        F10 = 121,
        //
        // Resumo:
        //     The F11 key.
        F11 = 122,
        //
        // Resumo:
        //     The F12 key.
        F12 = 123,
        //
        // Resumo:
        //     The F13 key.
        F13 = 124,
        //
        // Resumo:
        //     The F14 key.
        F14 = 125,
        //
        // Resumo:
        //     The F15 key.
        F15 = 126,
        //
        // Resumo:
        //     The F16 key.
        F16 = 127,
        //
        // Resumo:
        //     The F17 key.
        F17 = 128,
        //
        // Resumo:
        //     The F18 key.
        F18 = 129,
        //
        // Resumo:
        //     The F19 key.
        F19 = 130,
        //
        // Resumo:
        //     The F20 key.
        F20 = 131,
        //
        // Resumo:
        //     The F21 key.
        F21 = 132,
        //
        // Resumo:
        //     The F22 key.
        F22 = 133,
        //
        // Resumo:
        //     The F23 key.
        F23 = 134,
        //
        // Resumo:
        //     The F24 key.
        F24 = 135,
        //
        // Resumo:
        //     The NUM LOCK key.
        NumLock = 144,
        //
        // Resumo:
        //     The SCROLL LOCK key.
        Scroll = 145,
        //
        // Resumo:
        //     The left SHIFT key.
        LShiftKey = 160,
        //
        // Resumo:
        //     The right SHIFT key.
        RShiftKey = 161,
        //
        // Resumo:
        //     The left CTRL key.
        LControlKey = 162,
        //
        // Resumo:
        //     The right CTRL key.
        RControlKey = 163,
        //
        // Resumo:
        //     The left ALT key.
        LMenu = 164,
        //
        // Resumo:
        //     The right ALT key.
        RMenu = 165,
        //
        // Resumo:
        //     The browser back key.
        BrowserBack = 166,
        //
        // Resumo:
        //     The browser forward key.
        BrowserForward = 167,
        //
        // Resumo:
        //     The browser refresh key.
        BrowserRefresh = 168,
        //
        // Resumo:
        //     The browser stop key.
        BrowserStop = 169,
        //
        // Resumo:
        //     The browser search key.
        BrowserSearch = 170,
        //
        // Resumo:
        //     The browser favorites key.
        BrowserFavorites = 171,
        //
        // Resumo:
        //     The browser home key.
        BrowserHome = 172,
        //
        // Resumo:
        //     The volume mute key.
        VolumeMute = 173,
        //
        // Resumo:
        //     The volume down key.
        VolumeDown = 174,
        //
        // Resumo:
        //     The volume up key.
        VolumeUp = 175,
        //
        // Resumo:
        //     The media next track key.
        MediaNextTrack = 176,
        //
        // Resumo:
        //     The media previous track key.
        MediaPreviousTrack = 177,
        //
        // Resumo:
        //     The media Stop key.
        MediaStop = 178,
        //
        // Resumo:
        //     The media play pause key.
        MediaPlayPause = 179,
        //
        // Resumo:
        //     The launch mail key.
        LaunchMail = 180,
        //
        // Resumo:
        //     The select media key.
        SelectMedia = 181,
        //
        // Resumo:
        //     The start application one key.
        LaunchApplication1 = 182,
        //
        // Resumo:
        //     The start application two key.
        LaunchApplication2 = 183,
        //
        // Resumo:
        //     The OEM Semicolon key on a US standard keyboard.
        OemSemicolon = 186,
        //
        // Resumo:
        //     The OEM 1 key.
        Oem1 = 186,
        //
        // Resumo:
        //     The OEM plus key on any country/region keyboard.
        Oemplus = 187,
        //
        // Resumo:
        //     The OEM comma key on any country/region keyboard.
        Oemcomma = 188,
        //
        // Resumo:
        //     The OEM minus key on any country/region keyboard.
        OemMinus = 189,
        //
        // Resumo:
        //     The OEM period key on any country/region keyboard.
        OemPeriod = 190,
        //
        // Resumo:
        //     The OEM question mark key on a US standard keyboard.
        OemQuestion = 191,
        //
        // Resumo:
        //     The OEM 2 key.
        Oem2 = 191,
        //
        // Resumo:
        //     The OEM tilde key on a US standard keyboard.
        Oemtilde = 192,
        //
        // Resumo:
        //     The OEM 3 key.
        Oem3 = 192,
        //
        // Resumo:
        //     The OEM open bracket key on a US standard keyboard.
        OemOpenBrackets = 219,
        //
        // Resumo:
        //     The OEM 4 key.
        Oem4 = 219,
        //
        // Resumo:
        //     The OEM pipe key on a US standard keyboard.
        OemPipe = 220,
        //
        // Resumo:
        //     The OEM 5 key.
        Oem5 = 220,
        //
        // Resumo:
        //     The OEM close bracket key on a US standard keyboard.
        OemCloseBrackets = 221,
        //
        // Resumo:
        //     The OEM 6 key.
        Oem6 = 221,
        //
        // Resumo:
        //     The OEM singled/double quote key on a US standard keyboard.
        OemQuotes = 222,
        //
        // Resumo:
        //     The OEM 7 key.
        Oem7 = 222,
        //
        // Resumo:
        //     The OEM 8 key.
        Oem8 = 223,
        //
        // Resumo:
        //     The OEM angle bracket or backslash key on the RT 102 key keyboard.
        OemBackslash = 226,
        //
        // Resumo:
        //     The OEM 102 key.
        Oem102 = 226,
        //
        // Resumo:
        //     The PROCESS KEY key.
        ProcessKey = 229,
        //
        // Resumo:
        //     Used to pass Unicode characters as if they were keystrokes. The Packet key value
        //     is the low word of a 32-bit virtual-key value used for non-keyboard input methods.
        Packet = 231,
        //
        // Resumo:
        //     The ATTN key.
        Attn = 246,
        //
        // Resumo:
        //     The CRSEL key.
        Crsel = 247,
        //
        // Resumo:
        //     The EXSEL key.
        Exsel = 248,
        //
        // Resumo:
        //     The ERASE EOF key.
        EraseEof = 249,
        //
        // Resumo:
        //     The PLAY key.
        Play = 250,
        //
        // Resumo:
        //     The ZOOM key.
        Zoom = 251,
        //
        // Resumo:
        //     A constant reserved for future use.
        NoName = 252,
        //
        // Resumo:
        //     The PA1 key.
        Pa1 = 253,
        //
        // Resumo:
        //     The CLEAR key.
        OemClear = 254,
        //
        // Resumo:
        //     The bitmask to extract a key code from a key value.
        KeyCode = 65535,
        //
        // Resumo:
        //     The SHIFT modifier key.
        Shift = 65536,
        //
        // Resumo:
        //     The CTRL modifier key.
        Control = 131072,
        //
        // Resumo:
        //     The ALT modifier key.
        Alt = 262144
}