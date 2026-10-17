#if UNITY_EDITOR
using UnityEngine;
using BeyondTime.Memories.Building;

namespace BeyondTime.Memories.Content
{
    /// <summary>
    /// MEMORY 02: THE SECRET YOU SOLD
    /// Teaches: collecting under pressure, escorting someone who can be hurt, a choice with a survival penalty.
    /// </summary>
    public class Memory02BetrayalDefinition : IMemoryDefinition, IMemoryLayout
    {
        public string Id => "memory_02_betrayal";
        public string Folder => "Assets/Memories/Memory_02_Betrayal";
        public string Title => "The Secret You Sold";
        public float Radius => 15f;
        public float TimeLimit => 150f;

        public void Build(MemoryKit k)
        {
            // ---------- layout (a cold office floor) ----------
            var desk = new Color(0.2f, 0.18f, 0.17f);
            for (int i = 0; i < 3; i++)
            {
                k.Prop(PrimitiveType.Cube, "DeskL" + i, new Vector3(-5f, 0.5f, -3f + i * 5f), new Vector3(2.4f, 1f, 1.2f), desk);
                k.Prop(PrimitiveType.Cube, "DeskR" + i, new Vector3(5f, 0.5f, -3f + i * 5f), new Vector3(2.4f, 1f, 1.2f), desk);
            }
            k.Prop(PrimitiveType.Cylinder, "PillarL", new Vector3(-10f, 2.5f, 8f), new Vector3(1.2f, 2.5f, 1.2f), new Color(0.14f, 0.13f, 0.13f));
            k.Prop(PrimitiveType.Cylinder, "PillarR", new Vector3(10f, 2.5f, 8f), new Vector3(1.2f, 2.5f, 1.2f), new Color(0.14f, 0.13f, 0.13f));
            k.Fire(new Vector3(-11f, 0f, -6f)); k.Fire(new Vector3(11f, 0f, -2f)); k.Fire(new Vector3(0f, 0f, 13f), 1.2f);

            // The light she must reach
            var lanternPos = new Vector3(-9f, 0f, -7f);
            k.Prop(PrimitiveType.Cylinder, "LanternBase_REPLACE_ME", lanternPos + new Vector3(0, 0.6f, 0), new Vector3(0.6f, 0.6f, 0.6f), new Color(0.3f, 0.25f, 0.15f));
            var lantern = k.Point("TheLight", lanternPos);
            var glow = new GameObject("LanternLight", typeof(Light)).GetComponent<Light>();
            glow.transform.SetParent(lantern, false); glow.transform.localPosition = new Vector3(0, 2f, 0);
            glow.type = LightType.Point; glow.color = new Color(1f, 0.9f, 0.6f); glow.intensity = 6f; glow.range = 8f;

            var friend = k.Npc("Your Friend", new Vector3(0f, 0f, 10f), new Color(0.75f, 0.55f, 0.6f), 180f);

            var letters = k.Collectibles("LetterPieces", new Color(0.95f, 0.9f, 0.75f),
                                         new Vector3(-10f, 0f, 3f), new Vector3(9.5f, 0f, 6f), new Vector3(7f, 0f, -8f));

            var guards = k.SpawnSet("LetterGuards", new Vector3(-8f, 0f, 2f), new Vector3(8f, 0f, 5f), new Vector3(6f, 0f, -6f));
            var flank = k.SpawnSet("Flank", new Vector3(-11f, 0f, 0f), new Vector3(11f, 0f, 0f), new Vector3(0f, 0f, -11f));

            // ---------- steps ----------
            k.Narrate("You promised your best friend you would keep her secret.");                                        // 0
            k.Narrate("Two days later, you traded it for a promotion. She found out from a stranger.");                   // 1
            k.CollectStep(letters, "Gather the pieces of her letter",
                          "Her letter to you was torn apart that night. The shadows guard every piece.", 3).spawnPoints = guards; // 2
            k.Reach(friend.transform, 3f, "Bring the letter back to her",
                    "She is waiting at the far end. She does not know if she can trust you.", 2);                         // 3
            k.Escort(friend, lantern, 3f, "Lead her to the light",
                     "She will follow you. Keep the shadows away from her.", 3).spawnPoints = flank;                      // 4

            var choice = k.Choice("Your manager offers you a way to stay quiet. What do you do?", 10f, 1,                 // 5
                MemoryKit.Right("Tell everyone the truth. It was me.",
                                "Her eyes fill, but she nods. Trust is slow, but it starts here."),
                MemoryKit.Wrong("Keep the promotion. She will never know.",
                                "You chose the lie again. Lies need defending.", 10f, 4, 6));
            choice.spawnPoints = flank;

            k.Survive(15f, "Defend the lie", "Every lie needs someone to defend it. Survive.", 4).spawnPoints = flank;   // 6

            k.Endings("You told the truth. It cost you something real, and it gave you something back.",
                      "You kept the promotion. The memory holds, but it is hollow.");
        }
    }
}
#endif
