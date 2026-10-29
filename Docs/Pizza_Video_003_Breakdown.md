# Video 003 — dough rolling, circle cutting, sauce, cheese and topping sorting

Source: `/Users/harshikasharma/Downloads/VID_20261028_005627 (1).mp4`.
130.683 s, 7,841 frames at 60 fps, 3842×1746 landscape (2.2005:1), no rotation or letterbox. Every frame extracted at 1280×582 into `scratchpad/video003/frames`; full-video 0.5-second contact sheets in `Docs/reference_video003/`. Audio track confirmed: stereo AAC, 44.1 kHz; content not yet audited.

## Continuity and scope

The existing Cooking scene fades to white after stirring. Connect it directly to Dough, then PizzaPrep, then ToppingSort. Preserve Kitchen's revised art, attached lock badges, complete Nori paws, and all existing gameplay. No Nori is visible in this reference section. End with a completion event and white fade; do not invent an oven or subsequent topping-application stage.

Use real mouse/touch interactions, not automatic playback. Reference timestamps describe the recording, including user hesitation; progression depends on the child's actions. Reuse existing easing, white scene transitions, pointer injection, tutorial hand, back-button art and upper-left safe-area anchor. Back buttons in these new scenes return to Kitchen through a white fade.

## Timeline

| Time | State | Behaviour |
|---|---|---|
| 0.00–0.30 | Dough entry | White fades to a wooden tabletop, orange rounded board, small pale dough at centre. |
| 0.75–1.25 | Tool entry | Wooden rolling pin settles horizontally over dough. |
| 6.3–12.0 | Idle hint | White dotted rectangle and a hand/ghost pin demonstrate sweeping and rolling; actual pin stays put. |
| 14.3–28.2 | Rolling | Pin follows the finger, rotates perpendicular to movement, dough grows progressively into a thin irregular sheet filling the board. Pin exits when done. |
| 28.8–34.0 | Cut entry | Dashed white circular guide and blue arrow marker appear. |
| 34.2–39.5 | Cut hint | Hand demonstrates following the circle clockwise; no progress without user input. |
| 40.0–44.7 | Cut | Child traces clockwise from upper left. Amber cut line grows behind marker with small sparkles. |
| 44.7–46.6 | Cut complete | Outer dough fades away, leaving a round base, with a short sparkle burst. |
| 46.8–47.4 | Scene change | Fade through white to yellow worktop and cream preparation board. |
| 47.4–53.7 | Sauce entry | Round dough is centred slightly left, sauce icon at upper-right of preparation board. |
| 53.8–60.8 | Sauce interaction | Finger paints thick red sauce onto dough; initial strokes cover the upper-left/centre. |
| 61.4–66.1 | Sauce hint | When idle, a translucent hand/paint ghost demonstrates spreading. It does not complete the activity. |
| 66.3–74.1 | Sauce completion | Further strokes cover the circle, leaving crust visible. Red/white sparkles celebrate coverage. |
| 76.6–82.3 | Cheese entry | Sauce icon switches smoothly to cheese icon. |
| 82.4–86.4 | Cheese hint | Hand demonstrates a spreading/sprinkling gesture over pizza. |
| 88.0–100.4 | Cheese | Finger movement scatters small yellow shreds. Green confirm button appears after some cheese; player may add more, then taps confirm. |
| 101.0–103.8 | Sorting entry | Fade to turquoise upper surface/yellow lower surface. Tray fills with tomato slices, yellow cheese pieces, salami discs and basil. Four labelled matching plates appear below. |
| 108.8–113.4 | Sort hint | Hand/ghost demonstrates dragging salami to the matching plate; source item remains in tray. |
| 115.0–117.6 | Sort salami | Correct item snaps to plate, green check appears, filled plate exits; remaining targets recentre. |
| 119.1–122.1 | Sort cheese | Same feedback and exit. |
| 123.0–125.2 | Sort basil | Same feedback and exit. |
| 126.0–129.8 | Sort tomato | Same feedback and exit. Any matching instance/order is accepted in the playable version. |
| 130.0–130.68 | Completion | Plates are gone; fade to white. No further gameplay is shown. |

## Measured layout

Camera design height 10 u, design width 22.4 u (whole reference always fits). One Unity unit = 58.2 px in extracted frames. Origin = frame centre; y up.

