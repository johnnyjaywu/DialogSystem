// File: IDialogInputProvider.cs

using System;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Abstracts player advancement and skip inputs from Unity's New Input System.
    /// Allows the core dialog orchestrator to be driven deterministically by unit test mocks.
    /// </summary>
    public interface IDialogInputProvider : IDisposable
    {
        /// <summary>
        /// Raised when the player requests to advance the dialog.
        /// </summary>
        event Action AdvanceRequested;

        /// <summary>
        /// Enables the input provider to start listening for player input.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables the input provider from listening for player input.
        /// </summary>
        void Disable();
    }
}