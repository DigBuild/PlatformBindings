using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Input
{
    /// <summary>
    /// A gamepad or game controller.
    /// </summary>
    public sealed class Controller
    {
        internal readonly NativeHandle Handle;

        internal Controller(NativeHandle handle, Guid id)
        {
            Handle = handle;
            Id = id;
        }

        /// <summary>
        /// The unique identifier of this controller.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Whether the controller is currently connected or not.
        /// </summary>
        public bool Connected { get; internal set; } = true;
        /// <summary>
        /// The state of the buttons.
        /// </summary>
        public ButtonStates Buttons { get; } = new();
        /// <summary>
        /// The state of the joysticks.
        /// </summary>
        public JoystickStates Joysticks { get; } = new();
        /// <summary>
        /// The state of the hats.
        /// </summary>
        public HatStates Hats { get; } = new();
        
        /// <summary>
        /// A set of button states.
        /// </summary>
        public sealed class ButtonStates
        {
            internal bool[] States = Array.Empty<bool>();

            internal ButtonStates()
            {
            }

            /// <summary>
            /// The amount of buttons on the controller.
            /// </summary>
            public uint Count => (uint)States.Length;
            /// <summary>
            /// The state of a specific button on the controller.
            /// </summary>
            /// <param name="button">The button</param>
            /// <returns>The state of the button</returns>
            public bool this[uint button] => States[(int)button];
        }
        
        /// <summary>
        /// A set of joystick states.
        /// </summary>
        public sealed class JoystickStates
        {
            internal float[] States = Array.Empty<float>();

            internal JoystickStates()
            {
            }
            
            /// <summary>
            /// The amount of joysticks on the controller.
            /// </summary>
            public uint Count => (uint)States.Length;
            /// <summary>
            /// The state of a specific joystick on the controller.
            /// </summary>
            /// <param name="joystick">The joystick</param>
            /// <returns>The state of the joystick</returns>
            public float this[uint joystick] => States[(int)joystick];
        }
        
        /// <summary>
        /// A set of hat states.
        /// </summary>
        public sealed class HatStates
        {
            internal HatState[] States = Array.Empty<HatState>();

            internal HatStates()
            {
            }
            
            /// <summary>
            /// The amount of hats on the controller.
            /// </summary>
            public uint Count => (uint)States.Length;
            /// <summary>
            /// The state of a specific hat on the controller.
            /// </summary>
            /// <param name="hat">The hat</param>
            /// <returns>The state of the hat</returns>
            public HatState this[uint hat] => States[(int)hat];
        }

        /// <summary>
        /// The state of a hat.
        /// </summary>
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

    /// <summary>
    /// Helper functions for hat states.
    /// </summary>
    public static class ControllerHatStateExtensions
    {
        /// <summary>
        /// Checks if the hat state contains the specified direction.
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="value">The direction</param>
        /// <returns>Whether it is contained in the state</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this Controller.HatState state, Controller.HatState value)
        {
            return state.HasFlag(value);
        }
        
        /// <summary>
        /// Deconstructs the hat state into its 4 directions.
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="up">The up direction</param>
        /// <param name="right">The right direction</param>
        /// <param name="down">The down direction</param>
        /// <param name="left">The left direction</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Controller.HatState state, out bool up, out bool right, out bool down, out bool left)
        {
            up = state.Has(Controller.HatState.Up);
            right = state.Has(Controller.HatState.Right);
            down = state.Has(Controller.HatState.Down);
            left = state.Has(Controller.HatState.Left);
        }
    }

    /// <summary>
    /// Helper functions for controllers.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Tries to find a controller by ID in an enumeration.
        /// </summary>
        /// <param name="src">The enumeration of controllers</param>
        /// <param name="id">The ID to be matched</param>
        /// <returns>A controller, if found, or null</returns>
        public static Controller? Find(this IEnumerable<Controller> src, Guid id)
        {
            return src.FirstOrDefault(controller => controller.Id == id);
        }
    }
}