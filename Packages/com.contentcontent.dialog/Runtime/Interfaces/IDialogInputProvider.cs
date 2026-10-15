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
        event Action AdvanceRequested;

        void Enable();
        void Disable();
    }
}