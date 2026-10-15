// File: DialogNode.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Serializable graph node for authoring branching conversations inside a ScriptableObject.
    /// Converts into an immutable DialogLine payload on demand.
    /// </summary>
    [Serializable]
    public struct DialogNode
    {
        public const int EndConversationIndex = -1;

        [SerializeField]
        private string _speaker;

        [SerializeField]
        [TextArea(2, 5)]
        private string _text;

        [SerializeField]
        private int _nextNodeIndex;

        [SerializeField]
        private DialogChoice[] _choices;

        public string Speaker => _speaker ?? string.Empty;
        public string Text => _text ?? string.Empty;
        public int NextNodeIndex => _nextNodeIndex;
        public IReadOnlyList<DialogChoice> Choices => _choices ?? Array.Empty<DialogChoice>();

        public DialogNode(
            string speaker,
            string text,
            int nextNodeIndex = EndConversationIndex,
            DialogChoice[] choices = null)
        {
            _speaker = speaker ?? string.Empty;
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _nextNodeIndex = nextNodeIndex;
            _choices = choices ?? Array.Empty<DialogChoice>();
        }

        /// <summary>
        /// Constructs the runtime DialogLine value payload from the serialized node data.
        /// </summary>
        public DialogLine ToDialogLine()
        {
            if (_choices == null || _choices.Length == 0)
            {
                return new DialogLine(Speaker, Text);
            }

            var choiceLabels = new string[_choices.Length];
            for (int i = 0; i < _choices.Length; i++)
            {
                choiceLabels[i] = _choices[i].Text;
            }

            return new DialogLine(Speaker, Text, choiceLabels);
        }
    }
}