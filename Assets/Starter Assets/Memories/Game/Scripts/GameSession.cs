using System.Collections;
using UnityEngine;
using BeyondTime.Memories;

namespace BeyondTime.Game
{
    /// <summary>
    /// Runs the game inside the hellfire world:
    ///   HEALTH  the player's Fighter health (regenerates out of combat).
    ///   SOULS   3 lives. Falling (health 0) or a memory collapsing costs a soul. No souls left = GAME OVER.
    ///   SCORE   redeemed +1000, condemned +300, +10 per second left, +100 per enemy, -2 per damage, -200 per collapse,
    ///           +500 per soul left and +2000 for the good ending at the final gate.
    /// Lives on the GameSystems object in the world scene, next to GameHUD and GameMenus.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [Header("Health")]
        public float regenDelay = 4f;
        public float regenPerSecond = 6f;

        [Header("Score")]
        public int memoryRedeemed = 1000;
        public int memoryCondemned = 300;
        public int perSecondLeft = 10;
        public int perEnemy = 100;
        public int perDamage = 2;
        public int collapsePenalty = 200;
        public int perSoulAtEnd = 500;
        public int goodEndingBonus = 2000;

        [Header("Links")]
        public GameHUD hud;
        public GameMenus menus;

        public bool IsGameOver { get; private set; }

        Fighter player;
        float lastDamage = -99f;
        bool deathPending;
        int enemiesThisMemory;
        float damageThisMemory;

        void Awake()
        {
            Instance = this;
            if (!GameScore.CameFromMenu) GameScore.NewGame();   // pressed Play straight in this scene: start fresh
        }

        void OnEnable()
        {
            MemoryZone.AnyStarted += OnMemoryStarted;
            MemoryZone.AnyCompleted += OnMemoryCompleted;
            MemoryZone.AnyFailed += OnMemoryFailed;
            Fighter.AnyDefeated += OnAnyDefeated;
            MemoryFinalGate.Finished += OnFinalGate;
        }

        void OnDisable()
        {
            MemoryZone.AnyStarted -= OnMemoryStarted;
            MemoryZone.AnyCompleted -= OnMemoryCompleted;
            MemoryZone.AnyFailed -= OnMemoryFailed;
            Fighter.AnyDefeated -= OnAnyDefeated;
            MemoryFinalGate.Finished -= OnFinalGate;
            if (player) player.Damaged -= OnPlayerDamaged;
        }

        IEnumerator Start()
        {
            yield return null;   // let every memory read its saved state first
            foreach (var f in Fighter.All) if (f.team == FighterTeam.Player) { player = f; break; }
            if (player) player.Damaged += OnPlayerDamaged;
            else Debug.LogWarning("Beyond Time: no player Fighter found. Run Tools > Beyond Time > Memories > Add Fighting To Selected Player.");

            // Continue: put the player at the first unfinished memory
            MemoryZone next = null;
            bool anyDone = false;
            foreach (var z in MemoryZone.All)
            {
                if (z.IsCompleted) { anyDone = true; continue; }
                if (!next || z.transform.position.z < next.transform.position.z) next = z;
            }
            if (anyDone && next && next.respawnPoint && player)
            {
                MemoryCheckpoint.SetRespawn(next.respawnPoint);
                MemoryCheckpoint.Teleport(player.transform, next.respawnPoint);
            }
            GameScore.Notify();
        }

        void Update()
        {
            if (!player || IsGameOver || player.IsDefeated) return;
            if (Time.time - lastDamage > regenDelay && player.Health < player.maxHealth)
                player.Heal(regenPerSecond * Time.deltaTime);
        }

        // ------------------------------------------------------------ events

        void OnPlayerDamaged(Fighter attacker, float amount)
        {
            lastDamage = Time.time;
            damageThisMemory += amount;
            GameScore.DamageTaken += Mathf.RoundToInt(amount);
            GameScore.Add(-Mathf.RoundToInt(amount * perDamage));
        }

        void OnAnyDefeated(Fighter f)
        {
            if (f == player) { OnPlayerFell(); return; }
            if (f.team != FighterTeam.Hostile) return;
            enemiesThisMemory++;
            GameScore.EnemiesDefeated++;
            GameScore.Add(perEnemy);
        }

