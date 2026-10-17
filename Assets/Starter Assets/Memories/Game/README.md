# Beyond Time: the full game

Everything around the memories that makes it a complete game: menus, the intro video, health, souls, score, pause, game over, results, the paradise ending, saving, and **one asset library where you drag in all your own assets**.

```
BT_MainMenu  →  BT_GraveIntro  →  BT_HellfireWorld  →  BT_Paradise
 New Game        video or the        Alley → Siege →       peaceful ending,
 Continue        placeholder card    final gate            score, best score
 Settings
 Controls                            Esc = pause
 Credits                             0 souls = GAME OVER → Try Again / Main Menu
```

---

## 1. Build and play (2 minutes)

1. Install as the main README describes (delete the old `Assets/Memories`, then copy the new folder in).
2. If you installed the old **FrontEnd** pack, delete `Assets/FrontEnd`. This game layer replaces it.
3. Run **Tools → Beyond Time → Game → Build Whole Game**. It builds:
   - the asset library and both memories
   - all four scenes, with every button wired
   - Build Profiles in the order `BT_MainMenu`, `BT_GraveIntro`, `BT_HellfireWorld`, `BT_Paradise` (your other scenes are kept but unticked)
4. It opens `BT_MainMenu` and selects the asset library. Press **Play**.

You can also press Play directly in `BT_HellfireWorld` to test; it starts a fresh game automatically.

To rebuild one scene only, use **Tools → Beyond Time → Game → Rebuild Only → (scene)**.

---

## 2. The systems

### Health (SOUL bar, top left)
- 100 health.
- Enemies, the Harrier's orbs, fire and lava all hurt you. Fire and lava are 20 damage each.
- Health regenerates (6 per second) once you have not been hit for 4 seconds.
- Finishing a memory refills you completely.

### Souls (the three flames next to the bar)
You start with **3 souls**. You lose one when:
- your health reaches zero: the memory collapses, and you get back up at its entrance; or
- a memory collapses (the timer runs out, or someone you protect dies).

**Zero souls = GAME OVER** ("CONSUMED BY HELLFIRE"):
- **Try Again** reloads the world with 3 new souls. Memories you finished stay finished, and your score goes back to what it was after the last finished memory.
- **Main Menu**.

### Score (top right)

| Event | Points |
|---|---|
| Memory redeemed | +1000 |
| Memory condemned (wrong choice, or the escape door) | +300 |
| Time left when a memory ends | +10 per second |
| Enemy knocked out | +100 |
| Damage taken | -2 per point |
| Memory collapses | -200 |
| Final gate: each soul left | +500 |
| Final gate: good ending (most memories redeemed) | +2000 |

After each memory, a **result card** shows the breakdown. The best score is saved and shown on the main menu and in paradise.

Change any of these numbers on `GameSystems → Game Session` in `BT_HellfireWorld`.

### Pause (Esc)
- "TIME STANDS STILL", with buttons: Resume, Restart Memory (no soul lost), Settings, Controls, Main Menu, Quit.
- Pause works even during a choice.

### Settings
Master volume, music volume and fullscreen. They are saved and apply in every scene.

### Saving and Continue
- Finished memories, score and statistics are saved after every memory.
- **Continue** on the main menu returns you to the first unfinished memory.
- **New Game** wipes everything.
- Finishing the game clears the save and records the best score.

### The paradise ending
- A green island at sunrise with blossoms, water, drifting lights, a stone path and a marble shrine.
- **The people you saved are standing near the shrine, but only if you redeemed their memory.** The man from the alley appears if you redeemed The Alley; your friend appears if you redeemed The Siege.
- A condemned ending makes the light thinner and greyer, and changes the final words.
- Walk to the shrine, or wait 2.5 minutes, for the final screen: PEACE (or REST), score breakdown, best score, then Main Menu, Credits or Quit.

### The intro video
`BT_GraveIntro` plays your grave video, then loads the world. **Until you add a video, a placeholder title card plays for 6 seconds** with a progress bar. Hold Space, E or Esc for 1 second to skip.

---

## 3. THE ASSET LIBRARY (drag your assets here)

**Tools → Beyond Time → Game → Select Asset Library**, or select `Assets/Memories/Resources/BeyondTimeAssets.asset`.

Every field is an empty slot. Drag your asset in and press Play: every object of that kind in every scene is swapped automatically, with no rebuild needed. Empty slots keep the placeholders.

