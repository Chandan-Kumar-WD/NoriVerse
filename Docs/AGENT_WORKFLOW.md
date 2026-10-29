# NoriVerse — How We Work (agent handoff)

For future artwork, animation, and transition enhancements, also read [Animation and Art Enhancement Plan](Animation_And_Art_Enhancement_Plan.md). It records the 2026-10-29 discussion, the user's feedback-led role, and the development model's responsibilities. It preserves existing gameplay and the prior splash decision; proposed techniques are not blanket authorization to replace working systems.

Read this with `AGENTS.md` (project rules) and the per-video breakdown files in `Docs/`. This file is the **process** that has produced Video 001 (splash) and Video 002 (kitchen + batter). Follow it so the next agent produces the same kind of result.

---

## 0. Non-negotiables from the user

- **One video at a time.** Analyse it fully, implement it, test it, report it. Do not start the next video until told.
- **Match the reference video.** Same timing, layout, animation and transitions. Only the character (Nori), the name ("Nori" blue, "Verse" green), the tagline ("Play. Learn. Explore.") and the art style change.
- **Art is new.** Never reuse earlier project art (`~/Desktop/NoriCooking`, `~/Desktop/spoonderful`, `~/.codex/generated_images`, the artist-outfit atlas). Nori's own reference images are *Meshy references only*, never in-game sprites.
- **Do not draw Nori or props in code.** The user rejected a hand-coded vector Nori. Use Meshy image generation for art. Code is only for sprites that are simple shapes (dashed arrow, touch dot, shadows, white fills).
- **No ® mark**, no extra features not in the video, no tap-to-skip.
- **Ask before spending Meshy credits** beyond an approved batch. Show the count and cost first.
- **Never print the Meshy key.** Never echo `MESHY_API_KEY`.
- **Smooth transitions, not image-to-image jumps.** Cross-fade expressions, add in-between frames for big changes, ease all motion.
- **Report honestly.** Say what was not verified (audio, real device, anything not run).

---

## 1. Tooling that exists

| Thing | Where | Notes |
|---|---|---|
| Unity | `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity` | Unity 6000.6.7f1, project at `/Users/harshikasharma/Documents/NoriVerse` |
| Python venv | `scratchpad/venv` (opencv-headless, numpy, pillow, resvg-py) | Recreate if missing: `python3 -m venv venv && venv/bin/pip install opencv-python-headless numpy pillow` |
| Meshy client | `Tools/ArtPipeline/meshy_image.py` | Direct REST. **Use this, not the Meshy MCP tools** (they return 401 in the app) |
| Meshy batch scripts | `Tools/ArtPipeline/gen_splash_art.py`, `gen_kitchen_art.py` | Job dictionaries: prompt, references, aspect, `from_task` for variants |
| Post-processing | `Tools/ArtPipeline/process_art.py`, `process_kitchen_art.py` | Crop, align, chroma-key, write `layout.json` |
| Raw Meshy output | `Tools/ArtPipeline/meshy_raw/` (and `kitchen/`) | Rejected versions go in `v1_rejected/` |
| Reference frames | `Docs/reference_frames/`, and extract fresh with OpenCV | Video frames are rotated 90° CCW (splash) or letterboxed (kitchen) |
| Breakdown docs | `Docs/Splash_Video_001_Handoff.md`, `Docs/Kitchen_Video_002_Breakdown.md` | Timelines, layout in Unity units, prompts |
| Captures | `Docs/playmode_captures/`, `Docs/captures/` | Frames from the real running game, compared with the video |

Meshy credit check (free, safe to print the balance only):
```bash
curl -s -H "Authorization: Bearer $MESHY_API_KEY" https://api.meshy.ai/openapi/v1/balance
```

Cost per image: `nano-banana-pro` = 9 credits (the model the user chose: "best model you can use").

---

## 2. The procedure, per video

### Step 1 — Get the video
- Find it (user gives a path in `~/Downloads`). Check size, fps, frame count, dimensions.
- Splash video was 1748×3840 at 60 fps, recorded sideways: **rotate 90° CCW** to get landscape.
- Kitchen video was 1280×720, letterboxed. Find the content rows (`mean > 20`) — it was rows 68–650, aspect 2.2:1.

