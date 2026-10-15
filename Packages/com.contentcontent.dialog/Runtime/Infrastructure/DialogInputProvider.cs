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

        public event Action AdvanceRequested;

        public DialogInputProvider(InputAction advanceAction)
        {
            _advanceAction = advanceAction ?? throw new ArgumentNullException(nameof(advanceAction));
        }

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