#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BeyondTime.Memories.Building;
using BeyondTime.Game;

namespace BeyondTime.Memories.Content
{
    /// <summary>
    /// MEMORY 01: THE ALLEY
    /// You once watched a man being stabbed in an alley and walked away. This time you step in.
    /// A real brawl: the attacker hunts the victim with a knife, his two lookouts patrol and join the fight,
    /// the victim runs and cowers. Punch, kick, block, parry, dodge the stab, pin the attacker, then choose.
    /// Quick test: Tools > Beyond Time > Memories > Test One Memory > 01 The Alley
    /// </summary>
    public class Memory01AlleyDefinition : IMemoryDefinition, IMemoryLayout
    {
        public string Id => "memory_01_alley";
        public string Folder => "Assets/Memories/Memory_01_Alley";
        public string Title => "The Alley";
        public float Radius => 15f;
        public float TimeLimit => 180f;

        [MenuItem("Tools/Beyond Time/Memories/Test One Memory/01 The Alley", false, 50)]
        static void TestThisMemory() => MemoryBuilder.BuildSingleMemoryTest("Assets/Memories/Memory_01_Alley");

        public void Build(MemoryKit k)
        {
            // ================================================================ the alley
            var brick = new Color(0.2f, 0.12f, 0.1f);
            var dark = new Color(0.1f, 0.09f, 0.09f);
            k.Prop(PrimitiveType.Cube, "WallLeft",  new Vector3(-7.5f, 2.5f, 1.5f), new Vector3(1f, 5f, 21f), brick);
            k.Prop(PrimitiveType.Cube, "WallRight", new Vector3(7.5f, 2.5f, 1.5f),  new Vector3(1f, 5f, 21f), brick);
            k.Prop(PrimitiveType.Cube, "WallBackLeft",  new Vector3(-5f, 2.5f, 12.5f), new Vector3(6f, 5f, 1f), brick);   // gap in the middle = exit to the next bridge
            k.Prop(PrimitiveType.Cube, "WallBackRight", new Vector3(5f, 2.5f, 12.5f),  new Vector3(6f, 5f, 1f), brick);
            k.Prop(PrimitiveType.Cube, "Dumpster",  new Vector3(-5.6f, 0.75f, 4f),  new Vector3(2.2f, 1.5f, 1.3f), new Color(0.12f, 0.2f, 0.14f));
            k.Prop(PrimitiveType.Cube, "CratesA",   new Vector3(5.5f, 0.5f, -2f),   new Vector3(1.4f, 1f, 1.4f), new Color(0.3f, 0.2f, 0.12f), 15f);
            k.Prop(PrimitiveType.Cube, "CratesB",   new Vector3(5.8f, 1.4f, -2.1f), new Vector3(1f, 0.8f, 1f), new Color(0.3f, 0.2f, 0.12f), -10f);
            k.Prop(PrimitiveType.Cube, "Boxes",     new Vector3(-5.8f, 0.4f, -5f),  new Vector3(1.2f, 0.8f, 0.9f), dark, 25f);
            k.Prop(PrimitiveType.Cylinder, "Barrel", new Vector3(5.6f, 0.6f, 8f),   new Vector3(0.8f, 0.6f, 0.8f), new Color(0.25f, 0.1f, 0.06f));

            // One flickering streetlight over the attack, embers at the alley mouth
            var lamp = new GameObject("StreetLight_REPLACE_ME", typeof(Light)).GetComponent<Light>();
            lamp.transform.SetParent(k.Art, false);
            lamp.transform.localPosition = new Vector3(0f, 5.5f, 7f);
            lamp.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lamp.type = LightType.Spot; lamp.spotAngle = 80f; lamp.range = 12f; lamp.intensity = 30f; lamp.color = new Color(1f, 0.85f, 0.6f);
            lamp.gameObject.AddComponent<MemoryFireFlicker>().min = 22f;
            lamp.GetComponent<MemoryFireFlicker>().max = 32f;
            k.Fire(new Vector3(-9.5f, 0f, -9f)); k.Fire(new Vector3(9.5f, 0f, -8f)); k.Fire(new Vector3(-10f, 0f, 9f), 1.2f); k.Fire(new Vector3(10f, 0f, 10f), 1.2f);

            // ================================================================ the people
            var victim = k.Brawler("The Man", new Vector3(-0.8f, 0f, 9f), 160f, BrawlerAI.Role.Victim,
                                   new Color(0.55f, 0.65f, 0.75f), FighterPresets.Civilian, alwaysShowBar: true);
            victim.walkSpeed = 1.4f; victim.runSpeed = 3.4f; victim.panicRange = 7f; victim.leashRadius = 8f;
            var protectee = victim.gameObject.AddComponent<MemoryProtectee>();
            protectee.failText = "He died in the alley. Again.";

            var killer = k.Brawler("The Attacker", new Vector3(0.6f, 0f, 6.8f), -20f, BrawlerAI.Role.Attacker,
                                   new Color(0.35f, 0.05f, 0.04f), FighterPresets.Killer);
            killer.walkSpeed = 1.8f; killer.runSpeed = 4.3f; killer.attackRange = 1.8f;
            killer.aggression = 0.75f; killer.blockChance = 0.3f; killer.dodgeChance = 0.2f; killer.kickChance = 0.15f;
            killer.murderAttackGap = 2.6f; killer.murderTarget = victim.GetComponent<Fighter>(); killer.menaceUntilEngaged = true;
            var knife = k.Knife(killer);

            var lookoutA = k.Brawler("Lookout A", new Vector3(-3.5f, 0f, -3f), 90f, BrawlerAI.Role.Guard,
                                     new Color(0.18f, 0.18f, 0.2f), FighterPresets.Thug);
            lookoutA.patrolPoints = new[] { k.Point("PatrolA1", new Vector3(-4f, 0f, -3f)), k.Point("PatrolA2", new Vector3(4f, 0f, -3f)),
                                            k.Point("PatrolA3", new Vector3(4f, 0f, 1.5f)), k.Point("PatrolA4", new Vector3(-4f, 0f, 1.5f)) };
            lookoutA.sightRange = 11f; lookoutA.aggression = 0.55f; lookoutA.blockChance = 0.4f; lookoutA.dodgeChance = 0.1f; lookoutA.runSpeed = 4.2f;

            var lookoutB = k.Brawler("Lookout B", new Vector3(4.5f, 0f, 9.5f), -90f, BrawlerAI.Role.Guard,
                                     new Color(0.22f, 0.24f, 0.16f), FighterPresets.Thug);
            lookoutB.patrolPoints = new[] { k.Point("PatrolB1", new Vector3(5f, 0f, 10f)), k.Point("PatrolB2", new Vector3(5f, 0f, 3f)),
                                            k.Point("PatrolB3", new Vector3(-3f, 0f, 10.5f)) };
            lookoutB.sightRange = 10f; lookoutB.aggression = 0.65f; lookoutB.blockChance = 0.3f; lookoutB.dodgeChance = 0.2f; lookoutB.runSpeed = 4.2f;
            lookoutA.GetComponent<Fighter>().displayName = "Lookout";
            lookoutB.GetComponent<Fighter>().displayName = "Lookout";

            killer.alertOnEngage = new[] { lookoutA, lookoutB };

            // The knife after he drops it
            var droppedKnife = new GameObject("DroppedKnife_REPLACE_ME");
            droppedKnife.transform.SetParent(k.Hazards, false);
            droppedKnife.transform.localPosition = new Vector3(2.5f, 0.03f, 5f);
            droppedKnife.transform.localRotation = Quaternion.Euler(90f, 30f, 0f);
            MemoryKit.Prim(PrimitiveType.Cube, "Blade", new Vector3(0f, 0.1f, 0f), new Vector3(0.03f, 0.22f, 0.008f),
                           MemoryKit.Mat("Knife_Steel", new Color(0.75f, 0.76f, 0.8f), new Color(0.15f, 0.15f, 0.17f)), droppedKnife.transform, false);
            MemoryKit.MarkReplaceable(droppedKnife, AssetSlot.Knife, false, droppedKnife.transform.Find("Blade").gameObject);
            droppedKnife.SetActive(false);

            var shadowsBack = k.SpawnSet("Back", new Vector3(-4f, 0f, 10f), new Vector3(4f, 0f, 10f), new Vector3(0f, 0f, 11f));

            var vf = victim.GetComponent<Fighter>();
            var kf = killer.GetComponent<Fighter>();
            var af = lookoutA.GetComponent<Fighter>();
            var bf = lookoutB.GetComponent<Fighter>();

            // ================================================================ steps
            k.Narrate("This is the alley behind the market. You were walking home with your headphones on.");            // 0
            k.Narrate("You saw the knife. You heard him ask for help. You kept walking.");                                 // 1

            var first = k.Choice("The knife is already raised. What do you do?", 5f, 1,                                   // 2
                MemoryKit.Right("Step in.", "You run toward them.", endMemory: false),
                new MemoryChoice
                {
                    label = "Look away.", righteous = false, timePenalty = 10f, goToStep = -1,
                    resultText = "You turn your back. He screams. When you look again, he is bleeding.",
                    hurtFighter = vf, hurtAmount = 35f
                });

            k.DefeatFighters("Stop him",
                "Left mouse to punch, keep clicking for a combo. F to kick. Make him turn on you, and never let him reach the man again.",
                vf, new[] { killer }, kf);                                                                                  // 3

            k.DefeatFighters("Take down his lookouts",
                "His lookouts are coming. Hold right mouse to block. Block just before a punch lands to parry. Space to dodge.",
                vf, new[] { lookoutA, lookoutB }, af, bf);                                                                  // 4

            var pin = k.Hold(killer.transform, 2.2f, 3f, "Hold E to pin him down", "Pin him down",
                             "He is down. The knife has slid across the ground.");                                          // 5
            pin.activate = new[] { droppedKnife };
            pin.deactivate = new[] { knife };

            var last = k.Choice("He is pinned under you. The knife is in reach. What do you do?", 8f, 1,                   // 6
                MemoryKit.Right("Throw the knife away. Shout for help.",
                                "Footsteps. Voices. Someone is calling an ambulance. The man on the ground is still breathing."),
                MemoryKit.Wrong("Pick up the knife.", "Your hand closes around the handle. The memory turns red.", 10f, 4, 7),
                MemoryKit.Wrong("Walk away.", "You leave them both on the ground. The alley does not forgive you.", 5f, 3, 7));
            last.spawnPoints = shadowsBack;
            last.deactivate = new[] { droppedKnife };

            k.Defeat("Fight what you almost became", "This is how it starts. One step too far.", 0);                     // 7

            k.Endings("This time you stepped in. He is alive because you did not walk away.",
                      "You stopped the knife, but not the part of you that reached for it.");
        }
    }
}
#endif