        void OnPlayerFell()
        {
            if (IsGameOver) return;
            LoseSoul("Your soul faltered.");
            if (IsGameOver) return;

            var zone = MemoryZone.Active;
            if (zone && zone.IsRunning)
            {
                deathPending = true;
                zone.FailMemory("You fell. The memory slips away.");
                StartCoroutine(ReviveAfter(2.2f, null));
            }
            else StartCoroutine(ReviveAfter(1.5f, MemoryCheckpoint.Current));
        }

        IEnumerator ReviveAfter(float seconds, Transform teleportTo)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (IsGameOver || !player) yield break;
            if (teleportTo) MemoryCheckpoint.Teleport(player.transform, teleportTo);
            player.Revive();
        }

        void OnMemoryStarted(MemoryZone z)
        {
            enemiesThisMemory = 0;
            damageThisMemory = 0f;
            if (player && player.IsDefeated) player.Revive();
        }

        void OnMemoryFailed(MemoryZone z)
        {
            GameScore.Collapses++;
            GameScore.Add(-collapsePenalty);
            var lib = BeyondTimeAssets.Load();
            if (lib) MusicPlayer.PlaySting(lib.memoryCollapsedSting);

            if (deathPending) { deathPending = false; return; }   // the soul was already taken when the player fell
            LoseSoul("The memory collapsed.");
            if (!IsGameOver && player && player.IsDefeated) StartCoroutine(ReviveAfter(2.2f, null));
        }

        void OnMemoryCompleted(MemoryZone z, bool redeemed, float timeLeft)
        {
            int baseScore = redeemed ? memoryRedeemed : memoryCondemned;
            int timeBonus = Mathf.RoundToInt(timeLeft) * perSecondLeft;
            GameScore.Add(baseScore + timeBonus);
            if (redeemed) GameScore.MemoriesRedeemed++; else GameScore.MemoriesCondemned++;
            GameScore.Checkpoint();
            if (player) player.Heal(player.maxHealth);

            var lib = BeyondTimeAssets.Load();
            if (lib && redeemed) MusicPlayer.PlaySting(lib.memoryRedeemedSting);

            if (hud)
                hud.Result(redeemed ? "MEMORY REDEEMED" : "MEMORY STABILIZED",
                    (redeemed ? "Redeemed" : "Condemned") + "   +" + baseScore +
                    "\nTime bonus   +" + timeBonus +
                    "\nEnemies defeated   " + enemiesThisMemory + "  (+" + enemiesThisMemory * perEnemy + ")" +
                    "\nDamage taken   " + Mathf.RoundToInt(damageThisMemory) + "  (-" + Mathf.RoundToInt(damageThisMemory * perDamage) + ")" +
                    "\n\nTOTAL SCORE   " + GameScore.Score.ToString("N0"));
        }

        void OnFinalGate(bool goodEnding, int redeemed, int total)
        {
            GameScore.Add(GameScore.Souls * perSoulAtEnd + (goodEnding ? goodEndingBonus : 0));
            GameScore.Ending = goodEnding ? "Redeemed" : "Condemned";
        }

        // ------------------------------------------------------------ souls and game over

        void LoseSoul(string why)
        {
            GameScore.Souls = Mathf.Max(0, GameScore.Souls - 1);
            GameScore.Notify();
            var lib = BeyondTimeAssets.Load();
            if (GameScore.Souls <= 0) { StartCoroutine(GameOver()); return; }
            if (lib) MusicPlayer.PlaySting(lib.soulLostSting);
            if (hud) hud.Toast(why + "\nA SOUL IS LOST   " + GameScore.Souls + " remain", 3f);
        }

        IEnumerator GameOver()
        {
            IsGameOver = true;
            var lib = BeyondTimeAssets.Load();
            if (lib) MusicPlayer.PlaySting(lib.gameOverSting);
            yield return new WaitForSecondsRealtime(1.2f);
            if (menus) { Time.timeScale = 0f; menus.ShowGameOver(); yield break; }

            // Test scenes have no menus: show a toast and restart the scene with fresh souls
            if (hud) hud.Toast("CONSUMED BY HELLFIRE\nRestarting", 2f);
            yield return new WaitForSecondsRealtime(2f);
            GameScore.RetryFromCheckpoint();
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
