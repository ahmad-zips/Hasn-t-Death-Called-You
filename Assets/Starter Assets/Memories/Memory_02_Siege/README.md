# Memory 02: The Siege

**Past wrong:** you abandoned a close friend in a burning fortress to save yourself.
**Corrective action:** you stay, fight your way to them, and lift the rubble off them while the fortress burns.
**Core gameplay:** combat, unlocking a route, timed hazards, ranged pressure, a timed rescue under attack, and a constant temptation to leave.
**Time limit:** 150 s. **Island radius:** 20 m (the entrance faces -Z).

Quick test: **Tools → Beyond Time → Memories → Test One Memory → 02 The Siege**, then press Play.

---

## The fortress, screen by screen

```
          BACK WALL
   [door]   rescue area    [ramp up to balcony]
            FRIEND under rubble
   ─────── walkway end ─────────────
   |  lava  |  fire C |cover|  lava | balcony (Harrier)
   |  pit   |  fire B |cover|  pit  |
   |        |  fire A |cover|       |
   ─────── GATE ═══ lever ──────────
            courtyard (Gatekeeper rises here)
            2 Pursuers
   ─────── entrance ────────────────
```

| # | Section | Target time | What happens | Step |
|---|---|---|---|---|
| 1 | **The Courtyard** | 0:00 to 0:30 | You have control at once and the timer runs from the first second. Your friend's voice calls from beyond the gate, and the gold marker shows where they are. Two **Pursuers** charge you. | DefeatFighters: Pursuer A and B |
| 2 | **The Broken Gate** | 0:30 to 1:15 | The **Gatekeeper** rises out of the ground in front of the portcullis. It glows red before every attack. Dodge, punish its recovery, and knock it out. Then hold E at the lever. | DefeatFighters: Gatekeeper, then Hold E 1.5 s at the lever |
| 3 | **The Burning Walkway** | 1:15 to 2:00 | The gate rises and the lever swings. You cross a narrow wooden walkway over a lava pit. Three sections of fire burst on and off in a rolling rhythm. The **Harrier** throws fire from the balcony on the right; the low walls on the walkway block its shots. Two Pursuers rush you on the bridge, and falling into the pit costs 20 health and 5 s, and puts you back at the gate. | Reach the walkway end |
| 4 | **The Rescue** | final 20 to 30 s | Your friend lies under rubble. Three Pursuers pour out of the smoke. Hold E for 4 s to lift the rubble; any hit makes you drop it, so clear them first. An **open door** glows on the left. | Hold E 4 s at the friend |
| end | | | The rubble slides away. "This time, you did not leave." | Narrate, opens the debris |

**Hellfire pressure:**
- At **30 s** left, fires spread through the courtyard and along the walls.
- At **10 s** left, fires surround the rescue area.
- On top of that, the timer turns orange, then red.

---

## The temptation (what makes the rescue mean something)

From the walkway onward, a **glowing doorway** stands open in the left corner of the rescue area. It is daylight, a way out, closer than you think.

- Walking through it ends the memory **at once**, as **"YOU LEFT THEM AGAIN"** (condemned): *"You ran through the open door into the cold air. Behind you, your friend stopped calling. Just like last time."*
- The bridge onwards still opens, but this memory counts against the ending.

The player keeps choosing to spend seconds on someone else instead of taking the easy route. No dialogue choice is needed: the rescue itself is the decision.

---

## The enemies

### 1. The Pursuers (fast, fragile, in groups)
- A black humanoid shadow with burning eyes, using the same animated body as the people.
- 28 health: one full jab-cross-hook combo or two kicks drops one. 8 poise, so every hit staggers them.
- They run at 5.2 m/s, almost as fast as your sprint, so you cannot outrun them. They swipe in quick 2-hit bursts with 0.4 to 1 s gaps, never block, and rarely dodge.
- Their job is to break your movement on the walkway and during the rescue.
- They appear in three groups: 2 in the courtyard, 2 on the walkway, 3 at the rescue.

### 2. The Gatekeeper (slow boss)
- A shadow 1.55 times normal size, with 140 health and **super armour**: your punches never interrupt its swing.
- Two attacks, alternating. A **red light flares on its chest** during every windup:

| Attack | Windup | Damage | Rules |
|---|---|---|---|
| **Sweep** | about 0.6 s | 14, big knockback | Blockable, but it smashes your guard. **Parry it** (block at the last moment) to stagger it for 1.1 s. |
| **Overhead slam** | about 0.85 s | 22, knocks you down | **Unblockable.** Dodge sideways. |

- After each swing it needs roughly 0.5 s to recover, then pauses for 1 to 1.8 s. That window is your chance to land a full combo plus a kick.
- It walks slowly (1.3 m/s), so you can always reset distance.

### 3. The Harrier (ranged)
- A violet shadow on the balcony.
- When it can see you, its hand glows for about 0.55 s, then it throws a glowing orb at 13 m/s for 9 damage.
- Walls, cover and pillars stop the orbs.
- **Block** an orb to take a fraction of the damage. **Dodge** it to take none.
- To reach it: cross the walkway, then go up the stone ramp at the back right of the rescue area. Up close it only shoves, and 45 health knocks it out.
- Killing it is optional. Leaving it alive means the rescue happens under fire. If it is still standing when the memory ends, it vanishes with the memory.

---

## Tuning (Inspector)

