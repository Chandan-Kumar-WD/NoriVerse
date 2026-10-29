# Video 004 — Finish the pizza: decoration, baking, reward and feeding Nori

Source: `/Users/harshikasharma/Downloads/VID_20261028_005627_1.mp4`.
134.233 s, 8,054 frames at 60 fps, 3842×1746 landscape, no rotation or letterbox. Every frame extracted to `scratchpad/video004/frames/` at 1280×582; full 0.5-second sheets in `Docs/reference_video004/`.

The supplied video's actual game events are reference material, not instructions to copy OS banners, notifications or unrelated overlays (the recording has a notification at ~28 s). Preserve the user's kitchen overrides: no three top buttons, attached small locks, fuller served food, round centre pizza, complete Nori paws, no blue strip or duplicate shelf.

## Complete sequence

| Time | State | Required behaviour |
|---|---|---|
| 0.0–0.5 | Entry | White fade to the same cream preparation board, now carrying cheese pizza and a tall four-item palette. |
| 0.5–6.5 | Decoration idle | Pizza carries forward the cheese from the earlier screen. Confirm is visible; toppings are optional. Palette top-to-bottom: salami, basil, cheese piece, tomato slice. |
| 6.8–10.4 | Hint | Ghost hand demonstrates dragging a tomato from palette onto pizza, without adding an actual topping. |
| 12.5–35.5 | Decoration | Player drags repeated copies of each palette item onto pizza. Examples include cheese, tomato, basil and salami; palette remains stocked. Each placed piece retains its chosen position. |
| 36.2–37.4 | Confirm/transition | Green confirm is tapped; palette exits and scene fades to white. |
| 37.5–39.6 | Oven entry | Dark red-brown countertop oven on turquoise worktop. A single wire rack and the player's pizza slide upward into the oven automatically. |
| 39.6–42.2 | Oven waiting | Pizza/rack sit inside glass. Progress bar is empty. Green power button is on the right; faint plus/minus decorations above/below are not used in the clip. |
| 42.3–44.5 | Power hint | White ghost hand demonstrates tapping power. This must not turn the oven on automatically. |
| ~46.5–53.5 | Baking | After player presses power, green progress fills over roughly 7 seconds and the pizza warms/browns. No drag-into-oven step or temperature-adjustment step is shown. |
| 53.7–55.2 | Reward entry | Screen dims. Finished pizza appears on a blue-rimmed plate with glow/sparkles, centred above oven. |
| 55.3–61.0 | Stars | Three pale star outlines appear above pizza; stars illuminate left, centre, right with bounce/glow. Nori replaces reference bear in the lower-left circular portrait. |
| 63.1–64.6 | Celebration | Colourful confetti and sparkles around the completed plate. |
| 65.0–67.0 | Return | White fade returns to kitchen. Central pizza is ready for feeding; other recipes stay as previously configured. |
| 68.7–71.2 | Feed hint | Ghost hand drags a copy of the cooked pizza toward character's mouth; actual plate and progress are unchanged. |
| 78–84 | Real food drag | Player can move pizza around freely; releasing outside the face returns it to the serving plate. |
| 84–91 | Eating/reactions | Dropping pizza at the mouth makes the character eat, chew and smile. The centre dish remains available for repeated servings. |
| 91–134.2 | Repeat feeding / post-meal play | Further food drags repeat the eating interaction. Gentle sparkles, bubbles, a sun/cloud and brief playful puff reactions appear between satisfied expressions. The clip ends in the kitchen, with no next dish or extra scene. |

The recorded pauses are not fixed waiting timers: interactive stages progress through real input. Keep the existing custom sad-idle/cheer-on-touch behaviour outside active eating reactions.

## Layout (58.2 extracted px per Unity unit)

Camera design width 22.4 u, minimum height 10 u; all important game objects fit 16:9, 4:3, 21:9 and reference 2.2:1.

