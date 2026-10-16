# ContentContent Dialog System (`com.contentcontent.dialog`)

A lightweight, modular dialog package for Unity 6000.5+. It uses UI Toolkit (`PanelRenderer`) for rendering, the Unity Input System for player controls, and Unity's built-in `Awaitable` for character-by-character typewriter text effects.

## Package Structure

All scripts share the `ContentContent.Dialog` namespace and are organized into four folders based on what they do:

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

### Folder Breakdown

- **`Interfaces/`**: C# interfaces that define how the system's parts talk to each other (`IDialogService`, `IDialogSource`, `IDialogView`, `IDialogInputProvider`). Using interfaces lets you swap out the UI, input method, or narrative tool (such as Ink) without rewriting the core dialog logic.
- **`Core/`**: The main logic and data types.
  - `DialogService`: Controls the conversation flow—starting, typing text, waiting for player input, showing choices, and stopping.
  - `DialogSettings`: Controls typing speed (`SecondsPerCharacter`) and whether players can press a button to skip the typewriter animation (`AllowTypewriterSkip`).
  - `DialogNode`, `DialogChoice`, `DialogLine`: Data structures that store speaker names, body text, and branching choice links.
- **`Infrastructure/`**: Classes that connect the core logic to Unity components and assets.
  - `DialogAsset`: A `ScriptableObject` for writing and saving conversations in the Unity Inspector.
  - `DialogSource`: Reads a `DialogAsset` at runtime and keeps track of which line or branch comes next.
  - `DialogViewUIToolkit` & `DialogUISettings`: Updates the UI Toolkit labels and choice buttons on screen. It hides unrevealed letters using invisible rich-text tags (`<alpha=#00>`) so words never jump to a new line mid-sentence while typing.
  - `DialogInputProvider`: Listens to a Unity `InputAction` and notifies the dialog service when the player presses the advance button.
  - `DialogBehavior`: A `MonoBehaviour` component you can drop onto a scene GameObject to connect `PanelRenderer`, your input action, and a `DialogAsset` automatically.
- **`UI/`**: The default `DialogView.uxml` layout and `DialogView.uss` stylesheet.

---

## Quick Start (Inspector Setup)

### 1. Create a Conversation Asset
1. In the **Project** window, right-click and select **Create > ContentContent > Dialog > Dialog Asset**.
2. To load a working example immediately, right-click the `DialogAsset` header in the Inspector and select **Populate Sample Conversation**.

### 2. Add the Dialog Component to Your Scene
1. Create an empty GameObject in your scene and attach **`DialogBehavior`**.
2. When added in the Editor, `DialogBehavior` automatically attaches or links a `PanelRenderer` and finds the first Button `InputActionReference` in your project.
3. On the **`PanelRenderer`** component:
   - Assign your project's `PanelSettings` asset.
   - Assign `Packages/com.contentcontent.dialog/Runtime/UI/DialogView.uxml` as the **Visual Tree Asset**.
4. On the **`DialogBehavior`** component:
   - Drag your `DialogAsset` into the **Dialog Asset** field.
   - Press **Play** to test typing, skipping, and branching choices.

---

## Code-Only Setup (Dependency Injection)

If you use a dependency injection container (like VContainer) or prefer initializing systems in code without `DialogBehavior`, construct the classes directly:

```csharp
DialogSettings dialogSettings = DialogSettings.Default;
DialogUISettings uiSettings = DialogUISettings.Default;

var view = new DialogViewUIToolkit(uiSettings);

// Connect the UI view whenever PanelRenderer loads or reloads the UXML visual tree.
panelRenderer.RegisterUIReloadCallback((renderer, rootElement, version) =>
{
    if (rootElement != null)
    {
        view.BindRoot(rootElement);
    }
});

IDialogInputProvider inputProvider = new DialogInputProvider(playerInputActions.Dialog.Advance);
IDialogService dialogService = new DialogService(view, inputProvider, dialogSettings);

// Create a runtime source from your ScriptableObject asset and start the conversation.
IDialogSource source = dialogAsset.CreateSource();
dialogService.StartDialog(source);
```

---

## Customizing or Extending the System

Because `DialogService` only relies on `IDialogSource`, you can plug in external narrative tools like **Ink** without modifying the core package:
1. Create a class that implements `IDialogSource` (for example, `InkDialogSource`) to read lines and choices from an Ink `Story`.
2. Inherit from `DialogAsset` (for example, `InkDialogAsset`) to hold your compiled Ink `.json` file and override `CreateSource()` to return your `InkDialogSource`.