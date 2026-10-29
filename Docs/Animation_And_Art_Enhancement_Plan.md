# NoriVerse — future artwork and animation enhancements

Recorded: 2026-10-29. Status: documented direction; enhancements below are not a claim of completed implementation.

## User's goal and role

Improve the existing game with consistent 2D and 2.5D assets, smoother character animation, polished scene transitions, and responsive gameplay. Preserve Nori's identity and the established game flow.

The user does not know game development and intends to give creative feedback. The development model owns technical investigation, asset preparation, implementation, integration, testing, and documentation. Present playable results and visual previews in plain language. Do not require the user to choose technical architecture or manually assemble assets when the available tools can do it.

This document records a roadmap and working method. It does not authorize immediately rebuilding the game, spending new service credits, publishing builds, or adding unrequested recipes and levels. Follow the active user request, AGENTS.md, and existing per-video decisions.

## Existing project and decisions to preserve

- Continue the Unity 6 / C# / Unity 2D project. Do not restart in another engine.
- The existing playable flow already includes Splash, Kitchen, Cooking, Dough, PizzaPrep, ToppingSort, PizzaDecorate, PizzaBake, rewards, and feeding Nori. Audit and improve the existing flow; do not treat it as an unbuilt prototype.
- Keep the existing reference-video interactions, progression, saved completed pizza, exit confirmation, and audio behavior unless explicitly changed by the user.
- Nori remains an illustrated raccoon. Here, 2.5D means depth and volume through rendered-looking sprites, layered composition, perspective, and restrained parallax. It does not authorize replacing Nori with a 3D model.
- Preserve the original splash artwork/layout and current smoothing approach. The user previously rejected the articulated splash rig. The general rigging suggestions below do not reverse that decision or authorize restoring archived experiments.
- Read Docs/AGENT_WORKFLOW.md and the relevant implementation reports before making changes. Older reports describe historical states; inspect the current files before relying on them.

## Generation access and responsibilities

The user reported using their personal ChatGPT account for image generation and reaching its image generation limit. Treat this as reported access context, not a verified permanent outage or proof that a separate tool/API is unavailable.

An image model supplies artwork and poses. Coding tools and Unity implement input, animation, state transitions, audio synchronization, saving, and performance. Do not promise that one image model automatically delivers a finished game.

Use the generation tools actually available in the active session and follow their skill instructions. Existing pipeline scripts and source artwork should be inspected first. Earlier workflow notes contain Meshy-specific choices and historical costs; do not assume those costs or integrations are current. Switching production providers is a proposal until selected for the task. Do not silently change the art style or revive rejected assets. Preserve the existing requirement to obtain approval before spending Meshy credits outside an approved batch.

If generation is unavailable, document the exact missing assets and keep progressing on independent engineering work. Report the real limitation; do not ask the user to redraw sprites or learn rigging as a substitute for agent work.

## Art direction and identity lock

Preserve Nori's established face and proportions, grey and cream fur, dark raccoon mask markings, large brown eyes, white chef hat, yellow goggles, red neckerchief, white chef jacket, and teal apron with wooden spoon and whisk. Match the approved glossy, soft cartoon rendering and established scene style. Do not redesign Nori.

For the referenced tomato-and-basil pizza, preserve the light blue plate and recognizable approved food design. During gameplay, preserve the player's actual recipe state and topping placement; do not replace it with a generic generated pizza.

Before a new animation batch:

1. Locate the approved current references and source assets. Reuse approved assets within the active project where appropriate; do not revive assets rejected by earlier instructions.
2. Assemble a reference sheet as needed: front/three-quarter/side views, expressions, hand poses, palette, proportions, lighting, and camera. Generate missing views only when needed by the authorized action.
3. Keep the same identity reference across generations. Do not use only the previous generated frame as reference, since errors can accumulate.
4. Record exact prompt, tool/model when exposed, reference paths, source output, processed output, and intended use. Store durable references in the project, not only in temporary attachment paths.

A useful generation brief is:

> Using the approved Nori reference, create [specific pose/expression/part] for [existing scene/action]. Preserve face, fur markings, proportions, outfit, rendering, camera, and lighting. Change only [requested area]. Match [canvas size, scale, anchor, and orientation]. Use a transparent background for an isolated sprite. No text or extra objects.

Inspect every output. A prompt requesting consistency is not evidence that consistency was achieved.

## Choose animation methods per action

| Need | Preferred approach to evaluate |
| --- | --- |
| Breathing, small head/body motion, ingredient travel, button feedback | Existing Unity animation/procedural movement with intentional easing |
| Blinks, mouth shapes, hand changes | Aligned replacement sprites or localized facial edits |
| Reusable limb movement in an approved new animation | Layered sprites and a Unity-compatible rig if it improves the result |
| Large silhouette/perspective change or expressive action | Approved key poses plus cleaned intermediate drawings |
| Steam, flour, sparkles, confetti | Reusable effects/particles or compact effect sequences |

