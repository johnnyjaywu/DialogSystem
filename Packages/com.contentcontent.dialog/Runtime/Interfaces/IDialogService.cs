// File: IDialogService.cs
using System;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Top-level domain facade for starting, stopping, and observing dialog sessions.
    /// Implements IDisposable to ensure active typewriter tasks and input subscriptions are cleaned up.
    /// </summary>
    public interface IDialogService : IDisposable
    {
        event Action DialogStarted;
        event Action DialogEnded;

        bool IsActive { get; }

        void StartDialog(IDialogSource source);
        void StopDialog();
    }
}