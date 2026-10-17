#if UNITY_EDITOR
using UnityEngine;
using BeyondTime.Memories.Building;

namespace BeyondTime.Memories.Content
{
    /// <summary>
    /// MEMORY 03: THE WALLET
    /// Teaches: a branching path. Returning the wallet is a longer walk but a lighter fight;
    /// keeping it brings a hard fight. Both paths end in a timed escape through fire.
    /// </summary>
    public class Memory03GreedDefinition : IMemoryDefinition, IMemoryLayout
    {
        public string Id => "memory_03_greed";
        public string Folder => "Assets/Memories/Memory_03_Greed";
        public string Title => "The Wallet";
        public float Radius => 15f;
        public float TimeLimit => 140f;

        public void Build(MemoryKit k)
        {
            // ---------- layout (a bus stop and a hiding place) ----------
            var metal = new Color(0.18f, 0.18f, 0.2f);
            k.Prop(PrimitiveType.Cube, "BusShelterRoof", new Vector3(-8f, 2.6f, -5f), new Vector3(4f, 0.2f, 2f), metal);
            k.Prop(PrimitiveType.Cube, "BusShelterBack", new Vector3(-8f, 1.3f, -6f), new Vector3(4f, 2.6f, 0.15f), metal);
            k.Prop(PrimitiveType.Cube, "Bench", new Vector3(-8f, 0.45f, -5.5f), new Vector3(3f, 0.15f, 0.6f), new Color(0.3f, 0.2f, 0.12f));
            k.Prop(PrimitiveType.Cube, "HidingPlace", new Vector3(9f, 0.6f, 6f), new Vector3(1.6f, 1.2f, 1.6f), new Color(0.22f, 0.16f, 0.1f));
            k.Prop(PrimitiveType.Cube, "RubbleA", new Vector3(3f, 0.4f, 2f), new Vector3(2f, 0.8f, 1.2f), new Color(0.15f, 0.12f, 0.1f), 30f);
            k.Prop(PrimitiveType.Cube, "RubbleB", new Vector3(-3f, 0.4f, 8f), new Vector3(1.6f, 0.8f, 2f), new Color(0.15f, 0.12f, 0.1f), -20f);
            k.Fire(new Vector3(11f, 0f, -8f)); k.Fire(new Vector3(-11f, 0f, 7f)); k.Fire(new Vector3(6f, 0f, 12f));

            var oldMan = k.Actor("The Old Man", new Vector3(-8f, 0f, -4.6f), new Color(0.65f, 0.6f, 0.5f), 120f);

            // The wallet sits on the hiding place
            var wallet = new GameObject("WALLET_REPLACE_ME");
            wallet.transform.SetParent(k.Hazards, false);
            wallet.transform.localPosition = new Vector3(9f, 1.35f, 6f);
            MemoryKit.Prim(PrimitiveType.Cube, "Wallet", Vector3.zero, new Vector3(0.45f, 0.1f, 0.3f),
                           MemoryKit.Mat("Wallet_Gold", new Color(0.8f, 0.6f, 0.2f), new Color(2.5f, 1.6f, 0.3f)), wallet.transform, false);
            var wl = new GameObject("Glow", typeof(Light)).GetComponent<Light>();
            wl.transform.SetParent(wallet.transform, false); wl.type = LightType.Point; wl.color = new Color(1f, 0.8f, 0.3f); wl.intensity = 3f; wl.range = 4f;
            var walletPoint = k.Point("WalletSpot", new Vector3(9f, 0f, 6f));

            var exit = k.Point("MemoryExit", new Vector3(0f, 0f, 13f));
            var fireWall = k.FireWall("EscapeFire",
                new Vector3(-5f, 0f, 0f), new Vector3(5f, 0f, 3f), new Vector3(-6f, 0f, 7f), new Vector3(6f, 0f, 9f),
                new Vector3(-2f, 0f, 4f), new Vector3(2.5f, 0f, 11f));

            var guards = k.SpawnSet("WalletGuards", new Vector3(7f, 0f, 2f), new Vector3(11f, 0f, 3f), new Vector3(6f, 0f, 9f));
            var stopSide = k.SpawnSet("StopSide", new Vector3(-4f, 0f, -2f), new Vector3(-11f, 0f, -1f));
            var greed = k.SpawnSet("Greed", new Vector3(5f, 0f, 6f), new Vector3(9f, 0f, 1f), new Vector3(12f, 0f, 7f), new Vector3(8f, 0f, 10f));
            var route = k.SpawnSet("Route", new Vector3(-4f, 0f, 9f), new Vector3(4f, 0f, 10f));

            // ---------- steps ----------
            k.Narrate("You found an old man's wallet on the bus. Inside was the money for his medicine.");                 // 0
            k.Narrate("His name and address were inside it. You kept it anyway.");                                         // 1
            k.Reach(walletPoint, 2.8f, "Find the wallet",
                    "It is still where you hid it, guarded by what you became.", 3, guards);                              // 2
            k.Hold(walletPoint, 2.8f, 2f, "Hold E to pick it up", "Pick up the wallet",
                   "It feels heavier than it should.");                                                                    // 3

            var choice = k.Choice("The old man is still waiting at the bus stop. What do you do with it?", 8f, 1,        // 4
                MemoryKit.Right("Return it to him.", "You turn toward the bus stop.", endMemory: false, goToStep: 5),
                MemoryKit.Wrong("Keep it. Nobody will know.", "Your hand closes around it. Greed has teeth.", 0f, 4, 7));
            choice.spawnPoints = greed;
            choice.deactivate = new[] { wallet };

            k.Reach(oldMan.transform, 3f, "Return the wallet to the old man",
                    "He is still at the stop, checking his pockets again and again.", 2, stopSide);                       // 5
            var giveBack = k.Hold(oldMan.transform, 2.6f, 3f, "Hold E to hand it back", "Give it back",
                                  "Put it in his hands. Look him in the eye.");                                           // 6
            giveBack.actor = oldMan;
            giveBack.nextStep = 8;

            k.Defeat("Greed fights to keep you", "It is heavier than it looks. It always was.", 0);                      // 7

            var escape = k.Reach(exit, 3f, "Escape before the fire takes the memory",
                                 "The memory is burning. Run.", 2, route);                                                 // 8
            escape.activate = new[] { fireWall };
            escape.bonusTime = 10f;

            k.Endings("He did not recognise you. He only said thank you. That was enough.",
                      "You escaped with the wallet. It weighs more every step.");
        }
    }
}
#endif
