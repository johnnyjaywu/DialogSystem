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
        /// <summary>
        /// Special index value indicating that the conversation should terminate when this node is reached. Use this value for the NextNodeIndex of a node to end the conversation, or for the TargetNodeIndex of a DialogChoice to end the conversation when that choice is selected.
        /// </summary>
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

        /// <summary>
        /// Name of the speaker for this dialog node. Can be null or empty if the speaker is unknown or not relevant.
        /// </summary>
        public string Speaker => _speaker ?? string.Empty;

        /// <summary>
        /// Text of the dialog node. Cannot be null.
        /// </summary>
        public string Text => _text ?? string.Empty;

        /// <summary>
        /// Index of the next dialog node to transition to after this node. Use DialogNode.EndConversationIndex (-1) to terminate the conversation after this node.
        /// </summary>
        public int NextNodeIndex => _nextNodeIndex;

        /// <summary>
        /// List of dialog choices that the player can select in response to this dialog node. Can be null or empty if there are no choices available.
        /// </summary>
        public IReadOnlyList<DialogChoice> Choices => _choices ?? Array.Empty<DialogChoice>();

        /// <summary>
        /// Initializes a new instance of the DialogNode struct with the specified speaker, text, next node index, and optional choices.
        /// </summary>
        /// <param name="speaker">
        /// Name of the speaker for this dialog node. Can be null or empty if the speaker is unknown or not relevant.
        /// </param>
        /// <param name="text">
        /// Text of the dialog node. Cannot be null.
        /// </param>
        /// <param name="nextNodeIndex">
        /// Index of the next dialog node to transition to after this node. Use DialogNode.EndConversationIndex (-1) to terminate the conversation after this node.
        /// </param>
        /// <param name="choices">
        /// List of dialog choices that the player can select in response to this dialog node. Can be null or empty if there are no choices available.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the text parameter is null.
        /// </exception>
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