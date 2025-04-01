# NewgroundsIO Unity SDK
A lightweight NewgroundsIO Unity SDK with async-await support.

## Getting Started

1. Install the [UniTask package](https://github.com/Cysharp/UniTask?tab=readme-ov-file#upm-package).
2. Add this package via the Unity Package Manager (Git URL) or directly.
3. Initialize the SDK with your `appid` and `aeskey`:
    ```csharp
    NGIO.Init(appid, aeskey);
    ```
4. Now you can use the SDK methods. For example:
    ```csharp
    Medal[] medals = await NGIO.Instance.GetMedals();
    ```
