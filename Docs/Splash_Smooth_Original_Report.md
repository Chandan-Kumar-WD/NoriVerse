# Splash animation refinement — original version retained

The user's latest instruction superseded the articulated splash plan: keep the older version, improve its animation and smoothness. The original art, green table, bowl, cloche, logo, layout and overall duration remain in use. The 12 images generated under the approved 108-credit batch before this steering are retained unused; no additional images were generated. The experimental rig implementation is archived outside Unity compilation under `Tools/ArtPipeline/archived_splash_rig/`.

The original pose sequence now blends through an eased two-texture shader rather than abruptly changing the entire character sprite. The blend preserves full opacity in shared body regions. Repeated bites have readable half-second beats with a restrained eased lean and settle. Blinks retain shorter transitions. The lower-body cut moves from the uneven table hem to an area safely covered by the opaque cloth, including the existing bounce and lean motions.

Runtime: `Assets/_Project/Scripts/Gameplay/Splash/SplashPoseSmoother.cs`; shader: `Assets/_Project/Shaders/SplashPoseBlend.shader`. The existing splash builder retains the old scene construction and attaches only the smoother. Original cut sprites are regenerated from the same original full-body poses; `process_art.py` records the new safe cut row.

Validation: full scene rebuild and PlayMode suite, including rendered samples before/during/after an eating transition and an assertion that the discarded DiningRig is absent. Results: `scratchpad/splash_smooth_results.xml`; log: `scratchpad/splash_smooth_tests.log`. Captures: `Docs/playmode_captures/splash_smooth_original/`. Audio/device checks remain unverified. The full game is reopened from Splash after verification.

Final validation: all 18 PlayMode tests passed, 0 failures. The original splash was inspected at stable and intermediate eating frames; the crop stays hidden behind the tablecloth. Unity is relaunched from Splash in Play Mode.
