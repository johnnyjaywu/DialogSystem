# ContentContent Dialog System

Welcome to the documentation portal for `com.contentcontent.dialog`.

## Architecture Overview

The package enforces strict separation between domain orchestration and Unity engine adapters under the unified `ContentContent.Dialog` namespace:

- **Interfaces (`Interfaces/`)**: Pure C# contracts (`IDialogService`, `IDialogSource`, `IDialogView`, `IDialogInputProvider`) enabling isolated unit testing and external narrative adapters.
- **Core Domain (`Core/`)**: The `DialogService` MVP Presenter, `DialogSettings`, and immutable/serializable value payloads (`DialogLine`, `DialogNode`, `DialogChoice`).
- **Infrastructure (`Infrastructure/`)**: Engine adapters bridging Unity 6000.5+ `PanelRenderer`, `InputAction`, and `ScriptableObject` assets (`DialogBehavior`, `DialogAsset`, `DialogSource`, `DialogViewUIToolkit`, `DialogUISettings`, `DialogInputProvider`).

Select **API Reference** in the top navigation bar to browse all public types and members.