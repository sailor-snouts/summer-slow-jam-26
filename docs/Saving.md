# Saving

An event-driven save system with numbered slots and a reserved auto-save. Saving and loading are **decentralized**: the manager raises an event and every interested object writes/reads its own data into a shared container under its own key. One JSON file per slot lives under `Application.persistentDataPath/Saves/`.

## Setup

1. Nothing to place — the Save Manager is spawned automatically at startup by the [Core](Core.md) feature (persistent singleton, initializes before participants). To also drop a copy in a scene, run **`Tools ▸ Sailor Snouts ▸ Managers ▸ Create Save Manager`**.
2. Generate the slot menu with **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create Saves Scene`** (3 slots: Save/Load/Delete each, with timestamps). Open it via a `Menu Button Action` → `OpenAdditive` → `Saves`.
3. Drop an `Auto Save` component wherever auto-saving should run (e.g. a gameplay manager).

## How an object participates

Subscribe in `OnEnable`/`OnDisable`; write on save, read on load. Each object owns a unique key.

```csharp
using JamTemplate.Saving;
using UnityEngine;

public class PlayerSave : MonoBehaviour
{
    [System.Serializable] struct State { public int health; public Vector3 pos; }

    void OnEnable()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.Saving  += Write;
        SaveManager.Instance.Loading += Read;
    }
    void OnDisable()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.Saving  -= Write;
        SaveManager.Instance.Loading -= Read;
    }

    void Write(SaveData d) => d.Set("player", new State { health = hp, pos = transform.position });
    void Read(SaveData d)  { if (d.TryGet("player", out State s)) { hp = s.health; transform.position = s.pos; } }
}
```

`TransformSaveParticipant` (`Sailor Snouts/Transform Save Participant`, with a `key` field) is a ready-made example that persists position/rotation — copy its shape.

## API

```csharp
public static SaveManager Instance { get; }

// Contribution hooks
public event Action<SaveData> Saving;   // write your data
public event Action<SaveData> Loading;  // read your data
// Completion notifications
public event Action<int> Saved, Loaded, Deleted;
public event Action<int> SaveFailed;  // disk error; slot index, or -1 for the auto-save
public event Action AutoSaved;

// Manual slots
public void Save(int slot);
public bool Load(int slot);   // reloads the save's scene, then applies the data there
public void Delete(int slot);
public bool HasSave(int slot);
public SaveData Peek(int slot);   // header only, no Loading event

// Auto-save (reserved slot)
public void SaveAuto();
public bool LoadAuto();
public bool HasAuto();
public SaveData PeekAuto();

// Queries / resume
public bool HasAnySave();   // any manual or auto save (powers Title's Continue/Load buttons)
public void Continue();     // resume the most recent save: load its scene, then apply it
```

`SaveData`: `Set<T>(key, value)`, `bool TryGet<T>(key, out value)`, `T Get<T>(key)`, `bool Has(key)`, plus header `SavedAt` (UTC ISO), `Version`, `Scene` (the scene active when saved — used by `Continue` and `Load`). Values can be any `[Serializable]` class/struct, or a bare primitive/string/enum (`d.Set("score", 42)` works — they're boxed internally because `JsonUtility` can't serialize them on their own).

Like `Continue`, `Load(slot)` reloads the scene the save was made in (fresh, even if it's the current scene) and applies the data once that scene's participants have subscribed — so the Load button works from the Title screen's Saves menu, not just in-game.

Saves carry a format version (`SaveManager.Version`); files written with a different version are listed in menus but refused on load with a console warning. **Bump the constant whenever you change the shape of your saved structs** mid-jam so stale saves don't load as garbage.

## Auto-save

**Auto Save** — `Sailor Snouts/Auto Save`
- **Interval** — seconds between auto-saves (unscaled time).
- **Save On Start** — also save once when it starts.
- **Auto Saved** (`UnityEvent`) — wire an "Auto-saving…" indicator.

It calls `SaveManager.SaveAuto()` (the reserved slot, never a manual one) and the manager raises `AutoSaved`.

## Files

`Saves/slot{n}.json` (manual), `Saves/autosave.json` (auto). High scores live in a separate file (see [High Scores](HighScores.md)) so they don't count as saves.

### WebGL

On WebGL, `Application.persistentDataPath` is an in-memory filesystem that only reaches the browser's IndexedDB when `FS.syncfs` runs — without it, every save is lost when the tab closes. The manager handles this automatically: `WebGLFileSync.Flush()` (backed by `Plugins/WebGLFileSync.jslib`) is called after every write and delete. If you persist your own files outside the save system, call `JamTemplate.Saving.WebGLFileSync.Flush()` after writing.

## Editor tools

**`Tools ▸ Sailor Snouts ▸ Tools ▸ Delete Save Data`** wipes all local data for a clean first-run test: clears every PlayerPref and deletes all save files (slots, auto-save, and high scores). It confirms first; run it outside Play mode.

## Dependencies

None — engine packages only (TextMeshPro). Owns the `SaveSceneRouter` extension point (Continue/Load scene restore; raw loads when unwired); [Core](Core.md) wires it, and wires the UI's `MenuSaveState` to this manager.