Rigging is an option, not a mandatory conversion. Inspect the established implementation and user decisions first. Do not add paid dependencies or an external animation package as an assumed requirement.

For layered animation, separate only the necessary parts and paint hidden overlap areas so joints do not reveal gaps. Keep pivots and attachment points stable. Generate additional views for meaningful perspective changes; rotating a flat sprite cannot reveal its unseen side.

For frame animation, plan anticipation, main action, and settling poses before generating intermediates. Start with a small set of key poses and add drawings where motion requires them. More generated frames do not automatically improve quality. A game rendering at 60 FPS does not require 60 unique drawings per second.

## Frame preparation and refinement

- Standardize canvas, character scale, pixel density, camera angle, anchor/pivot, lighting, and transparency across a sequence.
- Align using stable landmarks such as face/hat or torso, not the changing bounds of a raised hand.
- Review frames individually, overlaid with neighboring frames, and playing at intended speed.
- Correct changing goggles, fur markings, eyes, outfit details, extra fingers, edge halos, jitter, and unwanted scale changes.
- Keep hands attached to wrists and held props attached to their intended grip. Avoid sliding feet or paws at contact points.
- Use targeted edits to repair a faulty region rather than regenerating the entire frame and introducing unrelated changes.
- Confirm loop seams and sprite-sheet extraction/import. Preserve source files and generation records; keep rejected variants out of active runtime art.
- Inspect at real gameplay size and relevant screen shapes, not only at source resolution.

## Smooth action and scene transitions

For each authorized enhancement, inventory entry pose, anticipation, action, follow-through, exit pose, interruption behavior, and next state.

Examples to map to existing gameplay, rather than new features to add automatically:

- Idle → notice player → raise hand → wave → lower hand → idle.
- Idle → reach → grip plate → lift → present → settle → return.
- Ingredient press/drag → valid placement or return → feedback → next permitted step.

Use explicit state transitions and existing architecture. Blend compatible poses; use connecting drawings for major changes. Do not blanket-crossfade unrelated silhouettes, which can create double faces or hands. Retain the current splash solution unless the user requests a specific change.

Give touch actions immediate visible feedback. Keep interactions playable; a prerecorded sequence is not a replacement for dragging, painting, sorting, or feeding. Connect sound and visual effects to actual gameplay events. Handle repeated taps, interrupted actions, pause/resume, back navigation, and scene changes without duplicate triggers or stuck input.

For screens, preserve composition and continuity, choose an appropriate short fade/slide/camera move, and prepare assets before the transition to avoid loading hitches. Optional initial tuning ranges are 100–200 ms for small UI feedback and 250–500 ms for larger screen transitions; these are suggestions to adjust against references and user feedback, not acceptance thresholds. Avoid moving every element continuously or introducing transitions that delay a child's next action.

## First enhancement slice and rollout

Start with an audit of existing behavior, not a wholesale regeneration batch. Pick one existing interaction covered by the active request, such as a greeting or pizza presentation. A small idle → greeting → presentation → idle demonstration is a proposed validation slice, not an instruction to replace the existing flow.

1. Capture the current interaction and identify concrete visual or timing problems.
2. Inventory reusable assets and missing poses; select the simplest suitable animation method.
3. Prepare a bounded asset batch and obtain any required spending approval.
4. Generate, clean, align, import, and integrate the assets.
5. Preview the full motion and transitions in the running game, not just standalone images.
6. Fix visual and interaction issues; provide a playable preview or captured clip for user feedback.
7. Record feedback and the final settings before applying the approach to other authorized scenes.

## Completion criteria and handoff

For each changed section, verify:

- Nori retains her approved identity with no distracting flicker, shape drift, joint gaps, or unintended clipping.
- Motion has readable timing, stable contacts/props, natural loop seams, and suitable entry/exit poses.
- Touch response and gameplay progression remain correct; repeated or invalid input does not break the sequence.
- Pause, exit confirmation, replay, recipe persistence, and existing behaviors affected by the change still work.
- Composition holds at supported landscape phone/tablet sizes and safe areas.
- Relevant Unity compile and gameplay checks pass. Visually inspect a running sequence, not only automated state assertions.
- Test on a target device when available and profile transition hitches, texture memory, and frame pacing. The project requests 60 FPS; do not claim measured 60 FPS or device verification without evidence.

Deliver a concise report with what changed, asset/prompt locations, checks actually performed, a preview/build location, remaining limitations, and the next concrete task. Mark work as planned, in progress, blocked by a named dependency, or verified; do not describe this roadmap as already implemented.

Ask the user about creative choices when needed and request plain-language feedback. The model should translate that feedback into technical tasks and carry out the implementation. User review guides the next iteration; it does not replace the model's own quality checks.