| Section | Slots | What happens |
|---|---|---|
| **Environment** | Lava Material, Lava Prefab, Ground Material, Wall Material, Fire Prefab, Fire Burst Prefab, Fire Ember Prefab, Bridge Prefab, Portal Prefab, Hell Skybox | Materials repaint the placeholders. Prefabs replace them; lava and bridges are stretched to fit. |
| **Props** | Knife, Gate, Lever, Rubble, Escape Door, Projectile | The prefab replaces the placeholder at the same spot. |
| **Humans** | Human Model (used for everyone), Human Animator, then Victim, Attacker, Lookout, Friend models | A humanoid model replaces the placeholder body. Its fighting, AI and health bar keep working. |
| **Shadow enemies** | Shadow Model (used for all), Shadow Material, Shadow Animator, then Pursuer, Gatekeeper, Harrier models | Same as humans. With no model yet, **Shadow Material** repaints the placeholder shadows. |
| **Paradise** | Ground Material, Tree, Flower, Rock, Water Material, Shrine, Paradise Skybox | Replaces each tree, flower and rock, and repaints the ground and water. |
| **Video** | Intro Video | Plays in BT_GraveIntro instead of the placeholder card. |
| **Music and stings** | Menu, World, Paradise music; Memory Redeemed, Memory Collapsed, Soul Lost, Game Over stings; Button Click | Played automatically. |
| **Combat sounds** | Punch Hits, Block, Whoosh, Footsteps | Used by every fighter that has no sounds of its own. |
| **UI** | Font, Menu Background, Logo, Panel, Button, Pause Background, Game Over Background, Victory Background, Result Card, Video Placeholder, HUD Frame | Sprites and the font are applied to every menu and the HUD. A **Logo** replaces the "BEYOND TIME" title text. |

### Replacing only ONE object
Every placeholder carries a **Replaceable Asset** component (search the Hierarchy for `t:ReplaceableAsset`).
1. Select the object.
2. Drag a prefab into **Override Prefab**, or a material into **Override Material**.
3. Only that object changes; the library is ignored for it.

Use **Offset**, **Rotation** and **Scale** to line the replacement up.

**Rules that keep gameplay safe:**
- **Keep Placeholder Colliders** is on by default. Your prefab is visual only, and the invisible placeholder collider still does the collision. Turn it off only if your prefab has its own correct colliders.
- **Fit To Placeholder** stretches the replacement to the placeholder's size. It is on for lava and bridges.
- **Characters:** the model must be imported as **Humanoid** (Rig tab). If it has no Animator Controller, the library's Human or Shadow Animator is used, then the Starter Assets controller. Attack clips are optional (main README, section 5.6).

### Format checklist for your assets

| Asset | Import settings |
|---|---|
| Character FBX | Rig → Animation Type **Humanoid**; Materials → extract to your folder |
| Environment or prop FBX | Scale so 1 unit = 1 metre; pivot at the base |
| Lava, ground, wall, water | URP Lit or URP Unlit materials (or your own shader) |
| Fire and effects | Prefabs with Particle Systems using URP particle materials |
| Skybox | A Material with a Skybox shader (6 Sided, Cubemap or Panoramic) |
| Video | MP4 (H.264). Importer: **Transcode** on if it will not play |
| Music | OGG or MP3. Load Type **Streaming** for long tracks |
| Sprites | Texture Type **Sprite (2D and UI)** (see section 4) |
| Font | Window → TextMeshPro → **Font Asset Creator** → make a TMP font asset from your TTF |

---

## 4. Importing the Canva UI designs into Unity

The Canva design has one 1920 × 1080 page per screen: Main Menu, Pause, HUD, Game Over, Memory Redeemed, Victory Paradise, Intro Video Placeholder, Settings and Controls.

### 4.1 Export from Canva
1. Open the design in Canva and make it your own: change text, colours and images.
2. **Make background versions without buttons or text.** Duplicate each page you will use as a background. On the copy, delete the button shapes, the button words and any numbers (scores, timer). Unity draws those itself so they can be clicked and updated. Keep titles, art, frames and decoration.
3. **Share → Download → File type PNG**, size 1920 × 1080, and select the background pages. Canva downloads a ZIP of numbered PNGs.
4. **Optional pieces:** for a custom button or panel, make a small separate design (for example 600 × 120 for a button), put just the button shape on it, and download it as PNG with a **transparent background** (Canva Pro).

