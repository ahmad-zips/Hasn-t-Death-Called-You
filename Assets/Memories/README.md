# Beyond Time: Memories

The hellfire world's memory system with full melee combat, and two finished memories:

| # | Memory | Past wrong | Core gameplay |
|---|---|---|---|
| 01 | **The Alley** | You watched a man being stabbed and walked away. | Brawl with real NPCs: stop a knife attacker, protect the victim, fight two lookouts, pin the attacker, choose. |
| 02 | **The Siege** | You abandoned your friend in a burning fortress to save yourself. | Pursuers, a Gatekeeper boss, a lever-locked gate, a burning walkway under ranged fire, and a timed rescue under attack, with an open door tempting you to leave. |

Every visual is a clearly named placeholder. **Section 5 explains, step by step, how to bring in your own environment, characters, enemies, animations, effects, audio and UI.**

---

## Contents

1. [What is in the folder](#1-what-is-in-the-folder)
2. [Install and first run](#2-install-and-first-run)
3. [How it plays](#3-how-it-plays)
4. [The golden rules (read before replacing anything)](#4-the-golden-rules)
5. [Using your own assets](#5-using-your-own-assets)
   - 5.1 Choose a workflow
   - 5.2 Environment
   - 5.3 Your player character
   - 5.4 Human NPCs
   - 5.5 Shadow enemies
   - 5.6 Animations (Mixamo or your own)
   - 5.7 Props that move: gate, lever, rubble, doors, knife
   - 5.8 Effects: fire, lava, projectiles, glows
   - 5.9 Audio
   - 5.10 UI and health bars
   - 5.11 Building the real hellfire world scene
   - 5.12 Grave video, menus and the build
6. [Tuning gameplay](#6-tuning-gameplay)
7. [Making a new memory](#7-making-a-new-memory)
8. [Team and Git rules](#8-team-and-git-rules)
9. [Playtest checklist](#9-playtest-checklist)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. What is in the folder

```
Assets/Memories/
├── README.md                          this file
├── Core/
│   ├── Scripts/                       runtime
│   │   ├── MemoryZone.cs              runs a memory: steps, timer, choices, win, collapse, abandon
│   │   ├── MemoryUI.cs                story at the bottom, objective, timer, choices, hold bar, banners, marker
│   │   ├── MemoryActor.cs             a person who does not fight (the trapped friend)
│   │   ├── MemoryNPC.cs               a person who follows you (escort steps)
│   │   ├── MemoryCollectible.cs       pickups
│   │   ├── MemoryShadow.cs            simple placeholder shadow (used by "extra shadows" from choices)
│   │   ├── MemorySettings.cs          one asset for shared settings (placeholder shadow, saving, text speed)
│   │   ├── MemoryProgress.cs          which memories are done, redeemed or condemned
│   │   ├── MemoryEvents.cs            "player was hit" event, reset interface
│   │   ├── MemoryMover.cs             gates, levers, rubble that open smoothly
│   │   ├── MemoryAbandonExit.cs       the easy way out (ends the memory condemned)
│   │   ├── FireHazard.cs              fire that bursts on and off across a path
│   │   ├── LavaZone.cs                fall in: back to the checkpoint, lose time
│   │   ├── MemoryCheckpoint.cs        respawn points
│   │   ├── BridgeReveal.cs            rises out of the lava when switched on (bridges, the Gatekeeper)
│   │   ├── MemoryFinalGate.cs         opens when all memories are done; gives the ending
│   │   ├── IntroVideo.cs              plays the grave video, then loads the world
│   │   ├── MemoryFireFlicker.cs       light flicker
│   │   ├── MemoryTestCombat.cs        old test combat (not used any more)
│   │   └── Combat/
│   │       ├── Fighter.cs             health, guard, poise, combos, kicks, block, parry, dodge,
│   │       │                          stagger, knockdown, super armour, presets
│   │       ├── FighterPose.cs         procedural punches, kicks, slams, sweeps, casts, guard, block, flinch, cower
│   │       ├── PlayerBrawler.cs       player controls on top of Starter Assets
│   │       ├── BrawlerAI.cs           melee NPC brain: patrol, spot, chase, circle, combo, block, dodge,
│   │       │                          flee, cower, murder
│   │       ├── RangedAI.cs            ranged NPC brain (the Harrier)
│   │       ├── MemoryProjectile.cs    projectiles (blocked by cover, blockable, dodgeable)
│   │       ├── FighterHealthBar.cs    floating name and health bar
│   │       ├── MemoryProtectee.cs     the memory collapses if this person dies
│   │       ├── BoneAttachment.cs      snaps an object into a hand (or any bone) at runtime
│   │       └── FighterAnimEvents.cs   footstep event receiver for NPCs
│   └── Editor/
│       ├── MemoryBuilder.cs           the Tools > Beyond Time > Memories menu
│       └── MemoryKit.cs               building blocks used by the memory definitions
├── Integration/
│   └── ShadowScriptBridge.cs          optional: makes Ahmad's ShadowScript cost memory time
├── Memory_01_Alley/
│   ├── README.md                      story, fight design, tuning, placeholders
│   └── Editor/Memory01AlleyDefinition.cs
└── Memory_02_Siege/
    ├── README.md
    └── Editor/Memory02SiegeDefinition.cs

Created by the builder (also commit these):
├── Resources/MemorySettings.asset     shared settings
├── Resources/MemoryUI.prefab          the HUD (spawns itself when a memory starts)
├── Core/Prefabs/Shadow_PLACEHOLDER.prefab
├── Core/Prefabs/Projectile_PLACEHOLDER.prefab
├── Core/Art/                          placeholder materials and post-processing
├── Memory_01_Alley/Memory_01_Alley.prefab  (+ _Test.unity)
├── Memory_02_Siege/Memory_02_Siege.prefab  (+ _Test.unity)
└── Memories_TestWorld.unity           start island → Alley → Siege → final gate
```

Everything is namespaced (`BeyondTime.Memories`). No file outside `Assets/Memories` is changed.

---

## 2. Install and first run

1. **Delete the old folder.** If you installed an earlier version, delete `Assets/Memories` completely in Unity (right-click → Delete). The old memories 01 to 05 and the template are gone in this version, and the alley is now Memory 01.
2. **Copy** the new `Memories` folder into `Assets/`.
3. **Wait** for the compile. The Console must show **zero red errors**.
   - If the only error mentions `ShadowScript`, delete `Assets/Memories/Integration/ShadowScriptBridge.cs`.
4. **Build:** run **Tools → Beyond Time → Memories → Build Everything (1 + 2 + 3)**. It builds the kit, both memory prefabs and the test world, then opens the test world.
5. Press **Play**. Walk forward over the bridge into the alley.

To test one memory on its own:
- **Tools → Beyond Time → Memories → Test One Memory → 01 The Alley**
- **Tools → Beyond Time → Memories → Test One Memory → 02 The Siege**

**Requirements:**
- Unity 6000.0.54f1 with URP, Input System, TextMesh Pro and Starter Assets (all already in the project).
- The player is the Starter Assets third-person armature, tagged `Player`, with a CharacterController. The builder adds the fighting components for you.

---

## 3. How it plays

**Controls:**

| Input | Action |
|---|---|
| WASD, mouse, Shift | Move, camera, sprint |
| Left mouse | Punch combo: jab, cross, hook |
| F | Front kick (breaks guards); F during a combo = roundhouse knockdown |
| Right mouse | Block; press it just before a hit to **parry** |
| Space | Dodge (invulnerable while dodging) |
| Q | Repel: stuns nearby enemies (8 s cooldown) |
| E | Continue story, hold to interact |
| 1, 2, 3 | Choices (time freezes while you choose) |

**The loop:**
1. Walking onto an island starts its memory, and the storyline types out at the bottom of the screen.
2. A timer runs, and **every hit you take costs time**. Falling into lava costs 5 s and sends you back to the last checkpoint.
3. Objectives chain together: fight, reach, hold E, protect, cross hazards.
4. **Winning:** the island's fires go out, a banner shows **REDEEMED** or **STABILIZED / YOU LEFT THEM AGAIN**, and the next bridge rises.
5. **Losing** (the timer hits zero, or someone you protect dies): "THE MEMORY COLLAPSED", and the memory restarts from its entrance. Earlier memories are kept.
6. After the last memory, the final gate gives the ending (Redeemed or Condemned).

---

## 4. The golden rules

1. **Find placeholders by name.** Everything you should swap contains `REPLACE_ME` (search for it in the Hierarchy or the Project window). Groups named `ENV_...` are environment.
2. **Swap the look, keep the logic.** Put your model *next to* or *inside* a `REPLACE_ME` object and delete only the placeholder mesh.
   - Do not delete objects that carry scripts: MemoryZone, Fighter, BrawlerAI, RangedAI, MemoryMover, FireHazard, LavaZone, MemoryCheckpoint, MemoryAbandonExit, BoneAttachment.
   - Do not delete the **Targets**, **SpawnPoints**, **Actors** or **StepObjects** groups.
3. **Keep colliders where the player must stand or be blocked.** Walls need colliders, floors need colliders, and triggers (lava, fire, checkpoints, the door) must stay triggers.
4. **Stop rebuilding once you edit by hand.** *Build All Memory Prefabs* regenerates both prefabs from their definition files and **overwrites hand edits**. The safe workflow:
   - **a.** Run Build Everything once to get the prefabs.
   - **b.** Make a copy for your art pass: duplicate `Memory_02_Siege.prefab` and rename it `Memory_02_Siege_FINAL.prefab` (same for the Alley). Do all asset work on the FINAL copies.
   - **c.** Use the FINAL prefabs in your real world scene. The builder never touches them.
   - **d.** Gameplay text and step changes can go into the definition file (then rebuild and compare), or straight into the FINAL prefab's Memory Zone → Steps.
5. **Humanoid models only for fighters.** Punches and kicks bend humanoid bones. Any model imported with **Rig → Animation Type: Humanoid** works.
6. **Commit `.meta` files** with every asset.

---

## 5. Using your own assets

### 5.1 Choose a workflow

**A. Swap inside the memory prefab.** Best for characters, props and effects. Open the FINAL prefab (double-click), swap models in place, save. Quick and safe.

**B. Drop the memory into your own environment.** Best for Zoha's islands, fortress and alley art. Your environment stays in your world scene, and the memory brings only the gameplay.

1. Open your world scene with your environment already built.
2. Drag the FINAL memory prefab into the scene and place it on the island where it happens.
3. Right-click the instance → **Prefab → Unpack Completely**, so you can move its children freely in this scene.
4. Delete `ART_REPLACE_ME` (the placeholder environment). Keep everything else.
5. Move the gameplay pieces onto your art. Each object keeps its name, so you can find it:

| Memory | Move these to match your environment |
|---|---|
| Alley | `Actors/*` (start positions), `Targets/Patrol*` (lookout routes), `SpawnPoints/*`, `StepObjects/DroppedKnife_REPLACE_ME`, `RespawnPoint` |
| Siege | `Actors/*`; `StepObjects/GATE_REPLACE_ME` (into your gate opening); `LEVER_REPLACE_ME` and `Targets/LeverSpot` (together); `LavaPitTrigger` (cover your pit, top below the walkway); `FireGapA..C` (across your walkway); `Checkpoint_Walkway` and `Targets/WalkwayStart`; `Targets/WalkwayEnd`; `ESCAPE_DOOR_REPLACE_ME`; `DEBRIS_REPLACE_ME` (on your friend); `PressureFire` and `FinalFire` children; `RespawnPoint` |

6. Resize the root **Box Collider** (the start trigger) so it covers the playable area of your island.
7. Put **RespawnPoint** just *outside* that trigger, at the entrance.
8. Re-assign anything the Memory Zone points to only if you deleted it. Normally nothing breaks, because you only moved objects.

Workflow B means the builder no longer manages that memory; it is now part of your scene. That is the intended end state for the final game.

### 5.2 Environment

**Ground and walls**
- Floors must have colliders (a Mesh Collider is fine) so the player and NPCs stand on them. NPCs refuse to step where there is no ground below, so gaps and lava are safe.
- Walls must have colliders. They stop the player and NPCs, and they stop NPCs and the Harrier from seeing or shooting through.
- **Siege:** the fortress must be closed, so there is no way round the gate. The walkway pit must be closed at its sides.
- **Alley:** the alley needs walls on three sides. The victim is kept within 8 m of where he starts (BrawlerAI → Leash Radius); raise it if your alley is bigger.

**Lava**
- The *look* can be anything: a plane with a lava shader, or a particle field.
- The *function* is a separate **trigger** with **Lava Zone** (see `LavaPitTrigger` in the Siege and `LAVA_REPLACE_ME` in the test world).
- Keep its top below the surfaces the player walks on, and above the bottom of the pit.

**Walkway (Siege)**
- Your bridge can be any shape.
- Keep it narrow (2 to 3 m), so the fire timing and the Pursuers matter.
- Put the three **FireGap** triggers across it: Box Collider width = bridge width, depth about 1.2 m, height about 2 m.
- Keep some cover between the walkway and the Harrier, for example low walls 1.2 to 1.5 m tall.

**Balcony (Siege)**
- The Harrier stands on any platform with a collider.
- The player must be able to walk up to it (stairs or a ramp, slope under 45°).
- Place the Harrier where it can see most of the walkway.

**Lighting and mood**
- Keep the island's **FIRE_REPLACE_ME** children. They go dark when the memory is redeemed, which is a big part of the payoff.
- The Alley has a flickering street light (`ART_REPLACE_ME/StreetLight_REPLACE_ME`); move it if you replace the art.

**NavMesh (optional, recommended for complex layouts)**

NPCs move with their CharacterController and avoid walls with simple steering, which works without a NavMesh. For smarter paths around complex geometry:
1. Add a **NavMesh Surface** to your world (AI Navigation package).
2. Bake.
3. Add a **NavMeshAgent** to each NPC root. They use it automatically for paths.

Hidden bridges must be **active while you bake**, then switched back off.

### 5.3 Your player character

**Option 1: keep the Starter Assets controller, change the model** (easiest)
1. Import your character FBX with Rig → **Humanoid**.
2. In the scene, select `PlayerArmature`, then its child `Geometry`. Replace the mesh with your character's mesh, or add your model as a child of `PlayerArmature` and delete the old geometry.
3. On `PlayerArmature`'s **Animator**, set **Avatar** to your model's avatar.
4. Keep the controller `StarterAssetsThirdPerson`.

**Option 2: your own player controller**
1. Your player needs: tag **Player**, a **CharacterController**, and an **Animator** with a humanoid avatar.
2. Select it and run **Tools → Beyond Time → Memories → Add Fighting To Selected Player**. This adds Fighter (Player preset), FighterPose, PlayerBrawler and a repel flash light.
3. PlayerBrawler freezes Starter Assets movement during attacks. With your own controller, disable your movement script while any of these is true: `Fighter.IsAttacking`, `IsDodging`, `IsStaggered`, `IsBlocking`.

Remove **MemoryTestCombat** and Ahmad's **LightPulse** from the player if present (they also use Q).

### 5.4 Human NPCs (the victim, the attacker, the lookouts, the trapped friend)

The fighting NPCs are already real animated humanoids (the Starter Assets body with idle, walk and run), tinted per role. To use your own characters, do this for each `Actors/<Person>/MODEL_REPLACE_ME`:

1. Import your character with Rig → **Humanoid**.
2. Drag the model onto the person object, so it becomes a child. Set its position and rotation to 0, scale 1.
3. On your model's **Animator**:
   - **Controller:** `StarterAssetsThirdPerson`, or your own controller (5.6).
   - **Apply Root Motion:** off.
   - **Culling Mode:** Always Animate.
4. If your animations contain footstep events, add **Fighter Anim Events** to the model.
5. Delete `MODEL_REPLACE_ME`.
6. Nothing else to do for held items: the Alley attacker's knife (`KNIFE_REPLACE_ME`) and the Harrier's `HAND_GLOW` find the new right hand on their own (BoneAttachment).
7. If your character is much taller or shorter, adjust the **CharacterController** height (about your height) and centre (half the height), then move the **HealthBar** object above the head.

The trapped friend (Siege) is a **MemoryActor**, not a fighter. Put your model under it the same way, give it a lying or trapped animation, and type the state name into Memory Actor → **Idle State**.

### 5.5 Shadow enemies (Pursuers, Gatekeeper, Harrier)

They are the same animated humanoid in a black material with a dark glow and two glowing eye spheres.

**Swap the model:** same steps as 5.4. Delete the **EyeL** and **EyeR** spheres if your model has its own eyes.

**Keep it reading as a shadow:**
- Use a dark material with a little emission. A URP Lit material with Base Colour near black and Emission dark red is the current look.
- Add a dark smoke or particle trail under the model if you have one.

**Size:** the Gatekeeper's root is scaled to 1.55. Keep the root scale, or set it to 1 and use a naturally big model. If you change the size, also change these on each move in its **Fighter → Combo**:
- **Reach**: distance to the hit sphere (about 1.1 × the arm length).
- **Radius**: size of the hit sphere.
- **Height**: how high the hit lands.

Select the root and watch the red wire sphere while the game runs.

**Telegraph lights:**
- The Gatekeeper's `TelegraphLight` glows during windups; move it to its chest, weapon or eyes.
- The Harrier's `HAND_GLOW` sits in its right hand.
- You can add particle effects as children. Keep a Light on the object, or assign your own Light in Fighter → **Telegraph Light**.

**Weapons:** give the Gatekeeper a club or the Pursuers claws by adding a model with **BoneAttachment** (bone RightHand) under the root. It is cosmetic; damage comes from the move settings.

**The placeholder shadow** (`Core/Prefabs/Shadow_PLACEHOLDER.prefab`) is only used by wrong-choice "extra shadows" in the Alley. To use a real enemy there:
1. Drag your enemy prefab into `Resources/MemorySettings.asset → Shadow Prefab`.
2. Your enemy must be destroyed when it dies.
3. Its hits must call `BeyondTime.Memories.MemoryEvents.RaisePlayerHit(seconds)`. Ahmad's ShadowScript does this through **ShadowScriptBridge**.

### 5.6 Animations (Mixamo or your own)

Punches, kicks, slams, sweeps, casts, guard, block, flinch and cower already work with **no clips**: FighterPose bends the real bones every frame. Real clips look better, and you can add them gradually.

**Locomotion (idle, walk, run):**
- NPCs drive a float **Speed** (0 idle, 2 walk, 6 run), set **Grounded** = true and **MotionSpeed** = 1. The Starter controller already uses these.
- In your own controller, make a blend tree on **Speed** with idle at 0, walk at 2 and run at 6.

**Attacks, hits and knockdowns:**
1. Download the clips. Useful Mixamo names: "Punching", "Hook Punch", "Mma Kick", "Stabbing", "Great Sword Slash", "Throw", "Hit Reaction", "Falling Back Death".
2. Import each with Rig → **Humanoid**, Avatar → Copy From Other Avatar (your model).
3. Duplicate `StarterAssetsThirdPerson.controller` and assign the copy to your model's Animator.
4. Drag each clip into the Base Layer as a **state**, and give it a clear name (for example `Jab`, `Cross`, `Hook`, `FrontKick`, `Slam`, `Sweep`, `Cast`, `Hit`, `Knockdown`).
5. Add a transition from each attack state back to **Idle Walk Run Blend**, with **Has Exit Time** on. No parameters are needed; the scripts CrossFade into states by name.
6. On the character's **Fighter**:
   - For each move in **Combo**, **Kick** and **Kick Finisher**, type the state name into **Anim State**.
   - Fill **Hit State** and **Knockdown State** the same way.
7. Match each move's **Duration** to its clip length. Set **Hit Start** and **Hit End** to the part of the clip where the fist or foot connects (0 to 1, as a fraction of the duration).

A move with an Anim State plays the clip, and the procedural pose stays out of the way. Moves without one keep using the procedural pose, so you can mix them.

**Player attacks:** do the same on the player's Fighter, with a duplicate of the Starter controller that has the attack states added.

**Harrier throw:** Ranged AI → **Cast Move** → Anim State.

### 5.7 Props that move: gate, lever, rubble, doors, knife

**MemoryMover** objects are pivots. They move by **Open Offset** (metres) and rotate by **Open Euler** (degrees) over **Duration**, and reset when the memory restarts.

- **Gate:**
  - Put your gate model under `GATE_REPLACE_ME`, centred on the opening.
  - Keep a collider that blocks the opening while closed: the invisible **Blocker**, or your model's own collider.
  - Set Open Offset to lift it clear (for example 0, 4.6, 0), or slide it sideways (4.5, 0, 0).
- **Lever:**
  - The pivot is the hinge. Put your lever under it so the hinge sits at the pivot's origin.
  - Set the closed angle on the pivot's own rotation, and the swing in Open Euler.
  - Move **Targets/LeverSpot** next to it; that is where the player stands to hold E.
- **Rubble on the friend:** put your rubble under `DEBRIS_REPLACE_ME`. Open Offset and Open Euler slide and tip it away when the rescue completes.
- **Escape door (Siege):**
  - Any doorway model under `ESCAPE_DOOR_REPLACE_ME`.
  - Keep its **trigger box** and **Memory Abandon Exit**.
  - Make it inviting: daylight, a cool colour, a soft sound.
- **Knife (Alley):**
  - Put your knife model under `KNIFE_REPLACE_ME` and delete Blade and Grip. Keep BoneAttachment and TelegraphLight.
  - If the grip looks wrong, change BoneAttachment → **Local Position** and **Local Euler** while the game runs, copy the values, stop, and paste them back.
  - The dropped knife on the ground is `StepObjects/DroppedKnife_REPLACE_ME`.

### 5.8 Effects: fire, lava, projectiles, glows

| Effect | Object | How to replace |
|---|---|---|
| Timed fire across the walkway | `FireGapA..C_REPLACE_ME_FX` | Put your fire burst particles under **Flames** and small embers under **Warning**, then delete the spheres. Flames switch on while burning; Warning switches on 0.6 s before. |
| Island fires (go out on success) | `FIRE_REPLACE_ME/*` | Replace each `Fire_REPLACE_ME` with your fire. Anything inside `FIRE_REPLACE_ME` is switched off on success. |
| Spreading hellfire at 30 s and 10 s | `StepObjects/PressureFire`, `FinalFire` (Siege) | Put your fire effects inside; the groups appear on their own. Add more groups in Memory Zone → **Pressure Objects / Final Objects**. |
| Lava look | `LavaPit_REPLACE_ME` (Siege), `LAVA_REPLACE_ME` (test world) | Any lava mesh or shader. The gameplay trigger is separate. |
| Harrier projectile | `Core/Prefabs/Projectile_PLACEHOLDER.prefab`, or Ranged AI → **Projectile Prefab** | Any model or particles, facing +Z. Its colliders are switched off automatically; hits use a sphere cast (the radius is on MemoryProjectile). Keep or add a Light for the impact flash. |
| Telegraph glows | Gatekeeper `TelegraphLight`, Harrier `HAND_GLOW`, Alley knife `TelegraphLight` | Keep a Light (it is switched on and off) and add particles as children. |
| Story-moment light | Alley `StreetLight_REPLACE_ME` | Any light. MemoryFireFlicker makes it stutter. |
| Post-processing | `Core/Art/HellfireVolume.asset` (test world) | Use your own Global Volume in the real world scene. |

### 5.9 Audio

All optional. Drop clips into these fields:

| Sound | Where |
|---|---|
| Punch and kick impacts, blocks, whooshes | Each Fighter → **Hit Sounds**, **Block Sound**, **Whoosh Sound** (the player's too) |
| NPC footsteps | Model → **Fighter Anim Events** → Footsteps |
| Gate rising, lever, rubble shifting | MemoryMover → **Sound** |
| Collectible pickup | MemoryCollectible → Pickup Sound |
| Lava sizzle | Lava Zone → Sizzle Sound |
| Bridge rising | Bridge Reveal → Rise Sound |
| Fortress or alley ambience | Add an Audio Source (Loop, Play On Awake) to the memory root |
| Friend calling for help (Siege) | Add a 3D Audio Source (Loop, Spatial Blend 1, Max Distance 40) to `Actors/YourFriend` |
| Music | One looping Audio Source in the world scene |

### 5.10 UI and health bars

**HUD:** `Assets/Memories/Resources/MemoryUI.prefab`
- Open it and restyle anything: fonts (create a TMP font asset), colours, the story panel background, button sprites, the marker icon.
- Keep the object names, and keep every field on the **Memory UI** component filled.
- **Story text speed:** `Resources/MemorySettings.asset → Typewriter Chars Per Second`.

**Floating health bars:** each fighter has a `HealthBar` child (a world-space canvas).
- Restyle colours, the name text and bar sizes on the prefab.
- Raise or lower it with **FighterHealthBar → Height**.
- **Always Visible** is on for people you must protect.

### 5.11 Building the real hellfire world scene

The test world (`Memories_TestWorld.unity`) shows the layout. To build the real one:

1. **Islands:** build or place your islands over a lava sea.
2. **Lava:** your lava visual, plus one large **trigger** collider with **Lava Zone** under everything. Set Fallback Respawn to the start point.
3. **Memories:** place each FINAL memory prefab on its island with the entrance facing the bridge you arrive on, or use workflow B.
4. **Bridges:** for each bridge that should rise after a memory:
   - **a.** Add **Bridge Reveal**.
   - **b.** Set the bridge **inactive**.
   - **c.** Drag it into the previous memory's Memory Zone → **Unlock On Complete**.
5. **Checkpoints:** at the start of each island, add a trigger with **Memory Checkpoint**. Set its Spawn Point to that memory's RespawnPoint.
6. **Final gate:** a trigger with **Memory Final Gate**. Its Gate Visual starts hidden. Set **Scene To Load** to your end scene.
7. **HUD:** drag `Resources/MemoryUI.prefab` in, or let the first memory spawn it.
8. **Player:** your player, tagged Player, with the fighting components (5.3).
9. **Timer:** do not put Maryam's GameManager in this scene. Each memory runs its own timer, so hits would count twice. If the team keeps both, remove ShadowScriptBridge.
10. **Saving:** for the final build, set `Resources/MemorySettings.asset → Save Progress Between Sessions` to on. Leave it off while developing.

### 5.12 Grave video, menus and the build

1. **Grave video scene:** a new scene with a Camera, plus one object with:
   - **Video Player**: Render Mode Camera Far Plane, Camera = your camera, Video Clip = the grave video, Loop off.
   - **Intro Video**: Next Scene = your world scene name.

   Holding Space, E or Esc for 1 second skips the video.
2. **Build Profiles order:** MainMenu → GraveIntro → your hellfire world → EndScreen.
3. **Front-end pack:** set MainMenu's FlowLoader → **First Scene** to `GraveIntro`. The end scene can read `PlayerPrefs.GetString("BeyondTime_Ending")` (Redeemed or Condemned) to show the right text.

---

## 6. Tuning gameplay

Everything is in the Inspector. The common settings:

| Want | Change |
|---|---|
| Easier or harder overall | Memory Zone → **Time Limit**; player Fighter → **Seconds Per Damage** (time lost per damage point, default 0.25) |
| Enemies hit harder or softer | Fighter → Combo → each move's **Damage** |
| Enemies attack more or less often | BrawlerAI → **Aggression**, **Min/Max Attack Gap** |
| Enemies block or dodge more | BrawlerAI → **Block Chance**, **Dodge Chance** |
| Enemies spot you sooner | BrawlerAI → **Sight Range**, **Sight Angle**, **Hearing Range** |
| Enemies tougher | Fighter → **Max Health**, **Max Poise** (stagger resistance), **Super Armor** |
| Bigger parry window | Player Fighter → **Parry Window** (0.22 s) |
| Longer dodge | Player Fighter → **Dodge Distance / Duration / Cooldown** |
| Story speed | MemorySettings → Typewriter Chars Per Second; Memory Zone → **Narrate Auto Advance** |

Right-click any Fighter's header to apply a **Preset**: Player, Thug, Killer, Civilian, Pursuer, Gatekeeper or Harrier. The presets live in `FighterPresets` inside Fighter.cs.

**Step types** (Memory Zone → Steps):

| Type | Player does | Key fields |
|---|---|---|
| Narrate | Reads (E to continue; the timer waits) | story |
| ReachPoint | Walks to a target | target, radius |
| HoldInteract | Holds E near a target; any hit resets it | target, radius, holdTime, holdText |
| DefeatShadows | Kills placeholder shadows | shadowsOnStart |
| DefeatFighters | Knocks out listed NPC fighters; an optional protected person is shown as % | fighters, protect, engageOnStart |
| SurviveTime | Survives X seconds, optionally inside a radius | surviveSeconds, mustStayNearTarget |
| Choice | Picks 1 to 3 while time is frozen; the default fires on timeout | choicePrompt, choiceTimeout, defaultChoice, choices |
| Escort | Leads a MemoryNPC to a target | escortNpc, target |
| Collect | Picks up every collectible under a root | collectRoot |

**When any step starts**, it can:
- **Activate** or **Deactivate** objects
- engage enemies: **Engage On Start** (melee NPCs) or **Ranged On Start** (shooters)
- **Open On Start** (MemoryMovers)
- play an **Actor State**
- add **Bonus Time**
- jump with **Next Step**, or end with **End Memory After**

**Choice fields:** label, result text, righteous (right or wrong), time penalty, extra shadows, go to step, end memory, and optionally a person who gets hurt by the choice (hurt fighter and hurt amount).

**Zone extras:**
- **Persistent Marker**: shown when a step has no target of its own.
- **Pressure Objects / Final Objects**: switched on at 30 s and at 10 s.
- **Unlock On Complete / Disable On Complete**.

---

## 7. Making a new memory

1. Duplicate the folder `Memory_02_Siege` and rename it, for example `Memory_03_Pride`.
2. Rename `Editor/Memory02SiegeDefinition.cs` to `Memory03PrideDefinition.cs`, and rename the class inside to match.
3. In the file, change **Id** (must be unique), **Folder**, **Title**, **Radius** and **TimeLimit**. Also change the MenuItem path text so it does not clash.
4. Replace the layout and steps. The building blocks available:
   - **Environment:** `k.Section`, `k.Block`, `k.Prop`, `k.Ramp`, `k.Fire`, `k.FireGroup`, `k.FireHazardAt`
   - **Moving parts:** `k.Mover`
   - **Places:** `k.Point`, `k.SpawnSet`
   - **People:** `k.Actor`, `k.Brawler`, `k.ShadowBrawler`, `k.Harrier`, `k.Knife`
   - **Steps:** `k.Narrate`, `k.Reach`, `k.Hold`, `k.DefeatFighters`, `k.Defeat`, `k.Survive`, `k.CollectStep`, `k.Choice` (with `MemoryKit.Right` and `MemoryKit.Wrong`)
5. Comment each step's index, as the two existing files do, so `goToStep` and `nextStep` stay correct.
6. Run **Build All Memory Prefabs**, then **Build Test World** or your own Test One Memory menu item.

---

## 8. Team and Git rules

- Work on your own branch. Commit everything under `Assets/Memories`, including `.meta` files and the generated prefabs.
- The builders never touch Build Profiles or anyone else's scenes.
- One person owns the real world scene. Everyone else delivers FINAL prefabs.
- If two people need to edit the same memory, agree who owns its FINAL prefab.

---

## 9. Playtest checklist

**Alley**
- [ ] The man cowers at the start, and after "Step in" the attacker chases him.
- [ ] Hitting the attacker turns him on you; backing off sends him back to the man.
- [ ] The lookouts join the fight.
- [ ] The knife stab glows red and can only be dodged.
- [ ] Parry works on punches.
- [ ] Pinning the attacker shows the knife on the ground, and all three endings work.
- [ ] If the man dies, the memory collapses and restarts cleanly.

**Siege**
- [ ] The timer runs from the first second, and the marker points at the friend.
- [ ] Two Pursuers charge you.
- [ ] The Gatekeeper rises, glows before each attack and ignores flinching. The Sweep can be parried; the Slam knocks you down.
- [ ] The lever opens the gate.
- [ ] Fire bursts in a rolling rhythm with embers as a warning, and pushes you along the walkway, not off it.
- [ ] The Harrier fires only with line of sight, and the low walls block its orbs.
- [ ] You can climb the ramp and knock the Harrier out.
- [ ] Falling into the pit costs 5 s and returns you to the gate.
- [ ] At the rescue, hits reset the hold bar.
- [ ] The escape door ends the memory with "YOU LEFT THEM AGAIN".
- [ ] Fire spreads at 30 s and again at 10 s.

**Both**
- [ ] The timer changes colour.
- [ ] "-Xs" and the red flash show when you are hit.
- [ ] A bridge rises after each memory.
- [ ] The final gate gives an ending.

---

## 10. Troubleshooting

| Problem | Fix |
|---|---|
| No Tools → Beyond Time → Memories menu | Compile errors; check the Console. Editor scripts must stay in folders named `Editor`. |
| Error about `ShadowScript` | Delete `Integration/ShadowScriptBridge.cs`. |
| Errors about old memories (Anger, Betrayal, Greed, Neglect, Template, Murder) | You copied over an old install. Delete `Assets/Memories` completely and copy again. |
| NPCs are capsules | The Starter Assets armature was not found. Check that `Assets/Starter Assets/Runtime/ThirdPersonController/Character/Models/Armature.fbx` exists, then rebuild. |
| NPCs slide without a walking animation | Their Animator needs a controller with a `Speed` float (5.6). |
| Punch poses look twisted on my model | The model must be Humanoid. For any move that still looks wrong, add a real clip (5.6). |
| Arms snap oddly during attacks | Lower FighterPose → **Stance Weight**, or give that move an Anim State. |
| Enemies do not notice me | Raise Sight Range and Hearing Range, and check no walls are in the way. The player must have a Fighter (team Player). |
| The Gatekeeper never appears | Step 1 activates it. Check `Actors/TheGatekeeper` is in that step's Activate list and has not been deleted. |
| The Harrier never shoots | It needs line of sight from its eyes to your chest, and it must be in the walkway step's Ranged On Start. |
| Fire does not hurt me | The FireGap box must be a trigger and cover the walkway, and the player must be tagged Player. |
| Falling into the pit does nothing | LavaPitTrigger must be a trigger, with its top below the deck but above the pit floor. |
| The memory restarts by itself after I fail | That is Auto Retry (Memory Zone). Untick it if the player should walk back in instead. |
| My prefab edits vanished | You re-ran Build All Memory Prefabs. Work on FINAL copies (section 4). |
| A finished memory is already done when I press Play | Save Progress is on. Turn it off in MemorySettings, or run Tools → Beyond Time → Memories → Reset Saved Memory Progress. |
| Hits count twice | Maryam's GameManager is in the same scene as ShadowScriptBridge. Keep only one timer system. |
