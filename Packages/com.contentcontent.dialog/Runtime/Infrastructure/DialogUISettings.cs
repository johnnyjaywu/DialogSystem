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
        public const string DefaultContainerElementName = "dialog-container";
        public const string DefaultSpeakerLabelName = "speaker-label";
        public const string DefaultBodyLabelName = "body-label";
        public const string DefaultChoicesContainerName = "choices-container";
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

        public string ContainerElementName => _containerElementName;
        public string SpeakerLabelName => _speakerLabelName;
        public string BodyLabelName => _bodyLabelName;
        public string ChoicesContainerName => _choicesContainerName;
        public string ChoiceButtonClassName => _choiceButtonClassName;

        public static DialogUISettings Default => new DialogUISettings(
            DefaultContainerElementName,
            DefaultSpeakerLabelName,
            DefaultBodyLabelName,
            DefaultChoicesContainerName,
            DefaultChoiceButtonClassName);

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