using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public Vector2 mouseInput;

    public const byte SPACEBAR = 0x01;

    public byte buttons;
}
