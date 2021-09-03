using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Input
{
    [NativeSymbols("dbp_global_input_context_", SymbolTransformationMethod.Underscore)]
    internal interface IGlobalInputContextBindings
    {
        public delegate void InitializeCallback(
            IntPtr controllers, uint controllerCount
        );
        public delegate void GetControllerGuidCallback(
            string guid
        );
        public delegate void ControllerStateCallback(
            bool connected,
            IntPtr buttonStates, uint buttonCount,
            IntPtr joystickStates, uint joystickCount,
            IntPtr hatStates, uint hatCount
        );

        void Initialize(IntPtr instance, InitializeCallback callback);

        void Update(IntPtr instance);

        void GetControllerGuid(IntPtr controller, GetControllerGuidCallback callback);

        void GetControllerState(IntPtr controller, ControllerStateCallback callback);
    }

    /// <summary>
    /// A global cross-platform input context.
    /// </summary>
    public sealed class GlobalInputContext
    {
        internal static readonly IGlobalInputContextBindings Bindings = NativeLib.Get<IGlobalInputContextBindings>();

        private readonly IntPtr _ptr;
        private readonly HashSet<Controller> _controllers = new();
        private bool _initialized;

        internal GlobalInputContext(IntPtr ptr)
        {
            _ptr = ptr;
        }

        /// <summary>
        /// The set of active game controllers.
        /// </summary>
        public ISet<Controller> Controllers => _controllers;

        /// <summary>
        /// Polls the state of all the active controllers.
        /// </summary>
        public unsafe void Update()
        {
            if (!_initialized)
            {
                _initialized = true;

                Bindings.Initialize(_ptr, (controllers, count) =>
                {
                    var span = new Span<IntPtr>(controllers.ToPointer(), (int) count);
                    foreach (var ptr in span)
                    {
                        Bindings.GetControllerGuid(ptr, guid =>
                        {
                            _controllers.Add(new Controller(new NativeHandle(ptr), Guid.Parse(guid)));
                        });
                    }
                });
            }

            foreach (var controller in _controllers)
            {
                Bindings.GetControllerState(
                    controller.Handle,
                    (connected, buttonStates, buttonCount, joystickStates, joystickCount, hatStates, hatCount) =>
                    {
                        controller.Connected = connected;
                        controller.Buttons.States = new Span<byte>(buttonStates.ToPointer(), (int)buttonCount).ToArray()
                            .Select(b => b > 0).ToArray();
                        controller.Joysticks.States = new Span<float>(joystickStates.ToPointer(), (int)joystickCount).ToArray();
                        controller.Hats.States = new Span<Controller.HatState>(hatStates.ToPointer(), (int)hatCount).ToArray();
                    }
                );
            }

            _controllers.RemoveWhere(c => !c.Connected);

            Bindings.Update(_ptr);
        }
    }
}