#if UNITY_EDITOR
using UnityEngine;
using BeyondTime.Memories.Building;

namespace BeyondTime.Memories.Content
{
    /// <summary>
    /// MEMORY 04: THE UNANSWERED CALL
    /// Teaches: holding a position while enemies keep coming (leaving the light pauses progress),
    /// then the emotional finale of the set.
    /// </summary>
    public class Memory04NeglectDefinition : IMemoryDefinition, IMemoryLayout
    {
        public string Id => "memory_04_neglect";
        public string Folder => "Assets/Memories/Memory_04_Neglect";
        public string Title => "The Unanswered Call";
        public float Radius => 15f;
        public float TimeLimit => 150f;

        public void Build(MemoryKit k)
        {
            // ---------- layout (a phone in a pool of light, her house behind) ----------
            var phonePos = new Vector3(0f, 0f, -1f);
            k.Prop(PrimitiveType.Cube, "Table", phonePos + new Vector3(0, 0.45f, 0), new Vector3(1.2f, 0.9f, 0.8f), new Color(0.25f, 0.18f, 0.12f));
            var phone = new GameObject("PHONE_REPLACE_ME");
            phone.transform.SetParent(k.Art, false);
            phone.transform.localPosition = phonePos + new Vector3(0f, 0.95f, 0f);
            MemoryKit.Prim(PrimitiveType.Cube, "Phone", Vector3.zero, new Vector3(0.18f, 0.03f, 0.35f),
                           MemoryKit.Mat("Phone_Screen", Color.white, new Color(2f, 2.2f, 2.6f)), phone.transform, false);

            var circle = MemoryKit.Prim(PrimitiveType.Cylinder, "LightCircle_REPLACE_ME", phonePos + new Vector3(0, 0.02f, 0), new Vector3(8f, 0.01f, 8f),
                                        MemoryKit.Mat("Light_Circle", new Color(0.3f, 0.35f, 0.4f), new Color(0.5f, 0.6f, 0.75f)), k.Art, false);
            var lamp = new GameObject("PhoneLight", typeof(Light)).GetComponent<Light>();
            lamp.transform.SetParent(k.Art, false); lamp.transform.localPosition = phonePos + new Vector3(0, 5f, 0);
            lamp.type = LightType.Spot; lamp.spotAngle = 70f; lamp.range = 12f; lamp.intensity = 25f; lamp.color = new Color(0.75f, 0.85f, 1f);
            lamp.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var phoneTarget = k.Point("PhoneSpot", phonePos);

            var house = new Color(0.2f, 0.16f, 0.14f);
            k.Prop(PrimitiveType.Cube, "HouseWallL", new Vector3(-4f, 2f, 11f), new Vector3(5f, 4f, 0.4f), house);
            k.Prop(PrimitiveType.Cube, "HouseWallR", new Vector3(4f, 2f, 11f), new Vector3(5f, 4f, 0.4f), house);
            k.Prop(PrimitiveType.Cube, "HouseTop", new Vector3(0f, 3.5f, 11f), new Vector3(3f, 1f, 0.4f), house);
            var door = k.Prop(PrimitiveType.Cube, "DOOR_REPLACE_ME", new Vector3(0f, 1.5f, 11f), new Vector3(3f, 3f, 0.25f), new Color(0.3f, 0.2f, 0.1f));
            var doorPoint = k.Point("DoorSpot", new Vector3(0f, 0f, 9.5f));
            var mother = k.Actor("Your Mother", new Vector3(0f, 0f, 13f), new Color(0.7f, 0.6f, 0.65f), 180f, lowered: true);

            k.Fire(new Vector3(-11f, 0f, -6f)); k.Fire(new Vector3(11f, 0f, -5f)); k.Fire(new Vector3(-10f, 0f, 6f)); k.Fire(new Vector3(10f, 0f, 7f));

            var dark = k.SpawnSet("Darkness", new Vector3(-9f, 0f, -1f), new Vector3(9f, 0f, -1f), new Vector3(0f, 0f, -10f),
                                  new Vector3(-7f, 0f, 6f), new Vector3(7f, 0f, 6f));
            var porch = k.SpawnSet("Porch", new Vector3(-6f, 0f, 8f), new Vector3(6f, 0f, 8f));

            // ---------- steps ----------
            k.Narrate("Your mother called you eleven times the night she fell ill.");                                      // 0
            k.Narrate("You saw every call. You told yourself you would call back tomorrow.");                              // 1
            k.Survive(25f, "Stay with the ringing phone",
                      "The phone is ringing. Stay in its light. Do not let the dark pull you away.", 4, phoneTarget, 4f)
             .spawnPoints = dark;                                                                                          // 2
            k.Reach(doorPoint, 2.6f, "Go to her", "The ringing stops. Go to her house.", 2, porch);                       // 3
            k.Hold(doorPoint, 2.6f, 3f, "Hold E to open the door", "Open the door",
                   "Your hand is shaking. Open it anyway.");                                                               // 4

            var choice = k.Choice("She looks up when you walk in. What do you say?", 10f, 2,                              // 5
                MemoryKit.Right("I am here now. I am staying.",
                                "She smiles. She does not ask where you were. She just holds your hand."),
                MemoryKit.Wrong("I was busy. You know how it is.", "The excuse sounds smaller out loud.", 10f, 3, 6),
                MemoryKit.Wrong("Check your phone.", "You look away from her. Again.", 10f, 3, 6));
            choice.deactivate = new[] { door };
            choice.actor = mother;
            choice.spawnPoints = porch;

            k.Defeat("Face what you left behind", "She waited for you. Now you wait for this to end.", 0);               // 6

            k.Endings("You stayed. Some calls can still be answered.",
                      "You came, but you were never really there. The memory stays cold.");
        }
    }
}
#endif
