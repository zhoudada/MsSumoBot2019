using System;

namespace SumoBotController
{
    [Flags]
    public enum CommandType
    {
        None = 0,
        Forward = 1,
        Backward = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        LeftForward = Left | Forward,
        RightForward = Right | Forward,
        LeftBackward = Left | Backward,
        RightBackward = Right | Backward,
        LowSpeed = 1 << 4,
        MediumSpeed = 1 << 5,
        HighSpeed = 1 << 6,
        Stop = 1 << 7
    }
}
