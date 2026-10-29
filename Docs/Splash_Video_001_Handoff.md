# Handoff — Video 001: Cooking Game Splash Screen

Read this whole file, then [AGENTS.md](../AGENTS.md), before doing anything. This file captures everything decided and analysed so far so you do not need to redo the video analysis.

---

## 1. What the user wants (non-negotiable)

- We are building the **NoriVerse cooking game for kids** in **Unity 6 (6000.6.7f1), 2D**. This video is the **splash screen**, the first part of the game.
- Recreate the reference splash **exactly**: same timing, same animations, same composition, same layout, same transitions, "every bit of it".
- Change only three things:
  1. **Character:** use the user's character **Nori**, the chef raccoon, in place of the reference bear.
  2. **Game name:** **NoriVerse** in place of "Bimi Boo". Keep the two-colour treatment: **"Nori" in blue, "Verse" in green**.
  3. **Tagline:** **"Play. Learn. Explore."** in place of "Where Learning Begins".
- **Art style must be the user's Nori style.** This is the soft, rendered, 3D-looking cartoon illustration with fluffy fur, the same as the reference images in `Tools/ArtPipeline/references/`. It is still a 2D sprite, never a 3D model.
  - The user **rejected** a hand-coded flat vector Nori I made: "totally not acceptable".
  - **Do not draw Nori in code.** Generate the art with Meshy image-to-image, using the user's references.
- **Create all art new.** Do **not** reuse any earlier project assets, such as `~/Desktop/NoriCooking`, `~/Desktop/spoonderful`, or `~/.codex/generated_images`. The user said this explicitly.
  - The user's own Nori reference images are the **character references** for generation. Use them as Meshy references only, never as in-game sprites.
- Decisions the user made (answered questions):
  - **Art style:** generated in the user's Nori style, as above.
  - **Ending:** stop at the **fade to white** (~11.4 s).
    - The blue pot appearing at 11.6–12.45 s belongs to the **next video**. Do **not** implement it.
  - **Logo mark:** **no ® mark** (the reference has one; NoriVerse should not).
  - **Image model:** "best model you can use", so **`nano-banana-pro`** (9 credits per image).
  - **Approval:** the user approved the test pose and **approved generating everything**: "it perfectly matches the video reference".
- Follow AGENTS.md:
  - Analyse, implement and **test**, then give the **Video Implementation Report** (AGENTS.md §12).
  - Don't claim untested things.
  - Don't move on to the next video.

---

## 2. Current state of the project

| Item | State |
|---|---|
| Unity project | Created at `/Users/harshikasharma/Documents/NoriVerse` with Unity **6000.6.7f1**: `Assets`, `Packages`, `ProjectSettings` (the default template). Unity binary: `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity`. No scenes or scripts yet. |
| Folders | `Assets/_Project/{Art/{Characters/Nori,Props,UI/Logo,FX},Animations/Splash,Scenes,Scripts/{Core,Characters,UI,Utilities,Editor},Prefabs,Audio,Settings}` |
| Rejected vector art | `Assets/_Project/Art/**` currently holds **rejected** flat-vector PNGs and a `nori_rig.json`, all made by `Tools/ArtPipeline/build_splash_art.py`. **Delete them and that script** before importing the Meshy art. **Ask the user first if unsure**, because the user once interrupted a delete. The script's logo-letter code is fine to keep (see §6). |
| Meshy client | `Tools/ArtPipeline/meshy_image.py`: `generate(out, prompt, refs, from_task, model, aspect, keep_background, text_only)` and a CLI. It calls the Meshy REST API directly and reads `MESHY_API_KEY` from the environment, never printing it. |
| Approved test pose | `Tools/ArtPipeline/meshy_raw/test_stand.png` (896×1200, transparent), Meshy task `01a17e98-497d-767b-b238-cb28f27fa648`. The user **approved** it. Small flaw: the hat touches the top edge, so prompts must ask for empty margin. |
| Nori references | `Tools/ArtPipeline/references/`: `nori_ref_wave.png` (best, high-res), `nori_ref_celebrate.png`, `nori_ref_mix.png`, `nori_ref_oops.png`, `nori_ref_think.png`, `nori_ref_wave_small.png`, `nori_ref_artist_atlas.webp` (artist outfit; **do not use** for the cooking game). |
| Reference video | `/Users/harshikasharma/Downloads/Cooking splash_Video_001.mp4` |
| Reference frames | `Docs/reference_frames/`: contact sheets and key frames, already **rotated to landscape**. |
| Meshy credits | ~2,608 remaining (Oct 27, 2026). |

