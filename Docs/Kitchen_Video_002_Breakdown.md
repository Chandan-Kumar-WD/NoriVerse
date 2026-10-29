# Video 002 — Kitchen menu + "mix the batter" cooking screen

Reference: `/Users/harshikasharma/Downloads/MicrosoftTeams-video.mp4` — 58.37 s, 60 fps, 1280×720 recording of a landscape game (letterboxed: content is rows 68–650, i.e. **1280×583 = 2.2:1**, same as the splash).
Coordinates below are Unity units, camera orthographic size 5 (screen 10 u tall, ≈22 u wide), **1 u = 58.3 px** of the content area, origin at the centre, y up.

User decisions: tapping the **first dish** starts the cooking screen (the others are padlocked and only wobble). New scenes only (splash is not retrofitted). Smoothness: pose **cross-fades**, extra in-between images, eased motion — never a hard image swap.

### Latest user overrides — 2026-10-28

The original timeline/layout below records the reference. The current kitchen deliberately follows the user's newer requests: no back/hands/chef buttons; the centre **round pizza** alone is unlocked and opens Cooking. After review, the added front display shelf was rejected and removed: dishes now lie flat on the original yellow counter, with elliptical plate perspective and contact shadows. Basil and mint retain their illustrated wall shelf. Two shelf images were originally approved at a maximum of 18 credits; the front shelf is now unused. Existing round pizza art is reused. User subsequently authorized regeneration for the hand issue: a dedicated pair of complete paws/cuffs replaces flawed extracted fragments.

Nori's bust crop now includes her full raised hands. Cross-fade sorting is bounded to two character layers behind the counter, preventing expressions from eventually exposing the chest's cut edge in front of it. Seven PlayMode tests passed, including repeated expression layering and the interactive cooking flow. Real-device and audio verification remain pending.

Latest visual revision: the user rejected vertically compressed food and requested new fuller artwork. Seven new complete food-and-plate illustrations use a coherent natural serving angle and retain their original proportions. Source: `Tools/ArtPipeline/meshy_raw/kitchen/served/`; processor: `process_served_dishes.py`; imported assets: `Kitchen/Menu/served_*.png`. Centre is round tomato/basil pizza; far right is vegetable pizza. Lock badges move to the top-left of each dish. The blue strip painted into the old counter image is cropped off in `counter_asset()`, and the counter fill extends behind the art to avoid a gap.

## Timeline

