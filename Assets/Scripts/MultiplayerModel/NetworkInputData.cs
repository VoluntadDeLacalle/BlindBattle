using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte SPACEBAR = 1;
    public const byte SLOW = 1 << 1;
    public const byte FAST = 1 << 2;
    public const byte CLIMB = 1 << 3;
    public const byte DESCEND = 1 << 4;

    public Vector3 direction;
    public Vector2 mouseInput;

    public byte buttons;
}
