"""Replacement paw artwork authorized by the user's regeneration request."""
from pathlib import Path
from meshy_image import generate
root=Path(__file__).parent
out=root/'meshy_raw/kitchen/counter_paws_clean.png'
if not out.exists():
 generate(str(out), 'Use the exact dark brown raccoon paws and white chef jacket cuffs of Nori in the reference. Draw ONLY TWO SEPARATE resting paws, each with a short rounded white chef sleeve cuff behind it, viewed from the front as they rest naturally on the near edge of an invisible kitchen counter. Left paw on the left, right paw on the right, with generous empty space between them. Four soft rounded furry fingers drape down slightly. Complete organic silhouettes, smooth rounded cuffs, no straight cut lines. NO torso, NO apron, NO arms beyond the short cuffs, NO head, NO counter, NO shelf, NO objects, NO text. Match reference soft rendered cartoon style, dark brown fur, warm white cuffs. Centered pair with large empty margins on a solid flat pure magenta background.', refs=[str(root/'meshy_raw/peek_smile.png')], aspect='16:9', keep_background=True)
