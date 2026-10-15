// File: README.md
# ContentContent Dialog System (`com.contentcontent.dialog`)

A modular, zero-prefab dialog package for Unity 6000.5+ built on UI Toolkit (`PanelRenderer`), the New Input System, and native `Awaitable` asynchronous text progression.

## Package Topology & Architectural Boundaries

All types reside under the unified `ContentContent.Dialog` block-scoped namespace, physically separated by Domain-Driven Design (DDD) responsibility:

```text
Packages/com.contentcontent.dialog/
├── package.json
├── README.md
└── Runtime/
    ├── ContentContent.Dialog.asmdef
    ├── Interfaces/
    │   ├── IDialogInputProvider.cs
    │   ├── IDialogService.cs
    │   ├── IDialogSource.cs
    │   └── IDialogView.cs
    ├── Core/
    │   ├── DialogChoice.cs
    │   ├── DialogLine.cs
    │   ├── DialogNode.cs
    │   ├── DialogService.cs
    │   └── DialogSettings.cs
    ├── Infrastructure/
    │   ├── DialogAsset.cs
    │   ├── DialogBehavior.cs
    │   ├── DialogInputProvider.cs
    │   ├── DialogSource.cs
    │   ├── DialogUISettings.cs
    │   └── DialogViewUIToolkit.cs
    └── UI/
        ├── DialogView.uxml
        └── DialogView.uss
```

### Layer Responsibilities
- **`Interfaces/` (Ports)**: Pure contracts (`IDialogService`, `IDialogSource`, `IDialogView`, `IDialogInputProvider`) decoupling the orchestrator from UI frameworks, input hardware, and narrative engines (e.g., Ink). Enables isolated unit testing via NSubstitute.
- **`Core/` (Domain Models & Presenter)**:
  - `DialogService`: Pure C# MVP Presenter coordinating line progression, `Awaitable` typewriter reveals, choice locking, and lifecycle events (`DialogStarted`, `DialogEnded`).
  - `DialogSettings`: Presentation-agnostic timing and input skip configuration (`SecondsPerCharacter`, `AllowTypewriterSkip`).
  - `DialogNode`, `DialogChoice`, `DialogLine`: Immutable and serializable value payloads representing branching narrative graphs without runtime heap allocations.
- **`Infrastructure/` (Engine & Framework Adapters)**:
  - `DialogAsset`: `ScriptableObject` authoring container that serializes `DialogNode[]` data in the Inspector and acts as a factory via `CreateSource()`.
  - `DialogSource`: Allocation-free, index-cursor state machine implementing `IDialogSource`. Pre-caches `DialogLine` instances at construction time.
  - `DialogViewUIToolkit` & `DialogUISettings`: Passive View adapter for UI Toolkit. Supports dynamic root re-binding via `BindRoot(VisualElement)` and uses rich-text `<alpha=#00>` tag injection to eliminate word-wrap layout jitter during typewriter progression.
  - `DialogInputProvider`: Adapts a Unity `InputAction` (`com.unity.inputsystem`) into domain `AdvanceRequested` events.
  - `DialogBehavior`: Concrete, extensible `MonoBehaviour` bridge managing `PanelRenderer.RegisterUIReloadCallback` and Editor auto-referencing (`OnValidate`).
- **`UI/` (Declarative Templates)**: Default `DialogView.uxml` layout and `DialogView.uss` stylesheet.

---

## Quick Start (Scene Workflow via `DialogBehavior`)

### 1. Create a `DialogAsset`
1. In the **Project** window, right-click and select **Create > ContentContent > Dialog > Dialog Asset**.
2. To populate a multi-branching test graph immediately, right-click the `DialogAsset` Inspector header and select **Populate Sample Conversation**.

### 2. Configure the Scene GameObject
1. Create an empty GameObject in your scene and attach **`DialogBehavior`**.
2. In the Editor, `DialogBehavior.OnValidate()` automatically resolves an attached `PanelRenderer` (or adds one in `Awake()`) and binds the first available Button `InputActionReference` in the project.
3. On the `PanelRenderer` component:
   - Assign your project's `PanelSettings` asset (ensure camera/clear settings are configured).
   - Assign `Packages/com.contentcontent.dialog/Runtime/UI/DialogView.uxml` as the **Visual Tree Asset**.
4. On the `DialogBehavior` component:
   - Assign your `DialogAsset` to the **Dialog Asset** field.
   - Adjust **Dialog Settings** (typewriter speed and skip toggle) and **UI Settings** (UXML element query names) as needed.

---

## Pure Dependency Injection Workflow (VContainer / Composition Root)

For architectures bypassing `DialogBehavior`, construct and inject the pure C# classes directly inside your `LifetimeScope` or bootstrapper:

```csharp
DialogSettings dialogSettings = DialogSettings.Default;
DialogUISettings uiSettings = DialogUISettings.Default;

var view = new DialogViewUIToolkit(uiSettings);

// Bind and hot-reload the UI Toolkit visual tree via PanelRenderer's versioned callback.
panelRenderer.RegisterUIReloadCallback((renderer, rootElement, version) =>
{
    if (rootElement != null)
    {
        view.BindRoot(rootElement);
    }
});

// Inject the generated C# InputAction directly.
IDialogInputProvider inputProvider = new DialogInputProvider(playerInputActions.Dialog.Advance);
IDialogService dialogService = new DialogService(view, inputProvider, dialogSettings);

// Start a conversation from any IDialogSource (e.g., DialogAsset.CreateSource() or a custom InkDialogSource).
IDialogSource source = dialogAsset.CreateSource();
dialogService.StartDialog(source);
```

---

## Extending the System (e.g., Ink Integration)

Because `DialogService` depends strictly on `IDialogSource`, external narrative engines require zero changes to `Core/`:
1. Implement `IDialogSource` in a custom adapter class (e.g., `InkDialogSource`) wrapping `Ink.Runtime.Story`.
2. Subclass `DialogAsset` (e.g., `InkDialogAsset`) to hold the compiled Ink `.json` `TextAsset` and override `CreateSource()` to return your `InkDialogSource`.