- Dough board: x=-6.3..6.3, y=-4.65..4.65, centred at (0,0); rounded orange board with an inset border. Wood background extends to all screen edges; yellow cloth/sieve and flour stay decorative at left/right, outside playable board.
- Starting dough: approx 5.0×5.6 u. Fully rolled sheet: approx 11.5×8.3 u. Rolling pin: approx 8.0 u wide, body 1.3 u high plus handles.
- Cutting guide: centre=(0,-0.1), radius=3.2 u; start angle≈125 degrees, trace clockwise. Finished round dough diameter≈6.8 u.
- Pizza preparation board: approx 12.6×9.3 u, centred (0,0). Round base: centre=(-0.85,0), diameter=8.45 u. Sauce interior diameter≈7.0 u; leave crust visible.
- Category tab at (5.15,3.4), pale cream rounded panel behind a 1.15-u ingredient icon. Confirm at (5.0,-3.55), approximately 2.2×1.4 u, green with white check; initially hidden. Its final size/position was measured again from the source frame's green-pixel bounds.
- Sorting backdrop: turquoise above y=-1.2, yellow below. Main tray centre=(0,2.0), approx 12.8×5.1 u. Ingredients distributed within x±4.8, y=0.5..3.9; food pieces around 1.3–1.7 u.
- Matching target plates: y=-3.3, diameter≈2.85 u, initial x=-4.95,-1.65,1.65,4.95. Labels above at y=-1.15. A grey silhouette identifies the item; label repeats its coloured icon. Completed plates/checks leave and remaining targets move toward centre.
- Back button: existing golden sprite, safe-area anchored at top-left with 0.35-u inset, as just approved.

## Asset batch — approved and generated

User explicitly approved 16 new Meshy images, up to **144 credits** (documented 9 credits each, `nano-banana-pro`). All 16 were generated. Exact prompts live in `Tools/ArtPipeline/gen_pizza_flow_art.py`. Two completed jobs interrupted during download were recovered without resubmitting them.

1. Wooden tabletop background with flour/cloth/sieve at sides, empty centre.
2. Empty orange rolling board, top-down.
3–5. Small dough, intermediate dough, fully rolled sheet (consistent pale dough material).
6. Wooden rolling pin, top-down, horizontal, separate from background.
7. Round unbaked pizza dough base with raised rim, top-down.
8. Empty rounded cream preparation board, top-down.
9. Red tomato sauce disc with a gentle spiral, top-down, separate from crust.
10. One short curved yellow cheese shred, reusable at varied angles.
11. Tomato slice.
12. Yellow cheese piece with holes.
13. Salami disc.
14. Basil sprig.
15. Turquoise/yellow sorting background, empty.
16. Empty pale-blue horizontal sorting tray with side handles, top-down.

Isolated props generated against magenta and keyed from exterior only; no white-on-white removal. All sprites preserve natural proportions. Reuse existing tutorial hand, dish plate and UI art. Dashed guides, progress arcs, touch dots, panel fills, green checks and sparkles are simple code-created UI/effects, not replacement food art.

## Implementation / verification plan

- DoughRollingController: touch-controlled pin, travel-based progress, eased/overlapping dough stages, release/regrab, no progress from hints or stationary touch.
- DoughCutController: ordered clockwise tracing with radial tolerance, resumable progress, no completing by jumping across the circle.
- DoughSceneController: phase hand-off and white transition.
- SaucePaintController: generated sauce texture revealed only beneath actual brush strokes, clipped inside crust; repeated strokes do not falsely increase covered area.
- CheeseSprinkleController: spawned generated shreds at finger positions, confined to pizza, optional further cheese and explicit confirm.
- PizzaPrepController: sauce/cheese transitions and hint scheduling.
- ToppingSortController + SortIngredient + SortTarget: actual drag/drop, wrong-drop return, correct-kind validation, one completion per category, smooth target recentering, final event/fade.
- Reuse touch input and camera fitting. PlayMode tests drive each step with injected finger input, test wrong actions/idle behaviour and completion, and capture all stages at 16:9, 4:3, 21:9 and reference ratio.
- Compare running-game captures against reference sheets. Check full props, food volume, layering, hint ghosts, safe button positions and transition coverage. Audio and real-device checks must be reported honestly.
