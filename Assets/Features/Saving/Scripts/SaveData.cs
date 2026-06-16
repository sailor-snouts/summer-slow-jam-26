using System;
using System.Collections.Generic;
using UnityEngine;

namespace JamTemplate.Saving
{
    /// <summary>
    /// The keyed container handed to <see cref="SaveManager.Saving"/> and
    /// <see cref="SaveManager.Loading"/> subscribers. Each participant owns a unique
    /// string key and reads/writes its own state there; values are stored as JSON,
    /// so any <c>[Serializable]</c> type works. Also carries a small header (when it
    /// was saved, a version) so a load menu can describe a slot without applying it.
    /// </summary>
    [Serializable]
    public class SaveData : ISerializationCallbackReceiver
    {
        [SerializeField] private string savedAt = string.Empty;
        [SerializeField] private int version;
        [SerializeField] private string scene = string.Empty;
        [SerializeField] private List<Entry> entries = new List<Entry>();

        [NonSerialized] private Dictionary<string, string> map = new Dictionary<string, string>();

        [Serializable]
        private struct Entry
        {
            public string key;
            public string json;
        }

        /// <summary>UTC timestamp (ISO 8601) of when this was written. Empty until saved.</summary>
        public string SavedAt => savedAt;

        /// <summary>The save format version this data was written with.</summary>
        public int Version => version;

        /// <summary>The scene that was active when this was saved (used by Continue). Empty if none.</summary>
        public string Scene => scene;

        /// <summary>Stamps the header just before writing. Called by the SaveManager.</summary>
        public void Stamp(int version, string scene)
        {
            this.version = version;
            this.scene = scene;
            savedAt = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Stores <paramref name="value"/> under <paramref name="key"/>, serialized as
        /// JSON. Primitives, strings, and enums are boxed internally (JsonUtility cannot
        /// serialize them bare), so <c>Set("score", 42)</c> works as expected.
        /// </summary>
        public void Set<T>(string key, T value)
        {
            if (map.ContainsKey(key))
                Debug.LogWarning($"[Saving] Key '{key}' was written twice in one save; last writer wins. Give each participant a unique key.");

            map[key] = Serialize(value);
        }

        /// <summary>Whether anything is stored under <paramref name="key"/>.</summary>
        public bool Has(string key) => map.ContainsKey(key);

        /// <summary>Reads the value stored under <paramref name="key"/>. Returns false if absent.</summary>
        public bool TryGet<T>(string key, out T value)
        {
            if (map.TryGetValue(key, out string json))
            {
                value = Deserialize<T>(json);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>Reads the value under <paramref name="key"/>, or default if absent.</summary>
        public T Get<T>(string key) => TryGet(key, out T value) ? value : default;

        // JsonUtility refuses bare primitives/strings/enums, so box those transparently.

        [Serializable] private struct IntBox { public long v; }
        [Serializable] private struct FloatBox { public double v; }
        [Serializable] private struct BoolBox { public bool v; }
        [Serializable] private struct StringBox { public string v; }

        private static string Serialize<T>(T value)
        {
            switch (value)
            {
                case int i: return JsonUtility.ToJson(new IntBox { v = i });
                case long l: return JsonUtility.ToJson(new IntBox { v = l });
                case float f: return JsonUtility.ToJson(new FloatBox { v = f });
                case double d: return JsonUtility.ToJson(new FloatBox { v = d });
                case bool b: return JsonUtility.ToJson(new BoolBox { v = b });
                case string s: return JsonUtility.ToJson(new StringBox { v = s });
                case Enum e: return JsonUtility.ToJson(new IntBox { v = Convert.ToInt64(e) });
                default: return JsonUtility.ToJson(value);
            }
        }

        private static T Deserialize<T>(string json)
        {
            Type type = typeof(T);
            if (type == typeof(int)) return (T)(object)(int)JsonUtility.FromJson<IntBox>(json).v;
            if (type == typeof(long)) return (T)(object)JsonUtility.FromJson<IntBox>(json).v;
            if (type == typeof(float)) return (T)(object)(float)JsonUtility.FromJson<FloatBox>(json).v;
            if (type == typeof(double)) return (T)(object)JsonUtility.FromJson<FloatBox>(json).v;
            if (type == typeof(bool)) return (T)(object)JsonUtility.FromJson<BoolBox>(json).v;
            if (type == typeof(string)) return (T)(object)JsonUtility.FromJson<StringBox>(json).v;
            if (type.IsEnum) return (T)Enum.ToObject(type, JsonUtility.FromJson<IntBox>(json).v);
            return JsonUtility.FromJson<T>(json);
        }

        // JsonUtility can't serialize a Dictionary, so mirror it to a list around IO.
        public void OnBeforeSerialize()
        {
            entries.Clear();
            foreach (KeyValuePair<string, string> pair in map)
                entries.Add(new Entry { key = pair.Key, json = pair.Value });
        }

        public void OnAfterDeserialize()
        {
            map = new Dictionary<string, string>();
            foreach (Entry entry in entries)
                map[entry.key] = entry.json;
        }
    }
}
