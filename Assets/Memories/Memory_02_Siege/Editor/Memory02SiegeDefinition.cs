#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BeyondTime.Memories.Building;

namespace BeyondTime.Memories.Content
{
    /// <summary>
    /// MEMORY 02: THE SIEGE
    /// Wrong: you abandoned a close friend in a burning fortress to save yourself.
    /// Corrective action: stay and rescue them.
    /// 1 Courtyard: two Pursuers.  2 Broken Gate: the Gatekeeper, then the lever.
    /// 3 Burning Walkway: timed fire, the Harrier shooting from a balcony, Pursuers on the bridge.
    /// 4 Rescue: shadows attack while you hold E to lift the rubble. An open door offers the easy way out.
    /// Quick test: Tools > Beyond Time > Memories > Test One Memory > 02 The Siege
    /// </summary>
    public class Memory02SiegeDefinition : IMemoryDefinition, IMemoryLayout
    {
        public string Id => "memory_02_siege";
        public string Folder => "Assets/Memories/Memory_02_Siege";
        public string Title => "The Siege";
        public float Radius => 20f;
        public float TimeLimit => 150f;

        [MenuItem("Tools/Beyond Time/Memories/Test One Memory/02 The Siege", false, 51)]
        static void TestThisMemory() => MemoryBuilder.BuildSingleMemoryTest("Assets/Memories/Memory_02_Siege");

        static readonly Color Stone = new Color(0.22f, 0.2f, 0.19f);
        static readonly Color DarkStone = new Color(0.14f, 0.13f, 0.13f);
        static readonly Color Wood = new Color(0.28f, 0.18f, 0.1f);
        static readonly Color Iron = new Color(0.12f, 0.12f, 0.13f);

        static BrawlerAI MakePursuer(MemoryKit k, string name, Vector3 at, float yaw)
        {
            var p = k.ShadowBrawler(name, at, yaw, FighterPresets.Pursuer);
            p.walkSpeed = 2f; p.runSpeed = 5.2f; p.attackRange = 1.5f; p.aggression = 1.2f;
            p.minAttackGap = 0.4f; p.maxAttackGap = 1f; p.maxComboHits = 2;
            p.blockChance = 0f; p.dodgeChance = 0.1f; p.kickChance = 0f; p.sightRange = 30f; p.hearingRange = 30f;
            p.GetComponent<Fighter>().displayName = "Pursuer";
            return p;
        }

