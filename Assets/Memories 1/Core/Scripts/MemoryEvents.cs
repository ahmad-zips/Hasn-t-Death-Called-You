namespace BeyondTime.Memories
{
    /// <summary>Global events so any enemy, hazard or teammate script can talk to the memories without a hard reference.</summary>
    public static class MemoryEvents
    {
        /// <summary>The player was hit. Value = seconds to take off the running memory's timer.</summary>
        public static event System.Action<float> PlayerHit;

        public static void RaisePlayerHit(float seconds) { PlayerHit?.Invoke(seconds); }
    }

    /// <summary>Anything inside a memory that must go back to its start state when the memory restarts.</summary>
    public interface IMemoryResettable
    {
        void ResetForMemory();
    }

    /// <summary>Anything the test combat can damage. The placeholder shadow implements it.</summary>
    public interface IMemoryDamageable
    {
        void TakeHit(int damage, float stunSeconds);
    }
}
