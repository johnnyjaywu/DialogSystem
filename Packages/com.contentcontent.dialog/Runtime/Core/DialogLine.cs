// File: DialogLine.cs
using System;
using System.Collections.Generic;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Immutable value payload representing a single dialog turn.
    /// Implemented as a readonly struct to avoid heap allocations during dialog progression.
    /// </summary>
    public readonly struct DialogLine
    {
        public string Speaker { get; }
        public string Text { get; }
        public IReadOnlyList<string> Choices { get; }

        public bool HasChoices => Choices != null && Choices.Count > 0;

        public DialogLine(string speaker, string text, IReadOnlyList<string> choices = null)
        {
            // Sanitize null inputs at construction so downstream views never require defensive null checks.
            Speaker = speaker ?? string.Empty;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Choices = choices ?? Array.Empty<string>();
        }
    }
}