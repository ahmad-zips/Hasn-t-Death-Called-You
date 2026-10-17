#if UNITY_EDITOR
using UnityEngine;
using BeyondTime.Memories.Building;

namespace BeyondTime.Memories.Content
{
    /// <summary>
    /// MEMORY 01: THE MOMENT OF ANGER (tutorial)
    /// Teaches: fighting, reaching a target, hold E under pressure, a timed moral choice.
    /// Edit the text or numbers here, then Tools > Beyond Time > Memories > 2. Build All Memory Prefabs.
    /// </summary>
    public class Memory01AngerDefinition : IMemoryDefinition, IMemoryLayout
    {
        public string Id => "memory_01_anger";
        public string Folder => "Assets/Memories/Memory_01_Anger";
        public string Title => "The Moment of Anger";
        public float Radius => 14f;
        public float TimeLimit => 120f;

        public void Build(MemoryKit k)
        {
            // ---------- layout (a narrow night street) ----------
            var wall = new Color(0.16f, 0.13f, 0.12f);
            k.Prop(PrimitiveType.Cube, "HouseLeft", new Vector3(-10f, 2.5f, 2f), new Vector3(3f, 5f, 16f), wall);
            k.Prop(PrimitiveType.Cube, "HouseRight", new Vector3(10f, 2.5f, 2f), new Vector3(3f, 5f, 16f), wall);
            k.Prop(PrimitiveType.Cube, "Crate1", new Vector3(-5f, 0.5f, 1f), Vector3.one, new Color(0.25f, 0.16f, 0.1f), 20f);
            k.Prop(PrimitiveType.Cube, "Crate2", new Vector3(5.5f, 0.5f, 5f), Vector3.one * 1.2f, new Color(0.25f, 0.16f, 0.1f), -15f);
            k.Prop(PrimitiveType.Cylinder, "LampPost", new Vector3(4f, 2f, -5f), new Vector3(0.15f, 2f, 0.15f), new Color(0.1f, 0.1f, 0.1f));
            k.Fire(new Vector3(-8f, 0f, -9f)); k.Fire(new Vector3(9f, 0f, 10f)); k.Fire(new Vector3(-9f, 0f, 11f), 1.3f);

            var brother = k.Actor("Your Brother", new Vector3(0f, 0f, 8f), new Color(0.45f, 0.55f, 0.75f), 180f, lowered: true);

            var front = k.SpawnSet("Front", new Vector3(-4f, 0f, -2f), new Vector3(4f, 0f, -2f));
            var mid = k.SpawnSet("Mid", new Vector3(-6f, 0f, 4f), new Vector3(6f, 0f, 4f));
            var back = k.SpawnSet("Back", new Vector3(-4f, 0f, 11f), new Vector3(4f, 0f, 11f), new Vector3(0f, 0f, 12f));

            // ---------- steps ----------
            k.Narrate("This is the night you hit your brother. He only asked you to stop shouting.");                       // 0
            k.Narrate("The shadows are your anger. They want this night to end the same way it did.");                     // 1
            k.Defeat("Fight back the shadows of your anger",
                     "Left mouse to strike. Space to dodge. Q to repel. Every hit you take costs you time.", 2, front);    // 2
            k.Reach(brother.transform, 3f, "Reach your brother",
                    "He is still on the ground. Reach him before the moment repeats.", 2, mid);                          // 3
            k.Hold(brother.transform, 2.6f, 3f, "Hold E to kneel beside him", "Kneel beside him",
                   "Kneel. If a shadow strikes you, you lose your focus.").actor = brother;                              // 4

            var choice = k.Choice("He looks up at you. What do you say?", 10f, 2,                                        // 5
                MemoryKit.Right("I was wrong. I am so sorry.",
                                "His shoulders loosen. For the first time in years, the street is quiet."),
                MemoryKit.Wrong("You made me do it.",
                                "The same words as that night. The shadows grow stronger.", 10f, 3, 6),
                MemoryKit.Wrong("Say nothing.",
                                "Silence fills the space where your apology should have been.", 5f, 2, 6));
            choice.spawnPoints = back;

            k.Defeat("Survive what your anger became", "The wrong repeats. Fight your way out of it.", 0);              // 6

            k.Endings("You knelt, and you said it. The anger has nothing left to hold.",
                      "You survived the memory, but you said it again. The anger remembers.");
        }
    }
}
#endif