### Step 2 — Extract every frame
```python
import cv2
cap = cv2.VideoCapture(path); i = 0
while True:
    ok, f = cap.read()
    if not ok: break
    cv2.imwrite(f'{out}/f{i:04d}.jpg', f, [cv2.IMWRITE_JPEG_QUALITY, 88]); i += 1
```

### Step 3 — Contact sheets, then zoom
- Build sheets of frames every 0.25–0.5 s (e.g. 4–8 per row, labelled with time).
- Read them, find the beats (appear, move, change, stop).
- For busy beats, make dense sheets (every 0.05–0.1 s) and crop to the area of interest.
- For expressions, crop the face region and sheet every 0.1 s — this is how the kitchen moods were found.

### Step 4 — Write the breakdown file (before any art)
Save `Docs/<Name>_Breakdown.md` containing:
- Source, fps, duration, aspect, letterbox crop.
- Unity unit conversion: camera orthographic size 5 (10 units tall); state the px-per-unit used.
- A timeline table: time | what happens | animation/easing.
- Layout in Unity units for every object (measured from frames, not guessed).
- Colours sampled from the video (`f[y,x]` median in a small box).
- Asset list with the exact prompt for each image.

The user approved every plan before art was generated. Present the plan and wait for approval when it spends credits.

### Step 5 — Plan Meshy generation (ask first)
- List every image: name, prompt, references, aspect, cost.
- Show total credits and the model. Get explicit approval ("generate everything").
- The user has approved: a **test pose first**, then the full batch.

### Step 6 — Generate with Meshy
Use `meshy_image.py` / the batch scripts. Key rules that worked:

- **Character references** (Nori): `Tools/ArtPipeline/references/nori_ref_wave.png` (best), `nori_ref_celebrate.png`, `nori_ref_think.png`, plus the approved test pose. Max 5 references.
- **Prompt skeleton for Nori:** the style text (fluffy, soft rendered, white chef hat with yellow goggles, red neckerchief, white jacket, teal apron, striped tail), "single character, centred, generous empty margin, plain white background, no shadow, no text". Always ask for margin: the hat was clipped once.
- **Variants:** generate one base pose, then make expression variants with `--from-task` or `refs=[base]` and prompts like "identical pose, framing, size, position; change ONLY the mouth". This keeps frames aligned so cross-fades don't jump.
- **Props and backgrounds:** text-to-image with `text_only=True`. For objects that are **white on white** (plate, flour, tray, bowl, pancakes), generate on a **solid magenta** background and chroma-key it. Meshy's `remove_background` is not reliable on white.
- **Backgrounds** (wall, counter, table runner): `keep_background=True`, fill the whole frame, no objects.
- **Check every batch.** Build a contact sheet, look at it, and regenerate anything that drifts (different face, extra limbs, cut-off, wrong colour). Keep rejected versions in `v1_rejected/`.
- **Failures:** timeouts happen. Check the log, regenerate only the failed name (the script skips names already present).
- Long batches: run in background and wait with a `until ! pgrep` loop, not repeated polling.

