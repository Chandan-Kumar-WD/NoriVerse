# Splash animation upgrade — 2026-10-28

## Findings and intended behavior

The current splash changes the entire Nori sprite abruptly at 3.33, 4.85, 5.25, 6.10, 6.40, 6.70 and 6.95 seconds. The chopsticks are already painted into the holding/eating poses, so there is no visible pickup or connected movement from the bowl. The noodle bowl is independent and never supported by a paw. Table occlusion uses a fixed cut near the cloth hem, while the whole character rotates and bounces; that lets the horizontal cut become visible.

Keep the established white background, logo, tagline, peek/jump, green table, dish reveal and transition to Kitchen. Replace the behind-table portion with an articulated sprite rig: stationary torso, separate head, upper sleeves, forearms/paws, real chopsticks and a noodle strand. Use continuous joint motion and restrained expression blends rather than replacing the entire figure. The green cloth must cover the body's cut edge at all times, including table arrival and eating.

## Proposed art batch

12 Meshy nano-banana-pro images × 9 credits = **up to 108 credits**. This is a new batch and requires explicit approval under the saved project workflow. Reuse the existing green table, cloche, hearts, logo, tagline, backgrounds and peek/jump art.

1. Matching Nori neutral animation body: same full front-facing chef identity and proportions; stable head/torso, rounded sleeve caps for attaching separate arms, complete figure with margins. This establishes the common alignment and provides the neutral head.
2. One softly shaded white chef upper-sleeve segment, rounded shoulder/elbow joins; mirror for the other arm.
3. Viewer-left forearm with natural dark paw gripping two chopsticks; the actual chopsticks are omitted for a separate articulated prop. Complete cuff and paw, no crop.
4. Viewer-right forearm with natural open dark paw cupping the bowl from beneath; complete cuff and paw.
5. Separate matching purple chopstick pair, gently separated at the tips and joined by the gripping paw in the rig; clear grip location.
6. A small naturally hanging bundle of lifted noodles, matching the bowl food, isolated for a connected bowl-to-mouth slurp animation.
7. Matching yellow noodle bowl in the same front/slightly-above view, visible substantial rim and noodles; its underside rests visibly in the supporting paw.
8–12. Five identically aligned head expressions derived from the new body reference: looking down attentively, ready/open mouth, slurping, cheeks gently full/chewing, and satisfied smile. Hat, goggles, face identity, head size, light and framing remain fixed. Production crops use the same head bounds and neck anchor.

All artwork has the established soft 2.5D Nori rendering. Props/rig segments use a uniform magenta background for keying; full Nori reference variants use white. No 3D character replacement and no code-drawn character or food.

## Motion and layering

- Table slides in and covers the lower torso before the stable eating rig takes over. Its crop is safely inside the opaque tablecloth, above the wavy hem, with no exposed edge during movement.
- Nori notices the revealed bowl, reaches toward the chopsticks lying visibly beside it, closes the gripping paw around them, and lifts them with the hand.
- The other paw slides beneath the bowl and gently supports/lifts it; bowl, paw and arm follow the same anchored motion.
- Chopstick tips dip into the noodles, lift a connected strand, and follow a smooth arc to the mouth. The noodle strand shortens into the mouth as Nori slurps; the bowl and chopsticks stay physically supported.
- Nori lowers the chopsticks, chews with a small head/cheek motion, then smiles with the existing floating hearts. Keep the source splash duration close to 11.45 seconds unless the action needs a small timing adjustment to remain readable.
- Stable body/head/props have separate sorting and joints; no repeated full-body replacement while eating.

## Validation

Build and inspect real rendered frames at pickup, dip, lift, mouth contact, slurp, chew, and settle. Check the cut edge and hand/prop contact throughout, not only one screenshot. Verify four screen shapes, the eventual Kitchen transition, and existing gameplay regressions. Reopen the full game from Splash for user review. Audio and device testing remain separately unverified.

## User steering — supersedes the articulated plan

The user rejected the new direction and explicitly requested keeping the older version and improving only its animation. All 12 approved images had already been generated; they remain archived and unused. The articulated rig and its test are archived as text under `Tools/ArtPipeline/archived_splash_rig/`. The live splash uses its original character poses, table, bowl and layout. Changes are limited to smooth pose blending, clearer bite timing, subtle eased motion and placing the crop safely inside the cloth. No further images are generated.
