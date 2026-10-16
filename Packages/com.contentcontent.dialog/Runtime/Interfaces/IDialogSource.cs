// File: IDialogSource.cs

namespace ContentContent.Dialog
{
    /// <summary>
    /// Abstracts the narrative data stream from the presentation controller.
    /// Implement this interface to adapt external dialog engines (such as Ink) without modifying core logic.
    /// </summary>
    public interface IDialogSource
    {
        /// <summary>
        /// Indicates whether the dialog source has more lines to present. If false, the dialog session will end after the current line is finished.
        /// </summary>
        bool CanContinue { get; }

        /// <summary>
        /// Returns the next line of dialog from the source, or null if there are no more lines to present. If the source has choices available, this method will return a line with the choices populated. If the source has no more lines and no choices, this method will return null.
        /// </summary>
        /// <returns></returns>
        DialogLine GetNextLine();

        /// <summary>
        /// Indicates whether the dialog source has choices available for the player to select. If true, the dialog session will present the choices to the player and wait for a selection before continuing.
        /// </summary>
        /// <param name="choiceIndex"></param>
        void SelectChoice(int choiceIndex);
    }
}