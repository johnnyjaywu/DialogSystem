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
        /// <summary>
        /// Occurs when a dialog session has started
        /// </summary>
        event Action DialogStarted;

        /// <summary>
        /// Occurs when a dialog session has ended
        /// </summary>
        event Action DialogEnded;

        /// <summary>
        /// Indicates whether a dialog session is currently active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Starts a dialog session using the provided dialog source. If a session is already active, it will be stopped before starting the new one.
        /// </summary>
        /// <param name="source"></param>
        void StartDialog(IDialogSource source);

        /// <summary>
        /// Stops the current dialog session if one is active. If no session is active, this method has no effect.
        /// </summary>
        void StopDialog();
    }
}