### Meshy access: important

- The `meshy` MCP server in `.mcp.json` **returns 401 inside the app** even though the key is valid. A direct `curl` to `/openapi/v1/balance` returns 200.
- **Use `Tools/ArtPipeline/meshy_image.py` (direct REST) instead of the MCP tools.** `MESHY_API_KEY` is set in `~/.zshrc` and visible to the Bash tool. **Never print, echo or log the key.**
- Every generation costs credits. The user approved ~145–225 credits for this batch. **Ask before spending much beyond that**, for example many retries.
- Check the balance (free):
  ```bash
  curl -s -H "Authorization: Bearer $MESHY_API_KEY" https://api.meshy.ai/openapi/v1/balance
  ```

---

## 3. Reference video analysis

### Video facts

- **Container:** 1748×3840, 60 fps, 747 frames, **12.45 s**.
- **The recording is rotated:** the game is **landscape**. Rotate frames **90° counter-clockwise** (`cv2.ROTATE_90_COUNTERCLOCKWISE`) to get **3840×1748 (≈2.2:1)**.
- **The game is landscape-only.** Lock orientation to landscape.
- **Background:** flat near-white **#FDFDFD** (253,253,253), with no texture.
- **Audio:** not analysed (no ffmpeg/ffprobe was available). If you can, check for a voice saying the name or a jingle, and note it as pending if it can't be done.
- **Tooling used:**
  - Python venv with `opencv-python-headless`, `numpy`, `pillow` at `/private/tmp/.../scratchpad/venv`. It may be gone, so recreate it with `python3 -m venv` and pip.
  - `brew` is available.

### Colours sampled from the reference

