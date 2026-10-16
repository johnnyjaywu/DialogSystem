// File: DialogChoice.cs
using System;
using UnityEngine;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Serializable routing entry pairing a localized choice label with its target node index in a dialog graph.
    /// Use DialogNode.EndConversationIndex (-1) as the target index to terminate the conversation upon selection.
    /// </summary>
    [Serializable]
    public struct DialogChoice
    {
        [SerializeField]
        private string _text;

        [SerializeField]
        private int _targetNodeIndex;

        /// <summary>
        /// Text of a dialog choice that the player can select. Cannot be null.
        /// </summary>
        public string Text => _text ?? string.Empty;

        /// <summary>
        /// Index of the dialog node this choice leads to. Use DialogNode.EndConversationIndex (-1) to terminate the conversation upon selection.
        /// </summary>
        public int TargetNodeIndex => _targetNodeIndex;

        /// <summary>
        /// Initializes a new instance of the DialogChoice struct with the specified text and target node index.
        /// </summary>
        /// <param name="text">
        /// The text of the dialog choice. Cannot be null.  
        /// </param>
        /// <param name="targetNodeIndex">
        /// The index of the target dialog node that this choice leads to. Use DialogNode.EndConversationIndex (-1) to terminate the conversation upon selection.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the text parameter is null.
        /// </exception>
        public DialogChoice(string text, int targetNodeIndex)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _targetNodeIndex = targetNodeIndex;
        }
    }
}