Open `Memory_02_Siege.prefab`.

| What | Where | Default |
|---|---|---|
| Time limit | root → Memory Zone → Time Limit | 150 |
| Pressure fire timing | Memory Zone → Pressure Objects / Final Objects | at 30 s / 10 s |
| Pursuer speed and aggression | `Actors/PursuerA..G` → BrawlerAI → Run Speed, Aggression, Attack Gaps | 5.2, 1.2, 0.4 to 1 s |
| Pursuer toughness | same → Fighter → Max Health / Max Poise | 28 / 8 |
| Gatekeeper toughness | `Actors/TheGatekeeper` → Fighter → Max Health | 140 |
| Gatekeeper attacks | same → Fighter → Combo (Sweep, Slam): duration, hit window, damage, reach | see table |
| Gatekeeper rhythm | same → BrawlerAI → Min/Max Attack Gap | 1.0 to 1.8 s |
| Harrier fire rate and damage | `Actors/TheHarrier` → Ranged AI → Fire Interval, Projectile Speed, Projectile Damage | 2.3 s, 13, 9 |
| Fire rhythm | `StepObjects/FireGapA..C_REPLACE_ME_FX` → Fire Hazard → On Time, Off Time, Warn Time, Phase | 1.5, 2.3, 0.6, 0 / 1.25 / 2.5 |
| Fire damage | same → Damage, Knockback | 8, 4.5 |
| Lava pit penalty | `StepObjects/LavaPitTrigger` → Lava Zone → Time Penalty | 5 |
| Rescue hold time | Memory Zone → Steps → 4 → Hold Time | 4 s |
| Lever hold time | Steps → 2 → Hold Time | 1.5 s |
| Gate and lever motion | `StepObjects/GATE_REPLACE_ME`, `LEVER_REPLACE_ME` → Memory Mover → Open Offset / Open Euler / Duration | rise 4.6 m over 2.5 s; swing -80° |

Changes made in the prefab are lost if you re-run *Build All Memory Prefabs*. Either make the change in `Editor/Memory02SiegeDefinition.cs`, or stop rebuilding once you start editing by hand (see the main README, "The golden rules").

---

## What to replace in this memory

| Object (inside the prefab) | What it is | How to replace it |
|---|---|---|
| `ART_REPLACE_ME/ENV_OuterWalls` | Fortress walls and broken battlements | Your fortress walls. Keep the enclosure closed so the player cannot walk round the gate. |
| `ART_REPLACE_ME/ENV_Courtyard` | Rubble, beam, cart, well | Your courtyard props. |
| `ART_REPLACE_ME/ENV_GateWall` | Gate wall, towers, lintel, lever post | Your gatehouse. Keep a 4.4 m gap where `GATE_REPLACE_ME` sits, or move the gate to match your gap. |
| `ART_REPLACE_ME/ENV_Walkway` | Inner walls, lava pit visual, wooden deck, 3 cover walls | Your bridge and lava. **Keep the deck top about 0.22 m above the pit trigger**, and keep cover on the Harrier's side. |
| `ART_REPLACE_ME/ENV_Balcony` | Balcony floor, rail, supports, ramp | Your balcony and stairs. The Harrier must stand on it, and the player must be able to walk up. |
| `ART_REPLACE_ME/ENV_RescueArea` | Collapsed arch, broken pillar | Your ruins. |
| `StepObjects/GATE_REPLACE_ME` | Portcullis (bars) plus an invisible blocker | Put your gate model under it, centred. Keep the **Blocker** box collider, or give your gate its own collider. |
| `StepObjects/LEVER_REPLACE_ME` | Pivot at the hinge; Handle and Grip | Put your lever under the pivot with its hinge at 0,0,0. Adjust Open Euler. |
| `StepObjects/DEBRIS_REPLACE_ME` | Rubble over the friend | Put your rubble under it. Open Offset slides it aside. |
| `StepObjects/ESCAPE_DOOR_REPLACE_ME` | Glowing doorway (abandon exit) | Your doorway. Keep the trigger box and **Memory Abandon Exit**. |
| `StepObjects/FireGapA..C_REPLACE_ME_FX` | Timed fire across the walkway | Put your fire particles under **Flames** (burst) and **Warning** (small embers), and delete the spheres. Keep the box collider sized to the burning area. |
| `StepObjects/PressureFire`, `FinalFire` | Fires that appear at 30 s / 10 s | Replace each `Fire_REPLACE_ME` child with your fire effect. |
| `FIRE_REPLACE_ME` | Ambient fires; they go out on success | Your braziers and fires. |
| `Actors/YourFriend/MODEL_REPLACE_ME` | Friend lying under rubble | A humanoid lying or sitting. A "trapped" idle animation goes in Memory Actor → Idle State. |
| `Actors/Pursuer*/MODEL_REPLACE_ME` | Shadow humanoid with eyes | Your Pursuer model (see main README, shadow enemies). |
| `Actors/TheGatekeeper/MODEL_REPLACE_ME` | Big shadow; scale is on the root | Your boss model. Move **TelegraphLight** to its chest or weapon. |
| `Actors/TheHarrier/MODEL_REPLACE_ME`, `HAND_GLOW` | Ranged shadow, hand light | Your Harrier model. HAND_GLOW snaps to the right hand. Set the projectile in Ranged AI → Projectile Prefab. |
