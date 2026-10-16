# Unit Testing Guide

This guide explains how the automated tests for `com.contentcontent.dialog` work, the tools involved, and how to read or write new tests—even if you are new to automated software testing.

---

## 1. What is Unit Testing?

There are two ways to test features in Unity:

1. **Manual Testing (Play Mode):** You open a scene, press Play, and press keys to see if the dialog box works on screen. While useful for visual checks, manual testing takes time and requires you to repeat every click whenever you change code.
2. **Unit Testing (Edit Mode):** Small scripts test one specific class at a time—such as `DialogSource` or `DialogService`—directly in the Unity Editor without launching Play Mode or loading a scene. All tests run in a fraction of a second and immediately report which behavior passed or failed.

Because the core classes in this package (`DialogService` and `DialogSource`) are plain C# classes rather than `MonoBehaviour` components, they can be tested entirely in Edit Mode.

---

## 2. Testing Tools Used

Our test suite uses three libraries that work together inside Unity's **Test Runner** window (**Window > General > Test Runner**):

- **NUnit:** Unity's built-in test framework. It identifies methods marked with the `[Test]` attribute and runs them when you click **Run All**.
- **NSubstitute:** A mocking library. `DialogService` requires an `IDialogView` (to show text) and an `IDialogInputProvider` (to detect button presses). During an Edit Mode test, there is no UI screen or player controller active. NSubstitute creates lightweight substitute objects ("mocks") that stand in for the real UI and input classes, recording every method `DialogService` calls on them.
- **FluentAssertions:** A library that makes test checks easier to read. Instead of writing `Assert.AreEqual(false, source.CanContinue)`, you can write `source.CanContinue.Should().BeFalse()`.

---

## 3. Anatomy of a Unit Test: Arrange, Act, Assert

Every test in the project follows a three-step pattern marked with comments:

### Step 1: Arrange (Setup)
Prepare the objects and data needed for the test.
- When testing `DialogSource`, we create a small array of `DialogNode` items.
- When testing `DialogService`, we configure our substitute `_source` to return specific `DialogLine` values when called.

### Step 2: Act (Execute)
Trigger the single method or event being tested.
- Example: `_sut.StartDialog(_source);`
- Example: `_inputProvider.AdvanceRequested += Raise.Event<Action>();` (simulates the player pressing the advance button).
*(Note: `_sut` stands for **System Under Test**, which refers to the main object being tested).*

### Step 3: Assert (Verify)
Check that the action produced the expected outcome. There are two types of checks:

- **State Checks (`DialogSourceTests`):** Check a property value on the object directly.
  ```csharp
  source.CanContinue.Should().BeFalse();
  ```

- **Interaction Checks (`DialogServiceTests`):** Check that `DialogService` called the expected method on a mock interface.
  ```csharp
  _view.Received(1).Show();
  _view.Received(1).SetFullText("Line 2");
  ```

---

## 4. Test Lifecycle: `[SetUp]` and `[TearDown]`

In `DialogServiceTests.cs`, two helper methods run automatically around every test:

- **`[SetUp]`:** Runs immediately before each `[Test]` method. It creates fresh mock objects (`_view`, `_inputProvider`, `_source`) so changes from one test never carry over into the next test.
- **`[TearDown]`:** Runs immediately after each `[Test]` method. It calls `_sut.Dispose()` to cancel any active timers and clean up event subscriptions.

---

## 5. Testing Timed Features (The Typewriter Effect)

Most tests set `secondsPerCharacter: 0f` so text completes immediately. However, we also need to test the character-by-character typewriter delay.

In a live game, `DialogService` uses `Awaitable.WaitForSecondsAsync`, which relies on Unity's Play Mode frame clock. Because the Play Mode clock does not tick during Edit Mode tests, waiting on `WaitForSecondsAsync` in Edit Mode would pause the test forever.

To make timed tests work in Edit Mode, `DialogService` accepts an optional delay function in its constructor:
- **In Play Mode:** It defaults to Unity's `Awaitable.WaitForSecondsAsync`.
- **In Edit Mode Tests:** We pass in `EditModeDelayAsync`, which uses the standard system clock (`Task.Delay`). This allows the test to wait a few milliseconds and verify that `SetVisibleCharacters(1)`, `SetVisibleCharacters(2)`, and `SetVisibleCharacters(3)` are called in sequence.

---

## 6. Summary of Included Tests

### `DialogSourceTests.cs` (Conversation Data & Branching)
- **`Constructor_WithEmptyNodeList_SetsCanContinueToFalse`**: An empty node list reports that it has no lines to read.
- **`GetNextLine_WithLinearNodes_AdvancesCursorAndTerminatesAtEnd`**: Reading lines in sequence moves through the list and stops after the last node.
- **`GetNextLine_OnNodeWithChoices_LocksProgressionUntilChoiceSelected`**: Nodes with choices set `CanContinue` to `false` until `SelectChoice(index)` is called, then jump to the chosen target node.
- **`Reset_AfterCompletingConversation_RestoresCursorToInitialNode`**: Calling `Reset()` moves the cursor back to the first node.
- **`GetNextLine_WhenCanContinueIsFalse_ThrowsInvalidOperationException`**: Calling `GetNextLine()` after the conversation has ended throws an error.
- **`SelectChoice_WithOutOfBoundsIndex_ThrowsArgumentOutOfRangeException`**: Passing an invalid choice index throws an error.

### `DialogServiceTests.cs` (Conversation Flow & UI Coordination)
- **`StartDialog_WithValidSource_ShowsViewEnablesInputAndDisplaysFirstLine`**: Starting a conversation opens the UI view, enables player input, and displays the first line.
- **`AdvanceRequested_WhenSourceCanContinue_AdvancesToSecondLine`**: Triggering the advance input moves to the next line and updates the UI.
- **`AdvanceRequested_WhenSourceCannotContinue_StopsDialogAndCleansUpView`**: Triggering the advance input after the final line disables input, clears choices, and hides the UI.
- **`AdvanceRequested_WhenCurrentLineHasChoices_LocksAdvancementUntilChoiceSelected`**: Pressing the advance button while choices are on screen is ignored until a choice is selected.
- **`ChoiceSelected_WhenLineHasChoices_NotifiesSourceAndAdvancesToTargetBranch`**: Selecting a choice clears the choice buttons, passes the selected index to the source, and displays the next line.
- **`StartDialog_WithAsyncTypewriter_ProgressivelyUpdatesVisibleCharacters`**: When typing speed is greater than zero, characters are revealed one by one over time.
- **`AdvanceRequested_WhileTyping_CompletesTypewriterImmediately`**: Pressing the advance button while text is still typing reveals the full sentence immediately.
- **`Dispose_WhileDialogIsActive_StopsDialogAndUnsubscribesEvents`**: Disposing of the service stops the active conversation and ignores any future input events.