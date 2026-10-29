# Pizza visual upgrade — user review 2026-10-28

The seven supplied screenshots identify the current batter, preparation, sorting, baking, reward and kitchen-serving states. Preserve gameplay order and Nori's identity; upgrade the specified art and animation rather than changing the recipe.

## Replacement art batch

14 Meshy nano-banana-pro images at the documented 9 credits each: **up to 126 credits**, explicitly approved by the user and all 14 generated. No additional generation attempts were made.

| Asset | Intended change | Reference / framing |
|---|---|---|
| prep_board_premium | Warm cream glazed ceramic preparation board with rounded raised bevel, visible rim depth and soft material highlights; broad empty work surface | Existing prep board, direct overhead, same silhouette, 12.8-unit width |
| sauce_holder | Small thick-rimmed ceramic ramekin containing rich red tomato sauce with soft spoon swirl; appetising material shading, complete object | Direct overhead, fit top-right 1.7-unit tab; no text or utensils |
| cheese_holder | Matching ramekin containing a natural pile of pale mozzarella shreds | Same camera, scale and vessel family as sauce holder |
| salami_clean | One appetising round pink salami/ham slice with a smooth intact edge, subtle natural marbling and thickness, no holes, debris or missing pixels | Existing slice for style only; replace sorting, palette and custom pizza uses consistently |
| oven_premium | Rounded cocoa/red oven chassis with dimensional enamel bevel, silver inner trim, deep empty left cavity and empty right control inset; layered material shading | Existing oven footprint and overhead layout, cavity sized for existing pizza; no rack, food, buttons or symbols baked in |
| oven_rack_premium | Matching single rounded polished wire rack with visible wire thickness, highlight and slight cast depth | Direct overhead; one rack only; same cavity alignment |
| reward_star_empty | Rounded five-point recessed gold-rimmed star with dimensional bevel and a softly shaded unlit cream/amber centre | Square, centred, full margin; no text, face or extra shapes |
| reward_star_gold | Identical star filled with luminous polished gold, bevelled rounded tips and soft cream highlight | Use empty-star reference; same contour, centre, camera and scale |
| nori_anticipate | Alert eager eyes, subtly open mouth, gentle expectant smile | Exact established Nori bust reference, fixed hat/head/body/paw framing |
| nori_bite_open | Natural wider open mouth and happy eyes, ready for a bite | Same Nori reference and framing |
| nori_bite_close | Eyes gently squinted, lips closing around a bite, slightly full cheeks | Same Nori reference and framing |
| nori_chew_left | Closed mouth, subtly fuller left cheek, pleased relaxed eyes | Same Nori reference and framing |
| nori_chew_right | Opposite subtle cheek fullness, small pleased expression | Same Nori reference and framing |
| nori_savour | Relaxed happy crescent eyes and warm satisfied smile, slight blush | Same Nori reference and framing |

All props remain illustrated 2D sprites with a soft 2.5D appearance, consistent light from upper left and real material depth. No new 3D models or replacement character design. Props on solid magenta for reliable keying; Nori on white. Exact final prompts are saved in `Tools/ArtPipeline/gen_pizza_visual_upgrade.py` before generation. Reuse existing background, pizza base, toppings other than salami, plates, kitchen, hand and UI controls.

## Code and composition work

- Add a small visible gap between batter bowl and ingredient tray without pushing either outside the minimum view.
- Use the new board in preparation and topping decoration; replace both generated-in-code category badges with actual illustrated ramekins.
- Increase sorting category images above each plate while keeping each status/check area separate.
- Replace oven/rack, retaining actual power start and seven-second baking. Emit warm twinkling sparkles from the advancing fill edge throughout progress; stop emission when idle/finished or behind the exit modal.
- Replace flat star silhouettes with matching dimensional empty/full illustrations, progressive pop/glow, small sparkle trails and staggered awards.
- Rework feeding: eager anticipation follows the offered meal; an eased bite approach, progressive food disappearance at the mouth, alternating chewing expressions with subtle cheek/head movement, swallow and satisfied settle. Do not scale/squash Nori's whole body or cover the eyes with a giant pizza. Post-meal effects should travel independently beside Nori, with depth, varied timing and soft fade, rather than staying rigidly attached.
- Retain all exit confirmation, screen-fit and menu-selection behavior.

## Verification

Build all scenes, test the entire recipe plus cancellation/repeated servings, inspect real rendered captures of each changed state and key animation beats, and verify 16:9, 4:3, 21:9 and 2.2:1. Relaunch Play Mode from Splash for user testing. Report pending audio/device checks honestly.
