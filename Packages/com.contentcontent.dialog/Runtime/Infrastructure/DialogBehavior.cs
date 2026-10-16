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

        /// <summary>
        /// Gets the IDialogService instance that manages dialog sessions, including dialog progression, input handling, and view updates. This service is created and initialized in the Awake method of the DialogBehavior MonoBehaviour. It provides methods to start and stop dialog sessions, as well as events to notify when a dialog session starts or ends.
        /// </summary>
        public IDialogService Service => _dialogService;

        /// <summary>
        /// Gets the IDialogView instance that represents the visual interface for displaying dialog content, including dialog lines, choices, and other UI elements. This view is created and initialized in the Awake method of the DialogBehavior MonoBehaviour. It can be customized or replaced with a different implementation to change the appearance and behavior of the dialog interface.
        /// </summary>
        protected DialogSettings DialogSettings => _dialogSettings;

        /// <summary>
        /// Gets the DialogUISettings instance that defines the visual and interactive configuration for the dialog interface, including layout, styling, and input behavior. This settings object is serialized in the Inspector and can be modified to customize the appearance and functionality of the dialog UI. It is used when creating the IDialogView instance in the Awake method of the DialogBehavior MonoBehaviour.
        /// </summary>
        protected DialogUISettings UISettings => _uiSettings;

#if UNITY_EDITOR
        /// <summary>
        /// Handles validation and automatic resolution of missing dependencies in the Unity Editor. This method is called when the script is loaded or a value is changed in the Inspector. It checks for missing PanelRenderer and InputActionReference components, and attempts to find or create them as needed. This ensures that the DialogBehavior MonoBehaviour has all required dependencies for proper operation during development and testing.  
        /// </summary>
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

        /// <summary>
        /// Awake method initializes the DialogBehavior MonoBehaviour, setting up the necessary components and services for dialog management. It ensures that a PanelRenderer is present, creates an IDialogInputProvider for handling player input, and constructs an IDialogView for displaying dialog content. The method also initializes the IDialogService, which manages dialog sessions, and subscribes to its events for dialog start and end notifications. If any critical component fails to initialize, the MonoBehaviour is disabled to prevent runtime errors.
        /// </summary>
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

        /// <summary>
        /// Handles cleanup and resource management when the DialogBehavior MonoBehaviour is destroyed. This method unregisters the UI reload callback from the PanelRenderer, unsubscribes from the dialog service events, and disposes of the input provider to release any resources it holds. It ensures that all components are properly cleaned up to prevent memory leaks or dangling references when the MonoBehaviour is removed from the scene or destroyed during gameplay.  
        /// </summary>
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

        /// <summary>
        /// Binds the dialog view to the root VisualElement of the PanelRenderer. This method is called whenever the UI is reloaded, allowing the dialog view to attach its UI elements to the new root. Derived classes can override this method to customize how the dialog view is bound to the UI hierarchy, enabling different layouts or visual structures for the dialog interface.
        /// </summary>
        /// <param name="rootElement"></param>
        protected virtual void BindViewToRoot(VisualElement rootElement)
        {
            if (_dialogView is DialogViewUIToolkit toolkitView)
            {
                toolkitView.BindRoot(rootElement);
            }
        }

        /// <summary>
        /// Creates an IDialogView instance that represents the visual interface for displaying dialog content. This method is called during the Awake method of the DialogBehavior MonoBehaviour to construct the dialog view based on the provided DialogUISettings. Derived classes can override this method to provide a custom implementation of IDialogView, allowing for different visual styles or behaviors in the dialog interface.   
        /// </summary>
        /// <param name="uiSettings"></param>
        /// <returns></returns>
        protected virtual IDialogView CreateView(DialogUISettings uiSettings)
        {
            return new DialogViewUIToolkit(uiSettings);
        }

        /// <summary>
        /// Creates an IDialogInputProvider instance that handles player input for advancing dialog. This method is called during the Awake method of the DialogBehavior MonoBehaviour to construct the input provider based on the configured InputActionReference. Derived classes can override this method to provide a custom implementation of IDialogInputProvider, allowing for different input handling strategies or input systems to be used in the dialog interface.
        /// </summary>
        /// <returns></returns>
        protected virtual IDialogInputProvider CreateInputProvider()
        {
            if (_advanceActionReference == null || _advanceActionReference.action == null)
            {
                Debug.LogError($"[{nameof(DialogBehavior)}] Missing Advance InputActionReference on {name}.", this);
                return null;
            }

            return new DialogInputProvider(_advanceActionReference.action);
        }

        /// <summary>
        /// Creates an IDialogService instance that manages dialog sessions, including dialog progression, input handling, and view updates. This method is called during the Awake method of the DialogBehavior MonoBehaviour to construct the dialog service using the provided IDialogView, IDialogInputProvider, and DialogSettings. Derived classes can override this method to provide a custom implementation of IDialogService, allowing for different dialog management strategies or behaviors in the dialog system.
        /// </summary>
        /// <param name="view">
        /// The IDialogView instance that represents the visual interface for displaying dialog content. This view is used by the dialog service to update the UI based on dialog progression and player input.
        /// </param>
        /// <param name="inputProvider">
        /// The IDialogInputProvider instance that handles player input for advancing dialog. This input provider is used by the dialog service to listen for player actions and trigger dialog progression accordingly.
        /// </param>
        /// <param name="dialogSettings">
        /// The DialogSettings instance that defines the configuration for dialog rendering, including typewriter cadence and input skip rules. This settings object is used by the dialog service to control the behavior of the dialog interface during a dialog session.
        /// </param>
        /// <returns>
        /// A new instance of IDialogService that manages dialog sessions, constructed with the provided view, input provider, and dialog settings. Derived classes can override this method to return a custom implementation of IDialogService if needed.
        /// </returns>
        protected virtual IDialogService CreateDialogService(
            IDialogView view,
            IDialogInputProvider inputProvider,
            DialogSettings dialogSettings)
        {
            return new DialogService(view, inputProvider, dialogSettings);
        }

        /// <summary>
        /// Overridable method that is called when a dialog session starts. Derived classes can implement this method to perform custom actions or logic when a dialog session begins, such as updating UI elements, triggering animations, or logging events. This method is invoked by the dialog service when a dialog session is initiated, allowing for flexible handling of dialog start events in different contexts.
        /// </summary>
        protected virtual void HandleDialogStarted()
        {
        }

        /// <summary>
        /// Overridable method that is called when a dialog session ends. Derived classes can implement this method to perform custom actions or logic when a dialog session concludes, such as resetting UI elements, triggering animations, or logging events. This method is invoked by the dialog service when a dialog session is terminated, allowing for flexible handling of dialog end events in different contexts.
        /// </summary>
        protected virtual void HandleDialogEnded()
        {
        }
    }
}