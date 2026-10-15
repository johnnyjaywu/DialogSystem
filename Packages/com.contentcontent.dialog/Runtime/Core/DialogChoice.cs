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

        public string Text => _text ?? string.Empty;
        public int TargetNodeIndex => _targetNodeIndex;

        public DialogChoice(string text, int targetNodeIndex)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _targetNodeIndex = targetNodeIndex;
        }
    }
}