// File: DialogSource.cs
using System;
using System.Collections.Generic;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Concrete, extensible implementation of IDialogSource backed by an in-memory DialogNode graph.
    /// Pre-caches DialogLine structs at construction time so runtime node traversal is allocation-free.
    /// </summary>
    public class DialogSource : IDialogSource
    {
        private readonly IReadOnlyList<DialogNode> _nodes;
        private readonly DialogLine[] _cachedLines;
        private int _currentNodeIndex;
        private int _nextNodeIndex;

        /// <summary>
        /// Indicates whether the dialog source can continue to the next node. Returns true if the next node index is valid (i.e., within the bounds of the node list); otherwise, returns false. This property is used to determine if there are more dialog lines to read in the conversation.
        /// </summary>
        public virtual bool CanContinue => _nextNodeIndex >= 0 && _nextNodeIndex < _nodes.Count;

        /// <summary>
        /// Initializes a new instance of the DialogSource class with the specified list of dialog nodes. The constructor pre-caches DialogLine structs for each node to enable allocation-free traversal during runtime. It also sets the initial state of the dialog source, preparing it for dialog progression. If the provided nodes list is null, an ArgumentNullException is thrown.
        /// </summary>
        /// <param name="nodes">
        /// A read-only list of DialogNode instances representing the dialog graph. Each node contains the speaker, text, next node index, and optional choices for branching conversations. This list is used to construct the dialog source and pre-cache the corresponding DialogLine structs for efficient runtime access.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the nodes parameter is null, indicating that a valid list of dialog nodes must be provided to create a DialogSource instance.
        /// </exception>
        public DialogSource(IReadOnlyList<DialogNode> nodes)
        {
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            _cachedLines = new DialogLine[_nodes.Count];

            for (int i = 0; i < _nodes.Count; i++)
            {
                _cachedLines[i] = _nodes[i].ToDialogLine();
            }

            Reset();
        }

        /// <summary>
        /// Retrieves the next dialog line from the dialog source, advancing the internal cursor to the next node in the conversation. If the current node has choices, progression is locked until a choice is selected using SelectChoice. If there are no valid nodes to continue to, an InvalidOperationException is thrown. This method provides a way to traverse the dialog graph and obtain the corresponding DialogLine payloads for display or processing.
        /// </summary>
        /// <returns>
        /// The next DialogLine in the conversation, representing the speaker, text, and any available choices. If the current node has choices, the next node index will be locked until a choice is selected. If there are no valid nodes to continue to, an InvalidOperationException is thrown.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when there are no valid dialog nodes available at the current cursor, indicating that the conversation has reached its end or that the next node index is out of bounds. This exception signals that the caller should not attempt to retrieve further dialog lines without resetting or selecting a choice to continue the conversation.
        /// </exception>
        public virtual DialogLine GetNextLine()
        {
            if (!CanContinue)
            {
                throw new InvalidOperationException("No valid dialog node available at the current cursor.");
            }

            _currentNodeIndex = _nextNodeIndex;
            DialogNode currentNode = _nodes[_currentNodeIndex];
            DialogLine line = _cachedLines[_currentNodeIndex];

            // Lock progression on choice nodes until SelectChoice resolves the target branch index.
            _nextNodeIndex = line.HasChoices
                ? DialogNode.EndConversationIndex
                : currentNode.NextNodeIndex;

            return line;
        }

        /// <summary>
        /// Selects a choice from the current dialog node, updating the next node index to the target of the selected choice. This method is used to branch the conversation based on player input, allowing for dynamic dialog progression. If the current node does not have any choices or if the choice index is out of bounds, an InvalidOperationException or ArgumentOutOfRangeException is thrown, respectively. This method must be called after GetNextLine when the current node has choices available.
        /// </summary>
        /// <param name="choiceIndex">
        /// The index of the choice to select from the current dialog node's list of choices. This index must be within the bounds of the available choices; otherwise, an ArgumentOutOfRangeException is thrown. The selected choice determines the next node in the conversation, allowing for branching dialog paths based on player decisions.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current node does not have any choices available, indicating that SelectChoice cannot be called at this point in the conversation. This exception signals that the caller should not attempt to select a choice without first retrieving a dialog line that contains choices.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the choiceIndex parameter is out of bounds for the current dialog node's list of choices, indicating that the specified index does not correspond to a valid choice. This exception signals that the caller should provide a valid index within the range of available choices for the current node.
        /// </exception>
        public virtual void SelectChoice(int choiceIndex)
        {
            if (_currentNodeIndex < 0 || _currentNodeIndex >= _nodes.Count)
            {
                throw new InvalidOperationException("Cannot select a choice before a dialog node has been read.");
            }

            IReadOnlyList<DialogChoice> choices = _nodes[_currentNodeIndex].Choices;

            if (choiceIndex < 0 || choiceIndex >= choices.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(choiceIndex), "Choice index is out of bounds for the active node.");
            }

            _nextNodeIndex = choices[choiceIndex].TargetNodeIndex;
        }

        /// <summary>
        /// Resets the dialog source to its initial state, allowing for a new conversation to be started from the beginning of the dialog graph. This method sets the current node index to the end conversation index and initializes the next node index to the first node in the list if available. If there are no nodes in the dialog source, both indices are set to the end conversation index, indicating that there are no valid nodes to traverse. This method can be called at any time to restart the dialog progression from the beginning.
        /// </summary>
        public virtual void Reset()
        {
            _currentNodeIndex = DialogNode.EndConversationIndex;
            _nextNodeIndex = _nodes.Count > 0 ? 0 : DialogNode.EndConversationIndex;
        }
    }
}