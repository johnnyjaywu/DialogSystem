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

        public virtual bool CanContinue => _nextNodeIndex >= 0 && _nextNodeIndex < _nodes.Count;

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

        public virtual void Reset()
        {
            _currentNodeIndex = DialogNode.EndConversationIndex;
            _nextNodeIndex = _nodes.Count > 0 ? 0 : DialogNode.EndConversationIndex;
        }
    }
}