        public void Build(MemoryKit k)
        {
            // ================================================================ ENVIRONMENT (each area in its own group)
            var outer = k.Section("ENV_OuterWalls");
            k.Block(outer, "FrontWallLeft",  new Vector3(-8.5f, 2.5f, -16.5f), new Vector3(13f, 5f, 1f), Stone);
            k.Block(outer, "FrontWallRight", new Vector3(8.5f, 2.5f, -16.5f),  new Vector3(13f, 5f, 1f), Stone);
            k.Block(outer, "SideWallLeft",   new Vector3(-15f, 2.5f, 1f),      new Vector3(1f, 5f, 36f), Stone);
            k.Block(outer, "SideWallRight",  new Vector3(15f, 2.5f, 1f),       new Vector3(1f, 5f, 36f), Stone);
            k.Block(outer, "BackWall",       new Vector3(0f, 2.5f, 18.5f),     new Vector3(31f, 5f, 1f), Stone);
            for (int i = 0; i < 8; i++)   // broken battlements
            {
                float z = -14f + i * 4.2f;
                k.Block(outer, "MerlonL" + i, new Vector3(-15f, 5.5f, z), new Vector3(1.1f, 1f, 1.6f), Stone);
                if (i % 3 != 1) k.Block(outer, "MerlonR" + i, new Vector3(15f, 5.5f, z + 1f), new Vector3(1.1f, 1f, 1.6f), Stone);
            }

            var court = k.Section("ENV_Courtyard");
            k.Block(court, "RubbleA", new Vector3(-6f, 0.5f, -11f), new Vector3(2.5f, 1f, 1.6f), DarkStone, 20f);
            k.Block(court, "RubbleB", new Vector3(7f, 0.6f, -7.5f), new Vector3(2f, 1.2f, 2.2f), DarkStone, -35f);
            k.Block(court, "FallenBeam", new Vector3(-8.5f, 0.3f, -5f), new Vector3(5f, 0.5f, 0.5f), Wood, 60f);
            k.Block(court, "BrokenCart", new Vector3(9.5f, 0.6f, -13f), new Vector3(2.4f, 1.2f, 1.4f), Wood, 15f);
            k.Prop(PrimitiveType.Cylinder, "Well", new Vector3(-9f, 0.5f, -12.5f), new Vector3(2.2f, 0.5f, 2.2f), DarkStone);
            k.Fire(new Vector3(-12f, 0f, -14f)); k.Fire(new Vector3(12f, 0f, -10f)); k.Fire(new Vector3(-11.5f, 0f, -6f), 1.2f);
            k.Fire(new Vector3(8f, 0f, -14.5f), 0.8f);

            var gateWall = k.Section("ENV_GateWall");
            k.Block(gateWall, "GateWallLeft",  new Vector3(-8.6f, 3f, -2f), new Vector3(12.8f, 6f, 1.2f), Stone);
            k.Block(gateWall, "GateWallRight", new Vector3(8.6f, 3f, -2f),  new Vector3(12.8f, 6f, 1.2f), Stone);
            k.Prop(PrimitiveType.Cylinder, "TowerLeft",  new Vector3(-3.2f, 3.5f, -2f), new Vector3(2f, 3.5f, 2f), DarkStone);
            k.Prop(PrimitiveType.Cylinder, "TowerRight", new Vector3(3.2f, 3.5f, -2f),  new Vector3(2f, 3.5f, 2f), DarkStone);
            k.Block(gateWall, "Lintel", new Vector3(0f, 5.4f, -2f), new Vector3(4.6f, 1.2f, 1.2f), Stone);

            // Gate (rises) and lever (swings)
            var gate = k.Mover("GATE_REPLACE_ME", k.Hazards, new Vector3(0f, 0f, -2f), new Vector3(0f, 4.6f, 0f), Vector3.zero, 2.5f);
            var blocker = MemoryKit.Prim(PrimitiveType.Cube, "Blocker", new Vector3(0f, 2.2f, 0f), new Vector3(4.4f, 4.4f, 0.3f), null, gate.transform, true);
            Object.DestroyImmediate(blocker.GetComponent<MeshRenderer>());
            var ironMat = MemoryKit.Mat("Prop_Iron", Iron);
            for (int i = 0; i < 9; i++)
                MemoryKit.Prim(PrimitiveType.Cube, "Bar" + i, new Vector3(-2f + i * 0.5f, 2.2f, 0f), new Vector3(0.12f, 4.4f, 0.12f), ironMat, gate.transform, false);
            for (int i = 0; i < 4; i++)
                MemoryKit.Prim(PrimitiveType.Cube, "Cross" + i, new Vector3(0f, 0.6f + i * 1.15f, 0f), new Vector3(4.4f, 0.12f, 0.14f), ironMat, gate.transform, false);

            k.Block(gateWall, "LeverPost", new Vector3(5.5f, 0.55f, -4.5f), new Vector3(0.35f, 1.1f, 0.35f), Iron);
            var lever = k.Mover("LEVER_REPLACE_ME", k.Hazards, new Vector3(5.5f, 1.05f, -4.5f), Vector3.zero, new Vector3(0f, 0f, -80f), 0.6f);
            lever.transform.localRotation = Quaternion.Euler(0f, 0f, 40f);
            MemoryKit.Prim(PrimitiveType.Cube, "Handle", new Vector3(0f, 0.45f, 0f), new Vector3(0.12f, 0.9f, 0.12f), MemoryKit.Mat("Prop_Wood", Wood), lever.transform, false);
            MemoryKit.Prim(PrimitiveType.Sphere, "Grip", new Vector3(0f, 0.92f, 0f), Vector3.one * 0.2f, MemoryKit.Mat("Lever_Glow", new Color(0.9f, 0.5f, 0.1f), new Color(2f, 1f, 0.2f)), lever.transform, false);
            var leverSpot = k.Point("LeverSpot", new Vector3(5.5f, 0f, -4.5f));

            var walk = k.Section("ENV_Walkway");
            k.Block(walk, "InnerWallLeft",  new Vector3(-8f, 2.5f, 5.3f), new Vector3(1f, 5f, 13.4f), Stone);
            k.Block(walk, "InnerWallRight", new Vector3(8f, 2.5f, 5.3f),  new Vector3(1f, 5f, 13.4f), Stone);
            var pit = MemoryKit.Prim(PrimitiveType.Cube, "LavaPit_REPLACE_ME", new Vector3(0f, 0.025f, 5.3f), new Vector3(15f, 0.05f, 13.4f),
                                     MemoryKit.Mat("Lava", new Color(0.5f, 0.08f, 0.01f), new Color(3f, 0.6f, 0.05f)), walk, false);
            k.Block(walk, "WalkwayDeck", new Vector3(0f, 0.12f, 5.3f), new Vector3(2.2f, 0.2f, 13.4f), Wood);
            k.Block(walk, "CoverA", new Vector3(0.75f, 0.87f, 2f),   new Vector3(0.6f, 1.3f, 1.2f), DarkStone);
            k.Block(walk, "CoverB", new Vector3(0.75f, 0.87f, 6f),   new Vector3(0.6f, 1.3f, 1.2f), DarkStone);
            k.Block(walk, "CoverC", new Vector3(0.75f, 0.87f, 9.6f), new Vector3(0.6f, 1.3f, 1.2f), DarkStone);

            var walkStart = k.Point("WalkwayStart", new Vector3(0f, 0.3f, -0.8f));
            var lavaZone = new GameObject("LavaPitTrigger");
            lavaZone.transform.SetParent(k.Hazards, false);
            lavaZone.transform.localPosition = new Vector3(0f, 0.08f, 5.3f);
            var lb = lavaZone.AddComponent<BoxCollider>(); lb.isTrigger = true; lb.size = new Vector3(15f, 0.16f, 13.4f);
            var lz = lavaZone.AddComponent<LavaZone>(); lz.timePenalty = 5f; lz.fallbackRespawn = walkStart;

            var cp = new GameObject("Checkpoint_Walkway");
            cp.transform.SetParent(k.Hazards, false);
            cp.transform.localPosition = new Vector3(0f, 1.5f, -0.6f);
            var cpb = cp.AddComponent<BoxCollider>(); cpb.isTrigger = true; cpb.size = new Vector3(4f, 3f, 1.2f);
            cp.AddComponent<MemoryCheckpoint>().spawnPoint = walkStart;

            k.FireHazardAt("FireGapA", new Vector3(0f, 0.22f, 4f),    new Vector3(2.4f, 2f, 1.2f), 0f);
            k.FireHazardAt("FireGapB", new Vector3(0f, 0.22f, 7.9f),  new Vector3(2.4f, 2f, 1.2f), 1.25f);
            k.FireHazardAt("FireGapC", new Vector3(0f, 0.22f, 11.2f), new Vector3(2.4f, 2f, 1.2f), 2.5f);
            var walkEnd = k.Point("WalkwayEnd", new Vector3(0f, 0f, 12.8f));

            var balcony = k.Section("ENV_Balcony");
            k.Block(balcony, "BalconyFloor", new Vector3(6.5f, 2.45f, 8f), new Vector3(3f, 0.3f, 6f), Stone);
            k.Block(balcony, "BalconyRail",  new Vector3(5.1f, 3.0f, 8f), new Vector3(0.2f, 0.8f, 6f), DarkStone);
            k.Prop(PrimitiveType.Cylinder, "SupportA", new Vector3(5.4f, 1.15f, 5.6f), new Vector3(0.5f, 1.15f, 0.5f), DarkStone);
            k.Prop(PrimitiveType.Cylinder, "SupportB", new Vector3(5.4f, 1.15f, 10.4f), new Vector3(0.5f, 1.15f, 0.5f), DarkStone);
            k.Ramp("BalconyRamp", balcony, new Vector3(6.5f, 0f, 17.2f), new Vector3(6.5f, 2.6f, 11f), 2f, Stone);

            var rescue = k.Section("ENV_RescueArea");
            k.Block(rescue, "CollapsedArch", new Vector3(-3.5f, 0.8f, 17.5f), new Vector3(3f, 1.6f, 1.2f), DarkStone, 12f);
            k.Block(rescue, "BrokenPillar", new Vector3(3.5f, 1.2f, 14.5f), new Vector3(0.9f, 2.4f, 0.9f), Stone, 8f);
            k.Fire(new Vector3(-11f, 0f, 13.5f)); k.Fire(new Vector3(11f, 0f, 16.5f), 1.2f); k.Fire(new Vector3(-6.5f, 0f, 17.3f), 0.9f);

            // ================================================================ THE FRIEND
            var friend = k.Actor("Your Friend", new Vector3(0f, 0f, 16f), new Color(0.7f, 0.65f, 0.5f), 180f, lowered: true);
            var debris = k.Mover("DEBRIS_REPLACE_ME", k.Hazards, new Vector3(0f, 0f, 16f), new Vector3(2.4f, 0f, 1.2f), new Vector3(0f, 35f, 0f), 1.2f);
            var rubbleMat = MemoryKit.Mat("Prop_" + ColorUtility.ToHtmlStringRGB(DarkStone), DarkStone);
            MemoryKit.Prim(PrimitiveType.Cube, "SlabA", new Vector3(0.1f, 0.35f, -0.5f), new Vector3(1.8f, 0.35f, 0.9f), rubbleMat, debris.transform, true).transform.localRotation = Quaternion.Euler(8f, 15f, 5f);
            MemoryKit.Prim(PrimitiveType.Cube, "SlabB", new Vector3(-0.6f, 0.6f, -0.9f), new Vector3(1.2f, 0.3f, 0.7f), rubbleMat, debris.transform, true).transform.localRotation = Quaternion.Euler(-12f, -20f, 10f);
            MemoryKit.Prim(PrimitiveType.Cube, "Beam", new Vector3(0.5f, 0.75f, -0.7f), new Vector3(2.6f, 0.25f, 0.25f), MemoryKit.Mat("Prop_Wood", Wood), debris.transform, true).transform.localRotation = Quaternion.Euler(0f, 30f, 12f);

            // The easy way out
            var door = new GameObject("ESCAPE_DOOR_REPLACE_ME");
            door.transform.SetParent(k.Hazards, false);
            door.transform.localPosition = new Vector3(-13.6f, 0f, 15f);
            door.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            var frame = MemoryKit.Mat("Door_Frame", Stone);
            MemoryKit.Prim(PrimitiveType.Cube, "PostL", new Vector3(-1.2f, 1.5f, 0f), new Vector3(0.3f, 3f, 0.4f), frame, door.transform, true);
            MemoryKit.Prim(PrimitiveType.Cube, "PostR", new Vector3(1.2f, 1.5f, 0f), new Vector3(0.3f, 3f, 0.4f), frame, door.transform, true);
            MemoryKit.Prim(PrimitiveType.Cube, "Top", new Vector3(0f, 3.1f, 0f), new Vector3(2.7f, 0.3f, 0.4f), frame, door.transform, true);
            MemoryKit.Prim(PrimitiveType.Cube, "Daylight", new Vector3(0f, 1.5f, 0.05f), new Vector3(2.1f, 3f, 0.05f),
                           MemoryKit.Mat("Door_Daylight", new Color(0.9f, 0.95f, 0.85f), new Color(2.5f, 2.8f, 2.3f)), door.transform, false);
            var dl = new GameObject("Light", typeof(Light)).GetComponent<Light>();
            dl.transform.SetParent(door.transform, false); dl.transform.localPosition = new Vector3(0f, 1.6f, 1f);
            dl.type = LightType.Point; dl.color = new Color(0.9f, 1f, 0.9f); dl.intensity = 6f; dl.range = 8f;
            var doorTrigger = door.AddComponent<BoxCollider>();
            doorTrigger.isTrigger = true; doorTrigger.center = new Vector3(0f, 1.5f, 0f); doorTrigger.size = new Vector3(2.1f, 3f, 1f);
            door.AddComponent<MemoryAbandonExit>().abandonText =
                "You ran through the open door into the cold air. Behind you, your friend stopped calling. Just like last time.";
            door.SetActive(false);

            // ================================================================ ENEMIES
            var c1 = MakePursuer(k, "Pursuer A", new Vector3(-3f, 0f, -9f), 180f);
            var c2 = MakePursuer(k, "Pursuer B", new Vector3(3f, 0f, -8f), 180f);
            var w1 = MakePursuer(k, "Pursuer C", new Vector3(-0.5f, 0.25f, 13.6f), 180f);
            var w2 = MakePursuer(k, "Pursuer D", new Vector3(1f, 0f, 14.6f), 180f);
            var r1 = MakePursuer(k, "Pursuer E", new Vector3(-5f, 0f, 14f), 120f);
            var r2 = MakePursuer(k, "Pursuer F", new Vector3(5f, 0f, 13.5f), -120f);
            var r3 = MakePursuer(k, "Pursuer G", new Vector3(-2f, 0f, 17.6f), 160f);
            foreach (var p in new[] { w1, w2, r1, r2, r3 }) p.gameObject.SetActive(false);

            var keeper = k.ShadowBrawler("The Gatekeeper", new Vector3(0f, 0f, -5.5f), 180f, FighterPresets.Gatekeeper, 1.55f);
            keeper.walkSpeed = 1.3f; keeper.runSpeed = 2.2f; keeper.attackRange = 2.3f; keeper.aggression = 0.7f;
            keeper.minAttackGap = 1f; keeper.maxAttackGap = 1.8f; keeper.maxComboHits = 1;
            keeper.blockChance = 0f; keeper.dodgeChance = 0f; keeper.kickChance = 0f; keeper.sightRange = 40f; keeper.hearingRange = 40f;
            var keeperGlow = new GameObject("TelegraphLight", typeof(Light)).GetComponent<Light>();
            keeperGlow.transform.SetParent(keeper.transform, false);
            keeperGlow.transform.localPosition = new Vector3(0f, 2.2f, 0.4f);
            keeperGlow.type = LightType.Point; keeperGlow.color = new Color(1f, 0.15f, 0.05f); keeperGlow.intensity = 12f; keeperGlow.range = 6f;
            keeperGlow.enabled = false;
            keeper.GetComponent<Fighter>().telegraphLight = keeperGlow;
            var rise = keeper.gameObject.AddComponent<BridgeReveal>();
            rise.riseFrom = -3f; rise.duration = 1.2f;
            keeper.gameObject.SetActive(false);

            var harrier = k.Harrier("The Harrier", new Vector3(6.5f, 2.65f, 8.5f), -90f);

            // ================================================================ HELLFIRE PRESSURE
            k.Zone.pressureObjects = new[] { k.FireGroup("PressureFire", 1.3f,
                new Vector3(-10f, 0f, -9f), new Vector3(10f, 0f, -4f), new Vector3(-4f, 0f, -14f), new Vector3(5f, 0f, -11f),
                new Vector3(-12f, 0f, 3f), new Vector3(12f, 0f, 6f)) };
            k.Zone.finalObjects = new[] { k.FireGroup("FinalFire", 1.5f,
                new Vector3(-9f, 0f, 13f), new Vector3(9f, 0f, 13f), new Vector3(-12f, 0f, 17f), new Vector3(12f, 0f, 17f),
                new Vector3(4f, 0f, 17.5f), new Vector3(-2f, 0f, -6f), new Vector3(2f, 0f, -6f)) };
            k.Zone.persistentMarker = friend.transform;

            // ================================================================ STEPS
            k.DefeatFighters("Fight through the courtyard",
                "\"Help me! I am trapped!\" The voice comes from beyond the broken gate. You know that voice. You remember leaving it behind.",
                null, new[] { c1, c2 }, c1.GetComponent<Fighter>(), c2.GetComponent<Fighter>());                                    // 0

            var boss = k.DefeatFighters("Defeat the Gatekeeper",
                "Something huge rises in front of the gate. When it glows, it is about to strike. Dodge with Space, then hit it while it recovers. Your punches will not stop its swing.",
                null, new[] { keeper }, keeper.GetComponent<Fighter>());                                                             // 1
            boss.activate = new[] { keeper.gameObject };

            k.Hold(leverSpot, 2f, 1.5f, "Hold E to pull the lever", "Open the gate",
                   "The lever. Pull it before the fire spreads.");                                                                 // 2

            var cross = k.Reach(walkEnd, 2f, "Cross the burning walkway",
                "The gate grinds open. The walkway is burning: wait for the flames to drop, then move. Something on the balcony is throwing fire. Stay behind the low walls, or climb up and stop it.");   // 3
            cross.openOnStart = new[] { gate, lever };
            cross.rangedOnStart = new[] { harrier };
            cross.activate = new[] { w1.gameObject, w2.gameObject, door };
            cross.engageOnStart = new[] { w1, w2 };

            var free = k.Hold(friend.transform, 2.6f, 4f, "Hold E to lift the rubble", "Free your friend",
                "\"You came back.\" Shadows pour out of the smoke. Clear them, then lift the rubble. A hit will make you drop it. Behind you, an open door leads to safety.");                    // 4
            free.activate = new[] { r1.gameObject, r2.gameObject, r3.gameObject };
            free.engageOnStart = new[] { r1, r2, r3 };

            var end = k.Narrate("The rubble shifts. Your friend crawls free and grabs your hand. This time, you did not leave.");   // 5
            end.openOnStart = new[] { debris };
            end.endMemoryAfter = true;

            k.Endings("You stayed. You spent the seconds that mattered on someone else, and you carried them out of the fire.",
                      "You ran. The memory holds, but it holds the same ending as before.");
        }
    }
}
#endif
