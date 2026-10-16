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

        public event Action DialogStarted;
        public event Action DialogEnded;

        public bool IsActive { get; private set; }

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