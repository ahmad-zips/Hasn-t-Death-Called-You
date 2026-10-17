# Beyond Time: Memories

A complete, self-contained memory system for the hellfire world, with full melee combat. You get five playable memories, a template for more, and a test world: lava, islands, bridges that rise when a memory is finished, and a final gate with two endings.

Every memory shows its storyline at the bottom of the screen, has choices and fights shadows. Every visual is a clearly named placeholder you can swap.

```
Grave video → Hellfire world
  Start island → bridge → Memory 1 island → (bridge rises) → Memory 2 → ... → Final gate
```

---

## What is in the folder

```
Assets/Memories/
├── README.md                     you are here
├── Core/
│   ├── Scripts/                  the engine (runtime)
│   │   ├── MemoryZone.cs         runs one memory: steps, timer, choices, win and lose
│   │   ├── MemoryUI.cs           story at the bottom, objective, timer, choices, hold bar, banner, marker
│   │   ├── MemoryActor.cs        a person in a memory (swap the model here)
│   │   ├── MemoryNPC.cs          a person you escort and protect
│   │   ├── MemoryCollectible.cs  pickups (letter pieces and so on)
│   │   ├── MemoryShadow.cs       PLACEHOLDER enemy (swap in MemorySettings)
│   │   ├── MemorySettings.cs     one asset to swap the shadow enemy for every memory
│   │   ├── MemoryProgress.cs     which memories are done, redeemed or condemned
│   │   ├── MemoryEvents.cs       "player was hit" event any enemy can raise
│   │   ├── LavaZone.cs           fall in: back to checkpoint, lose time
│   │   ├── MemoryCheckpoint.cs   respawn points
│   │   ├── BridgeReveal.cs       bridge rises out of the lava when unlocked
│   │   ├── MemoryFinalGate.cs    opens when all memories are done; gives the ending
│   │   ├── IntroVideo.cs         plays the grave video, then loads the world
│   │   ├── MemoryTestCombat.cs   old test combat (no longer used; PlayerBrawler replaces it)
│   │   ├── MemoryFireFlicker.cs  fire light flicker
│   │   └── Combat/
│   │       ├── Fighter.cs        health, guard, poise, combos, kicks, block, parry, dodge, knockdown, presets
│   │       ├── FighterPose.cs    procedural punches, kicks, knife, guard, block, flinch, cower on any humanoid
│   │       ├── BrawlerAI.cs      NPC brain: patrol, spot, chase, circle, combo, block, dodge, flee, cower, murder
│   │       ├── PlayerBrawler.cs  player controls on top of Starter Assets
│   │       ├── FighterHealthBar.cs floating name and health bar
│   │       ├── MemoryProtectee.cs  memory fails if this person dies
│   │       ├── BoneAttachment.cs   puts a knife (or anything) in a hand bone
│   │       └── FighterAnimEvents.cs footstep event receiver for NPCs
│   └── Editor/
│       ├── MemoryBuilder.cs      Tools > Beyond Time > Memories menu
│       └── MemoryKit.cs          building blocks the memory definitions use
├── Integration/
│   └── ShadowScriptBridge.cs     optional: makes Ahmad's ShadowScript cost memory time
├── Memory_00_Template/           copy this folder to make a new memory
├── Memory_01_Anger/              The Moment of Anger (tutorial)
├── Memory_02_Betrayal/           The Secret You Sold
├── Memory_03_Greed/              The Wallet
└── Memory_04_Neglect/            The Unanswered Call
     (each has Editor/MemoryXXDefinition.cs + README.md; the builder adds MemoryXX.prefab)

Created by the builder:
├── Resources/MemorySettings.asset    swap the shadow enemy here
├── Resources/MemoryUI.prefab         the HUD (auto-spawned by any memory)
├── Core/Prefabs/Shadow_PLACEHOLDER.prefab
├── Core/Art/                         placeholder materials
└── Memories_TestWorld.unity          the playable test world
```

