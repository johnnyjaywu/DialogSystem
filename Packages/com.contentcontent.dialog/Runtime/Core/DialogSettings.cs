// File: DialogSettings.cs
using System;
using UnityEngine;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Immutable domain configuration defining typewriter cadence and input skip rules.
    /// Kept strictly free of UI framework identifiers so DialogService remains presentation-agnostic.
    /// </summary>
    [Serializable]
    public struct DialogSettings
    {
        /// <summary>
        /// Default number of seconds to wait per character when rendering dialog text with a typewriter effect. This value is used if no custom setting is provided.
        /// </summary>
        public const float DefaultSecondsPerCharacter = 0.03f;

        /// <summary>
        /// Default flag indicating whether the player is allowed to skip the typewriter effect by pressing an input button. This value is used if no custom setting is provided.
        /// </summary>
        public const bool DefaultAllowTypewriterSkip = true;

        [SerializeField]
        private float _secondsPerCharacter;

        [SerializeField]
        private bool _allowTypewriterSkip;

        /// <summary>
        /// Gets the number of seconds to wait per character when rendering dialog text with a typewriter effect. This value is used to control the speed of the typewriter animation. A value of 0 means that the text will appear instantly without any delay.
        /// </summary>
        public float SecondsPerCharacter => _secondsPerCharacter;

        /// <summary>
        /// Gets a value indicating whether the player is allowed to skip the typewriter effect by pressing an input button. If true, the player can skip the typewriter animation and display the full dialog text immediately. If false, the player must wait for the typewriter effect to complete before proceeding.
        /// </summary>
        public bool AllowTypewriterSkip => _allowTypewriterSkip;

        /// <summary>
        /// Gets a default instance of DialogSettings with predefined values for seconds per character and typewriter skip allowance. This instance can be used as a fallback or starting point for dialog configuration.
        /// </summary>
        public static DialogSettings Default => new DialogSettings(
            DefaultSecondsPerCharacter,
            DefaultAllowTypewriterSkip);

        /// <summary>
        /// Initializes a new instance of the DialogSettings struct with the specified seconds per character and typewriter skip allowance. This constructor allows customization of the dialog rendering behavior, including the speed of the typewriter effect and whether the player can skip it.
        /// </summary>
        /// <param name="secondsPerCharacter">Gets the number of seconds to wait per character when rendering dialog text with a typewriter effect. This value is used to control the speed of the typewriter animation. A value of 0 means that the text will appear instantly without any delay.
        /// </param>
        /// <param name="allowTypewriterSkip">
        /// Gets a value indicating whether the player is allowed to skip the typewriter effect by pressing an input button. If true, the player can skip the typewriter animation and display the full dialog text immediately. If false, the player must wait for the typewriter effect to complete before proceeding.
        /// </param>
        public DialogSettings(
            float secondsPerCharacter = DefaultSecondsPerCharacter,
            bool allowTypewriterSkip = DefaultAllowTypewriterSkip)
        {
            _secondsPerCharacter = Mathf.Max(0f, secondsPerCharacter);
            _allowTypewriterSkip = allowTypewriterSkip;
        }
    }
}