| Element | RGB |
|---|---|
| Title blue ("Nori") | 87, 182, 202 (#57B6CA) |
| Title green ("Verse") | 126, 202, 78 (#7ECA4E) |
| Tagline gray | 106, 106, 106 (#6A6A6A) |
| Tablecloth green | 140, 181, 27 (#8CB51B) |
| Background | 253, 253, 253 |

### Unity coordinate system used for layout

- Orthographic camera, **size 5** (screen height = 10 units, y from −5 to +5), centred at the origin.
- Reference pixel→unit conversion: **1 unit = 174.8 px** in the 3840×1748 landscape frame (0.01144 units per half-res px).
- Screen width at 2.2:1 is ±10.98 units; at 16:9 it is ±8.9; at 4:3 it is ±6.67.
- All content sits within x ±5, so it fits 4:3 tablets.

### Layout measurements (converted to Unity units)

**Phase A: big centred title with peeking character (≈0.8–2.4 s)**
- Title "Bimi Boo": cap height ≈ **1.49 u**, horizontally centred.
  - Vertical centre: **y ≈ +0.88**.
  - Total width ≈ 8.5 u.
  - NoriVerse has 9 letters, so aim for cap height ≈ 1.35 u, keep width ≲ 9.5 u, and centre it.
- Tagline: centred, y ≈ **−0.69**, cap height ≈ 0.46 u, about the same width as the title. It sits 1.57 u below the title centre.
- Peeking character:
  - Head width ≈ **3.6 u** (1.44× the later full-body size). Head top at y ≈ −1.46; chin near y ≈ −4.6.
  - Two paws grip the **bottom screen edge** (y ≈ −5), just outside the head width at x ≈ ±1.9.
  - **Nori's chef hat makes him taller than the bear.** Shift the big title up a little (e.g. title centre y ≈ +1.2, tagline ≈ −0.35) and/or use peek scale ≈ 1.25, so the hat never overlaps the tagline. Keep the composition looking like the reference.

**Phase B: final layout (≈2.6 s → end)**
- Title shrinks to **scale ≈ 0.785** and moves up:
  - Title centre **y ≈ +2.92**; tagline centre **y ≈ +1.78**.
  - Title width ≈ 6.7 u (reference).
- Character, centred at x = 0:
  - Head width ≈ **2.5 u**; the hair/hat top sits ≈ 1.2 u below the tagline.
  - The character stands **behind** the table, visible from the chest up.
- Table, centred at x = 0:
  - Width ≈ **4.27 u**. Cloth top **y ≈ −2.78**, wavy hem y ≈ −3.79, round wooden legs to ≈ −3.95.
  - Soft gray floor shadow at ≈ −4.0.
- Covered dish (cloche): width ≈ **1.66 u**, height ≈ 0.95 u, sits on the table top at centre (x ≈ 0).
- Noodle bowl (under the cloche): width ≈ **0.97 u**, sits on the table top at centre.
- Hearts: ~5 small pink hearts (≈0.2–0.35 u) in an arc around the top of the head.

### Timeline (frame-accurate, landscape view)

| Time (s) | Character | Title / objects | Animation details |
|---|---|---|---|
| 0.00–0.07 | — | Blank white | — |
| 0.07–0.27 | **Two paws** rise from below the bottom edge at x ≈ ±1.9 (paws first, head still hidden) | — | Ease-out rise |
| 0.27–0.55 | **Head** rises up between the paws (peek), ending with head and paws resting on the bottom edge | — | Ease-out with a slight overshoot/settle |
| 0.60–0.70 | Peek: quick happy blink/squint | — | Expression swap |
| 0.80–1.75 | Peek: **talking** (mouth opens and closes, as if saying the name) | **Letters pop in one by one**, left to right, about one letter every 0.13 s (7 letters in the reference; use ≈0.11 s spacing for NoriVerse's 9 letters so it ends ≈1.75 s) | Each letter: scale 0 → ~1.15 → 1.0 over ~0.2 s, popping in place. "Nori" blue, "Verse" green |
| 1.75–2.25 | Peek: "ooh" mouth (excited) | — | — |
| 1.95–2.25 | — | **Tagline fades in** (alpha 0→1). In the reference, ® also fades in here; **skip ® for NoriVerse** | Linear fade |
| 2.27–2.45 | Peek character **sinks down** out of the bottom edge (head and paws) | — | Ease-in drop |
| 2.38–2.62 | — | **Title and tagline shrink (×0.785) and move up** to the final layout | Ease-in-out |
| 2.40–2.67 | Full-body character **jumps up from below the screen** to centre, arms down | — | Fast ease-out rise; stretch while rising |
| 2.67–2.93 | Falls and **lands** at the standing position; ground shadow fades in under the feet | — | Squash on landing (≈ scale 1.08×0.92), then settle |
| 2.93–3.07 | Landed: **eyes closed** happy blink | — | Expression swap |
| 2.93–3.47 | Stands still while the **table with the covered dish slides in from the left** (from off-screen, x ≈ −14) to centre, passing **in front** of the character | Table alpha fades 0→1 over 2.93–3.10 | Ease-out slide. Afterwards the character is behind the table (chest up visible) |
| 3.33–4.60 | **Curious lean:** head and body tilt toward the cloche (head rotates ~10–15°, leans down and right), eyes looking down at it, "o" mouth, one paw on or near the dish | — | Slow ease in-out lean with small idle sway |
| 4.70–4.85 | Paw lifts the cover | **Cloche flies up and to the right**, rotating, fading out (4.75–5.00) | Ease-out; alpha → 0 |
| 4.80 | — | **Noodle bowl revealed** (it was under the cloche) | — |
| 4.85–5.10 | **Surprised:** eyes wide, "oh" mouth | — | — |
| 5.10–5.25 | Quick happy eyes-closed squint | — | — |
| 5.25–6.05 | **Arms spread wide** (ta-da), big happy open smile, slight bounce | — | Arms swing out with ease-out, gentle bob |
| 6.10–6.35 | Picks up **chopsticks** (purple) in the paw on the viewer's left, pointing to the viewer's left | — | — |
| 6.35–7.40 | **Eating:** brings the chopsticks with noodles up to the mouth and back down (≈2 cycles); mouth open while eating | — | Ease in-out arm/chopstick motion |
| 7.40–7.90 | **Chewing**, eyes closed happily, cheeks moving | — | Small chew bob |
| 7.80–8.75 | Happy face | **Pink hearts** pop in around the head in an arc, pulse (scale 1 ↔ 1.15) and fade | Staggered pop-in; fade out by 8.75 |
| 8.75–11.07 | **Idle** holding the chopsticks, smiling at the viewer; blink at ≈9.1 s and ≈10.6 s; subtle breathing | — | Gentle breathing (scale y 1 ↔ 1.01) |
| 11.07–11.40 | — | **Whole scene fades to white** | Full-screen white overlay, alpha 0→1, linear |
| 11.40 | — | **Splash complete**: raise a completion event (hand off to the next screen, which comes in the next video) | — |
| 11.6–12.45 | (reference only: blue pot appears) | **Not part of this task** | — |

Visual check frames are in `Docs/reference_frames/`, especially `poses_3.07s_to_8.67s.jpg` (8 key poses) and `t5.00s_final_layout.jpg`.

---

## 4. Asset inventory and Meshy generation plan

All sprites are **new**, generated with Meshy **`nano-banana-pro`** and `remove_background` (transparent PNG). Raw outputs go to `Tools/ArtPipeline/meshy_raw/`. Only processed, approved PNGs go into `Assets/_Project/Art/...`.

### Shared style text (prepend to every Nori prompt)

> The exact same chef raccoon character Nori from the reference images, same art style (soft rendered 3D-like cartoon illustration, fluffy fur, white chef hat, yellow goggles resting on the hat, red neckerchief, white chef jacket with dark buttons, teal apron with front pocket holding a wooden spoon and whisk, striped tail). Same face, proportions and colours as the reference. Single character, centred, whole figure inside the frame with generous empty margin on all sides (nothing cut off, especially the top of the hat), plain white background, no ground shadow, no text.

- **References for new poses:** `references/nori_ref_wave.png`, `references/nori_ref_celebrate.png`, `references/nori_ref_think.png`, plus `meshy_raw/test_stand.png` once approved (up to 5).
- **Expression variants** of a pose: use `--from-task <task id of the base pose>` with a prompt like "Same image, identical pose, framing, size and position; change only …". This keeps the body pixel-aligned so swaps don't jump.
- **Full-body poses:** `--aspect 3:4`.

### Nori sprites

| File (in `Art/Characters/Nori/`) | Prompt (after the style text) | Source |
|---|---|---|
| `nori_stand.png` | Standing upright facing the viewer, arms relaxed down at the sides, gentle closed-mouth smile | **Done**: `meshy_raw/test_stand.png` (task `01a17e98-497d-767b-b238-cb28f27fa648`). Regenerate only if the hat crop is a problem |
| `nori_stand_blink.png` | Same image; only eyes closed in a happy squint | from-task stand |
| `nori_peek_smile.png` | Only head and upper chest visible, peeking up from below the bottom edge of the image; both dark-brown paws rest on the bottom edge to the left and right of the face, fingers up, gripping the edge; facing viewer, happy closed-mouth smile. The image is cut straight horizontally at chest level (replace "nothing cut off" in the style text for this one). Aspect 4:3 | refs |
| `nori_peek_talk.png` | Same image; only the mouth open wide, happily talking, tongue visible | from-task peek_smile |
| `nori_peek_ooh.png` | Same image; only the mouth a small round "ooh", eyebrows raised, excited | from-task peek_smile |
| `nori_peek_blink.png` | Same image; only both eyes closed in a happy blink | from-task peek_smile |
| `nori_lean_curious.png` | Leaning forward and to the viewer's right, head tilted down-right, looking down curiously at something in front of him at waist height, small "o" mouth, one paw reaching forward-down | refs |
| `nori_surprised.png` | Standing upright, both paws slightly raised in front of the chest, eyes wide, mouth open in a delighted round "oh" | refs |
| `nori_arms_spread.png` | Both arms stretched wide to the sides, paws open, big happy open-mouth smile | refs |
| `nori_hold_chopsticks.png` | Holding a pair of purple chopsticks in the paw on the viewer's left at chest height, chopsticks pointing horizontally to the viewer's left; other paw resting forward at waist height; happy open smile looking at viewer | refs |
| `nori_eat.png` | Same image; now bringing the chopsticks with a few noodles up to his open mouth, eating | from-task hold_chopsticks |
| `nori_chew.png` | Same image; chopsticks lowered at chest height, eyes closed happily, mouth closed and chewing, cheeks puffed | from-task hold_chopsticks |
| `nori_hold_blink.png` | Same image; only eyes closed in a blink | from-task hold_chopsticks |

### Props and FX (text-to-image, `--text-only`, same rendered style)

Prop style text:

> soft rendered 3D-like cartoon illustration in the style of a premium kids' game, smooth shading, soft highlights, front view, centred, generous empty margin, plain white background, no shadow, no text

| File | Prompt | Aspect |
|---|---|---|
| `Art/Props/table.png` | A small table covered with a bright green tablecloth (#8CB51B) with a wavy hem and a subtle darker-green raindrop pattern, short round wooden legs, straight-on front view | 16:9 |
| `Art/Props/cloche.png` | A closed silver-blue serving dome cloche with a round knob on top, front view | 1:1 |
| `Art/Props/noodle_bowl.png` | A yellow ceramic bowl full of noodles with green bok choy leaves, front view, slightly from above | 1:1 |
| `Art/FX/heart.png` | A single soft pink puffy heart | 1:1 |

### Made locally (no Meshy)

- `Art/UI/Logo/logo_XX_<letter>.png`: one sprite per letter of "NoriVerse" so each can pop in individually.
  - Rounded bold font (`/System/Library/Fonts/Supplemental/Arial Rounded Bold.ttf`; the reference looks like Nunito/Varela Round).
  - "Nori" #57B6CA, "Verse" #7ECA4E.
  - `build_logo()` in the old `build_splash_art.py` already does this and writes `logo_layout.json` (each letter's x offset and pivot). Reuse that function and delete the rest of that script.
- `Art/UI/Logo/tagline.png`: "Play. Learn. Explore." in #6A6A6A (Avenir Next Demi Bold, `.ttc` index 2).
- `Art/FX/ground_shadow.png` (soft radial ellipse) and `Art/FX/white_pixel.png` (fade overlay).

### Post-processing each generated sprite

1. Check the alpha is clean: no white halo, no leftover background blobs (the test pose has one small dark wedge near the tail, fix if visible).
2. **Align all full-body poses:** same character scale (match head width) and the same feet/body anchor, so pose swaps don't jump. Compute it from each image's alpha bounding box and head width, then rescale and pad to a common canvas, or set per-sprite pivots. Variants made with `--from-task` should already align; verify.
3. Import into Unity as Sprite (Single), compression none or high quality, a consistent Pixels Per Unit (≈ sprite px for a 2.5 u head width), and a pivot at bottom-centre (feet) for full-body poses.
4. **Visually compare every pose against the references.** Same face, mask, outfit and colours. Regenerate any that drift (AGENTS.md: "Do not generate a completely different raccoon for every animation").

---

## 5. Unity implementation plan

### Animation approach

- With whole-pose illustrated sprites, use **sprite swaps for poses and expressions**, plus **transform animation** on the character root and props (rise, jump, squash, lean/rotate, bob, slide, fly-off, fades).
- Keep it **one Unity scene** (`Assets/_Project/Scenes/Splash.unity`).
- Build the timeline as **one AnimationClip** (`Animations/Splash/SplashIntro.anim`) played by an **Animator** (`SplashIntro.controller`, a single state, no loop). Then:
  - The real timing lives in Unity animation data, editable in the Animation window.
  - Testing can **sample the clip at any time** (`AnimationClip.SampleAnimation`) and render frames to compare with the video.
- Generate the clip from code in an Editor script (curves for `localPosition`, `localEulerAngles`, `localScale`, `SpriteRenderer.m_Color.a`, and `ObjectReferenceKeyframe` curves on `SpriteRenderer.m_Sprite` for pose swaps). Hand-keying hundreds of keys isn't reasonable.
- Add an **AnimationEvent** at 11.40 s calling `SplashController.OnIntroFinished()`.

### Scene hierarchy (suggested)

```
Splash (scene)
├── Main Camera            orthographic, size 5, background #FDFDFD, landscape
├── SplashRoot             Animator (SplashIntro.controller) + SplashController
│   ├── Logo               (scale/position animated: phase A → phase B)
│   │   ├── Letters/L0..L8 SpriteRenderer each (pop-in scale curves)
│   │   └── Tagline        SpriteRenderer (alpha fade)
│   ├── PeekNori           SpriteRenderer (peek_* sprite swaps), rise/sink
│   ├── NoriStage          SpriteRenderer (stand/lean/…/chew sprite swaps), jump/squash/lean
│   │   └── GroundShadow
│   ├── Table              slides in from left; order in front of NoriStage
│   │   ├── NoodleBowl
│   │   └── Cloche         flies off at 4.75 s
│   ├── Hearts/H0..H4      pop/pulse/fade
│   └── FadeOverlay        full-screen white sprite, alpha 0→1 at 11.07–11.40
```

- If peek paws must rise before the head (0.07–0.27 s), and the generated peek sprite has paws baked in, either:
  - mask the sprite's lower part with a `SpriteMask` while it rises, or
  - generate separate paw sprites, or
  - keep the rise fast so the difference is negligible.
  Paws-first is a visible beat in the reference, so prefer keeping it.
- Sorting order (back to front): NoriStage < NoodleBowl/Cloche < Table < Hearts < Logo < FadeOverlay.
  - Check the bowl reads as sitting on the table top in front of Nori's chest.
- Scale the FadeOverlay to cover any aspect ratio. `ScreenFitter`-style code that reads `Camera.orthographicSize * aspect` is fine.

### Scripts (small, single-purpose, per AGENTS.md §6)

- `Scripts/Core/SplashController.cs`:
  - Plays the intro on Start.
  - Exposes a `UnityEvent OnSplashComplete`, plus an optional next-scene name (empty for now).
  - Handles the animation event.
  - No `Update` loop.
- `Scripts/Utilities/CameraBackgroundFitter.cs` (or similar): keeps the white background and fade overlay covering any aspect ratio, and handles safe areas if needed.
- `Scripts/Editor/SplashSceneBuilder.cs`:
  - Menu item **NoriVerse/Build Splash Scene**.
  - Sets sprite import settings, builds the hierarchy, generates the AnimationClip and Animator, saves the scene, and adds it to Build Settings.
  - Must be runnable in batch mode with `-executeMethod`.
- `Scripts/Editor/SplashFrameCapture.cs`: samples the clip at given times, renders the camera to PNGs (e.g. 1920×874), and writes them to a folder for comparison with the reference frames.
- **Project settings:** landscape-left/right only. Default resolution 2.2:1 is fine for testing.
- **No user interaction exists in this video.** Don't invent tap-to-skip (AGENTS.md: don't invent gameplay).

### Batch-mode commands

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity"
"$UNITY" -batchmode -quit -projectPath /Users/harshikasharma/Documents/NoriVerse -executeMethod NoriVerse.EditorTools.SplashSceneBuilder.Build -logFile -
"$UNITY" -batchmode -quit -projectPath /Users/harshikasharma/Documents/NoriVerse -executeMethod NoriVerse.EditorTools.SplashFrameCapture.Capture -logFile -
```

Don't use `-nographics` for the capture step, because rendering needs the GPU. Check the log for compile errors.

---

## 6. Testing (required before reporting)

1. The project compiles with no errors or warnings that matter (batch-mode log).
2. The scene builds and saves, the Animator is assigned, and there are no missing sprite references.
3. Capture frames at the same times as the reference sheets: 0.13, 0.33, 0.47, 0.6, 1.0, 1.4, 1.8, 2.13, 2.4, 2.53, 2.67, 2.93, 3.2, 3.47, 4.0, 4.8, 5.0, 5.5, 6.33, 7.0, 7.5, 8.0, 8.5, 9.5, 11.2, 11.4.
4. Put them **side by side** with the rotated reference frames (`Docs/reference_frames/` or re-extract from the video) and check:
   - positions, scales and timing
   - character consistency
   - nothing overlapping wrongly (the hat vs. the tagline)
   - the fade completes
5. Fix differences and re-capture.
6. If possible, also do a short play-mode run (or a built macOS player) to confirm real-time playback and that the completion event fires.
7. Report with the AGENTS.md §12 template.
   - Say explicitly what wasn't verified (e.g. audio, a real device).
   - The **pot at 11.6 s is intentionally left for video 002**.

---

## 7. Things to avoid

- Don't redraw or vectorise Nori in code. Don't use the old project art. Don't use the artist-outfit atlas.
- Don't add ®, tap-to-skip, the ending pot, or anything not in the video.
- Don't print the Meshy key. Don't call the Meshy MCP tools (401 in this app); use `meshy_image.py`.
- Don't spend Meshy credits far beyond the approved batch without asking.
- Don't claim tests you didn't run.
