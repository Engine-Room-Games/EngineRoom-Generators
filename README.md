# EngineRoom Generators

A Unity package of Roslyn source generators that take care of the boilerplate around common gameplay patterns. Every generator ships with a companion analyzer that catches common misuse at edit time, so the pitfalls of each pattern surface as compiler diagnostics instead of runtime surprises.

## Table of contents

- [Installation](#installation)
- [Singletons](#singletons)
  - [Singleton Attribute](#singleton)
  - [Dependency Attribute](#dependency)
  - [Swapping in tests](#swapping-in-tests)
  - [Lazy instantiation](#lazy-instantiation)
- [Requirements](#requirements)

## Installation

Available on [OpenUPM](https://openupm.com/):

```
openupm add com.engineroom.generators
```

## Singletons

> [!WARNING]
> Singletons are bad and I do not recommend using them. With that said, I know I can't change the world — there is a big following of the pattern, and people will reach for it whether I like it or not. I strongly believe that any code should be ready for change. If I can make singletons ready to be swapped out for dependency injection while keeping the ergonomics that make people use them in the first place - I'm jumping on the opportunity.

### `[Singleton]`

`[Singleton]` turns a `partial class : MonoBehaviour` into a singleton. Consumers reach it through the generated `I<ClassName>.Instance`.

```csharp
using EngineRoom.Runtime.Singleton;
using UnityEngine;

[Singleton]
public partial class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip _tapClip;
    private AudioSource _audioSource;

    public void PlayTap() => _audioSource.PlayOneShot(_tapClip);

    partial void OnAwake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
}
```

This will generate the following code:

```csharp
public interface ISoundManager : ISingleton<ISoundManager>
{
    void PlayTap();
}

public partial class SoundManager : ISoundManager
{
    public static ISoundManager Create()
    {
        var obj = new GameObject();
        return obj.AddComponent<SoundManager>();
    }

    private void Awake()
    {
        var existing = ISoundManager.Instance as Object;
        if (existing != null && existing != this)
        {
            Object.Destroy(gameObject);
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        ISoundManager.Instance = this;
        OnAwake();
    }

    partial void OnAwake();
}
```

Access it from anywhere via the generated `Instance`:

```csharp
ISoundManager.Instance.PlayTap();
```

If you'd rather curate the public surface yourself, pass an interface to the attribute and the generator will wire the class up to it instead of synthesising one:

```csharp
public partial interface IDataStoreManager
{
    int GetScore();
    void SetScore(int value);
}

[Singleton(typeof(IDataStoreManager))]
public partial class DataStoreManager : MonoBehaviour, IDataStoreManager
{
    public int GetScore() => PlayerPrefs.GetInt("Score", 0);
    public void SetScore(int value) => PlayerPrefs.SetInt("Score", value);
}
```

`[SingletonInclude]` / `[SingletonIgnore]` are also available on individual members for finer control over the auto-generated interface.

### `[Dependency]`

`[Dependency]` resolves a private field from the matching singleton's `Instance` in a generated `Start()`. The field's type must be the singleton's interface.

```csharp
using EngineRoom.Runtime.Singleton;
using UnityEngine;

public partial class Egg : MonoBehaviour
{
    [Dependency] private ISoundManager _soundManager;

    public void Tap()
    {
        _soundManager.PlayTap();
    }
}
```

This will generate the following code:

```csharp
public partial class Egg
{
    private void Start()
    {
        _soundManager = ISoundManager.Instance;
        OnStart();
    }

    partial void OnStart();
}
```

### Swapping in tests

Because consumers see only the generated interface, mocking is a one-liner:

```csharp
public class MockSoundManager : ISoundManager
{
    public int TapPlayCount { get; private set; }
    public static MockSoundManager Install()
    {
        var mock = new MockSoundManager();
        ISoundManager.Instance = mock;
        return mock;
    }
    public void PlayTap() => TapPlayCount++;
}

[Test]
public void Tapping_plays_the_sound()
{
    var sound = MockSoundManager.Install();
    var egg = new GameObject().AddComponent<Egg>();

    egg.Tap();

    Assert.AreEqual(1, sound.TapPlayCount);
}
```

This doesn't solve the fundamental issue with singletons — `Instance` is still global state and every test has to install its mocks up front — but it's a meaningful step up from a classic singleton where there's no seam to mock against at all.

### Lazy instantiation

The generators deliberately don't support lazy instantiation. Auto-spawning a singleton the first time `Instance` is read leads to hard-to-trace initialization order bugs once the objects do real work — so the package leaves the instantiation moment in your hands.

The preferred approach is to place each singleton on a scene (or instantiate it from a prefab) so Unity wires it up like any other component. If you'd rather create them from code, a tiny bootstrap `MonoBehaviour` on the first scene does the job:

```csharp
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        SoundManager.Create();
        UiManager.Create();
        DataStoreManager.Create();
        GameManager.Create();
    }
}
```

The `[DefaultExecutionOrder]` attribute makes `Bootstrap.Awake` run before any other script, so every `Create()` (the factory emitted by `[Singleton]`) registers its instance before anything else touches it.

## Requirements

Unity **2022.3** or newer. Tested on **Unity 2022.3.62** and **Unity 6000.4.0f1**.
