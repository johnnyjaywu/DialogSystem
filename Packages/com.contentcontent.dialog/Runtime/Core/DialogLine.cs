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
        /// <summary>
        /// Name of the speaker for this dialog line. Can be null or empty if the speaker is unknown or not relevant.
        /// </summary>
        public string Speaker { get; }

        /// <summary>
        /// Text of the dialog line. Cannot be null.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// List of localized choice labels that the player can select in response to this dialog line. Can be null or empty if there are no choices available.
        /// </summary>
        public IReadOnlyList<string> Choices { get; }

        /// <summary>
        /// Indicates whether this dialog line has any choices available for the player to select. Returns true if Choices is not null and contains at least one element; otherwise, returns false.
        /// </summary>
        public bool HasChoices => Choices != null && Choices.Count > 0;

        /// <summary>
        /// Initializes a new instance of the DialogLine struct with the specified speaker, text, and optional choices.
        /// </summary>
        /// <param name="speaker">
        /// Name of the speaker for this dialog line. Can be null or empty if the speaker is unknown or not relevant.
        /// </param>
        /// <param name="text">
        /// Text of the dialog line. Cannot be null.
        /// </param>
        /// <param name="choices">
        /// List of localized choice labels that the player can select in response to this dialog line. Can be null or empty if there are no choices available.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the text parameter is null.
        /// </exception>
        public DialogLine(string speaker, string text, IReadOnlyList<string> choices = null)
        {
            // Sanitize null inputs at construction so downstream views never require defensive null checks.
            Speaker = speaker ?? string.Empty;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Choices = choices ?? Array.Empty<string>();
        }
    }
}