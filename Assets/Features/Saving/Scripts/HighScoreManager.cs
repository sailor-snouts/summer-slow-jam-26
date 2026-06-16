using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JamTemplate.Saving
{
    /// <summary>One entry on the high score board: which save slot it came from, who set it, and the score.</summary>
    [Serializable]
    public struct HighScore
    {
        public int slot;
        public string name;
        public int score;
    }

    /// <summary>
    /// Keeps a persistent high score board, sorted highest-first. Submit a score
    /// with <see cref="Submit"/> and read the leaderboard with <see cref="Top"/>.
    /// Stored as its own JSON file (separate from the save slots, so it never counts
    /// as a save game), mirroring how the <see cref="SaveManager"/> persists data.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/High Score Manager")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public class HighScoreManager : MonoBehaviour
    {
        /// <summary>The active High Score Manager. Persists across scene loads.</summary>
        public static HighScoreManager Instance { get; private set; }

        /// <summary>Raised whenever the board changes, so menus can refresh.</summary>
        public event Action Changed;

        [Serializable]
        private class Board
        {
            public List<HighScore> scores = new List<HighScore>();
        }

        private Board board = new Board();

        private string FilePath => Path.Combine(Application.persistentDataPath, "highscores.json");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Every recorded score, highest first.</summary>
        public IReadOnlyList<HighScore> Scores => board.scores;

        /// <summary>The top <paramref name="count"/> scores, highest first.</summary>
        public IReadOnlyList<HighScore> Top(int count)
        {
            int n = Mathf.Clamp(count, 0, board.scores.Count);
            return board.scores.GetRange(0, n);
        }

        /// <summary>How many entries the board keeps; lower scores fall off the end.</summary>
        private const int MaxEntries = 100;

        /// <summary>Records a score (no save slot), keeps the board sorted highest-first, and saves.</summary>
        public void Submit(string name, int score) => Submit(-1, name, score);

        /// <summary>Records a score for a slot and name, keeps the board sorted highest-first, and saves.</summary>
        public void Submit(int slot, string name, int score)
        {
            board.scores.Add(new HighScore { slot = slot, name = name, score = score });
            board.scores.Sort((a, b) => b.score.CompareTo(a.score));
            if (board.scores.Count > MaxEntries)
                board.scores.RemoveRange(MaxEntries, board.scores.Count - MaxEntries);
            Save();
            Changed?.Invoke();
        }

        /// <summary>Removes every high score.</summary>
        public void Clear()
        {
            board.scores.Clear();
            Save();
            Changed?.Invoke();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    board = JsonUtility.FromJson<Board>(File.ReadAllText(FilePath)) ?? new Board();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Saving] Failed to read high scores: {e.Message}");
                board = new Board();
            }
        }

        private void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(board));
                WebGLFileSync.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Saving] Failed to write high scores: {e.Message}");
            }
        }
    }
}
