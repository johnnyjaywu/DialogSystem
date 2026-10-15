// File: DialogSourceTests.cs
using System;
using FluentAssertions;
using NUnit.Framework;

namespace ContentContent.Dialog.Tests
{
    /// <summary>
    /// Isolated state-verification unit tests for DialogSource.
    /// Verifies linear progression, choice-locked branching, cursor resets, and boundary exceptions.
    /// </summary>
    public sealed class DialogSourceTests
    {
        [Test]
        public void Constructor_WithEmptyNodeList_SetsCanContinueToFalse()
        {
            // Arrange & Act
            var source = new DialogSource(Array.Empty<DialogNode>());

            // Assert
            source.CanContinue.Should().BeFalse();
        }

        [Test]
        public void GetNextLine_WithLinearNodes_AdvancesCursorAndTerminatesAtEnd()
        {
            // Arrange
            var nodes = new[]
            {
                new DialogNode("Speaker A", "First line", nextNodeIndex: 1),
                new DialogNode("Speaker B", "Second line", nextNodeIndex: DialogNode.EndConversationIndex)
            };
            var source = new DialogSource(nodes);

            // Act & Assert: Line 1
            source.CanContinue.Should().BeTrue();
            DialogLine firstLine = source.GetNextLine();
            firstLine.Speaker.Should().Be("Speaker A");
            firstLine.Text.Should().Be("First line");

            // Act & Assert: Line 2
            source.CanContinue.Should().BeTrue();
            DialogLine secondLine = source.GetNextLine();
            secondLine.Speaker.Should().Be("Speaker B");
            secondLine.Text.Should().Be("Second line");

            // Assert termination
            source.CanContinue.Should().BeFalse();
        }

        [Test]
        public void GetNextLine_OnNodeWithChoices_LocksProgressionUntilChoiceSelected()
        {
            // Arrange
            var nodes = new[]
            {
                new DialogNode(
                    speaker: "Operator",
                    text: "Choose a branch:",
                    nextNodeIndex: 1,
                    choices: new[]
                    {
                        new DialogChoice("Branch A", targetNodeIndex: 1),
                        new DialogChoice("Branch B", targetNodeIndex: 2)
                    }),
                new DialogNode("Operator", "Outcome A", nextNodeIndex: DialogNode.EndConversationIndex),
                new DialogNode("Operator", "Outcome B", nextNodeIndex: DialogNode.EndConversationIndex)
            };
            var source = new DialogSource(nodes);

            // Act: Read the branching prompt node
            DialogLine promptLine = source.GetNextLine();

            // Assert: CanContinue must lock to false until SelectChoice routes the cursor
            promptLine.HasChoices.Should().BeTrue();
            promptLine.Choices.Should().HaveCount(2);
            source.CanContinue.Should().BeFalse();

            // Act: Select the second choice (index 1 -> targets Node 2)
            source.SelectChoice(1);

            // Assert: Cursor unlocks and points to Outcome B
            source.CanContinue.Should().BeTrue();
            DialogLine branchLine = source.GetNextLine();
            branchLine.Text.Should().Be("Outcome B");
            source.CanContinue.Should().BeFalse();
        }

        [Test]
        public void Reset_AfterCompletingConversation_RestoresCursorToInitialNode()
        {
            // Arrange
            var nodes = new[]
            {
                new DialogNode("Operator", "Single line", nextNodeIndex: DialogNode.EndConversationIndex)
            };
            var source = new DialogSource(nodes);
            source.GetNextLine();
            source.CanContinue.Should().BeFalse();

            // Act
            source.Reset();

            // Assert
            source.CanContinue.Should().BeTrue();
            source.GetNextLine().Text.Should().Be("Single line");
        }

        [Test]
        public void GetNextLine_WhenCanContinueIsFalse_ThrowsInvalidOperationException()
        {
            // Arrange
            var source = new DialogSource(Array.Empty<DialogNode>());

            // Act
            Action act = () => source.GetNextLine();

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void SelectChoice_WithOutOfBoundsIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var nodes = new[]
            {
                new DialogNode(
                    speaker: "Operator",
                    text: "Prompt",
                    nextNodeIndex: DialogNode.EndConversationIndex,
                    choices: new[] { new DialogChoice("Only Option", targetNodeIndex: DialogNode.EndConversationIndex) })
            };
            var source = new DialogSource(nodes);
            source.GetNextLine();

            // Act
            Action act = () => source.SelectChoice(5);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}