Everything is namespaced (`BeyondTime.Memories`) and lives in its own folder, so it cannot clash with teammates' scripts. The only file that references teammates' code is `Integration/ShadowScriptBridge.cs`; delete it if your branch has no `ShadowScript`.

---

## 1. Install and play in 5 minutes

- [ ] Branch: `git checkout -b feature/memories origin/GamePlay2` (or your team's latest branch).
- [ ] Copy the `Memories` folder into `Assets/`, so you have `Assets/Memories/Core/...`.
- [ ] Wait for compile. Console: **zero red errors**.
  - If the only error mentions `ShadowScript`, delete `Assets/Memories/Integration/ShadowScriptBridge.cs`.
- [ ] Menu: **Tools → Beyond Time → Memories → Build Everything (1 + 2 + 3)**.
- [ ] It opens `Assets/Memories/Memories_TestWorld.unity`. Press **Play**.
- [ ] Walk forward over the first bridge onto Memory 1's island. The memory starts on its own.

**Controls** (the builders give the test player the full fighting kit):

| Key | Action |
|---|---|
| WASD, Mouse, Shift | Move, look, sprint (Starter Assets) |
| Left mouse | Punch combo: jab, cross, hook |
| F | Front kick (guard breaker); F during a combo = roundhouse knockdown |
| Right mouse | Block; tap just before a hit to parry |
| Space | Dodge (invulnerable) |
| Q | Repel: stuns everything nearby |
| E | Continue story, hold to interact |
| 1, 2, 3 | Pick a choice (mouse also works) |

To test only the new brawl memory: **Tools → Beyond Time → Memories → Test One Memory → 05 The Alley**.

---|---|
| WASD, Mouse, Shift | Move, look, sprint (Starter Assets) |
| Left mouse | Strike (MemoryTestCombat) |
| Q | Stun nearby shadows |
| E | Continue story, hold to interact |
| 1, 2, 3 | Pick a choice (mouse also works) |

---

## 2. How a memory plays (and why it is worth playing)

Each memory is one island. Walking onto it starts the memory.

1. **Story first.** Short lines type out at the bottom of the screen. Press E to speed them up. The timer waits while you read.
2. **The timer starts** (120 to 150 seconds). Every shadow hit takes 3 seconds off. Falling into lava takes 5 seconds and sends you back to the entrance.
3. **Objectives chain together.** A different mix in each memory: fight, reach someone, hold E while shadows try to break your focus, collect pieces, escort a person who can die, or hold your ground in a circle of light.
4. **The choice.** Time freezes and the screen turns cold blue ("TIME STANDS STILL"). You pick 1, 2 or 3 before the decision bar empties. **If you hesitate, the default is the original wrong.**
   - Right choice: the memory is **REDEEMED**, the fight ends.
   - Wrong choice: **the wrong repeats**. You lose time, more shadows rise, and an extra fight or survival phase follows. If you get through it, the memory is **STABILIZED** but marked condemned.
5. **Win.** The island's fires go out, a banner shows your result, and the next bridge rises out of the lava.
6. **Lose.** If the timer hits zero or the person you escort dies, "THE MEMORY COLLAPSED" shows and the memory restarts from its entrance. You never lose earlier memories.
7. **The ending.** The final gate opens when every memory is done. Redeem 3 of 4 for the good ending ("REDEEMED"), otherwise "CONDEMNED". The result is saved in PlayerPrefs `BeyondTime_Ending` for the end screen.

What keeps it interesting:
- Every memory adds a new mechanic: fight, then collect and escort, then a branching path with an escape, then survival.
- Choices have real cost in time and danger, not only text, and the default punishes indecision.
- Being hit breaks your hold E, so fighting matters even when the goal is not combat.
- The two endings give a reason to replay and choose differently.

---

## 3. The four memories at a glance

| # | Memory | New mechanic | Steps | Time |
|---|---|---|---|---|
| 1 | The Moment of Anger | Basics: fight, reach, hold E, choice | Narrate ×2 → Defeat 2 → Reach brother (2 more) → Hold E 3 s → Choice (3 options, 10 s) → wrong path: Defeat | 120 s |
| 2 | The Secret You Sold | Collect under guard, escort a person who can die | Narrate ×2 → Collect 3 letter pieces (3 guards) → Reach friend → Escort her to the light (3 shadows hunt her) → Choice (2 options) → wrong path: Survive 15 s | 150 s |
| 3 | The Wallet | Branching paths and a fire escape | Narrate ×2 → Reach wallet (3 guards) → Hold E → Choice: return it (walk to the old man, hold E) or keep it (hard fight) → both: Escape through fire (+10 s) | 140 s |
| 4 | The Unanswered Call | Hold your ground; leaving the light pauses progress | Narrate ×2 → Survive 25 s inside the phone's light (shadows keep coming) → Reach door → Hold E → Choice (3 options) → wrong path: Defeat | 150 s |

| 5 | The Alley | Full brawl with real NPCs: stop a knife attack, protect the victim, fight two lookouts, parry, dodge the stab, pin the attacker | Narrate ×2 → Choice (step in or look away, 5 s) → Defeat the attacker while protecting the man → Defeat 2 lookouts → Hold E to pin → Choice (3 options) → wrong path: shadows | 180 s |

Each memory's own README has the full script and its placeholder list.

---

## 4. Replacing placeholders with your real assets

Placeholders are named so you can find them: search the Hierarchy or Project for `REPLACE_ME`.

### 4.1 Shadows (one place for every memory)
- [ ] Select `Assets/Memories/Resources/MemorySettings.asset`.
- [ ] Drag your real enemy prefab (for example Ahmad's `Assets/Prefabs/ShadowChaser.prefab`) into **Shadow Prefab**.
- [ ] That is it. Every memory now spawns your enemy.

What your enemy needs:
1. It chases the player by itself (ShadowChaser already does).
2. **It is destroyed when it dies.** Memories count living enemies to know when a fight is won. Ahmad's `EnemyHealth` already destroys the object.
3. When it hits the player, the memory timer should drop. With Ahmad's `ShadowScript`: put **ShadowScriptBridge** on any object in the world scene (for example the MemoryUI). With your own script, call `BeyondTime.Memories.MemoryEvents.RaisePlayerHit(3f);` in its attack.
4. If it uses a NavMeshAgent, bake a NavMesh on the world (see 4.6).

One memory can use a different enemy: select its root → Memory Zone → **Shadow Prefab Override**.

### 4.2 People (brother, friend, old man, mother)
Each person is a `MemoryActor` with a child named `MODEL_REPLACE_ME`.
- [ ] Open the memory prefab (double-click `Memory_0X_Name.prefab`).
- [ ] Find `Actors/<Person>/MODEL_REPLACE_ME`.
- [ ] Drag your character model onto the person object (so it becomes a sibling of `MODEL_REPLACE_ME`).
- [ ] Line it up (feet at y = 0, facing the same way), then delete `MODEL_REPLACE_ME`.
- [ ] Animations: the Animator is found automatically. Put an idle state name into Memory Actor → **Idle State**. For per-step animations, open the Memory Zone steps and type a state name into **Actor State** on the step (for example a kneel or crying state).
- [ ] For the escorted friend (Memory 2), also fill Memory NPC → **Walk State**, **Idle State**, **Hurt State**.

### 4.3 Pickups, wallet, phone, door, lantern
Same pattern: put your model next to `MODEL_REPLACE_ME` (or inside the `*_REPLACE_ME` object) and delete the placeholder mesh. **Keep the colliders and scripts** on the parent objects.

### 4.4 Island art and fire
- `ART_REPLACE_ME` inside each memory holds the ground disc and props. Replace freely. Keep a walkable surface with colliders where the old ground was, and keep the **Targets** and **SpawnPoints** objects in sensible places.
- `FIRE_REPLACE_ME` holds the island fires. Replace them with your real fire particles. Anything left inside goes out automatically when the memory is completed.
- Fire walls used in escapes (Memory 3, `StepObjects/EscapeFire`) start hidden and are switched on by the escape step.

### 4.5 UI look
Open `Assets/Memories/Resources/MemoryUI.prefab` and restyle it: fonts, colours, sprites, sizes. Keep the references in the **Memory UI** component filled, and keep the object names.

### 4.6 The real hellfire world (lava, bridges, islands)
The test world is a template for the layout. In your real world scene:
- [ ] Drag memory prefabs onto your islands. Their entrance faces **-Z** (rotate the prefab so the entrance faces the bridge you arrive on).
- [ ] Lava: any surface with a **trigger** collider and the **Lava Zone** script. Set **Fallback Respawn** to your start point.
- [ ] Put a **Memory Checkpoint** (trigger) at the start of each island. Set its Spawn Point to that memory's `RespawnPoint`.
- [ ] Bridges you want hidden: add **Bridge Reveal**, set them **inactive**, and drag each into the previous memory's **Unlock On Complete** list.
- [ ] Final gate: a trigger with **Memory Final Gate**; its Gate Visual starts hidden. Set **Scene To Load** to your end scene (`EndScreen` from the front-end pack).
- [ ] Drop `Resources/MemoryUI.prefab` in, or let the first memory spawn it.
- [ ] Remove **Memory Test Combat** from the player once the real PlayerCombat is on it.
- [ ] **NavMesh** (needed by ShadowChaser): add a NavMesh Surface, then **temporarily activate every hidden bridge**, bake, and set the bridges inactive again. The baked data keeps the bridge paths.
- [ ] Do not put Maryam's `GameManager` in the same scene as memories; each memory runs its own timer. If the team wants both, remove `ShadowScriptBridge` so hits are not counted twice.

### 4.7 Intro video
- [ ] New scene `GraveIntro`: Camera, plus an object with **Video Player** (Render Mode Camera Far Plane, drag the camera in, Video Clip = grave video, Loop off) and **Intro Video** (Next Scene = your world scene name).
- [ ] Build Profiles order: MainMenu → GraveIntro → your world → EndScreen.
- [ ] If you use the front-end pack, set MainMenu's FlowLoader **First Scene** to `GraveIntro`. The Grave and Briefing scenes are optional now, since each memory has its own story intro.

---

## 5. Editing a memory

Two ways:

**A. In the Inspector (fast tweaks).** Open the memory prefab → root → Memory Zone → **Steps**. Each step shows its type, story, objective and numbers. You can change text, timer, shadow counts and hold times. Choices are inside the Choice step.
**Warning:** running *Build All Memory Prefabs* again overwrites the prefab from its definition file. Once you start changing prefabs by hand, do not rebuild that memory, or copy your edits into the definition first.

**B. In the definition file (recommended for story changes).** Open `Memory_0X_Name/Editor/Memory0XNameDefinition.cs`. Every line of story and every step is there in plain order. Edit, save, then **Tools → Beyond Time → Memories → 2. Build All Memory Prefabs**.

**Step types:**

| Type | What the player does | Key fields |
|---|---|---|
| Narrate | Reads; E to continue | story |
| ReachPoint | Walks to the target | target, radius, shadowsOnStart |
| HoldInteract | Holds E near the target; a hit resets progress | target, radius, holdTime, holdText |
| DefeatShadows | Kills everything alive | shadowsOnStart |
| SurviveTime | Survives X seconds; optionally must stay near the target | surviveSeconds, mustStayNearTarget, keepShadowsAlive |
| Choice | Picks 1 to 3 while time is frozen | choicePrompt, choiceTimeout, defaultChoice, choices |
| Escort | Leads a MemoryNPC to the target; shadows near it hurt it | escortNpc, target, radius |
| Collect | Picks up every MemoryCollectible under the root | collectRoot |
| DefeatFighters | Knocks out every NPC Fighter in the list; optionally protects someone (shown as %) | fighters, protect, engageOnStart |

Every step can also, when it starts, **activate** or **deactivate** objects, play an **actor state**, add **bonus time**, and jump with **nextStep** or end the memory with **endMemoryAfter**.

Each choice has: label, result text, righteous (right or wrong), time penalty, extra shadows, go to step (-1 = next), end memory, and optionally a person who gets hurt by the choice (hurt fighter and hurt amount).

---

## 6. Making a new memory (10 minutes)

- [ ] Duplicate `Memory_00_Template`, rename it `Memory_05_Pride`.
- [ ] Rename the file inside `Editor/` to `Memory05PrideDefinition.cs`, and the class name inside to match.
- [ ] Change **Id** (must be unique), **Folder** and **Title**.
- [ ] Lay out props and people with `k.Prop`, `k.Actor`, `k.Npc`, `k.Collectibles`, `k.Fire`, `k.FireWall`, `k.Point`, `k.SpawnSet`.
- [ ] Write steps with `k.Narrate`, `k.Defeat`, `k.Reach`, `k.Hold`, `k.Survive`, `k.Escort`, `k.CollectStep`, `k.Choice`, using `MemoryKit.Right(...)` and `MemoryKit.Wrong(...)`.
- [ ] Count step numbers carefully (the existing files comment each index) for `goToStep` and `nextStep`.
- [ ] **Build All Memory Prefabs**, then **Build Test World** to see it in the chain.

---

## 7. Commit rules
- Commit everything under `Assets/Memories/` including `.meta` files, the generated prefabs, `Resources/` and the test world.
- Do not commit changes to anyone else's scenes from this branch.
- If you ran the builders, `ProjectSettings/EditorBuildSettings.asset` is unchanged (the builders never touch it).

---

## 8. Playtest checklist
- [ ] Memory 1: two shadows at the start; reaching the brother spawns two more; a hit while holding E resets the bar; waiting 10 s on the choice picks "Say nothing" (wrong); the right choice ends it instantly with REDEEMED.
- [ ] Bridge to Memory 2 rises out of the lava after Memory 1.
- [ ] Memory 2: the letter pieces only appear once the collect step starts; the friend follows you; if shadows stay on her, her % drops; at 0% the memory collapses and restarts.
- [ ] Memory 3: "Return it" sends you to the old man, then the escape; "Keep it" brings four shadows, then the escape; fire appears on the route during the escape.
- [ ] Memory 4: leaving the light circle shows "Return to the light!" and pauses the countdown.
- [ ] Falling off a bridge: back to the last checkpoint, minus 5 s if a memory is running.
- [ ] After all four: the final gate appears; walking in shows REDEEMED or CONDEMNED.
- [ ] Timer turns orange at 30 s and red at 10 s; the "-3s" penalty and red hit flash show on hits.
- [ ] Turn on MemorySettings → **Save Progress Between Sessions** for the final build only.

---

## 9. Troubleshooting

| Problem | Fix |
|---|---|
| No Tools → Beyond Time → Memories menu | Compile errors. Check the Console. The Editor scripts must stay in folders named `Editor`. |
| Error about `ShadowScript` | Delete `Integration/ShadowScriptBridge.cs`. |
| "no shadow prefab" error | Run 1. Build Memory Kit, or fill MemorySettings → Shadow Prefab. |
| "MemoryUI prefab missing" | Run 1. Build Memory Kit. |
| Memory does not start | The player must be tagged **Player** and have a CharacterController (Starter Assets has both). The memory root needs its Box Collider set as a trigger. |
| Real shadows stand still | They need a baked NavMesh (see 4.6). |
| Fights never end with the real enemy | The enemy must be destroyed on death. |
| Hits do not cost time with the real enemy | Add ShadowScriptBridge, or call `MemoryEvents.RaisePlayerHit` in its attack. |
| Camera swings while choosing with the mouse | Use keys 1, 2 and 3, or ignore it; the world is frozen anyway. |
| A finished memory is already done when I press Play | Save Progress is on. Turn it off in MemorySettings, or use Tools → Beyond Time → Memories → Reset Saved Memory Progress. |
| My hand edits to a prefab disappeared | You re-ran Build All Memory Prefabs. Edit the definition file instead (section 5). |
| Pink materials | The project must use URP (it does). Re-run 1. Build Memory Kit. |
