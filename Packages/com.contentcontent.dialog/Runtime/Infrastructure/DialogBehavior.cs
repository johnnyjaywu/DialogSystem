// File: DialogBehavior.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ContentContent.Dialog
{
    /// <summary>
    /// Concrete, extensible MonoBehaviour bridge that connects Unity's PanelRenderer and DialogAsset to DialogService.
    /// Automatically resolves missing PanelRenderer and button InputActionReference dependencies in the Editor.
    /// </summary>
    public class DialogBehavior : MonoBehaviour
    {
        [SerializeField]
        private PanelRenderer _panelRenderer;

        [SerializeField]
        private InputActionReference _advanceActionReference;

        [SerializeField]
        private DialogAsset _dialogAsset;

        [SerializeField]
        private bool _startOnUIReload = true;

        [SerializeField]
        private DialogSettings _dialogSettings = DialogSettings.Default;

        [SerializeField]
        private DialogUISettings _uiSettings = DialogUISettings.Default;

        private IDialogInputProvider _inputProvider;
        private IDialogView _dialogView;
        private IDialogService _dialogService;
        private int _uiVersion;

        public IDialogService Service => _dialogService;
        protected DialogSettings DialogSettings => _dialogSettings;
        protected DialogUISettings UISettings => _uiSettings;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (_panelRenderer == null)
            {
                TryGetComponent(out _panelRenderer);
            }

            if (_advanceActionReference == null)
            {
                _advanceActionReference = FindFirstButtonInputActionReferenceInEditor();
            }
        }

        private static InputActionReference FindFirstButtonInputActionReferenceInEditor()
        {
            string[] assetGuids = UnityEditor.AssetDatabase.FindAssets("t:InputActionReference");

            for (int i = 0; i < assetGuids.Length; i++)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                Object[] subAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);

                for (int j = 0; j < subAssets.Length; j++)
                {
                    if (subAssets[j] is InputActionReference actionRef
                        && actionRef.action != null
                        && actionRef.action.type == InputActionType.Button)
                    {
                        return actionRef;
                    }
                }
            }

            return null;
        }
#endif

        protected virtual void Awake()
        {
            if (_panelRenderer == null && !TryGetComponent(out _panelRenderer))
            {
                _panelRenderer = gameObject.AddComponent<PanelRenderer>();
            }

            _inputProvider = CreateInputProvider();
            if (_inputProvider == null)
            {
                enabled = false;
                return;
            }

            _dialogView = CreateView(_uiSettings);
            if (_dialogView == null)
            {
                _inputProvider.Dispose();
                enabled = false;
                return;
            }

            _dialogService = CreateDialogService(_dialogView, _inputProvider, _dialogSettings);
            _dialogService.DialogStarted += HandleDialogStarted;
            _dialogService.DialogEnded += HandleDialogEnded;

            _panelRenderer.RegisterUIReloadCallback(HandleUIReload);
        }

        protected virtual void OnDestroy()
        {
            if (_panelRenderer != null)
            {
                _panelRenderer.UnregisterUIReloadCallback(HandleUIReload);
            }

            if (_dialogService != null)
            {
                _dialogService.DialogStarted -= HandleDialogStarted;
                _dialogService.DialogEnded -= HandleDialogEnded;
                _dialogService.Dispose();
            }

            _inputProvider?.Dispose();
        }

        /// <summary>
        /// Starts a dialog session using the serialized DialogAsset assigned in the Inspector.
        /// </summary>
        public void StartDialog()
        {
            if (_dialogAsset == null)
            {
                Debug.LogWarning($"[{nameof(DialogBehavior)}] Cannot start dialog because no DialogAsset is assigned on {name}.", this);
                return;
            }

            StartDialog(_dialogAsset);
        }

        /// <summary>
        /// Starts a dialog session from a specific DialogAsset instance.
        /// </summary>
        public void StartDialog(DialogAsset asset)
        {
            if (_dialogService == null || asset == null)
            {
                return;
            }

            IDialogSource source = asset.CreateSource();
            _dialogService.StartDialog(source);
        }

        /// <summary>
        /// Stops any currently active dialog session immediately.
        /// </summary>
        public void StopDialog()
        {
            _dialogService?.StopDialog();
        }

        private void HandleUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version)
        {
            if (_uiVersion == version || rootElement == null)
            {
                return;
            }

            _uiVersion = version;
            BindViewToRoot(rootElement);

            if (_startOnUIReload && _dialogAsset != null && !_dialogService.IsActive)
            {
                StartDialog(_dialogAsset);
            }
        }

        protected virtual void BindViewToRoot(VisualElement rootElement)
        {
            if (_dialogView is DialogViewUIToolkit toolkitView)
            {
                toolkitView.BindRoot(rootElement);
            }
        }

        protected virtual IDialogView CreateView(DialogUISettings uiSettings)
        {
            return new DialogViewUIToolkit(uiSettings);
        }

        protected virtual IDialogInputProvider CreateInputProvider()
        {
            if (_advanceActionReference == null || _advanceActionReference.action == null)
            {
                Debug.LogError($"[{nameof(DialogBehavior)}] Missing Advance InputActionReference on {name}.", this);
                return null;
            }

            return new DialogInputProvider(_advanceActionReference.action);
        }

        protected virtual IDialogService CreateDialogService(
            IDialogView view,
            IDialogInputProvider inputProvider,
            DialogSettings dialogSettings)
        {
            return new DialogService(view, inputProvider, dialogSettings);
        }

        protected virtual void HandleDialogStarted()
        {
        }

        protected virtual void HandleDialogEnded()
        {
        }
    }
}