| Time (s) | Scene | What happens |
|---|---|---|
| 0.00–0.90 | Pot intro (white screen) | The blue pot from the end of the splash sits at the centre (x≈0, y≈−0.3, 3.3 u wide, 2.9 u tall) with its lid ajar and ingredients peeking out; the lid rocks gently. At ~0.85 it fades out. |
| 0.90–1.55 | | Blank white. |
| 1.55–1.95 | Kitchen menu | The whole menu fades in from white (≈0.4 s). |
| 1.95–2.4 | | Nori settles into place (tiny bob). |
| 2.5–2.7 | | Happy blink (eyes close, smile). |
| 3.0–9.4 | | Happy idle: smile, eyes open, tiny head sway; another blink at 6.1–6.4. |
| 9.5–11.2 | | Waiting gets worrying: smile fades to a worried frown, eyes widen and look left-down, head tilts left (9.6–10.0), eyes shut in a worried blink (10.1–10.4), then look right-down. |
| 11.3–11.9 | | Surprise: eyes big, small round "o" mouth, arms lift off the counter (11.5). |
| 11.9–12.5 | | Sleepy yawn: eyes closed, mouth open (12.0–12.4), eyes half open at 12.4. |
| 12.5–14.2 | | Worried frown again, eyes glance aside. |
| 14.3–14.5 | | Perks up: head pops up, big smile. |
| 14.5–17.2 | | Happy idle again; blink at 15.8–16.1. |
| 17.2–17.8 | Transition | Menu fades to white. (In the game this runs when the first dish is tapped.) |
| 17.8–18.4 | | White. |
| 18.37–18.67 | Cooking screen | Fades in from white: yellow background with orange runner, empty bowl at left, tray at right. |
| 18.45–19.1 | | Ingredients pop into the tray one by one (flour, butter, flour, butter, sugar, butter), each scaling 0→1.15→1 in ~0.25 s. |
| 19.2–24.4 | | Idle, waiting for the child. |
| 24.4–28.9 | Hint | A white pointing hand taps the middle-right butter, drags a faded ghost copy to the bowl (reaches the bowl at 26.65), holds there, then fades (28.75). The hint repeats while nothing has been touched. |
| 29.9–40.0 | Drag & drop | Player drags the 6 ingredients (any order) into the bowl. A dragged item follows the finger (slightly rotated, a faint grey dot marks the touch), and on drop it settles in the bowl at the drop spot; flour lands as a larger fluffy pile; items overlap and stack. When the last one is in (≈40.0) the tray is empty. |
| 40.5–41.3 | Whisk | A balloon whisk slides in from the top edge near the bowl's right side and swings to rest (handle pointing right, head just right of centre, ~1 o'clock → 3 o'clock). |
| 41.4–44.2 | | A dashed arrow draws itself clockwise along the bottom of the bowl (right → left) with an arrowhead, then fades — a "stir in a circle" hint (repeats while idle). |
| 44.5–52.0 | | Waiting for the child to whisk. |
| 52.0–58.0 | Whisking | The whisk head follows the finger and the handle trails behind it. As it stirs, the contents dissolve: ingredient blobs go translucent (52.4), milky yellow liquid (52.8), vivid golden swirl (53.4), paler swirl (54.5), smooth pale-golden batter by ≈56.0. The whisk shrinks slightly as it sinks into the batter. |
| 57.8–58.4 | Transition | Screen fades to white (leads to the next scene). |

## Layout (units)

Menu: back button (−9.78, 4.1) r 0.57; two square buttons at (7.2, 3.9) and (9.06, 3.9) (blue "hands" icon, purple chef-hat icon); plant shelf at (−8.5, 2.4); window (partly cut off) at right x 7.4–11, y 0.5–3.7; Nori head+shoulders centred x=0, ears top y=+3.97, paws on the counter edge (y≈−1.44); yellow counter strip from y=−1.44 down with a dotted pattern; **7 round dishes** (2.9 u diameter) at x = −9.38, −6.26, −3.12, 0, 3.14, 6.26, 9.38, y = −3.2; padlock on the lower right of each locked dish (dishes 1,2,3,6,7 in the video; for the game dish 1 is unlocked, the rest locked).
Cooking: bowl centre (−2.78, 0.0), outer rim diameter 7.4 u (handles top and bottom, 9.3 u tall in total); tray centre (3.91, 0.03), 5.2 u × 9.4 u; ingredients in the tray at butter (2.83, 1.65), flour (4.8, 2.6), butter (4.97, 0.75), flour (2.88, −0.49), sugar (5.06, −1.26), butter (3.91, −2.75); butter ≈ 1.8 u, flour ≈ 2.4 u, sugar ≈ 1.5 u. Back button at (−9.78, 4.1).

## Assets (all new, generated with Meshy, transparent background where isolated)

- Nori behind the counter (bust + paws on the counter): neutral, blink (+ half-blink in-between), happy squint, worried, worried look-left, worried look-right, eyes-shut worried, surprised "o" (arms up), yawn, sleepy half-open, big smile.
- Pot: body with ingredients, separate lid.
- Menu: wall, plants on shelf, window, 7 dishes (pancakes + 6 others), dish plate base, padlock, three round buttons.
- Cooking: background, bowl, tray, butter curl, flour pile, sugar crystals, whisk, pointing hand, 4 batter stages (disc).
- Locally drawn: dashed arrow arc, grey touch dot, soft shadows.
