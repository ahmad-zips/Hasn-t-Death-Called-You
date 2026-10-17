# Memory 01: The Alley

**Wrong being corrected:** you watched a man being stabbed in an alley and kept walking.
**This time:** a knife attack is happening in front of you, and you can stop it.
**Time limit:** 180 s. **Island radius:** 15 m (a walled alley, open at the -Z end where the bridge arrives).

## Quick test (2 minutes)

1. Install the package as the main README describes (delete the old `Assets/Memories` folder first).
2. Wait for the compile to finish. The Console should show zero red errors.
3. Run **Tools → Beyond Time → Memories → Test One Memory → 01 The Alley**.
   - This builds the memory and opens `Memory_01_Alley_Test.unity`: a start island, a bridge, the alley and a gate.
4. Press **Play** and walk over the bridge.

## Controls

| Input | Action |
|---|---|
| WASD, mouse, Shift | Move, look, sprint |
| Left mouse | Punch. Keep clicking for a combo: jab, cross, hook (the hook knocks back) |
| F | Front kick: breaks guards and pushes enemies back |
| F during a punch combo | Roundhouse: knocks the enemy down |
| Right mouse (hold) | Block. You turn to face the nearest enemy automatically |
| Right mouse just before a hit lands | **Parry**: no damage, and the attacker is stunned for about 1 second |
| Space | Dodge. You cannot be hit while dodging, and it is the only answer to the knife stab |
| Q | Repel: stuns everyone nearby for 2.5 s (8 s cooldown) |
| E | Continue the story, or hold to interact |
| 1, 2, 3 | Choices |

A hit you take costs health (the SOUL bar), and heavy hits stagger you. Blocked hits cost far less, and parried or dodged hits cost nothing. Health regenerates when you have not been hit for a few seconds.

## Who is in the alley

| Person | Role | Behaviour |
|---|---|---|
| **The Man** (blue-grey) | Victim, 100 health, can die | At the start he cowers against the wall. Once the attack begins he panics and runs from the attacker, staying inside the alley. When cornered he cowers and covers his head. His green health bar is always shown. **If he dies, the memory collapses.** |
| **The Attacker** (dark red, knife) | Attacker, 130 health | He stands over the man until the fight starts, then hunts him and slashes every 2.6 s. Hit him (or get within 2.4 m) and he turns on you for 6 s. Back off and he goes back to the man. He calls his lookouts the moment he starts fighting. |
| **Lookout A** (grey) | Guard, 70 health | Patrols a square near the alley mouth. Spots you within 11 m in a 130° cone (walls block his view), or hears you within 4 m. |
| **Lookout B** (olive) | Guard, 70 health | Patrols the back of the alley near the attack. More aggressive, and dodges more. |

All three hostiles circle you between attacks and keep their distance. They throw combos of 1 to 3 punches, block about a third of your attacks, and sometimes dodge. When their poise breaks they stagger or get knocked down, then get back up. At zero health they are knocked out for good (nobody is killed by your fists).

**The knife:**
- Slashes can be blocked, and they can be parried.
- Every third attack is a **stab**: a longer windup while the blade **glows red**. It cannot be blocked. **Dodge it** (Space).
- Stabs do 22 damage, the heaviest in the game.

## Script

| # | Step | Story at the bottom | Objective |
|---|---|---|---|
| 0 | Narrate | This is the alley behind the market. You were walking home with your headphones on. | |
| 1 | Narrate | You saw the knife. You heard him ask for help. You kept walking. | |
| 2 | Choice, 5 s, default 2 | **The knife is already raised. What do you do?** | |
| | 1. Step in. | You run toward them. | |
| | 2. Look away. (default) | You turn your back. He screams. When you look again, he is bleeding. **The man loses 35 health, and you lose 10 s.** | |
| 3 | Defeat: Attacker (the attack begins) | Left mouse to punch, keep clicking for a combo. F to kick. Make him turn on you, and never let him reach the man again. | Stop him, with The Man's health % |
| 4 | Defeat: both Lookouts | His lookouts are coming. Hold right mouse to block. Block just before a punch lands to parry. Space to dodge. | Take down his lookouts (n left) |
| 5 | Hold E 3 s at the attacker (his knife now lies on the ground) | He is down. The knife has slid across the ground. | Pin him down |
| 6 | Choice, 8 s, default 2 | **He is pinned under you. The knife is in reach. What do you do?** | |
| | 1. Throw the knife away. Shout for help. | Footsteps. Voices. Someone is calling an ambulance. The man on the ground is still breathing. | **REDEEMED** |
| | 2. Pick up the knife. (default) | Your hand closes around the handle. The memory turns red. | -10 s, +4 shadows, go to 7 |
| | 3. Walk away. | You leave them both on the ground. The alley does not forgive you. | -5 s, +3 shadows, go to 7 |
| 7 | Defeat (shadows) | This is how it starts. One step too far. | Fight what you almost became |

