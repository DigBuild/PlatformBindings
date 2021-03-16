using System;
using System.Runtime.CompilerServices;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Input
{
    public sealed class Controller
    {
        internal readonly NativeHandle Handle;

        internal Controller(NativeHandle handle, Guid id)
        {
            Handle = handle;
            Id = id;
        }

        public Guid Id { get; }

        public bool Connected { get; internal set; } = true;
        public ButtonStates Buttons { get; } = new();
        public JoystickStates Joysticks { get; } = new();
        public HatStates Hats { get; } = new();
        
        public sealed class ButtonStates
        {
            internal bool[] States = Array.Empty<bool>();

            internal ButtonStates()
            {
            }

            public uint Count => (uint)States.Length;
            public bool this[uint button] => States[(int)button];
        }

        public sealed class JoystickStates
        {
            internal float[] States = Array.Empty<float>();

            internal JoystickStates()
            {
            }

            public uint Count => (uint)States.Length;
            public float this[uint button] => States[(int)button];
        }

        public sealed class HatStates
        {
            internal HatState[] States = Array.Empty<HatState>();

            internal HatStates()
            {
            }

            public uint Count => (uint)States.Length;
            public HatState this[uint button] => States[(int)button];
        }

        [Flags]
        public enum HatState : byte
        {
            None = 0,
            Up = 1 << 0,
            Right = 1 << 1,
            Down = 1 << 2,
            Left = 1 << 3
        }
    }

    public static class ControllerHatStateExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this Controller.HatState state, Controller.HatState value)
        {
            return state.HasFlag(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Controller.HatState state, out bool up, out bool right, out bool down, out bool left)
        {
            up = state.Has(Controller.HatState.Up);
            right = state.Has(Controller.HatState.Right);
            down = state.Has(Controller.HatState.Down);
            left = state.Has(Controller.HatState.Left);
        }
    }

    // public static class ControllerExtensions
    // {
    //     public static Controller? Find(this IEnumerable<Controller> src, Guid id)
    //     {
    //         return src.FirstOrDefault(controller => controller.Id == id);
    //     }
    // }
}