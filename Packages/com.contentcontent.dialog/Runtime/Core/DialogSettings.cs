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
        public const float DefaultSecondsPerCharacter = 0.03f;
        public const bool DefaultAllowTypewriterSkip = true;

        [SerializeField]
        private float _secondsPerCharacter;

        [SerializeField]
        private bool _allowTypewriterSkip;

        public float SecondsPerCharacter => _secondsPerCharacter;
        public bool AllowTypewriterSkip => _allowTypewriterSkip;

        public static DialogSettings Default => new DialogSettings(
            DefaultSecondsPerCharacter,
            DefaultAllowTypewriterSkip);

        public DialogSettings(
            float secondsPerCharacter = DefaultSecondsPerCharacter,
            bool allowTypewriterSkip = DefaultAllowTypewriterSkip)
        {
            _secondsPerCharacter = Mathf.Max(0f, secondsPerCharacter);
            _allowTypewriterSkip = allowTypewriterSkip;
        }
    }
}