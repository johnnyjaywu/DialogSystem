// File: DialogAsset.cs
using System.Collections.Generic;
using UnityEngine;

namespace ContentContent.Dialog
{
    /// <summary>
    /// ScriptableObject authoring container for node-based conversations.
    /// Separates immutable Inspector-serialized node data from mutable runtime DialogSource cursors.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogAsset", menuName = "ContentContent/Dialog/Dialog Asset")]
    public class DialogAsset : ScriptableObject
    {
        [SerializeField]
        private DialogNode[] _nodes = System.Array.Empty<DialogNode>();

        public IReadOnlyList<DialogNode> Nodes => _nodes;

        /// <summary>
        /// Factory method that creates a fresh runtime IDialogSource state machine over this asset's nodes.
        /// Override in derived classes (e.g., InkDialogAsset) to return custom IDialogSource adapters.
        /// </summary>
        public virtual IDialogSource CreateSource()
        {
            return new DialogSource(_nodes);
        }

        [ContextMenu("Populate Sample Conversation")]
        private void PopulateSampleConversation()
        {
            _nodes = new DialogNode[]
            {
                // Node 0: Intro line (advances to Node 1)
                new DialogNode(
                    speaker: "Operator",
                    text: "Link established. Press your Advance input while this text is typing to test immediate typewriter skip.",
                    nextNodeIndex: 1),

                // Node 1: Narration line with empty speaker (advances to Node 2)
                new DialogNode(
                    speaker: string.Empty,
                    text: "[The terminal hums quietly as telemetry packets synchronize across the local bus.]",
                    nextNodeIndex: 2),

                // Node 2: Branching hub (routes to Node 3, Node 4, or stops immediately via EndConversationIndex)
                new DialogNode(
                    speaker: "Operator",
                    text: "Select a transmission protocol to test internal graph branching or immediate termination:",
                    nextNodeIndex: DialogNode.EndConversationIndex,
                    choices: new[]
                    {
                        new DialogChoice("Route via Encrypted Tunnel", targetNodeIndex: 3),
                        new DialogChoice("Broadcast on Open Frequency", targetNodeIndex: 4),
                        new DialogChoice("Terminate Connection Immediately (StopDialog)", targetNodeIndex: DialogNode.EndConversationIndex)
                    }),

                // Node 3: Encrypted Branch -> converges to Node 5
                new DialogNode(
                    speaker: "Operator",
                    text: "Encrypted tunnel engaged. Zero packet leakage detected.",
                    nextNodeIndex: 5),

                // Node 4: Broadcast Branch -> converges to Node 5
                new DialogNode(
                    speaker: "Operator",
                    text: "Broadcasting on open frequency. All nearby nodes have been alerted.",
                    nextNodeIndex: 5),

                // Node 5: Shared closing line -> terminates conversation
                new DialogNode(
                    speaker: "Operator",
                    text: "Demonstration complete. Closing transmission channel.",
                    nextNodeIndex: DialogNode.EndConversationIndex)
            };

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}