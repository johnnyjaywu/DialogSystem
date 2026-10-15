// File: IDialogView.cs

using System;
using System.Collections.Generic;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Presentation contract for rendering dialog state.
    /// Keeps the domain service decoupled from UI Toolkit types to allow isolated unit testing via NSubstitute.
    /// </summary>
    public interface IDialogView
    {
        event Action<int> ChoiceSelected;

        void Show();
        void Hide();
        void SetSpeaker(string speakerName);

        /// <summary>
        /// Assigns the full target string upfront so the view can pre-calculate text layout and word wrapping.
        /// </summary>
        void SetFullText(string text);

        /// <summary>
        /// Reveals a specific count of characters from the pre-assigned full text without reflowing layout.
        /// </summary>
        void SetVisibleCharacters(int characterCount);

        void DisplayChoices(IReadOnlyList<string> choices);
        void ClearChoices();
    }
}