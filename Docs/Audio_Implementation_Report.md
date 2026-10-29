# Audio Implementation Report — 2026-10-28

**Reference:** audio tracks of the four Bimi Boo videos (splash, kitchen/batter, dough→sorting, decorate→feed). Analysis and sound map: `Docs/Audio_Reference_Breakdown.md`.

**Status:** Completed in code and automated tests. Not yet listened to by a person; mix and real-device playback unverified.

**Assets created (ElevenLabs, approved "Generate all"):** 38 clips, 2,539 credits in total (subscription count 132,228 → 134,767).
- Voice (Nori - Spoonderful, eleven_v3): vo_brand, vo_giggle, vo_hmm_worried, vo_yawn, vo_ooh, vo_yay, vo_wow, vo_mmm_anticipate, vo_nom, vo_chew, vo_yummy, vo_hum_happy. Local Whisper check confirmed the words. Retakes: `vo_nom` (first take read as "Mmm, no!") and `vo_brand` (best of three; Whisper hears "Nori first!" unprompted, "Nori Verse" when primed — needs a human listen). Rejected takes: `Tools/AudioPipeline/raw/v1_rejected/`.
- 24 sound effects, 5 of them seamless activity loops (whisk, rolling pin, dough cut, sauce, oven).
- Music: 9 s splash jingle, 60 s gameplay loop (end crossfaded into start).
- Processed to mono OGG with trimmed silence and per-category loudness: `Assets/_Project/Audio/{Music,Voice,SFX}`.

**Runtime:** `Scripts/Audio/AudioDirector` creates itself on first use and survives scene loads. Music continues from Kitchen through every activity; Nori speaks one line at a time; one-shots skip immediate repeats (game time); activity loops play only while the finger moves (`Sustain`) and fade in 0.18 s. `AudioLibraryBuilder` (first step of NoriVerse/Build All Scenes) applies mobile import settings and writes `Resources/NoriVerseAudio.asset`.

**Events wired:** splash jingle, name call, eating; kitchen loop, locked wobble, dish choice "Ooh!", idle worried hmm/yawn, giggle on touch; ingredient pop-ins, pickup, bowl plop, wrong-drop return, whisk fly-in, stirring; rolling, cutting, cut sparkle; sauce painting, cheese sprinkles, confirm pop/press; sort pickup, correct ding, wrong return; topping pickup/place/return; oven rack, power click, bake hum, done ding; reward chime + "Wow!", rising star dings, "Yay!", confetti; feeding pickup, "Mmm!" near the mouth, "Nom nom nom", chewing, "Mmm, yummy!", giggle/hum reaction, missed-drop return; scene whoosh; back button and exit dialog buttons. Hint animations stay silent, as in the reference.

**Testing:** Batch rebuild of all scenes succeeded with no compiler errors (`BuildReports/audio-build.log`). PlayMode: **29/29 passed** (`BuildReports/audio-tests.xml`), including 6 new audio tests (library completeness; splash cues; kitchen music + locked dish; cooking pop/pickup/plop/return; rolling loop starts and stops; oven/reward beats with one ding per star; feeding sound order) and all 23 earlier tests. The first run found that repeat-skipping used real time, which dropped closely spaced pops and star dings; fixed by using game time.

**Fix after user review:** no scene had an AudioListener (scene builders create cameras without one), so the game was silent although every sound started. `AudioDirector` now adds its own listener and enables it only when the scene has none; tests assert exactly one active listener on Splash and Kitchen. Rebuild + 29/29 PlayMode tests passed again.

**Pending:** listening review and volume balance; physical-device audio; the unrelated Bimi Boo artist splash (`MicrosoftTeams-video (1).mp4`) was not used.
