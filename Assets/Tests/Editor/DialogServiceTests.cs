// File: DialogServiceTests.cs
using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace ContentContent.Dialog.Tests
{
    /// <summary>
    /// Isolated interaction and asynchronous unit tests for DialogService using NSubstitute mocks.
    /// Verifies MVP presenter coordination, synchronous choice locking, and async typewriter timing/skip behavior.
    /// </summary>
    public sealed class DialogServiceTests
    {
        private IDialogView _view;
        private IDialogInputProvider _inputProvider;
        private IDialogSource _source;
        private DialogSettings _instantSettings;
        private DialogSettings _asyncSettings;
        private DialogService _sut;

        [SetUp]
        public void SetUp()
        {
            _view = Substitute.For<IDialogView>();
            _inputProvider = Substitute.For<IDialogInputProvider>();
            _source = Substitute.For<IDialogSource>();

            _instantSettings = new DialogSettings(secondsPerCharacter: 0f, allowTypewriterSkip: true);
            _asyncSettings = new DialogSettings(secondsPerCharacter: 0.01f, allowTypewriterSkip: true);
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }

        [Test]
        public void StartDialog_WithValidSource_ShowsViewEnablesInputAndDisplaysFirstLine()
        {
            // Arrange
            _sut = new DialogService(_view, _inputProvider, _instantSettings);
            var expectedLine = new DialogLine("Operator", "System initialized.");
            _source.CanContinue.Returns(true);
            _source.GetNextLine().Returns(expectedLine);

            var startedEventFired = false;
            _sut.DialogStarted += () => startedEventFired = true;

            // Act
            _sut.StartDialog(_source);

            // Assert
            _sut.IsActive.Should().BeTrue();
            startedEventFired.Should().BeTrue();

            _view.Received(1).Show();
            _inputProvider.Received(1).Enable();
            _view.Received(1).SetSpeaker("Operator");
            _view.Received(1).SetFullText("System initialized.");
            _view.Received(1).SetVisibleCharacters(expectedLine.Text.Length);
        }

        [Test]
        public void AdvanceRequested_WhenSourceCanContinue_AdvancesToSecondLine()
        {
            // Arrange
            _sut = new DialogService(_view, _inputProvider, _instantSettings);
            var firstLine = new DialogLine("Operator", "Line 1");
            var secondLine = new DialogLine("Operator", "Line 2");

            _source.CanContinue.Returns(true);
            _source.GetNextLine().Returns(firstLine, secondLine);

            _sut.StartDialog(_source);
            _view.ClearReceivedCalls();

            // Act: Simulate player advancing input
            _inputProvider.AdvanceRequested += Raise.Event<Action>();

            // Assert
            _sut.IsActive.Should().BeTrue();
            _view.Received(1).SetSpeaker("Operator");
            _view.Received(1).SetFullText("Line 2");
            _view.Received(1).SetVisibleCharacters(secondLine.Text.Length);
        }

        [Test]
        public void AdvanceRequested_WhenSourceCannotContinue_StopsDialogAndCleansUpView()
        {
            // Arrange
            _sut = new DialogService(_view, _inputProvider, _instantSettings);
            var onlyLine = new DialogLine("Operator", "Final line");

            _source.CanContinue.Returns(true, false);
            _source.GetNextLine().Returns(onlyLine);

            var endedEventFired = false;
            _sut.DialogEnded += () => endedEventFired = true;

            _sut.StartDialog(_source);
            _view.ClearReceivedCalls();
            _inputProvider.ClearReceivedCalls();

            // Act
            _inputProvider.AdvanceRequested += Raise.Event<Action>();

            // Assert
            _sut.IsActive.Should().BeFalse();
            endedEventFired.Should().BeTrue();

            _inputProvider.Received(1).Disable();
            _view.Received(1).ClearChoices();
            _view.Received(1).Hide();
        }

        [Test]
        public void AdvanceRequested_WhenCurrentLineHasChoices_LocksAdvancementUntilChoiceSelected()
        {
            // Arrange
            _sut = new DialogService(_view, _inputProvider, _instantSettings);
            var choices = new[] { "Accept", "Decline" };
            var branchingLine = new DialogLine("Operator", "Make a choice:", choices);

            _source.CanContinue.Returns(true);
            _source.GetNextLine().Returns(branchingLine);

            _sut.StartDialog(_source);
            _view.Received(1).DisplayChoices(choices);

            _source.ClearReceivedCalls();

            // Act: Attempt manual advance while choices are displayed
            _inputProvider.AdvanceRequested += Raise.Event<Action>();

            // Assert
            _source.DidNotReceive().GetNextLine();
            _sut.IsActive.Should().BeTrue();
        }

        [Test]
        public void ChoiceSelected_WhenLineHasChoices_NotifiesSourceAndAdvancesToTargetBranch()
        {
            // Arrange
            _sut = new DialogService(_view, _inputProvider, _instantSettings);
            var choices = new[] { "Option 0", "Option 1" };
            var promptLine = new DialogLine("Operator", "Choose:", choices);
            var branchLine = new DialogLine("Operator", "Selected Option 1.");

            _source.CanContinue.Returns(true);
            _source.GetNextLine().Returns(promptLine, branchLine);

            _sut.StartDialog(_source);
            _view.ClearReceivedCalls();

            // Act: Simulate player selecting choice index 1
            _view.ChoiceSelected += Raise.Event<Action<int>>(1);

            // Assert
            _view.Received().ClearChoices();
            _source.Received(1).SelectChoice(1);
            _view.Received(1).SetFullText("Selected Option 1.");
        }

        [Test]
        public async Task StartDialog_WithAsyncTypewriter_ProgressivelyUpdatesVisibleCharacters()
        {
            // Arrange
            _sut = new DialogService(_view, _inputProvider, _asyncSettings);
            var line = new DialogLine("Operator", "ABC");
            _source.CanContinue.Returns(true);
            _source.GetNextLine().Returns(line);

            // Act
            _sut.StartDialog(_source);

            // Initial frame sets visible characters to 0
            _view.Received(1).SetVisibleCharacters(0);

            // Allow time for typewriter coroutine to process characters
            await Task.Delay(50);

            // Assert: View received intermediate or full character reveal updates
            _view.Received().SetVisibleCharacters(Arg.Is<int>(count => count > 0 && count <= 3));
        }

        [Test]
        public async Task AdvanceRequested_WhileTyping_CompletesTypewriterImmediately()
        {
            // Arrange
            _sut = new DialogService(_view, _inputProvider, _asyncSettings);
            var line = new DialogLine("Operator", "Long typewriter string for testing skip behavior.");
            _source.CanContinue.Returns(true);
            _source.GetNextLine().Returns(line);

            _sut.StartDialog(_source);
            _view.ClearReceivedCalls();

            // Act: Press advance input immediately while typing is active
            _inputProvider.AdvanceRequested += Raise.Event<Action>();

            // Allow short yield for async task cancellation processing
            await Task.Yield();

            // Assert: Typewriter skipped straight to full text length
            _view.Received(1).SetVisibleCharacters(line.Text.Length);
        }

        [Test]
        public void Dispose_WhileDialogIsActive_StopsDialogAndUnsubscribesEvents()
        {
            // Arrange
            _sut = new DialogService(_view, _inputProvider, _instantSettings);
            _source.CanContinue.Returns(true);
            _source.GetNextLine().Returns(new DialogLine("Operator", "Active text"));
            _sut.StartDialog(_source);

            // Act
            _sut.Dispose();
            _view.ClearReceivedCalls();

            _inputProvider.AdvanceRequested += Raise.Event<Action>();

            // Assert
            _sut.IsActive.Should().BeFalse();
            _view.DidNotReceiveWithAnyArgs().SetFullText(default);
        }
    }
}