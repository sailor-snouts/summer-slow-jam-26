using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Reads and writes save files, one per slot, under
    /// <see cref="Application.persistentDataPath"/>/Saves. Saving and loading are
    /// event-driven: <see cref="Save"/> raises <see cref="Saving"/> so every
    /// interested object writes its own data into the shared <see cref="SaveData"/>,
    /// and <see cref="Load"/> raises <see cref="Loading"/> so they read it back.
    /// Auto-save uses its own reserved slot, so it never overwrites a manual save.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Save Manager")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)] // Initialise before participants subscribe.
    public class SaveManager : MonoBehaviour
    {
        private const int Version = 1;
        private const string AutoSaveId = "autosave";

        /// <summary>The active Save Manager. Persists across scene loads.</summary>
        public static SaveManager Instance { get; private set; }

        /// <summary>Raised during a save before the file is written; subscribers write their data into it.</summary>
        public event Action<SaveData> Saving;

        /// <summary>Raised during a load after the file is read; subscribers apply their data from it.</summary>
        public event Action<SaveData> Loading;

        /// <summary>Raised after a manual slot is saved, with the slot index.</summary>
        public event Action<int> Saved;

        /// <summary>Raised after a manual slot is loaded, with the slot index.</summary>
        public event Action<int> Loaded;

        /// <summary>Raised after a manual slot is deleted, with the slot index.</summary>
        public event Action<int> Deleted;

        /// <summary>Raised when writing a save file fails, with the slot index (-1 for the auto-save).</summary>
        public event Action<int> SaveFailed;

        /// <summary>Raised after an auto-save completes — distinct from <see cref="Saved"/> for indicators.</summary>
        public event Action AutoSaved;

        private string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            Instance = null;
        }

        // --- Manual slots -------------------------------------------------------

        /// <summary>Collects data from <see cref="Saving"/> subscribers and writes it to <paramref name="slot"/>.</summary>
        public void Save(int slot)
        {
            if (Write(SlotId(slot)))
                Saved?.Invoke(slot);
            else
                SaveFailed?.Invoke(slot);
        }

        /// <summary>
        /// Loads <paramref name="slot"/>: reloads the scene it was saved in, then hands
        /// the data to that scene's <see cref="Loading"/> subscribers once they are
        /// ready. False if the slot is empty/unreadable. <see cref="Loaded"/> is raised
        /// after the data is applied.
        /// </summary>
        public bool Load(int slot)
        {
            string id = SlotId(slot);
            SaveData info = PeekFile(id);
            if (info == null)
                return false;

            if (string.IsNullOrEmpty(info.Scene))
            {
                // No recorded scene (foreign/edited file): apply to the current scene's subscribers.
                if (!Read(id))
                    return false;

                Loaded?.Invoke(slot);
                return true;
            }

            return RestoreInScene(id, info.Scene, slot);
        }

        /// <summary>Deletes the save in <paramref name="slot"/>, if any.</summary>
        public void Delete(int slot)
        {
            DeleteFile(SlotId(slot));
            Deleted?.Invoke(slot);
        }

        /// <summary>Whether <paramref name="slot"/> holds a save.</summary>
        public bool HasSave(int slot) => Exists(SlotId(slot));

        /// <summary>Reads a slot's header (timestamp, version) without raising <see cref="Loading"/>. Null if empty.</summary>
        public SaveData Peek(int slot) => PeekFile(SlotId(slot));

        // --- Auto-save (reserved slot) ------------------------------------------

        /// <summary>Saves to the reserved auto-save slot and raises <see cref="AutoSaved"/>.</summary>
        public void SaveAuto()
        {
            if (Write(AutoSaveId))
                AutoSaved?.Invoke();
            else
                SaveFailed?.Invoke(-1);
        }

        /// <summary>Loads the auto-save, if present.</summary>
        public bool LoadAuto() => Read(AutoSaveId);

        /// <summary>Whether an auto-save exists.</summary>
        public bool HasAuto() => Exists(AutoSaveId);

        /// <summary>Reads the auto-save header without raising <see cref="Loading"/>. Null if none.</summary>
        public SaveData PeekAuto() => PeekFile(AutoSaveId);

        // --- Continue / queries -------------------------------------------------

        private string pendingRestoreId;
        private string pendingRestoreScene;
        private int pendingRestoreSlot;

        /// <summary>Whether any readable save (manual or auto) exists.</summary>
        public bool HasAnySave()
        {
            if (!Directory.Exists(SaveDirectory))
                return false;

            foreach (string path in Directory.GetFiles(SaveDirectory, "*.json"))
            {
                if (PeekFile(Path.GetFileNameWithoutExtension(path)) != null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Resumes the most recent save: loads the scene it was made in, then
        /// applies the save once that scene is ready (its participants subscribed).
        /// </summary>
        public void Continue()
        {
            string id = MostRecentId();
            if (id == null)
            {
                Debug.LogWarning("[Saving] Continue requested but there is no save.");
                return;
            }

            SaveData info = PeekFile(id);
            string scene = info != null ? info.Scene : null;
            if (string.IsNullOrEmpty(scene))
            {
                Debug.LogWarning($"[Saving] Save '{id}' has no recorded scene; cannot continue.");
                return;
            }

            RestoreInScene(id, scene, -1);
        }

        /// <summary>
        /// Loads <paramref name="scene"/> (fresh, even if already active) and applies
        /// save <paramref name="id"/> once it is ready. <paramref name="slot"/> is
        /// raised on <see cref="Loaded"/> afterwards, or -1 to skip the event.
        /// </summary>
        private bool RestoreInScene(string id, string scene, int slot)
        {
            if (SaveSceneRouter.IsBusy)
            {
                Debug.LogWarning($"[Saving] Cannot load '{id}' while a scene transition is running.");
                return false;
            }

            // Replace any earlier pending restore rather than stacking handlers.
            SceneManager.sceneLoaded -= ApplyPendingRestore;
            pendingRestoreId = id;
            pendingRestoreScene = scene;
            pendingRestoreSlot = slot;
            SceneManager.sceneLoaded += ApplyPendingRestore;

            SaveSceneRouter.Load(scene);
            return true;
        }

        private void ApplyPendingRestore(Scene scene, LoadSceneMode mode)
        {
            // Wait for the save's own scene (ignore any additive overlays loading meanwhile).
            if (pendingRestoreId == null || scene.name != pendingRestoreScene)
                return;

            SceneManager.sceneLoaded -= ApplyPendingRestore;
            string id = pendingRestoreId;
            int slot = pendingRestoreSlot;
            pendingRestoreId = null;
            pendingRestoreScene = null;

            if (Read(id) && slot >= 0)
                Loaded?.Invoke(slot);
        }

        private string MostRecentId()
        {
            if (!Directory.Exists(SaveDirectory))
                return null;

            string bestId = null;
            string bestTime = null;
            foreach (string path in Directory.GetFiles(SaveDirectory, "*.json"))
            {
                SaveData info = PeekFile(Path.GetFileNameWithoutExtension(path));
                if (info == null)
                    continue;

                // ISO 8601 UTC timestamps sort chronologically as plain strings.
                if (bestTime == null || string.CompareOrdinal(info.SavedAt, bestTime) > 0)
                {
                    bestTime = info.SavedAt;
                    bestId = Path.GetFileNameWithoutExtension(path);
                }
            }

            return bestId;
        }

        // --- IO -----------------------------------------------------------------

        private static string SlotId(int slot) => "slot" + slot;

        private string PathFor(string id) => Path.Combine(SaveDirectory, id + ".json");

        private bool Write(string id)
        {
            try
            {
                var data = new SaveData();
                Saving?.Invoke(data);
                data.Stamp(Version, SceneManager.GetActiveScene().name);

                Directory.CreateDirectory(SaveDirectory);
                File.WriteAllText(PathFor(id), JsonUtility.ToJson(data));
                WebGLFileSync.Flush();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Saving] Failed to write '{id}': {e.Message}");
                return false;
            }
        }

        private bool Read(string id)
        {
            SaveData data = PeekFile(id);
            if (data == null)
                return false;

            if (data.Version != Version)
            {
                Debug.LogWarning($"[Saving] Save '{id}' is format version {data.Version} but the game expects {Version}; not applying it. Bump SaveManager.Version whenever your saved structs change shape.");
                return false;
            }

            Loading?.Invoke(data);
            return true;
        }

        private SaveData PeekFile(string id)
        {
            string path = PathFor(id);
            if (!File.Exists(path))
                return null;

            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogError($"[Saving] Failed to read '{id}': {e.Message}");
                return null;
            }
        }

        private void DeleteFile(string id)
        {
            try
            {
                string path = PathFor(id);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    WebGLFileSync.Flush();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Saving] Failed to delete '{id}': {e.Message}");
            }
        }

        private bool Exists(string id) => File.Exists(PathFor(id));
    }
}
