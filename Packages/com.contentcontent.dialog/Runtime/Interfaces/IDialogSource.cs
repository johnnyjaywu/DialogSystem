// File: IDialogSource.cs

namespace ContentContent.Dialog
{
    /// <summary>
    /// Abstracts the narrative data stream from the presentation controller.
    /// Implement this interface to adapt external dialog engines (such as Ink) without modifying core logic.
    /// </summary>
    public interface IDialogSource
    {
        bool CanContinue { get; }
        DialogLine GetNextLine();
        void SelectChoice(int choiceIndex);
    }
}