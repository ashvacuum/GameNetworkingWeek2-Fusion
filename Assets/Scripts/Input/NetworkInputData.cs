
using Fusion;
using UnityEngine;

namespace GNW2.Input
{
    public struct NetworkInputData : INetworkInput
    {
        public const byte MOUSEBUTTON0 = 1;
        public const byte JUMPBUTTON = 2;
        public NetworkButtons buttons;
        public Vector3 Direction;
    }
}