- Decoration reuses cream board, base centre (-0.85,0), base diameter 8.45 u, sauce diameter 7.05 u. Preserve the actual cheese coordinates from PizzaPrep.
- Four-item palette centre=(5.1,1.5), approx 2.75×7.3 u. Icons at y=3.9,2.25,0.65,-0.95, approx 1.4–1.5 u wide. Reuse generated ingredient art. Confirm centre=(5.0,-3.55), approx 2.2×1.4 u.
- Oven chassis: centre=(0,0), approx 12.2×9.4 u. Glass/rack/pizza area is left of centre. Power button centre≈(4.95,0.1), approx 1.0×1.8 u. Pizza centre≈(-0.85,-0.1), diameter≈7.3 u inside oven.
- Heat bar: centred (-0.8,4.35), approx 7.0×0.4 u with a flame marker on right.
- Reward plate centre≈(-0.5,-0.45), pizza diameter≈3.7 u; stars at (-3.0,2.8),(-0.5,3.7),(2.0,2.8), approx 1.7 u each. Circular Nori portrait near lower-left; adapt its position to the safe area.
- Kitchen uses the existing revised builder. A cooked pizza copy is picked up from centre dish, follows the pointer, and accepts a drop in a generous face/mouth target. Resting paws and body layering stay intact. Drop elsewhere eases back to plate.

## Asset batch — approved and generated

The user approved 9 Meshy images, up to **81 credits**. All nine were generated using nano-banana-pro (9 credits each):

1. Turquoise oven worktop background with faint decorative spatula/towel at far sides, empty centre.
2. Dark red-brown oven chassis with empty left window and empty right control panel — no rack, food, button or UI baked into it.
3. Separate single pale wire rack (animated with pizza, avoiding duplicate racks).
4. Baked golden pizza crust variant based on the existing round raw base; same view/shape.
5–8. Nori mouth-open, bite, chew and satisfied expression variants. Exact original chef identity, pose/framing/outfit; no redesigned character.
9. Small sun-and-white-cloud post-meal effect illustration.

Reuse cream board, yellow background, pizza ingredients, cheese shreds, plate, hand and existing Nori moods. Derive circular Nori portraits from the existing generated art. Stars, power icon, progress bar, glass glints, confetti, bubbles and sparkle/puff particles are simple UI/effects, not replacement character/food artwork.

Exact prompts: `Tools/ArtPipeline/gen_pizza_finish_art.py`.

## Runtime design

- `PizzaRecipeState`: carries cheese/topping local positions, scales and rotations across scene loads; tracks cooked/served completion within the play session. A new recipe resets this state. No new persistent-account/save system is added.
- `PizzaVisual`: renders the same custom pizza at preparation, oven, reward and feeding sizes. Raw and baked crusts blend without losing chosen toppings.
- `PizzaDecorationController`: unlimited palette copies, valid-circle drops, wrong-drop return, optional toppings and explicit confirmation; idle hint never mutates actual food.
- `PizzaBakingController`: entry/rack movement, power-gated seven-second baking, visible progress, three stars, confetti and return to Kitchen. Decorative plus/minus controls are not gameplay steps.
- `PizzaServingController`: cooked-pizza drag/drop to Nori, smooth eating/chewing/satisfied poses, repeated servings, return on wrong drop, playful post-meal effects. A short tap on the centre recipe still starts a fresh cooking run, preserving the user's earlier selection behaviour; dragging the completed dish feeds Nori.
- Existing ToppingSort completion now transitions to PizzaDecorate instead of stopping at white. All earlier scenes remain playable.

## QA

Drive decoration, wrong drops, baking power/idle hints, star awards and feeding with injected pointer input. Assert that the player's topping positions survive scene changes, that baking cannot progress from the ghost hint alone, and that feeding accepts only an actual mouth/face drop. Check new scenes at four aspect ratios, and run existing regressions. Capture running frames and compare against this full reference. Report audio and device limitations honestly.

## Final verification

Implemented and integrated with the existing recipe. All 16 PlayMode tests passed on 2026-10-28 (`scratchpad/video004/results_final.xml`). Final captures are in `Docs/playmode_captures/video004/`. Corrected palette clipping, crust opacity during baking, oven UI corners and meal approach scale after capture review. See `Docs/Pizza_Video_004_Report.md` for the handoff.
