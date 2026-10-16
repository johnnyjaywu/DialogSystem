// File: DialogUISettings.cs
using System;
using UnityEngine;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Configuration payload holding UI Toolkit VisualElement query names and USS class identifiers.
    /// Consumed exclusively by DialogViewUIToolkit and DialogBehavior to prevent polluting the core domain.
    /// </summary>
    [Serializable]
    public struct DialogUISettings
    {
        /// <summary>
        /// Default query name for the root container VisualElement that holds all dialog UI elements. This name is used to locate the container in the UI hierarchy when binding the dialog view to the root VisualElement. If no custom name is provided, this default value will be used to ensure consistent identification of the dialog container across different UI layouts and implementations.
        /// </summary>
        public const string DefaultContainerElementName = "dialog-container";

        /// <summary>
        /// Default query name for the VisualElement that displays the speaker's name in the dialog UI. This name is used to locate the speaker label in the UI hierarchy when binding the dialog view to the root VisualElement. If no custom name is provided, this default value will be used to ensure consistent identification of the speaker label across different UI layouts and implementations.
        /// </summary>
        public const string DefaultSpeakerLabelName = "speaker-label";

        /// <summary>
        /// Default query name for the VisualElement that displays the body text of the dialog in the UI. This name is used to locate the body label in the UI hierarchy when binding the dialog view to the root VisualElement. If no custom name is provided, this default value will be used to ensure consistent identification of the body label across different UI layouts and implementations.
        /// </summary>
        public const string DefaultBodyLabelName = "body-label";

        /// <summary>
        /// Default query name for the VisualElement that contains the dialog choice buttons in the UI. This name is used to locate the choices container in the UI hierarchy when binding the dialog view to the root VisualElement. If no custom name is provided, this default value will be used to ensure consistent identification of the choices container across different UI layouts and implementations.
        /// </summary>
        public const string DefaultChoicesContainerName = "choices-container";

        /// <summary>
        /// Default USS class name for the VisualElements that represent individual dialog choice buttons in the UI. This class name is used to style and identify the choice buttons when binding the dialog view to the root VisualElement. If no custom class name is provided, this default value will be used to ensure consistent styling and identification of the choice buttons across different UI layouts and implementations.
        /// </summary>
        public const string DefaultChoiceButtonClassName = "dialog-choice-button";

        [SerializeField]
        private string _containerElementName;

        [SerializeField]
        private string _speakerLabelName;

        [SerializeField]
        private string _bodyLabelName;

        [SerializeField]
        private string _choicesContainerName;

        [SerializeField]
        private string _choiceButtonClassName;

        /// <summary>
        /// Gets the query name for the root container VisualElement that holds all dialog UI elements. This property returns the name used to locate the container in the UI hierarchy when binding the dialog view to the root VisualElement. If no custom name was provided during initialization, this property will return the default value defined by DefaultContainerElementName, ensuring consistent identification of the dialog container across different UI layouts and implementations.
        /// </summary>
        public string ContainerElementName => _containerElementName;

        /// <summary>
        /// Gets the query name for the VisualElement that displays the speaker's name in the dialog UI. This property returns the name used to locate the speaker label in the UI hierarchy when binding the dialog view to the root VisualElement. If no custom name was provided during initialization, this property will return the default value defined by DefaultSpeakerLabelName, ensuring consistent identification of the speaker label across different UI layouts and implementations.
        /// </summary>
        public string SpeakerLabelName => _speakerLabelName;

        /// <summary>
        /// Gets the query name for the VisualElement that displays the body text of the dialog in the UI. This property returns the name used to locate the body label in the UI hierarchy when binding the dialog view to the root VisualElement. If no custom name was provided during initialization, this property will return the default value defined by DefaultBodyLabelName, ensuring consistent identification of the body label across different UI layouts and implementations.
        /// </summary>
        public string BodyLabelName => _bodyLabelName;

        /// <summary>
        /// Gets the query name for the VisualElement that contains the dialog choice buttons in the UI. This property returns the name used to locate the choices container in the UI hierarchy when binding the dialog view to the root VisualElement. If no custom name was provided during initialization, this property will return the default value defined by DefaultChoicesContainerName, ensuring consistent identification of the choices container across different UI layouts and implementations.
        /// </summary>
        public string ChoicesContainerName => _choicesContainerName;

        /// <summary>
        /// Gets the USS class name for the VisualElements that represent individual dialog choice buttons in the UI. This property returns the class name used to style and identify the choice buttons when binding the dialog view to the root VisualElement. If no custom class name was provided during initialization, this property will return the default value defined by DefaultChoiceButtonClassName, ensuring consistent styling and identification of the choice buttons across different UI layouts and implementations.
        /// </summary>
        public string ChoiceButtonClassName => _choiceButtonClassName;

        /// <summary>
        /// Gets a default instance of DialogUISettings with predefined query names and class identifiers for the dialog UI elements. This instance can be used as a fallback or starting point for configuring the dialog view, ensuring that all necessary UI elements are properly identified and styled according to the default values defined in the struct. The default instance provides a consistent baseline for dialog UI configuration across different implementations and layouts.
        /// </summary>
        public static DialogUISettings Default => new DialogUISettings(
            DefaultContainerElementName,
            DefaultSpeakerLabelName,
            DefaultBodyLabelName,
            DefaultChoicesContainerName,
            DefaultChoiceButtonClassName);

        /// <summary>
        /// Initializes a new instance of the DialogUISettings struct with the specified query names and class identifiers for the dialog UI elements. This constructor allows customization of the dialog view configuration, enabling developers to specify their own query names and class identifiers for the root container, speaker label, body label, choices container, and choice buttons. If any of the provided parameters are null or whitespace, the corresponding default values defined in the struct will be used to ensure consistent identification and styling of the dialog UI elements across different implementations and layouts.
        /// </summary>
        /// <param name="containerElementName"></param>
        /// <param name="speakerLabelName"></param>
        /// <param name="bodyLabelName"></param>
        /// <param name="choicesContainerName"></param>
        /// <param name="choiceButtonClassName"></param>
        public DialogUISettings(
            string containerElementName = DefaultContainerElementName,
            string speakerLabelName = DefaultSpeakerLabelName,
            string bodyLabelName = DefaultBodyLabelName,
            string choicesContainerName = DefaultChoicesContainerName,
            string choiceButtonClassName = DefaultChoiceButtonClassName)
        {
            // Fall back to default query names if uninitialized or whitespace strings are provided.
            _containerElementName = string.IsNullOrWhiteSpace(containerElementName)
                ? DefaultContainerElementName
                : containerElementName;

            _speakerLabelName = string.IsNullOrWhiteSpace(speakerLabelName)
                ? DefaultSpeakerLabelName
                : speakerLabelName;

            _bodyLabelName = string.IsNullOrWhiteSpace(bodyLabelName)
                ? DefaultBodyLabelName
                : bodyLabelName;

            _choicesContainerName = string.IsNullOrWhiteSpace(choicesContainerName)
                ? DefaultChoicesContainerName
                : choicesContainerName;

            _choiceButtonClassName = string.IsNullOrWhiteSpace(choiceButtonClassName)
                ? DefaultChoiceButtonClassName
                : choiceButtonClassName;
        }
    }
}