### Step 7 — Post-process into Unity-ready sprites
- Trim to content (`alpha > 8`), keep a few px of pad.
- Chroma-key magenta by sampling the **corner colour** (Meshy's magenta varies per image).
- De-spill edge pixels; zero RGB where alpha is 0 (prevents halos under bilinear filtering).
- **Align full-body poses** on one canvas with a shared feet anchor, so swaps don't jump. Check with IoU of the alpha masks between a pose and its variant (should be > 0.95).
- Write `layout.json` with `ppu` (= sprite px width ÷ units wide) and pivot for each sprite. The Unity importer (`ArtSpec.cs` / `ArtImportSettings.cs`) reads it, so sprites import at the right on-screen size.
- Cut "below-the-counter" versions with a hard alpha cut plus a short fade band, rather than a sprite mask. **Sprite masks did not render in a real play run** (they worked only in sampled captures). Bake the cut into the sprite.

### Step 8 — Build the scene from code
Pattern used for every scene:
- **`XxxSceneBuilder.cs`** (Editor): creates the scene, camera, sprites, components, animation clip, Animator, saves the scene, registers it in Build Settings. Exposed as a menu item and as a static `Build()` for `-executeMethod`.
- **Runtime controllers** (`Scripts/Gameplay/...`, `Scripts/Characters/...`): small single-purpose components. No giant monolith. Serialized references, `Configure(...)` methods so the builder can wire them.
- **Tweens:** `Utilities/Tween.cs` (coroutines), `Utilities/Easing.cs`. No animation package needed for interactive scenes. The splash used an `AnimationClip` (sampled for frame capture); interactive scenes use code tweens.
- **Input:** `Utilities/PointerInput.cs` reads touch or mouse through the Input Manager. It has `InjectForTests()` so tests can act as a finger.
- **Expressions:** `Utilities/SpriteCrossfader.cs` cross-fades between poses (the incoming pose fades in on top; the outgoing one stays visible until done).
- **Scene change:** `Core/SceneTransition.cs` — white fade out, load, fade in. Created at runtime, survives scene loads.
- **Camera framing:** `Utilities/CameraFramingFitter.cs` (fixed design width, grows the orthographic size for taller screens). Background colour fills extra room. **Always test at 16:9 and 4:3, not only 2.2:1.**

Camera rule that fixed a real bug: a design built for 2.2:1 is cut off on 16:9. Set `designWidth` to the full design width (e.g. 22.4 u) and extend backgrounds past the art.

### Step 9 — Batch build
```bash
cd /Users/harshikasharma/Documents/NoriVerse
UNITY="/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity"
"$UNITY" -batchmode -quit -projectPath "$PWD" -executeMethod NoriVerse.EditorTools.BuildAllScenes.Run -logFile /tmp_or_scratch/build.log
grep -n -E "error CS|Exception|NoriVerse:|Missing" /tmp_or_scratch/build.log
```
Exit code 0 plus `NoriVerse: ... built` lines means it compiled and built. Check the log for `error CS`.

### Step 10 — Tests (Unity Test Framework, PlayMode)
```bash
"$UNITY" -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults results.xml -logFile test.log
```
Check the results XML: `result="Passed"` and `failed="0"`. Read failures with a regex on `<test-case … result="Failed">` and its `<message>`.

Patterns that worked:
- **Real play-mode, not sampled.** Load the scene, wait real time, assert state.
- **Check that things are drawn, not only active.** A pixel count in a region of the captured frame (non-background pixels) caught a real bug the state checks missed.
- **Drive input like a finger:** `PointerInput.InjectForTests(Began/Held/Ended, worldPos)` per frame, then `ClearInjected()`.
- **Fixed frame rate in tests:** `Time.captureFramerate = 60` in `[SetUp]`, reset to 0 in `[TearDown]`. Without it, headless drags finish too fast to be real.
- Batch mode does **not** support `WaitForEndOfFrame` (throws). Capture with `Camera.Render()` into a `RenderTexture`, then `ReadPixels`.
- Set `cam.aspect` before rendering to test other screen shapes, and reset it afterwards.
- Tests that wait on time: keep the waits generous. Scene load adds time.

### Step 11 — Capture and compare with the video
- Save frames from the **running game** (`Docs/playmode_captures/`) at the same times as the reference.
- Build side-by-side sheets: reference (left) vs game (right), both resized to the same size, labelled with time.
- Look for: position, scale, timing, missing sprites, overlap, wrong order, colour.
- Fix and recapture. Iterate until frames match the reference closely.
- **Do not claim a match you have not looked at.** The user checks in the Editor too, and has caught misses that the tests did not.

### Step 12 — Let the user see it
- After a build, relaunch the editor on the first scene in Play mode so the user can see it:
```bash
nohup "$UNITY" -projectPath "$PWD" -executeMethod NoriVerse.EditorTools.SplashRunner.OpenAndPlay -logFile run.log >/dev/null 2>&1 &
```
- Kill it first if it is open (`pkill -TERM -f "Unity.app/Contents/MacOS/Unity -projectPath"`), otherwise batch runs fail on the project lock.
- Tell the user to press Play if it does not start playing.

### Step 13 — Report (AGENTS.md §12 format)
Video name, status, scenes, assets created, animations, gameplay, testing (exact commands and results), pending items, next step. Say what was **not** verified (audio, device). Do not mark complete while critical behaviour is missing.

---

## 3. Mistakes we made — do not repeat

1. **Vector-drawn Nori** — rejected. Use Meshy.
2. **Reusing old project art** — user said never. Generate everything new.
3. **Sprite masks to hide the lower body** — looked right in sampled captures, did not draw in a real run. Bake a cut sprite.
4. **Play-mode test only checked `activeInHierarchy`** — the character was invisible and the test passed. Add a pixel check on the rendered frame.
5. **Framing for 2.2:1 only** — cut off on 16:9 (the user's Game view). Test several shapes.
6. **White-on-white Meshy images** — background removal fails. Use magenta and corner-sampled keying.
7. **Hard expression swaps** — the user saw "image to image". Cross-fade, slow down, add in-between frames.
8. **Test timing drift** — scene load adds seconds. Use generous waits, or assert on state changes not exact times.
9. **Using the Meshy MCP tools** — 401 in the app. Use `meshy_image.py`.
10. **Sprite flipping/mirroring errors** — a mirrored hand needs `flipX`, and check the pivot.
11. **Spending credits without asking** — always show count and cost, get approval.
12. **Batch-mode `-executeMethod` while the editor is open** — the project lock makes it fail. Close the editor first.
13. **Editing a file the user changed** — if a file changed on disk, re-read it before editing.

---

## 4. Conventions

- **Units:** 1 Unity unit = 200 design px (some sprites use 1.5× render scale). Camera orthographic size 5, origin centre, y up.
- **Sorting order:** back (0) → background → props → characters → UI → overlay (white fade) at the top. Write the order table in the scene's breakdown.
- **Naming:** sprites `nori_<pose>.png` (full body), `nori_bust_<mood>.png` (kitchen bust), `<prop>.png`. Scenes `Splash`, `Kitchen`, `Cooking`.
- **Folders:** `Assets/_Project/{Art, Animations, Scenes, Scripts/{Core,Utilities,Characters,Gameplay/<Scene>,Editor}, Tests/PlayMode, Settings}`.
- **Asmdefs:** `NoriVerse.Runtime`, `NoriVerse.Editor` (editor-only), `NoriVerse.PlayModeTests`.
- **Git:** the user did not ask for commits. Do not commit unless asked.

---

## 5. Current state (update as you go)

- **Video 001 (splash):** built, tests pass, runs to the kitchen. Breakdown: `Docs/Splash_Video_001_Handoff.md`.
- **Video 002 (kitchen + batter):** built; mood and layout fixes applied. Nori gets sad while idle and cheers on touch. No kitchen top buttons. The centre round tomato/basil pizza is the only unlocked dish. Current food art is the newly generated full plated set; the rightmost vegetable pizza is distinct.
- **Cropping root causes fixed:** kitchen bust canvas widened from 690 to 1000 px at the same 150 PPU and face anchor; `SpriteCrossfader` now uses only `baseSortingOrder` and `baseSortingOrder + 1`, so repeated expression changes cannot overtake the counter's layer. Raised hands are fully visible; resting paws stay above the counter and fade during raised-arm moods.
- **Counter correction after user review:** the extra dish-display ledge was rejected and removed. Keep ONE existing yellow countertop, with the painted blue strip removed by `counter_asset()`. User rejected vertically compressed food and explicitly requested new images. Seven complete plated dishes are generated by `gen_served_dishes.py` (documented batch estimate 63 credits) and keyed/imported by `process_served_dishes.py`. Each includes plate and food in a natural serving perspective; keep `localScale = Vector3.one`. The centre tomato/basil pizza alone opens Cooking; the rightmost veggie pizza is distinct. Lock badges are smaller (0.63 scale), attached over each plate's left rim at local (-1, -0.1), instead of floating above it. Dish row remains y=-3.65, separate from Nori's hands. The illustrated herb shelf remains.
- **Cooking back-button placement:** `ScreenCornerAnchor` positions the existing sprite inside the top-left safe area with a 0.35-unit inset, after camera framing. The fixed old (-9.78, 4.1) world position appeared too low at 16:9. Tests check corner placement at four screen shapes.
- **Paw art correction:** rectangular extraction included apron fragments; fur-only extraction lost cuffs. User explicitly authorized regeneration to solve the issue. `gen_counter_paws.py` generated one new pair of complete paws and cuffs, split/keyed by `bust_paws()` and displayed above the counter, following the neck transform. Do not restore the rectangular crops. Original two shelf images remain archived as source; only the herb shelf is used.
- **Validation on 2026-10-28:** kitchen batch build succeeded; all 7 PlayMode tests passed (`scratchpad/kitchen_results.xml`), including a new repeated-expression layering regression, scene flow, drag/stir completion and four aspect ratios. Actual running captures visually reviewed at 16:9 and raised-arm timing (`kitchen_14.4.png`).
- **Video 003:** implemented from `/Users/harshikasharma/Downloads/VID_20261028_005627 (1).mp4` (130.683 s). Full flow is now Splash → Kitchen → Cooking → Dough → PizzaPrep → ToppingSort. Detailed breakdown/report: `Docs/Pizza_Video_003_Breakdown.md`, `Docs/Pizza_Video_003_Report.md`. That clip ends after sorting four ingredient categories; the subsequently supplied Video 004 now provides the decoration, oven and serving continuation.
- **Video 003 art:** 16 images explicitly approved up to 144 credits. Prompts in `gen_pizza_flow_art.py`; processor `process_pizza_flow_art.py`; raw art `meshy_raw/pizza_flow/`; imported art `Assets/_Project/Art/PizzaFlow/`. Imported sizing entries merge into `Art/Kitchen/layout.json`, also copied to `Art/PizzaFlow/layout.json`. Run the new processor after any deliberate reprocessing of new art. Main kitchen processing now preserves unrelated manifest entries.
- **Video 003 mechanics:** real pin travel → ordered clockwise cut → unique-pixel sauce painting → cheese touch-sprinkling with explicit confirm → valid-kind drag/drop matching. Idle hints use separate ghost sprites and never modify actual progress. Wrong drops return; completed targets exit and remaining targets recentre. New back buttons return to Kitchen. Menu **NoriVerse/Run Dough** opens the new section directly.
- **Video 003 QA:** 12 PlayMode tests passed, including old flow regression and new gameplay, back navigation and four screen ratios. Latest results `scratchpad/video003/results_final.xml`; actual captures `Docs/playmode_captures/video003/`; state comparisons `Docs/reference_video003/reference_vs_game.jpg`. Reviewed and fixed magenta cutout fringes, pin clipping, dough overlap and grey target silhouettes.
- **Video 004:** full 134.233-second reference `/Users/harshikasharma/Downloads/VID_20261028_005627_1.mp4` analyzed. Full playable chain: Splash → Kitchen → Cooking → Dough → PizzaPrep → ToppingSort → PizzaDecorate → PizzaBake → Kitchen feeding. Decoration allows unlimited optional topping copies; confirm leads to a power-gated seven-second bake, three stars/confetti, then repeated feeding. Wrong drops return. A short tap on the cooked centre menu dish starts a new recipe; dragging feeds Nori.
- **Video 004 state/art:** `PizzaRecipeState` preserves actual cheese/topping positions, rotations and scales across decoration, baking, reward and the rendered meal snapshot. A new recipe resets it. Nine Meshy images explicitly approved up to 81 credits, all generated by `gen_pizza_finish_art.py`; `process_pizza_finish_art.py` imports oven/background/rack/baked crust/four aligned Nori eating expressions/sun-cloud, plus derived circular avatars. No duplicate oven rack. Preserve opaque raw crust underneath the incoming baked crust.
- **Video 004 QA:** all 16 PlayMode tests passed in `scratchpad/video004/results_final.xml`, including new interaction/persistence, no automatic hint progression, three-star reward, repeated feeding/replay and four screen shapes. Captures `Docs/playmode_captures/video004/`. Breakdown and report `Docs/Pizza_Video_004_{Breakdown,Report}.md`. Menu **NoriVerse/Run Pizza Finish** opens the new decoration section directly in Play Mode; **Run Dough** remains available. Unity was reopened for user testing.
- **Shared exit confirmation (2026-10-28):** every gameplay BackButton (Cooking, Dough, PizzaPrep, ToppingSort, PizzaDecorate, PizzaBake) opens `ExitConfirmation` before navigating. Green check returns to Kitchen; lower red X and upper pink X resume the same activity. Modal pauses scaled time and blocks all shared gameplay pointer input; dismissal suppresses the current press to prevent click-through. Dim/panel/text/buttons are simple runtime UI; no new generated art or credits. Kitchen remains without top buttons per the user. Oven back navigation is available during rewards too. Keep `Physics2D.SyncTransforms()` before back-button hit testing because corner anchors can move after physics, including across pause/resize.
- **Pizza preparation polish (2026-10-28):** shared `Check()` now builds a glossy rounded green button with an even border and a continuous white check instead of a stretched circle. Prep category tab uses simple rounded UI and a circular plate badge; sauce reuses the sauce disc, cheese reuses a small arrangement of the actual mozzarella shred art (not the Swiss topping). Group fading includes every badge child. Sprinkled shreds use natural uniform scales 1.15–1.65, retained in recipe state. Sauce reveal texture is 512² with the same physical brush softness. No new generated art or credits. All 18 PlayMode tests passed in `scratchpad/prep_polish_results.xml`; inspected `video003/sauce_first_stroke.png` and `cheese_ready.png`.
- **Pizza visual upgrade (2026-10-28):** user approved 14 Meshy nano-banana-pro images up to 126 credits; all generated, no extra variants. Production art `Assets/_Project/Art/PizzaUpgrade/`, raw `meshy_raw/pizza_upgrade/`, exact prompts `gen_pizza_visual_upgrade.py`, processor `process_pizza_visual_upgrade.py`. New ceramic board/filled sauce and cheese ramekins, intact salami, enamel oven/separate rack, bevelled unlit/gold stars and six eating expressions. Shared builders route board/salami consistently across prep/sorting/decorating/baking. Nori facial variants align to the hat and blend only the facial region onto the existing generated bust so hat/outfit/paws remain fixed. Salami uses a solid feathered ellipse derived from the generated disk to eliminate chroma-key rim fragments/pinholes. Batter bowl shifts left .35 and tray/ingredients right .35. Sorting category icons grow from .45 to .70 scale with larger labels. `BakingProgressSparkles` pools moving glints at the actual fill edge. `MealBiteVisual` removes three rounded bites from the actual recipe snapshot, and `NoriEatingAnimator` adds anticipation, alternating chewing, swallow and satisfied follow-through while keeping counter paws planted. Post-meal effects drift independently. Full 18-test run passed (`scratchpad/pizza_upgrade_results_final.xml`); additional salami-edge check recorded separately. New captures `Docs/playmode_captures/pizza_upgrade/{prep,finish}/`. Plan/report `Docs/Pizza_Visual_Upgrade_{Plan,Report}.md`.
- **Splash steering (2026-10-28, latest user instruction):** keep the older splash art/layout and improve only animation smoothness. The articulated rig direction was rejected. Its 12 approved images had already generated (108-credit batch) and are retained unused in `SplashUpgrade/`/`meshy_raw/splash_upgrade/`; experimental rig scripts/test are archived as `.txt` under `Tools/ArtPipeline/archived_splash_rig/` and must not be restored without a new request. Live Splash uses only original poses/table/bowl/cloche. `SplashPoseSmoother` and `SplashPoseBlend.shader` ease transitions between the original same-canvas poses with constant overlapping-body opacity. Bite intervals are .45–.50 seconds, with small eased lift/settle. Original `_cut` images now cut at row 902 (world -3.08), safely inside the opaque cloth, rather than row 1088 at its wavy hem; `process_art.py` records that. All 18 PlayMode tests passed in `scratchpad/splash_smooth_results.xml`. Captures `Docs/playmode_captures/splash_smooth_original/`; report `Docs/Splash_Smooth_Original_Report.md`.
- **Device-prep cleanup (2026-10-28):** 41 unused art files and rejected splash experiments are outside the project in `../NoriVerse_Archive/2026-10-28-device-prep/`, together with historical captures/logs and original paths in its manifest. Reproducible video frame extractions were deleted; original videos remain in Downloads. Keep only active scene art in Assets; source generation tools/raw current art remain in Tools. `.gitignore` excludes Unity state, Builds/BuildReports, captures and local MCP config. Tests write screenshots only with `NORIVERSE_CAPTURE_FRAMES=1`. `ProjectAudit.Run` audits all eight scenes. `IOSBuildTools` exports separate Device/Simulator Xcode projects, IL2CPP, landscape iPhone/iPad, iOS 15 minimum; `MobileFrameRate` requests 60 FPS. iOS art uses ASTC 4×4 with CPU-readable sauce kept RGBA32. The unsigned arm64 device app compiled successfully in Xcode and measures 145.18 MB (138.46 MiB), 60.2% smaller than the initial uncompressed export; this is a Unity development export with native Release compilation. Simulator project export succeeded but simulator runtime/physical install were not tested. See root README and `Docs/Device_Prep_Report.md`.
- **Latest home behavior (2026-10-28):** the user superseded the rapid mood sequence. Keep Nori cheerful for ~25 seconds idle, with sparse blinks and broad smiles. Only worried (12 s) and sleepy/bored (14 s) follow sustained inactivity; remove the surprise/raised-arm/yawn chain from idle behavior. `CompletedPizzaStore` persists a versioned completed recipe under `NoriVerse.CompletedPizza.v1` using PlayerPrefs; `BeginNewRecipe` clears only the unfinished attempt, and Kitchen Awake restores the completed dish if available. Baking calls `MarkCompleted()` once; restarting/replaying never erases saved availability. Feeding guide requires completion: one cue after new completion, then only after 12 seconds idle; any interaction cancels/resets it, and a normal saved-dish launch has no immediate cue. Fresh players never see a feeding guide before completing a dish. Tests use an isolated preference key and passed 23/23 (`BuildReports/home-behavior-tests.xml`). Old builds did not save completion, so one post-update completion is required to establish a save. Report `Docs/Home_Behavior_Report.md`.
- **Audio (2026-10-28):** reference audio analysed locally (no speech; music + SFX + non-verbal character vocals) in `Docs/Audio_Reference_Breakdown.md`. User approved generating all 38 clips with ElevenLabs: Nori voice = "Nori - Spoonderful" (`FleaHIqMqAL0jx5Znrdf`, eleven_v3), 24 SFX, 2 music. Client `Tools/AudioPipeline/elevenlabs_audio.py`, job list `gen_audio.py` (skips existing files), processing `process_audio.py` (trim/normalise/seamless loops → OGG in `Assets/_Project/Audio`), check `verify_audio.py`. Rejected takes in `raw/v1_rejected/`. Runtime: `Scripts/Audio/AudioDirector` (self-creating, persistent; `Music`, `Voice` one line at a time, `Play` with cooldown, `Sustain` for activity loops), `AudioLibrary` in `Resources/NoriVerseAudio.asset` built by `AudioLibraryBuilder` (runs first in Build All Scenes). ElevenLabs MCP is launched via `Tools/AudioPipeline/run_elevenlabs_mcp.sh` (reads the key from ~/.zshrc). Report `Docs/Audio_Implementation_Report.md`.
- **TestFlight (2026-10-29):** App Store Connect app "Nori Cooking" (ID 6828105223, WorksDelight team S8A97WCTL2, bundle `com.noriverse.game`, SKU `noriverse-cooking-001`; the name "Cooking" was taken). Home-screen name is "Cooking" (`PlayerSettings.productName`). Icon = user-supplied AI image `Art/AppIcon/app_icon_1024.png` (source `Tools/ArtPipeline/icon/`). First build **1.0.0 (1)** uploaded via `NoriVerse/iOS/Export for TestFlight (Release)` → `xcodebuild archive` (Builds/iOS-TestFlight → Builds/Archive/Cooking.xcarchive) → `xcodebuild -exportArchive` with `Builds/ExportOptions.plist` (method app-store-connect, destination upload, automatic signing). Next builds: bump `PlayerSettings.iOS.buildNumber` in `IOSBuildTools.Export` (build numbers cannot repeat). Version bumps for releases: `PlayerSettings.bundleVersion`.
- Open items: physical-device testing; listening review of generated audio (no one has heard it). Nori's custom idle sequence remains slower than the reference by user preference (`idleSecondsBeforeSad` in `NoriMoodDirector.cs`).

When the next video arrives, start at Step 1, write its breakdown file, and get approval before spending Meshy credits.
