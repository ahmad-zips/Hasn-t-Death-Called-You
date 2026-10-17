using UnityEngine;
using BeyondTime.Memories;

namespace BeyondTime.Game
{
    /// <summary>Score, souls (lives) and run statistics. Survives scene loads; saved for Continue.</summary>
    public static class GameScore
    {
        public const int StartSouls = 3;

        public static int Score, Souls = StartSouls, EnemiesDefeated, DamageTaken, MemoriesRedeemed, MemoriesCondemned, Collapses;
        public static bool CameFromMenu;
        public static string Ending = "";
        static int checkpointScore;

        public static event System.Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Clear() { Score = 0; Souls = StartSouls; EnemiesDefeated = DamageTaken = MemoriesRedeemed = MemoriesCondemned = Collapses = 0; CameFromMenu = false; Ending = ""; checkpointScore = 0; Changed = null; }

        public static int HighScore => PlayerPrefs.GetInt("BT_HighScore", 0);
        public static bool HasSave => PlayerPrefs.GetInt("BT_HasSave", 0) == 1;

        public static void NewGame()
        {
            Clear();
            MemoryProgress.ForgetAll();
            PlayerPrefs.SetInt("BT_HasSave", 1);
            Save();
            CameFromMenu = true;
        }

        public static void LoadSaved()
        {
            Score = checkpointScore = PlayerPrefs.GetInt("BT_Score", 0);
            EnemiesDefeated = PlayerPrefs.GetInt("BT_Enemies", 0);
            DamageTaken = PlayerPrefs.GetInt("BT_Damage", 0);
            MemoriesRedeemed = PlayerPrefs.GetInt("BT_Redeemed", 0);
            MemoriesCondemned = PlayerPrefs.GetInt("BT_Condemned", 0);
            Souls = StartSouls;
            CameFromMenu = true;
        }

        public static void RetryFromCheckpoint()
        {
            Score = checkpointScore;
            Souls = StartSouls;
            CameFromMenu = true;
        }

        public static void Add(int points)
        {
            Score = Mathf.Max(0, Score + points);
            Changed?.Invoke();
        }

        public static void Notify() { Changed?.Invoke(); }

        /// <summary>Called after each finished memory: this score is kept if you lose all souls later.</summary>
        public static void Checkpoint() { checkpointScore = Score; Save(); }

        static void Save()
        {
            PlayerPrefs.SetInt("BT_Score", Score);
            PlayerPrefs.SetInt("BT_Enemies", EnemiesDefeated);
            PlayerPrefs.SetInt("BT_Damage", DamageTaken);
            PlayerPrefs.SetInt("BT_Redeemed", MemoriesRedeemed);
            PlayerPrefs.SetInt("BT_Condemned", MemoriesCondemned);
            PlayerPrefs.Save();
        }

        /// <summary>Returns true if this is a new best score.</summary>
        public static bool SubmitFinal()
        {
            bool best = Score > HighScore;
            if (best) PlayerPrefs.SetInt("BT_HighScore", Score);
            PlayerPrefs.SetInt("BT_HasSave", 0);
            PlayerPrefs.Save();
            return best;
        }
    }
}
