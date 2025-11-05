using System;

namespace Watermelon
{
    [Flags]
    public enum UIGamepadButtonTag
    {
        None = 0,
        Settings = 1,
        MainMenu = 2,
        Game = 4,
        Complete = 8,
        GameOver = 16,
    }
}