**Redeemed:** This time you stepped in. He is alive because you did not walk away.
**Condemned:** You stopped the knife, but not the part of you that reached for it.

**Fail states:**
- The timer reaches zero.
- The man dies ("He died in the alley. Again.").

Either one makes the memory collapse. Everyone returns to their starting places, and the memory restarts from the alley mouth.

## Why it plays well

- **Pressure with a clock you can see.** The man's health % sits in the objective. You choose between hitting the attacker now to pull him off, and taking the lookouts first. Ignore the attacker for too long and the man dies.
- **Readable enemies.** Every attack has a windup, so you can learn to block, parry or dodge. The red knife glow teaches the one attack you must dodge.
- **Real choices in the fight.** Kicks break guards, the roundhouse knocks enemies down, parries open enemies up, and repel buys time to get between the attacker and the man.
- **Choices with teeth.** Looking away costs the man real health and you real time. Picking up the knife turns the ending into a shadow fight and a condemned result.

## Tuning (all in the Inspector, no code)

Open `Memory_01_Alley.prefab`, then go to `Actors/<Person>`.
- **Fighter**: health, poise, guard, dodge, and every move (damage, speed, reach, knockback, knockdown, unblockable).
  - Right-click the component header for **Preset: Player / Thug / Killer / Civilian**.
- **BrawlerAI**: walk and run speed, sight and hearing, aggression, attack gaps, block, dodge and kick chances, patrol points, leash radius.
  - The attacker also has **Murder Attack Gap** (how often he stabs the man) and **Aggro Memory**.
- To make the murder slower or faster, change the attacker's Murder Attack Gap (default 2.6 s), the man's run speed (3.4) or the man's max health (100).
- To change the time limit, select the prefab root → Memory Zone → **Time Limit**.

If you re-run *Build All Memory Prefabs*, the prefab is rebuilt from `Editor/Memory01AlleyDefinition.cs`. Put permanent changes in that file, or stop rebuilding this memory.

## Replacing placeholders

**People** (`Actors/<Person>/MODEL_REPLACE_ME`). These are already real animated humanoids (the Starter Assets body with idle, walk and run) in tinted clothes. To use your own characters:
1. Drop any **humanoid** model (with an Animator and an avatar) onto the person object, at position 0,0,0.
2. On its Animator, set the controller to `StarterAssetsThirdPerson` (or your own controller with a `Speed` float: 0 idle, 2 walk, 6 run).
3. Add **FighterAnimEvents** to the model if your animations have footstep events.
4. Delete `MODEL_REPLACE_ME`.

Punches, kicks, blocks and flinches keep working on any humanoid, because they bend the real bones.

**Real attack animations** (optional, for example from Mixamo):
1. Add the clips as states in the model's Animator controller.
2. On the Fighter, type each state name into that move's **Anim State** field.

That move now plays the clip instead of the procedural pose. Fill **Hit State** and **Knockdown State** the same way.

**The knife** (`Actors/TheAttacker/KNIFE_REPLACE_ME`):
- Put your knife model inside it and delete the Blade and Grip placeholders.
- Keep **BoneAttachment** (it snaps the knife into the right hand at runtime) and adjust its offsets if the grip looks wrong.
- Keep **TelegraphLight**, which glows during stabs.

**Other props:**
- `StepObjects/DroppedKnife_REPLACE_ME`: the knife on the ground after the pin.
- `ART_REPLACE_ME`: the alley walls, dumpster, crates, barrel, and the flickering street light. Keep walkable ground with colliders.
- `FIRE_REPLACE_ME`: the four island fires. They go out when the memory is finished.

## Using the fighting outside this memory

- To give your real player the full moveset, select the player (tagged Player, with a CharacterController) and run **Tools → Beyond Time → Memories → Add Fighting To Selected Player**.
  - Remove Ahmad's LightPulse if it is on the same object, since both use Q.
- To add NPC fighters to any memory, use `k.Brawler(...)`, `k.Knife(...)` and `k.DefeatFighters(...)` in a definition file. See this memory's definition for an example.
- Placeholder shadows can also be punched and kicked.
