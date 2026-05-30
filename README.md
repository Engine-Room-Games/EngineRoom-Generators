<div align="center">

# MGenerators

A Unity package of Roslyn source generators that take care of the boilerplate around common gameplay patterns. Every generator ships with a companion analyzer that catches common misuse at edit time, so the pitfalls of each pattern surface as compiler diagnostics instead of runtime surprises.

[![openupm](https://img.shields.io/npm/v/games.engine-room.generators?label=openupm&registry_uri=https://package.openupm.com&color=brightgreen)](https://openupm.com/packages/games.engine-room.generators/)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity)](#requirements)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

</div>

<p align="center">
  <a href="https://youtu.be/WSVPbAtFQCs">
    <img src="https://img.youtube.com/vi/WSVPbAtFQCs/maxresdefault.jpg" alt="Watch the MGenerators tutorial on YouTube" width="640">
  </a>
  <br>
  <em>▶ Watch the tutorial on YouTube</em>
</p>

---

## Table of contents

- [Installation](#installation)
- [Singletons](#singletons)
  - [Attributes](#attributes)
  - [Guidance](#guidance)
- [Requirements](#requirements)

---

## Installation

<details>
<summary><b>Via OpenUPM CLI</b> &nbsp;<sub>— recommended</sub></summary>

&nbsp;

The fastest path. Requires the [OpenUPM CLI](https://openupm.com/docs/getting-started.html#installing-openupm-cli) (`npm install -g openupm-cli`):

```
openupm add games.engine-room.generators
```

This adds the scoped registry, pins the latest version in `Packages/manifest.json`, and triggers Unity to import it.

</details>

<details>
<summary><b>Via Git URL</b></summary>

&nbsp;

Open **Window → Package Manager**, click the **+** button, choose **Install package from git URL…**, and paste:

```
https://github.com/Engine-Room-Games/MGenerators.git?path=src/generators-unity/Packages/games.engine-room.generators
```

> **Tip** — Pin a version by appending `#v<version>` to the URL. Without a version, Unity locks to whatever the default branch points at the time of install and never updates on its own; pinning makes that choice explicit.

```
# pin to a release
https://github.com/Engine-Room-Games/MGenerators.git?path=src/generators-unity/Packages/games.engine-room.generators#v1.0.0

# track the main branch (rolling)
https://github.com/Engine-Room-Games/MGenerators.git?path=src/generators-unity/Packages/games.engine-room.generators#main
```

Available versions are listed on the [releases page](https://github.com/Engine-Room-Games/MGenerators/releases).

</details>

<details>
<summary><b>Via OpenUPM (manual / scoped registry)</b></summary>

&nbsp;

Use this if you don't want to install the OpenUPM CLI. You'll add OpenUPM as a scoped registry once, then install the package from the Package Manager UI.

**1.** Open **Edit → Project Settings → Package Manager** and add a new **Scoped Registry**:

| Field   | Value                                |
| ------- | ------------------------------------ |
| Name    | `package.openupm.com`                |
| URL     | `https://package.openupm.com`        |
| Scope(s)| `games.engine-room.generators`       |

**2.** Open **Window → Package Manager**, switch the top-left dropdown to **My Registries**, find **Engine Room Generators**, and click **Install**.

</details>

---

## Singletons

> [!WARNING]
> Singletons are bad and I do not recommend using them. With that said, I know I can't change the world — there is a big following of the pattern, and people will reach for it whether I like it or not. I strongly believe that any code should be ready for change. If I can make singletons ready to be swapped out for dependency injection while keeping the ergonomics that make people use them in the first place — I'm jumping on the opportunity.

### Attributes

<details>
<summary><b><code>[Singleton]</code></b> &nbsp;<sub>— turn a MonoBehaviour into a singleton</sub></summary>

&nbsp;

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
        var obj = new GameObject(nameof(SoundManager));
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

#### Options

The attribute accepts two optional arguments — a custom interface type and a `destroyOnLoad` flag:

```csharp
[Singleton(destroyOnLoad: false)]                       // default — survives scene loads
[Singleton(destroyOnLoad: true)]                        // per-scene singleton
[Singleton(typeof(IDataStoreManager))]                  // bring your own interface
[Singleton(typeof(IDataStoreManager), destroyOnLoad: true)]
```

**`destroyOnLoad`** *(default: `false`)*

By default the generated `Awake()` calls `DontDestroyOnLoad(gameObject)` and re-parents the host to the scene root, so the singleton outlives scene transitions. Set `destroyOnLoad: true` when you want the singleton scoped to its scene — useful for per-level managers that should reset on reload. With the flag on, the `DontDestroyOnLoad` and reparenting calls are omitted from the generated `Awake()`.

**Bring-your-own interface**

If you'd rather curate the public surface yourself, pass an interface to the attribute and the generator will wire the class up to it instead of synthesising one. Your interface must extend `ISingleton<TSelf>`:

```csharp
public partial interface IDataStoreManager : ISingleton<IDataStoreManager>
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

#### Generated members

| Member                    | What it does                                                                            |
| ------------------------- | --------------------------------------------------------------------------------------- |
| `I<ClassName>` interface  | Auto-generated when no interface is supplied. Lists the public-and-non-`[SingletonIgnore]` members of the class (or only `[SingletonInclude]`-tagged members in explicit mode). |
| `static Create()`         | Factory that spawns a fresh GameObject named after the class and adds the component.    |
| `private void Awake()`    | Publishes the instance, enforces the singleton invariant, optionally calls `DontDestroyOnLoad`. |
| `partial void OnAwake()`  | Your hook — define it for post-awake setup; called after the instance is published.     |

</details>

<details>
<summary><b><code>[SingletonInclude]</code> &amp; <code>[SingletonIgnore]</code></b> &nbsp;<sub>— curate the generated interface</sub></summary>

&nbsp;

When the generator synthesises the interface, it has to decide which members of your class show up on it. By default that's every public method, property, and event. `[SingletonInclude]` and `[SingletonIgnore]` give you fine-grained control. They are mutually exclusive *per class* — using `[SingletonInclude]` anywhere on the class flips the generator into **explicit mode**.

> **Note** — Neither attribute applies when you supply your own interface via `[Singleton(typeof(IFoo))]`; in that case the interface contract is whatever you typed.

**Auto mode** *(default — no `[SingletonInclude]` on the class)*

Every public instance method, property, and event is on the interface. Use `[SingletonIgnore]` to hide individual members:

```csharp
[Singleton]
public partial class SoundManager : MonoBehaviour
{
    public void PlayTap() { /* ... */ }     // → exposed on ISoundManager
    public void PlayWin() { /* ... */ }     // → exposed on ISoundManager

    [SingletonIgnore]
    public void DebugDumpMixerState() { /* ... */ }  // → omitted from ISoundManager
}
```

**Explicit mode** *(any `[SingletonInclude]` present on the class)*

Only members tagged `[SingletonInclude]` appear on the interface. `[SingletonIgnore]` becomes redundant (and the analyzer will warn you):

```csharp
[Singleton]
public partial class SoundManager : MonoBehaviour
{
    [SingletonInclude]
    public void PlayTap() { /* ... */ }     // → exposed on ISoundManager

    public void PlayWin() { /* ... */ }     // → NOT on ISoundManager (no [SingletonInclude])
    public void DebugDumpMixerState() { }   // → NOT on ISoundManager
}
```

**Constraints**

- Both attributes target methods, properties, and events only.
- `[SingletonInclude]` members must be **public** and **non-static** — the analyzer raises `ER0xxx` diagnostics otherwise.

</details>

<details>
<summary><b><code>[Dependency]</code></b> &nbsp;<sub>— inject a singleton into a field</sub></summary>

&nbsp;

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

Multiple `[Dependency]` fields on the same class are all assigned in the same generated `Start()` before `OnStart()` runs.

</details>

### Guidance

<details>
<summary><b>Swapping in tests</b></summary>

&nbsp;

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

</details>

<details>
<summary><b>Lazy instantiation</b></summary>

&nbsp;

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

</details>

---

## Requirements

Unity **2022.3** or newer. Tested on **Unity 2022.3.62** and **Unity 6000.4.0f1**.
