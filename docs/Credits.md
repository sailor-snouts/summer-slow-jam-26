# Credits

A data-driven, scrollable credits screen. You list role sections (Design, Programming, …) and the people under each in a `CreditsData` asset; at runtime `CreditsBuilder` renders them into a scroll view.

## Setup

1. Run **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create Credits Scene`**. This:
   - Auto-creates `Assets/Features/Core/ScriptableObjects/Credits.asset` (a `CreditsData`) plus `Assets/Features/Core/Prefabs/Role Text.prefab` and `Names Text.prefab` if missing (non-destructive if they exist).
   - Generates/opens `Assets/Features/Core/Scenes/Credits.unity` — a header, a scroll view, and a **Back** button (`LoadScene` → `Title`).
2. Edit `Credits.asset` → its **Sections** list (roles + names). It ships with placeholder sections.
3. Restyle by editing the `Role Text` / `Names Text` prefabs.

## Data & components

**CreditsData** (ScriptableObject) — create via **Assets ▸ Create ▸ Sailor Snouts ▸ Credits**
- **Role Prefab** (`TMP_Text`) — instantiated as each role heading.
- **Names Prefab** (`TMP_Text`) — instantiated for each section's names.
- **Sections** (`List<CreditsSection>`) — reorderable; each has a `role` string and a `names` list.

**Credits Builder** — `Sailor Snouts/Credits Builder` (`[DisallowMultipleComponent]`)
- **Data** — the `CreditsData`.
- **Content** — the container sections are parented under (the scroll view's Content).

On `Awake` it instantiates, per section with at least one non-blank name, the role prefab (`role`) then the names prefab (names joined by newlines). No public API — configured via the inspector.

## Dependencies

None — TextMeshPro only. The scene generator and the credits assets live in [Core](Core.md).
