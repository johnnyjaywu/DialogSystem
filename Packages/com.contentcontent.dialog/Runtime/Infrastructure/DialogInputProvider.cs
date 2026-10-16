// File: DialogInputProvider.cs
using System;
using UnityEngine.InputSystem;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Adapts a Unity Input System action into domain-level advancement requests.
    /// Accepts an injected InputAction directly from generated C# input classes to avoid scene prefab dependencies.
    /// </summary>
    public sealed class DialogInputProvider : IDialogInputProvider
    {
        private readonly InputAction _advanceAction;
        private bool _isEnabled;
        private bool _isDisposed;

        /// <summary>
        /// Occurs when the player requests to advance the dialog, typically by pressing a designated input button. Subscribers can handle this event to trigger dialog progression in response to player input.
        /// </summary>
        public event Action AdvanceRequested;

        /// <summary>
        /// Initializes a new instance of the DialogInputProvider class with the specified InputAction for advancing dialog. The provided InputAction is used to detect player input and raise the AdvanceRequested event when the action is performed. This constructor ensures that the InputAction is not null, throwing an ArgumentNullException if it is.
        /// </summary>
        /// <param name="advanceAction">
        /// The InputAction that represents the player's input for advancing dialog. This action should be configured in the Unity Input System and passed to the DialogInputProvider to enable dialog progression based on player input.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the advanceAction parameter is null, indicating that a valid InputAction must be provided for dialog advancement.
        /// </exception>
        public DialogInputProvider(InputAction advanceAction)
        {
            _advanceAction = advanceAction ?? throw new ArgumentNullException(nameof(advanceAction));
        }

        /// <summary>
        /// Enables the dialog input provider, allowing it to listen for player input and raise the AdvanceRequested event when the configured InputAction is performed. This method binds the event handler to the InputAction's performed event and enables the action, allowing the dialog system to respond to player input for advancing dialog. If the provider is already enabled or has been disposed, this method has no effect.
        /// </summary>
        public void Enable()
        {
            ThrowIfDisposed();

            if (_isEnabled)
            {
                return;
            }

            // Bind to performed rather than started so hold/tap interaction modifiers evaluate completely before advancing.
            _advanceAction.performed += HandleAdvancePerformed;
            _advanceAction.Enable();
            _isEnabled = true;
        }

        /// <summary>
        /// Disables the dialog input provider, preventing it from listening for player input and raising the AdvanceRequested event. This method unbinds the event handler from the InputAction's performed event and disables the action, effectively stopping the dialog system from responding to player input for advancing dialog. If the provider is already disabled or has been disposed, this method has no effect.
        /// </summary>
        public void Disable()
        {
            if (_isDisposed || !_isEnabled)
            {
                return;
            }

            _advanceAction.performed -= HandleAdvancePerformed;
            _advanceAction.Disable();
            _isEnabled = false;
        }

        /// <summary>
        /// Disposes of the dialog input provider, releasing resources and preventing further use. This method disables the provider if it is currently enabled, unbinds event handlers, and marks the instance as disposed. After calling Dispose, the instance should not be used again, and any attempts to enable or disable it will throw an ObjectDisposedException. This method is idempotent; calling it multiple times has no additional effect.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            Disable();
            _isDisposed = true;
        }

        private void HandleAdvancePerformed(InputAction.CallbackContext context)
        {
            AdvanceRequested?.Invoke();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(DialogInputProvider));
            }
        }
    }
}