# Beyond Time: front-end flow (independent work package)

**What you build:** the four scenes around the gameplay.

`MainMenu → Grave → Briefing1 → [Trial1, Maryam's] → EndScreen`

| Scene | Design screen | What happens |
|---|---|---|
| `MainMenu` | Screen 1 | Title, New Game, Continue, Controls, Quit |
| `Grave` | Screen 2 | Fade in from black, wake up, walk to two figures, questioning subtitles, portal opens, walk in |
| `Briefing1` | Screen 3 | Trial 01 card layout, Enter Memory button |
| `EndScreen` | After screen 6 | "The memory is at peace", credits, Main Menu, Quit |

**Why it is safe to work on alone:**
- Everything lives in a new folder, `Assets/FrontEnd/`. You do not open, edit or save any existing scene, prefab or script.
- All classes are inside the `BeyondTime.FrontEnd` namespace, so they cannot clash with anyone's class names (including the `SceneLoader` and `DialogueSequence` from the earlier pack).
- The only shared file you touch is the build list, and you keep that change local until merge day (step 4.3).
- The link to Trial1 is just the scene name `"Trial1"`. Nobody has to give you anything.

**What it needs from the team (the contract):**
1. Maryam's gameplay scene stays named `Trial1`. If renamed, change one line in `FlowLoader.cs` (`FrontEndScenes.Trial1`) and rebuild Briefing1.
2. The player in Trial1 is tagged `Player` (already true for the Starter Assets armature).
3. Trial1 has a GameManager with Win, Fail and Pause panels (Maryam's Build HUD already makes these).

**Time:** about 1.5 to 2.5 hours for build, test and polish. The base is generated in one click, so most of your time goes into testing and making it look good.

---

## 1. Set up your branch (10 minutes)

The latest branch is now `GamePlay2`. It contains everything in `GamePlay3` plus Maryam's HUD and `Trial1` scene. Your files are all new, so they merge cleanly into either branch.

- [ ] Open a terminal in the repo folder:
  ```bash
  git fetch --all --prune
  git checkout -b feature/frontend origin/GamePlay2
  ```
  If the team told you to work from GamePlay3, use `origin/GamePlay3` instead. Everything works except the last hop from Briefing1 into Trial1, which only exists after merging.
- [ ] Open the project in Unity **6000.0.54f1**. Wait for import.
- [ ] Copy the `FrontEnd` folder from this pack into `Assets/`. You should end up with:
  ```
  Assets/FrontEnd/Scripts/FlowLoader.cs
  Assets/FrontEnd/Scripts/ScreenFade.cs
  Assets/FrontEnd/Scripts/GraveDialogue.cs
  Assets/FrontEnd/Scripts/PortalTrigger.cs
  Assets/FrontEnd/Scripts/SlowSpin.cs
  Assets/FrontEnd/Editor/FrontEndBuilder.cs
  Assets/FrontEnd/README_FrontEnd.md
  ```
  The `Editor` folder name matters. Unity only allows editor-only code inside a folder called `Editor`.
- [ ] Back in Unity, wait for the compile spinner (bottom right) to finish.
- [ ] Console: **zero red errors**. If you see any, check section 7.
- [ ] Confirm the menu exists: **Tools → Beyond Time → Front End**.

---

## 2. Generate the scenes (5 minutes)

- [ ] **Tools → Beyond Time → Front End → 1. Build All Front-End Scenes**.
- [ ] Wait a few seconds. It creates:
  - [ ] `Assets/FrontEnd/Scenes/` containing `MainMenu`, `Grave`, `Briefing1`, `EndScreen`
  - [ ] `Assets/FrontEnd/Art/` containing materials and post-processing profiles
- [ ] It finishes by opening `MainMenu`.
- [ ] **Tools → Beyond Time → Front End → 2. Add Front-End Scenes To Build**. This puts `MainMenu` at index 0 and `EndScreen` last, and keeps everything already in the list. You need this to test scene changes. Do not commit this change yet (see 4.3).
- [ ] Console shows the new build order.
- [ ] Optionally, in File → Build Profiles → Scene List, untick `SampleScene` and `Maryam`. These are old test scenes.

---

## 3. Test each scene

Always press Play from the scene named in each section.

### 3.1 MainMenu
- [ ] Screen fades in from black over about 1 second.
- [ ] Title "BEYOND TIME", tagline above, orange subtitle below.
- [ ] An orange portal glows on the right behind the UI.
- [ ] The mouse cursor is visible.
- [ ] Hovering a button turns it ember orange.
- [ ] **CONTINUE is greyed out** on a first run, because there is no save yet.
- [ ] **CONTROLS** opens the list; **BACK** closes it.
- [ ] **QUIT** stops Play mode in the editor.
- [ ] **NEW GAME** fades to black and loads `Grave`.

### 3.2 Grave
- [ ] It stays black for 1 second, then slowly fades in (the waking-up beat).
- [ ] Opening subtitles play: "Where am I?", then two more lines.
- [ ] Top left reads "OBJECTIVE: Approach the figures".
- [ ] Top right shows the controls hint.
- [ ] WASD moves, the mouse looks around, Shift sprints. The cursor is hidden.
- [ ] Two dark figures stand ahead with glowing halos and rim lights.
- [ ] Walking toward them triggers five questioning lines. The figures' lights brighten while they speak.
- [ ] Press **E** to skip a line. One press skips one line.
- [ ] After the last line, a cyan portal appears behind the figures and the objective changes to "Step into the memory portal".
- [ ] Walking into the portal fades to black and loads `Briefing1`.
- [ ] The whole scene takes about 30 to 45 seconds, matching the design.

### 3.3 Briefing1
- [ ] Cursor visible. Shows "TRIAL 01", "MEMORY FRAGMENT" and "The Moment of Anger".
- [ ] Three cards: Mission Objective, Time Limit 90 seconds, Threat.
- [ ] A hint line explains holding E.
- [ ] **ENTER MEMORY** saves progress, then loads `Trial1`.
- [ ] If Trial1 is not in your branch, the Console says the scene is not in Build Profiles. That is expected until merge.

### 3.4 EndScreen
- [ ] "THE MEMORY IS AT PEACE", subtitle, thanks and credits line.
- [ ] **MAIN MENU** goes back to `MainMenu`. CONTINUE should now be clickable there, because ENTER MEMORY saved progress.
- [ ] **QUIT** stops Play.

### 3.5 Full run
- [ ] Play from `MainMenu` and go all the way to Briefing1 without touching the editor.
- [ ] Commit (see 4.3 for which files to include).

---

## 4. Make it yours (spend the remaining time here)

### 4.1 Wording
- [ ] Grave lines: select `QuestionZone` in Grave → Grave Dialogue → edit **Opening Lines** and **Lines**. Agree the final wording with the team; the religious setting deserves care.
- [ ] Credits: EndScreen → `EndCanvas/Credits` → replace "ADD TEAM NAMES HERE".
- [ ] Briefing card text: Briefing1 → `BriefingCanvas` → each card's `Body`.
- [ ] Line speed: Grave Dialogue → Seconds Per Line (3.5 by default).

**Important:** once you start editing scenes by hand, do not run "Build All" again; it rebuilds from scratch. Use **Rebuild Only → (one scene)** if you need to regenerate a single scene.

### 4.2 Look and sound (in priority order)
- [ ] Music: add an Audio Source (Loop on, Play On Awake on, volume 0.3) to `FlowLoader` in MainMenu, plus a low ambient drone in Grave and EndScreen.
- [ ] Portal sound: Grave → `Portal` → add an Audio Source with a whoosh clip and drag it into Portal Trigger → **Enter Sound**.
- [ ] Figure presence: a low voice-like drone on each figure, as a 3D Audio Source (Spatial Blend 1, Max Distance 15).
- [ ] Grave walls: swap the grey stone material for a texture from `Assets/environment stuff/`, but only by assigning materials. Do not move or edit Zoha's assets.
- [ ] Menu backdrop: if Zoha is happy, drop one of her environment prefabs (for example a bridge) behind the menu portal.
- [ ] Fonts: if the team picks a font, create a TMP Font Asset (Window → TextMeshPro → Font Asset Creator) and assign it to the titles.
- [ ] Use only audio and fonts the team has the right to use, and add them to the credits.

### 4.3 Committing without stepping on anyone
The build list file is shared, so keep your local change out of commits:
```bash
git restore ProjectSettings/EditorBuildSettings.asset
git add Assets/FrontEnd
git status            # only Assets/FrontEnd files should be staged
git commit -m "Front end: MainMenu, Grave, Briefing1, EndScreen"
git push -u origin feature/frontend
```
- [ ] After `git restore`, run **2. Add Front-End Scenes To Build** again locally if you want to keep testing.
- [ ] Never commit changes to any scene outside `Assets/FrontEnd/`.

---

## 5. Merge day (20 minutes, together with Maryam)

Do this at the integration point, with Maryam present, because step 5.3 edits her scene.

### 5.1 Merge
- [ ] Make sure Maryam has pushed and closed Unity, or at least saved and is not editing Trial1.
  ```bash
  git checkout GamePlay2        # or whichever branch the team integrates into
  git pull
  git merge feature/frontend
  git push
  ```
- [ ] Open Unity, wait for compile, zero red errors.

### 5.2 Build list
- [ ] **Tools → Beyond Time → Front End → 2. Add Front-End Scenes To Build**.
- [ ] Confirm the order in the Console: `0 MainMenu`, `1 Grave`, `2 Briefing1`, then the existing scenes including `Trial1`, then `EndScreen` last.
- [ ] In Build Profiles, untick `SampleScene` and `Maryam` if the team agrees.

### 5.3 Connect Trial1 (adds buttons to Maryam's panels)
- [ ] Open `Assets/Scenes/Trial1.unity`.
- [ ] **Tools → Beyond Time → Front End → 3. Connect Open Trial Scene (merge step)**. It adds:
  - [ ] a **NEXT** button on the Win panel, which saves progress and loads `EndScreen`
  - [ ] a **MAIN MENU** button on the Fail panel and the Pause panel
  - [ ] a `FlowLoader` object, set to leave the cursor to GameManager
- [ ] The buttons sit at y = -210, below Maryam's RETRY and RESUME buttons, so they do not overlap.
- [ ] Running it twice does not create duplicates.
- [ ] **Ctrl+S**.

### 5.4 Full flow test
- [ ] Play from `MainMenu` → New Game → Grave → portal → Briefing1 → Enter Memory → Trial1.
- [ ] In Trial1, let the timer run out → Fail panel → **MAIN MENU** works. Then **RETRY** still works.
- [ ] Esc → Pause panel → **MAIN MENU** works.
- [ ] Win the trial → **NEXT** → EndScreen → Main Menu. **CONTINUE** now resumes at Briefing1.
- [ ] Commit and push:
  ```bash
  git add -A
  git commit -m "Connect front end to Trial1, build order"
  git push
  ```

### 5.5 When Trial 2 exists later
- [ ] Duplicate `Briefing1` → rename it `Briefing2`, then edit its texts.
- [ ] On its ENTER MEMORY button, change the scene names in the OnClick events to `Briefing2` (save) and `Trial2` (load).
- [ ] In Trial1, change the NEXT button's two OnClick arguments from `EndScreen` to `Briefing2`.
- [ ] Add both scenes to the build list.

---

## 6. File reference

| File | Purpose |
|---|---|
| `Scripts/FlowLoader.cs` | One per scene. `NewGame`, `Continue`, `LoadScene(name)`, `SaveProgress(name)`, `Quit`. Sets the cursor on start, greys out Continue with no save, fades before loading, and logs a clear error if a scene is missing from the build. Also holds the scene names in `FrontEndScenes`. |
| `Scripts/ScreenFade.cs` | Black overlay that fades in on start and fades out before a load. Uses unscaled time, so it works from a paused or failed state. |
| `Scripts/GraveDialogue.cs` | Opening lines on wake-up, then questioning on entering the trigger. E skips a line. Brightens the figure lights while they speak, then reveals the portal and updates the objective. |
| `Scripts/PortalTrigger.cs` | Loads the next scene when the Player enters, through FlowLoader so the fade plays. |
| `Scripts/SlowSpin.cs` | Rotation plus light pulse for the portals and halos. |
| `Editor/FrontEndBuilder.cs` | The Tools menu. Builds the four scenes, orders the build list, and connects the trial panels. |

---

## 7. Troubleshooting

| Problem | Fix |
|---|---|
| No **Tools → Beyond Time → Front End** menu | `FrontEndBuilder.cs` must be inside a folder named `Editor`. Check the Console for compile errors. |
| Compile error about `GameManager` | Your branch does not have Maryam's scripts. Branch from `GamePlay2` or `GamePlay3`, not `main` or `dev`. |
| Compile error about `Bloom`, `Vignette` or `GetUniversalAdditionalCameraData` | URP must be installed (it is in this project). Reopen Unity so packages finish resolving. |
| Text is pink or invisible | Window → TextMeshPro → Import TMP Essential Resources, then Rebuild Only that scene. |
| Grave has no player and the Console warns about the prefab | The Starter Assets third-person prefab is missing on your branch. Branch from GamePlay2. |
| Pink materials | The URP Lit shader was not found. Check that the project's Graphics setting uses the URP asset (`PC_RPAsset`). |
| Buttons do not react | Each scene needs one EventSystem with an Input System UI Input Module (the builder adds one). Delete any duplicates. |
| Cursor visible in Grave | Click inside the Game view once. In a build it locks automatically. |
| "Scene 'X' is not in Build Profiles" | Run **2. Add Front-End Scenes To Build**. For `Trial1` this is expected until merge. |
| The whole screen is black in the editor and never fades | The `FadeCanvas` alpha was changed by hand. Set the Fade object's Canvas Group Alpha to 0 in edit mode; the script sets it at runtime. |
| A merge conflict in `EditorBuildSettings.asset` | Accept either side, then run **2. Add Front-End Scenes To Build** again. It rebuilds the order. |
