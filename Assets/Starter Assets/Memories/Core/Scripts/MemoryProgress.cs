using System.Collections.Generic;
using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>Remembers which memories are finished and whether each was redeemed (right choice) or condemned.</summary>
    public static class MemoryProgress
    {
        const string Prefix = "BT_Memory_";
        static readonly Dictionary<string, int> session = new Dictionary<string, int>();   // 0 none, 1 redeemed, 2 condemned

        public static event System.Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearSession() { session.Clear(); Changed = null; }

        static bool Persist { get { var s = MemorySettings.Load(); return s != null && s.saveProgressBetweenSessions; } }

        static int Get(string id)
        {
            if (Persist) return PlayerPrefs.GetInt(Prefix + id, 0);
            return session.TryGetValue(id, out var v) ? v : 0;
        }

        public static bool IsDone(string id) => Get(id) != 0;
        public static bool IsRedeemed(string id) => Get(id) == 1;

        public static void Record(string id, bool redeemed)
        {
            int v = redeemed ? 1 : 2;
            session[id] = v;
            if (Persist)
            {
                PlayerPrefs.SetInt(Prefix + id, v);
                string index = PlayerPrefs.GetString(Prefix + "Index", "");
                if (!("," + index + ",").Contains("," + id + ",")) PlayerPrefs.SetString(Prefix + "Index", index.Length == 0 ? id : index + "," + id);
                PlayerPrefs.Save();
            }
            Changed?.Invoke();
        }

        /// <summary>New Game: forget every memory, in this session and on disk.</summary>
        public static void ForgetAll()
        {
            session.Clear();
            foreach (var id in PlayerPrefs.GetString(Prefix + "Index", "").Split(','))
                if (id.Length > 0) PlayerPrefs.DeleteKey(Prefix + id);
            PlayerPrefs.DeleteKey(Prefix + "Index");
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void Forget(string id)
        {
            session.Remove(id);
            PlayerPrefs.DeleteKey(Prefix + id);
            Changed?.Invoke();
        }
    }
}
