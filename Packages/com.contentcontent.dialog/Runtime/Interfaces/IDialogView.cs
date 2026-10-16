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
        /// <summary>
        /// Raised when the player selects a choice from the presented options. The integer parameter represents the index of the selected choice.
        /// </summary>
        event Action<int> ChoiceSelected;

        /// <summary>
        /// Shows the dialog view, making it visible to the player. This method can be used to initiate the display of dialog content, such as when a new dialog session starts or when transitioning from a non-dialog state.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the dialog view, making it invisible to the player. This method can be used to remove dialog content from the screen, such as when a dialog session ends or when transitioning to a non-dialog state.
        /// </summary>
        void Hide();

        /// <summary>
        /// Sets the name of the current speaker in the dialog view. This method can be used to update the UI with the name of the character or entity currently speaking, providing context for the dialog content being displayed.
        /// </summary>
        /// <param name="speakerName"></param>
        void SetSpeaker(string speakerName);

        /// <summary>
        /// Assigns the full target string upfront so the view can pre-calculate text layout and word wrapping.
        /// </summary>
        void SetFullText(string text);

        /// <summary>
        /// Reveals a specific count of characters from the pre-assigned full text without reflowing layout.
        /// </summary>
        void SetVisibleCharacters(int characterCount);

        /// <summary>
        /// Displays a list of choices to the player in the dialog view. This method can be used to present multiple options for the player to select from, allowing for branching dialog paths or decision-making within the narrative. The choices parameter is a read-only list of strings representing the available options.
        /// </summary>
        /// <param name="choices"></param>
        void DisplayChoices(IReadOnlyList<string> choices);

        /// <summary>
        /// Clears any currently displayed choices from the dialog view. This method can be used to remove previously presented options, typically after the player has made a selection or when transitioning to a different part of the dialog where choices are no longer relevant.
        /// </summary>
        void ClearChoices();
    }
}