### 4.2 Import into Unity
1. Create `Assets/Memories/Game/UI/Canva/` and drag the PNGs in.
2. Select them all. In the Inspector:
   - **Texture Type:** Sprite (2D and UI)
   - **Sprite Mode:** Single
   - **Max Size:** 2048
   - **Compression:** High Quality (or None for crisp text)
   - click **Apply**
3. **Buttons and panels** (so they stretch without blurring the corners):
   - **a.** Select the sprite and click **Sprite Editor**.
   - **b.** Drag the green border lines inward to just inside the rounded corners or frame (the 9-slice border).
   - **c.** Click **Apply**.

### 4.3 Put them in the game

| Canva page | Asset library slot |
|---|---|
| Main Menu (background version) | **Menu Background** |
| Title art or logo, cut out as a transparent PNG | **Logo** (it replaces the text title) |
| Pause Menu (background version) | **Pause Background** |
| Game Over (background version) | **Game Over Background** |
| Victory Paradise (background version) | **Victory Background** |
| Memory Redeemed card | **Result Card** |
| Intro Video Placeholder | **Video Placeholder** |
| Settings and Controls (background version) | **Panel** (used behind Settings, Controls and Credits) |
| HUD health area cut out | **HUD Frame** |
| Button shape (9-sliced) | **Button** |
| The font you used in Canva (download the TTF from its source) | **Font**: make a TMP font asset first |

Press Play: every menu uses your art.

**Fine-tuning positions** (if your Canva layout puts things somewhere other than the defaults):
1. Open the scene and expand the Canvas: `MenuCanvas` in `BT_MainMenu`, `GameSystems/GameMenus` and `GameSystems/GameHUD` in `BT_HellfireWorld`, `ParadiseCanvas` in `BT_Paradise`.
2. Select a button or text and move it in the Scene view, using the 2D button at the top of the Scene view.
3. Save the scene.

Do not re-run Rebuild Only for that scene afterwards, or your layout is replaced.

**HUD tip:** keep the HUD art light. A thin frame behind the health bar and a small frame behind the score read better in a fight than a full-screen overlay.

---

## 5. Where things live (to tweak by hand)

| Scene | Object | What you can change |
|---|---|---|
| BT_MainMenu | `MenuCanvas` | Title, tagline, button text and positions, Credits text (`CreditsPanel/Names`) |
| BT_GraveIntro | `GraveIntroVideo` | Video Clip, Placeholder Seconds, skip hold time |
| BT_HellfireWorld | `GameSystems` (Game Session) | Health regen, every score value |
| BT_HellfireWorld | `GameSystems/GameHUD`, `GameMenus` | HUD layout, pause and game over screens |
| BT_HellfireWorld | `FinalGate` (Memory Final Gate) | Redeemed count needed for the good ending, ending texts |
| BT_Paradise | `ParadiseController` | The ending lines, shrine radius, auto-end time, condemned light colour |
| BT_Paradise | `PARADISE_ENVIRONMENT` | Trees, flowers, rocks, shrine (all Replaceable), path |
| BT_Paradise | `TheManYouSaved`, `YourFriend` | Paradise Guest → which memory makes them appear |

---

## 6. Troubleshooting

| Problem | Fix |
|---|---|
| "scene is not in Build Profiles" | Run Tools → Beyond Time → Game → Set Build Order (or Build Whole Game). |
| My asset in the library does not show | Press Play; replacements happen when the scene starts. Check the slot name matches the object (for example walls use Wall Material). |
| A replaced character T-poses | Its model must be Humanoid. Set Human Animator (or Shadow Animator) in the library, or give the model's Animator a controller. |
| A replaced character floats or sinks | Adjust Offset on that character's Replaceable Asset (root object). |
| My prefab falls through or blocks nothing | Keep Placeholder Colliders on (default). |
| The video does not play | MP4 H.264; in the video importer tick Transcode. Check it is in the library's Intro Video or on GraveIntroVideo. |
| UI art looks blurry | Sprite import: Compression None or High Quality, Max Size 2048, and use the full 1920 × 1080 export. |
| Buttons stretch badly | Set a 9-slice border in the Sprite Editor. |
| Continue is greyed out | No save yet: finish at least one memory first. |
| A memory is already done when I press Play in the world | Expected when continuing a save. Start from the main menu with New Game, or run Tools → Beyond Time → Memories → Reset Saved Memory Progress. |
