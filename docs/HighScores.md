# High Scores

A persistent leaderboard kept sorted highest-first. Part of the Saving feature, but stored in its **own file** (`Application.persistentDataPath/highscores.json`) — separate from the save slots, so it never counts as a save game (and never trips the Title's Continue/Load buttons). On WebGL the file is flushed to IndexedDB after every write (see the WebGL note in [Saving](Saving.md)).

## Setup

1. Nothing to place — the High Score Manager is spawned automatically at startup by the [Core](Core.md) feature (persistent singleton). To also drop a copy in a scene, run **`Tools ▸ Sailor Snouts ▸ Managers ▸ Create High Score Manager`**.
2. Generate the menu with **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create High Scores Scene`** (registered in Build Settings). Open it via a `Menu Button Action` → `OpenAdditive` → `HighScores`.

## Recording a score

Call this when a run ends:

```csharp
using JamTemplate.Saving;

HighScoreManager.Instance.Submit(playerName, score);        // slot-less (most jam games)
HighScoreManager.Instance.Submit(slot, playerName, score);  // tied to a save slot
```

It adds the entry, re-sorts the board highest-first, persists immediately, and raises `Changed` (so an open menu refreshes). The board keeps at most 100 entries; lower scores fall off the end.

## API

```csharp
[Serializable] public struct HighScore { public int slot; public string name; public int score; }

public static HighScoreManager Instance { get; }
public event Action Changed;

public IReadOnlyList<HighScore> Scores { get; }      // all, highest first
public IReadOnlyList<HighScore> Top(int count);      // top N, highest first
public void Submit(string name, int score);          // slot recorded as -1
public void Submit(int slot, string name, int score);
public void Clear();
```

## Displaying scores

**High Score List** — `Sailor Snouts/High Score List`
- **Count** — how many rows to show (default **10**). This is the "how many to display" option.
- **Content** — the container rows are added under (a vertical layout).

It builds the rows at runtime (the board length is dynamic) in descending order, rebuilding whenever the board `Changed`. In the generated scene it sits on the Scroll View's **Content** object; change **Count** there to show more or fewer.

## Dependencies

None — TextMeshPro only. Lives in the `JamTemplate.Saving` assembly alongside the [save system](Saving.md); the menu generator lives in [Core](Core.md).
