// File: DialogViewUIToolkit.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentContent.Dialog
{
    /// <summary>
    /// UI Toolkit Passive View implementation of IDialogView.
    /// Consumes DialogUISettings to bind VisualElements and uses rich-text alpha tags to prevent word-wrap jitter.
    /// </summary>
    public sealed class DialogViewUIToolkit : IDialogView
    {
        private const string HiddenAlphaTag = "<alpha=#00>";

        private readonly string _containerElementName;
        private readonly string _speakerLabelName;
        private readonly string _bodyLabelName;
        private readonly string _choicesContainerName;
        private readonly string _choiceButtonClassName;
        private readonly List<Button> _activeChoiceButtons = new List<Button>();

        private VisualElement _dialogContainer;
        private Label _speakerLabel;
        private Label _bodyLabel;
        private VisualElement _choicesContainer;

        private bool _isVisible;
        private string _currentSpeaker = string.Empty;
        private string _fullText = string.Empty;
        private int _visibleCharacters;
        private IReadOnlyList<string> _currentChoices;

        /// <summary>
        /// Occurs when a choice button is clicked, passing the index of the selected choice as an argument.
        /// </summary>
        public event Action<int> ChoiceSelected;

        /// <summary>
        /// Initializes a new instance of the DialogViewUIToolkit class with the specified DialogUISettings.
        /// </summary>
        /// <param name="uiSettings">
        /// The DialogUISettings object containing the names of the UI Toolkit elements to bind.
        /// </param>
        public DialogViewUIToolkit(DialogUISettings uiSettings)
        {
            _containerElementName = string.IsNullOrWhiteSpace(uiSettings.ContainerElementName)
                ? DialogUISettings.DefaultContainerElementName
                : uiSettings.ContainerElementName;

            _speakerLabelName = string.IsNullOrWhiteSpace(uiSettings.SpeakerLabelName)
                ? DialogUISettings.DefaultSpeakerLabelName
                : uiSettings.SpeakerLabelName;

            _bodyLabelName = string.IsNullOrWhiteSpace(uiSettings.BodyLabelName)
                ? DialogUISettings.DefaultBodyLabelName
                : uiSettings.BodyLabelName;

            _choicesContainerName = string.IsNullOrWhiteSpace(uiSettings.ChoicesContainerName)
                ? DialogUISettings.DefaultChoicesContainerName
                : uiSettings.ChoicesContainerName;

            _choiceButtonClassName = string.IsNullOrWhiteSpace(uiSettings.ChoiceButtonClassName)
                ? DialogUISettings.DefaultChoiceButtonClassName
                : uiSettings.ChoiceButtonClassName;
        }

        /// <summary>
        /// Initializes a new instance of the DialogViewUIToolkit class with the specified root VisualElement and DialogUISettings.
        /// </summary>
        /// <param name="rootElement">
        /// The root VisualElement of the UI Toolkit hierarchy to bind the dialog view elements from.
        /// </param>
        /// <param name="uiSettings">
        /// The DialogUISettings object containing the names of the UI Toolkit elements to bind.
        /// </param>
        public DialogViewUIToolkit(VisualElement rootElement, DialogUISettings uiSettings)
            : this(uiSettings)
        {
            BindRoot(rootElement);
        }

        /// <summary>
        /// Queries and binds the required UI Toolkit elements from the PanelRenderer root visual element.
        /// Scrubs UXML template placeholder text and restores active presentation state.
        /// </summary>
        public void BindRoot(VisualElement rootElement)
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException(nameof(rootElement));
            }

            _activeChoiceButtons.Clear();

            _dialogContainer = rootElement.Q<VisualElement>(_containerElementName)
                ?? throw new InvalidOperationException($"VisualElement '{_containerElementName}' not found in PanelRenderer root.");

            _speakerLabel = _dialogContainer.Q<Label>(_speakerLabelName)
                ?? throw new InvalidOperationException($"Label '{_speakerLabelName}' not found in '{_containerElementName}'.");

            _bodyLabel = _dialogContainer.Q<Label>(_bodyLabelName)
                ?? throw new InvalidOperationException($"Label '{_bodyLabelName}' not found in '{_containerElementName}'.");

            _choicesContainer = _dialogContainer.Q<VisualElement>(_choicesContainerName)
                ?? throw new InvalidOperationException($"VisualElement '{_choicesContainerName}' not found in '{_containerElementName}'.");

            _bodyLabel.enableRichText = true;

            // Unconditionally scrub any stale or UXML design-time placeholder text before applying state.
            _speakerLabel.text = string.Empty;
            _bodyLabel.text = string.Empty;
            _choicesContainer.Clear();

            RestoreStateToBoundElements();
        }

        /// <summary>
        /// Shows the dialog view by setting its visibility to true and displaying the dialog container.
        /// </summary>
        public void Show()
        {
            _isVisible = true;

            if (_dialogContainer != null)
            {
                _dialogContainer.style.display = DisplayStyle.Flex;
            }
        }

        /// <summary>
        /// Hides the dialog view by setting its visibility to false, clearing the speaker and body text, and hiding the dialog container.
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _currentSpeaker = string.Empty;
            _fullText = string.Empty;
            _visibleCharacters = 0;

            ClearChoices();

            if (_speakerLabel != null)
            {
                _speakerLabel.text = string.Empty;
                _speakerLabel.style.display = DisplayStyle.None;
            }

            if (_bodyLabel != null)
            {
                _bodyLabel.text = string.Empty;
            }

            if (_dialogContainer != null)
            {
                _dialogContainer.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Sets the speaker name to be displayed in the dialog view. If the speaker name is null or whitespace, the speaker label will be hidden.
        /// </summary>
        /// <param name="speakerName"></param>
        public void SetSpeaker(string speakerName)
        {
            _currentSpeaker = speakerName ?? string.Empty;

            if (_speakerLabel == null)
            {
                return;
            }

            bool hasSpeaker = !string.IsNullOrWhiteSpace(_currentSpeaker);
            _speakerLabel.text = hasSpeaker ? _currentSpeaker : string.Empty;
            _speakerLabel.style.display = hasSpeaker ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Sets the full text of the dialog body. If the provided text is null, it will be treated as an empty string. The visible character count will be reset to zero, and the body label will be cleared.
        /// </summary>
        /// <param name="text"></param>
        public void SetFullText(string text)
        {
            _fullText = text ?? string.Empty;
            _visibleCharacters = 0;

            if (_bodyLabel != null)
            {
                _bodyLabel.text = string.Empty;
            }
        }

        /// <summary>
        /// Sets the number of visible characters in the dialog body. If the provided character count is less than or equal to zero, the body label will be cleared. If the character count exceeds the length of the full text, the entire text will be displayed. Otherwise, the body label will display the specified number of characters, with unrevealed characters rendered as fully transparent using rich-text alpha tags.
        /// </summary>
        /// <param name="characterCount"></param>
        public void SetVisibleCharacters(int characterCount)
        {
            _visibleCharacters = characterCount;

            if (_bodyLabel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_fullText))
            {
                _bodyLabel.text = string.Empty;
                return;
            }

            int clampedCount = Mathf.Clamp(_visibleCharacters, 0, _fullText.Length);

            if (clampedCount >= _fullText.Length)
            {
                _bodyLabel.text = _fullText;
                return;
            }

            if (clampedCount <= 0)
            {
                _bodyLabel.text = string.Concat(HiddenAlphaTag, _fullText);
                return;
            }

            // Inject the alpha tag at the current character index so UI Toolkit computes word wrap on the full line
            // while culling unrevealed characters as fully transparent.
            _bodyLabel.text = _fullText.Insert(clampedCount, HiddenAlphaTag);
        }

        /// <summary>
        /// Displays a list of choices in the dialog view. Each choice is represented as a button. If the provided list of choices is null or empty, the choices container will be hidden. Otherwise, the choices container will be displayed, and buttons for each choice will be created and added to the container. Clicking a choice button will invoke the ChoiceSelected event with the index of the selected choice.
        /// </summary>
        /// <param name="choices"></param>
        public void DisplayChoices(IReadOnlyList<string> choices)
        {
            ClearChoices();
            _currentChoices = choices;

            if (_choicesContainer == null)
            {
                return;
            }

            if (choices == null || choices.Count == 0)
            {
                _choicesContainer.style.display = DisplayStyle.None;
                return;
            }

            _choicesContainer.style.display = DisplayStyle.Flex;

            for (int i = 0; i < choices.Count; i++)
            {
                int capturedIndex = i;
                var choiceButton = new Button(() => HandleChoiceButtonClicked(capturedIndex))
                {
                    text = choices[i]
                };

                choiceButton.AddToClassList(_choiceButtonClassName);
                _choicesContainer.Add(choiceButton);
                _activeChoiceButtons.Add(choiceButton);
            }
        }


        /// <summary>
        /// Clears all currently displayed choices from the dialog view. This method removes all choice buttons from the choices container, clears the list of active choice buttons, and hides the choices container. It also sets the current choices to null.
        /// </summary>
        public void ClearChoices()
        {
            _currentChoices = null;

            for (int i = 0; i < _activeChoiceButtons.Count; i++)
            {
                Button button = _activeChoiceButtons[i];
                if (button != null)
                {
                    button.Blur();
                    button.RemoveFromHierarchy();
                }
            }

            _activeChoiceButtons.Clear();

            if (_choicesContainer != null)
            {
                _choicesContainer.Clear();
                _choicesContainer.style.display = DisplayStyle.None;
            }
        }

        private void RestoreStateToBoundElements()
        {
            if (_isVisible)
            {
                Show();
                SetSpeaker(_currentSpeaker);
                SetVisibleCharacters(_visibleCharacters);
                DisplayChoices(_currentChoices);
            }
            else
            {
                Hide();
            }
        }

        private void HandleChoiceButtonClicked(int choiceIndex)
        {
            ChoiceSelected?.Invoke(choiceIndex);
        }
    }
}