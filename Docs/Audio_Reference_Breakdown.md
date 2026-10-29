# Audio reference breakdown (Bimi Boo videos) and NoriVerse audio plan — 2026-10-28

## How this was analysed
No one listened to the audio. Each track was extracted with a bundled ffmpeg and analysed locally:
loudness envelope, onset detection, tempo/key estimate, labelled spectrograms (`Docs/reference_audio/v*_spec.png`)
and frames at the key moments (`v1_frames.jpg`, `v2_frames.jpg`). Speech was checked with a local Whisper model:
**no words were found in any video** (only low-confidence noise), so the reference uses music, sound effects and
non-verbal character vocals (sing-song "oohs", giggles, munching), not spoken sentences. Sound identities below are
inferred from spectrogram shape + what is on screen; exact timbres are unverified.
Scripts: `Tools/AudioPipeline/{envelope,music,spec,frames}.py`.

| File | Video | Length | Audio |
|---|---|---|---|
| `Cooking splash_Video_001.mp4` | 001 Splash | 12.5 s | stereo AAC 44.1 kHz |
| `MicrosoftTeams-video.mp4` | 002 Kitchen + batter | 58.4 s | same |
| `MicrosoftTeams-video (1).mp4` | Different Bimi Boo splash (artist outfit) — not part of this game | 7.2 s | same |
| `VID_20261028_005627 (1).mp4` | 003 Dough → sorting | 130.7 s | same |
| `VID_20261028_005627_1.mp4` | 004 Decorate → bake → feed | 134.3 s | same |

## Music
- **Splash jingle:** ~9 s, bright, ~150 BPM, G major. Brand vocal on the logo letters (two sung syllable sweeps at 0.7–1.3 s and 2.0–2.5 s = "Bi-mi … Boo!"), then a light eating/table tune 3–8 s, a sparkle tail at 8–9 s, silence from 9.3 s.
- **Gameplay loop:** one continuous, quiet (≈ −30 dB) cheerful loop plays under every gameplay screen (kitchen, cooking, dough, prep, sorting, oven, feeding). ~145–150 BPM with a 4-beat bar of ~1.65 s, A major / E. Music never stops between scenes; it only dips during transitions.

## Sound map (reference time → event)
**Splash (001)** 0.7–2.5 brand vocal on logo · 3–8 munching/eating tune while the character eats · 8–9 sparkle.
**Kitchen (002)** 0.5 pot on white (jingle tail) · 2.6 kitchen loop starts · 10.2/11.0/12.4 character vocals with worried → surprised → yawn faces · 17.5 whoosh to cooking · 18.5–19.1 rising pop per ingredient appearing · 30–40 soft "plop" each time an ingredient lands in the bowl · 40.5 descending whoosh as the whisk flies in · 52–57 continuous whisking swish · 55.5 rising arpeggio when batter is done.
**Dough → sorting (003)** 14–19.5 rolling-pin texture while rolling · ~20 success arpeggio · 40–44.7 cutting trace · 44.7–46 sparkle burst when cut completes · 54–56 & 66–74 squishy sauce painting · 74.5 success chime · 88–100 rhythmic sprinkle ticks for cheese · 100.5 confirm + success arpeggio · 115–130 cheerful ding per correct sort · 130 final arpeggio.
**Decorate → feed (004)** 12.5–35.5 soft pop per topping placed · 36 confirm arpeggio + whoosh · 37.5 rack slide · ~46.5 power click · 47–53 steady oven hum (flat noise band) · 53.7 reward chime · 55–61 star dings with character cheers · 62–64 confetti burst · 66 return whoosh · 74–80 character "ooh/mmm" as food approaches · 90.5–97 munching/chewing + happy vocals · 101–133 giggles, satisfied hums, bubbly sparkles.

## NoriVerse audio list (game event → sound)
Proposed clips follow the reference events exactly; no extra features. Nori's voice = ElevenLabs **"Nori - Spoonderful"** (`FleaHIqMqAL0jx5Znrdf`, "bright, warm, slightly high, bouncy and playful"). Non-verbal lines use Eleven v3 audio tags.

### Music (ElevenLabs Music) — 2 clips
| id | use | brief |
|---|---|---|
| mus_splash | Splash | 9 s cheerful kids' brand jingle, G major ~150 BPM, glockenspiel/ukulele, sparkle ending |
| mus_gameplay_loop | Kitchen + all activities, looping | 60 s seamless loop, A major ~145 BPM, soft marimba/ukulele/light percussion, gentle and unobtrusive |

### Nori voice (TTS, Nori voice, eleven_v3) — 12 clips
| id | use | text |
|---|---|---|
| vo_brand | Splash logo | "Nori… Verse!" (sung-style, matches the "Bimi Boo" call) |
| vo_giggle | Kitchen happy / tap reaction | [giggles] hee-hee! |
| vo_hmm_worried | Idle worried mood | [softly] hmm… |
| vo_yawn | Idle sleepy mood | [yawns] aaah… |
| vo_ooh | Surprise / dish chosen | [excited] ooh! |
| vo_yay | Step complete / stars | [cheerful] yay! |
| vo_wow | Reward reveal | [amazed] wow! |
| vo_mmm_anticipate | Pizza dragged near mouth | [hungry] mmm! |
| vo_nom | Bite | [munching] nom! |
| vo_chew | Chewing | [chewing happily] mm-mm-mm |
| vo_yummy | Satisfied after eating | [satisfied] mmm, yummy! |
| vo_hum_happy | Post-meal idle | [happy humming] hmm-hmm-hmm |

### Sound effects (ElevenLabs Sound Effects) — 24 clips
ui_tap · dish_locked_wobble · scene_whoosh · item_pop_in · item_pickup · drop_plop_bowl · drop_return · whisk_fly_in · whisk_stir_loop · success_arpeggio · rolling_pin_loop · dough_cut_trace · cut_complete_sparkle · sauce_squish_loop · cheese_sprinkle · sort_correct_ding · topping_place_pop · confirm_button · oven_rack_slide · oven_power_click · oven_bake_loop · oven_done_ding · star_ding · confetti_pop

Exit dialog open/close reuses `ui_tap`. Hints stay silent (no hint sound was found in the reference).

## Cost estimate (plan — not spent)
Balance at analysis time: 1,367,772 characters/credits remaining of 1,500,000 (Pro).
- Voice: 12 short lines ≈ 300 characters ≈ **300 credits** (one take each).
- Sound effects: 24 clips, ~1–3 s each (loops ~4 s) ≈ **~1,000–2,500 credits**.
- Music: 2 clips (≈70 s total) ≈ **a few thousand credits** depending on ElevenLabs' current music rate.
Exact cost is read from the subscription before and after generation and reported.

## Integration (after approval)
Files → `Assets/_Project/Audio/{Music,Voice,SFX}`. A small `AudioDirector` (persistent like `SceneTransition`) owns one music source (keeps the loop playing across scenes, dips during fades) and a pooled SFX/voice player with per-clip volume and a short anti-spam cooldown. Gameplay scripts call `AudioDirector.Play(...)` at existing events; no gameplay changes. PlayMode tests assert the right clip plays on each event. Device loudness check remains manual.
