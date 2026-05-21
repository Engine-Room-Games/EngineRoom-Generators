# Coding Conventions

Guidelines for naming conventions and coding style.

## Naming

- **Default:** PascalCase for everything unless noted below
- **camelCase:** local variables, parameters
- **_camelCase:** private fields (underscore prefix)
- **Avoid:** snake_case, kebab-case, Hungarian notation
- Use meaningful, searchable, pronounceable names. Don't abbreviate (except math)
- Prefix booleans with a verb (e.g. `isReady`, `hasTarget`)
- MonoBehaviour class name must match file name

## Formatting

- Allman style braces — always on a new line, always present (even for single-line blocks)
- Single space before flow control conditions: `while (x == y)`
- Single space after commas: `Invoke(a, b)`
- No spaces inside brackets or parentheses: `data[i]`, `Call(a, b)`
- Use blank lines for visual separation

## File Structure Order

1. Usings
2. Namespace
3. Type declaration (class / interface / struct / record)
4. Constants → Static members → Events → Properties → Fields → Readonly fields
5. Constructors
6. Public methods → Private methods
7. Dispose / OnDestroy
8. Destructor
9. Nested types

Within each section, order by access modifier: **public → protected → private**.

## Serialization

- No public fields — ever
- Private fields needing editor exposure: `[SerializeField] private`
- Public access to a serialized value: property with `[field: SerializeField]`
- Auto-properties just to expose a serialized field: forbidden

## Type Conventions

| Type | Rule |
| --- | --- |
| Namespace | PascalCase, no symbols. Use dots for sub-namespaces. Should mirror assembly namespace + folder structure |
| Enum | Singular name. Explicit numeric values. Reserve `0` for None/Empty |
| Flags enum | Plural name. Use bit-shift values (`1 << 0`, `1 << 1`, …) |
| Interface | Prefix with `I` |
| Class / Struct | Noun or noun phrase. No prefixes. One MonoBehaviour per file |

## Members

| Member | Rule |
| --- | --- |
| Property | PascalCase. No auto-property for serialized fields |
| Field | Nouns. Prefix booleans with verb. Specify access modifier explicitly |
| Method | Start with verb or verb phrase. Parameters in camelCase |
| Event | Verb phrase name. Present participle = before (`OpeningDoor`), past participle = after (`DoorOpened`). Handler method adds `On` prefix (`OnDoorOpened`). Use `System.Action` for simple events; custom delegate when needed |

## Var Usage

- Use `var` when the type is obvious from the right side: `var list = new List<int>()`
- Avoid `var` when the type is ambiguous: `int n = Utils.GetRandom()`

## Strings

- Prefer interpolation over concatenation: `$"{name} has {health} HP"`
- Use `nameof()` when referencing member names: `nameof(MaxMana)`

## Regions

- Avoid `#region`. It usually signals a class that should be split. If you need regions to navigate your file, the file is too large

## Examples

### Enum

```csharp
namespace MyGame.Combat
{
    public enum Direction
    {
        North = 1,
        South = 2,
        East = 3,
        West = 4,
    }
}
```

### Flags Enum

```csharp
using System;

namespace MyGame.Combat
{
    [Flags]
    public enum AttackModes
    {
        None = 0,
        Melee = 1 << 0,
        Ranged = 1 << 1,
        Special = 1 << 2,
        MeleeAndSpecial = Melee | Special,
    }
}
```

### Interface

```csharp
namespace MyGame.Combat
{
    public interface IDamageable
    {
        string DamageTypeName { get; }
        float DamageValue { get; }
        bool ApplyDamage(string description, float damage, int numberOfHits);
    }
}
```

### MonoBehaviour Class

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Combat
{
    public class StyleExample : MonoBehaviour
    {
        public const int DefaultCount = 12;
        
        public static readonly Person DefaultPerson = new Person();
        
        public event Action OpeningDoor;      // before
        public event Action DoorOpened;        // after
        public event Action<int> PointsScored;
        public event Action<CustomEventArgs> ThingHappened;
        
        public string DescriptionName { get; set; } = "Fireball";
        
        [field: SerializeField] public float MaxMana { get; private set; }

        [SerializeField] private bool _isPlayerDead;
        
        private int _elapsedTimeInDays;

        public struct CustomEventArgs
        {
            public int ObjectID { get; }
            public Color Color { get; }

            public CustomEventArgs(int objectId, Color color)
            {
                ObjectID = objectId;
                Color = color;
            }
        }

        public void SetInitialPosition(float x, float y, float z)
        {
            transform.position = new Vector3(x, y, z);
        }

        public bool IsNewPosition(Vector3 newPosition)
        {
            return transform.position == newPosition;
        }

        public string GetStatusText()
        {
            // string interpolation over concatenation
            return $"{DescriptionName} — {MaxMana} mana";
        }

        public void LogState()
        {
            // nameof() for member references
            Debug.Log($"{nameof(MaxMana)}: {MaxMana}");
        }

        private void FormatExamples(int someExpression)
        {
            // var — type is obvious
            var powerUps = new List<PlayerStats>();
            var isPowerUp = Utils.IsPowerUp(someExpression);

            // explicit type — var would be ambiguous
            int someNumber = Utils.GetRandomNumber();

            for (int i = 0; i < 100; i++)
            {
                DoSomething(i);
            }

            // braces always required
            if (shouldExit)
            {
                return;
            }
        }
    }
}
```