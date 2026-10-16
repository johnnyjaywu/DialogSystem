// File: DialogService.cs
using System;
using System.Threading;
using UnityEngine;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Pure C# domain orchestrator (MVP Presenter) that coordinates dialog progression, typewriter timing, and input.
    /// Completely agnostic to UI Toolkit or MonoBehaviours to guarantee isolated unit testing.
    /// </summary>
    public sealed class DialogService : IDialogService
    {
        private readonly IDialogView _view;
        private readonly IDialogInputProvider _inputProvider;
        private readonly DialogSettings _settings;
        private readonly Func<float, CancellationToken, Awaitable> _delayAsync;

        private IDialogSource _currentSource;
        private DialogLine _currentLine;
        private CancellationTokenSource _typewriterCts;
        private bool _isTyping;
        private bool _isDisposed;

        /// <summary>
        /// Event triggered when a dialog session starts. Subscribers can use this to perform any necessary setup or state changes when dialog begins.
        /// </summary>
        public event Action DialogStarted;

        /// <summary>
        /// Event triggered when a dialog session ends. Subscribers can use this to perform any necessary cleanup or state changes when dialog concludes.
        /// </summary>
        public event Action DialogEnded;

        /// <summary>
        /// Indicates whether a dialog session is currently active. Returns true if a dialog session is in progress; otherwise, returns false.  
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DialogService class with the specified view, input provider, settings, and optional delay function for typewriter effect.
        /// </summary>
        /// <param name="view">
        /// The IDialogView implementation responsible for rendering dialog lines and choices to the user interface. Cannot be null.
        /// </param>
        /// <param name="inputProvider">
        /// The IDialogInputProvider implementation responsible for capturing user input to advance dialog or select choices. Cannot be null.
        /// </param>
        /// <param name="settings">
        /// The DialogSettings instance containing configuration options for dialog behavior, such as typewriter speed and skipping behavior. Cannot be null.
        /// </param>
        /// <param name="delayAsync">
        /// Optional asynchronous delay function used for the typewriter effect. If not provided, defaults to Unity's WaitForSecondsAsync. This allows for custom delay implementations, such as editor-compatible delays during testing.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the required parameters (view, inputProvider, or settings) are null.
        /// </exception>
        public DialogService(
            IDialogView view,
            IDialogInputProvider inputProvider,
            DialogSettings settings,
            Func<float, CancellationToken, Awaitable> delayAsync = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _inputProvider = inputProvider ?? throw new ArgumentNullException(nameof(inputProvider));
            _settings = settings;

            // Default to Unity's player-loop WaitForSecondsAsync at runtime while allowing EditMode tests
            // to inject an editor-compatible async delay strategy.
            _delayAsync = delayAsync ?? Awaitable.WaitForSecondsAsync;

            _view.ChoiceSelected += HandleChoiceSelected;
            _inputProvider.AdvanceRequested += HandleAdvanceRequested;
        }

        /// <summary>
        /// Begins a dialog session using the provided IDialogSource. If a dialog session is already active, it will be stopped before starting the new one.
        /// </summary>
        /// <param name="source">
        /// The IDialogSource implementation that provides the dialog lines and choices for the session. Cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the source parameter is null.
        /// </exception>
        public void StartDialog(IDialogSource source)
        {
            ThrowIfDisposed();

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (IsActive)
            {
                StopDialog();
            }

            _currentSource = source;
            _currentLine = default;
            IsActive = true;

            _view.ClearChoices();
            _view.SetSpeaker(string.Empty);
            _view.SetFullText(string.Empty);
            _view.Show();
            _inputProvider.Enable();

            DialogStarted?.Invoke();
            AdvanceToNextLine();
        }

        /// <summary>
        /// Stops the current dialog session, if one is active. This method cancels any ongoing typewriter effect, clears the dialog view, disables input, and triggers the DialogEnded event. If no dialog session is active, this method has no effect.
        /// </summary>
        public void StopDialog()
        {
            if (!IsActive)
            {
                return;
            }

            CancelTypewriter();

            IsActive = false;
            _currentSource = null;
            _currentLine = default;

            _inputProvider.Disable();
            _view.ClearChoices();
            _view.SetSpeaker(string.Empty);
            _view.SetFullText(string.Empty);
            _view.Hide();

            DialogEnded?.Invoke();
        }

        /// <summary>
        /// Disposes of the DialogService instance, stopping any active dialog session and releasing resources. After calling Dispose, the instance should not be used again. This method is idempotent; calling it multiple times has no additional effect.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            StopDialog();

            _view.ChoiceSelected -= HandleChoiceSelected;
            _inputProvider.AdvanceRequested -= HandleAdvanceRequested;

            _isDisposed = true;
        }

        private void HandleAdvanceRequested()
        {
            if (!IsActive)
            {
                return;
            }

            // First advance press during active typing completes the reveal immediately if skipping is enabled.
            if (_isTyping)
            {
                if (_settings.AllowTypewriterSkip)
                {
                    CompleteTypewriterImmediately();
                }

                return;
            }

            // Lock manual input advancement while waiting on explicit choice selection.
            if (_currentLine.HasChoices)
            {
                return;
            }

            AdvanceToNextLine();
        }

        private void HandleChoiceSelected(int choiceIndex)
        {
            if (!IsActive || _isTyping || !_currentLine.HasChoices)
            {
                return;
            }

            _view.ClearChoices();
            _currentSource.SelectChoice(choiceIndex);
            AdvanceToNextLine();
        }

        private void AdvanceToNextLine()
        {
            if (_currentSource == null || !_currentSource.CanContinue)
            {
                StopDialog();
                return;
            }

            _currentLine = _currentSource.GetNextLine();
            _view.ClearChoices();
            _view.SetSpeaker(_currentLine.Speaker);
            _view.SetFullText(_currentLine.Text);

            if (_settings.SecondsPerCharacter <= 0f || string.IsNullOrEmpty(_currentLine.Text))
            {
                CompleteLinePresentation();
                return;
            }

            CancelTypewriter();
            _typewriterCts = new CancellationTokenSource();
            _ = RunTypewriterAsync(_currentLine.Text.Length, _typewriterCts.Token);
        }

        private async Awaitable RunTypewriterAsync(int totalCharacters, CancellationToken cancellationToken)
        {
            _isTyping = true;
            _view.SetVisibleCharacters(0);

            try
            {
                for (int i = 1; i <= totalCharacters; i++)
                {
                    await _delayAsync(_settings.SecondsPerCharacter, cancellationToken);
                    _view.SetVisibleCharacters(i);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected control flow when skipping typewriter reveal or stopping dialog.
                return;
            }

            CompleteLinePresentation();
        }

        private void CompleteTypewriterImmediately()
        {
            CancelTypewriter();
            CompleteLinePresentation();
        }

        private void CompleteLinePresentation()
        {
            _isTyping = false;
            int totalLength = _currentLine.Text != null ? _currentLine.Text.Length : 0;
            _view.SetVisibleCharacters(totalLength);

            if (_currentLine.HasChoices)
            {
                _view.DisplayChoices(_currentLine.Choices);
            }
        }

        private void CancelTypewriter()
        {
            if (_typewriterCts == null)
            {
                return;
            }

            _typewriterCts.Cancel();
            _typewriterCts.Dispose();
            _typewriterCts = null;
            _isTyping = false;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(DialogService));
            }
        }
    }
}