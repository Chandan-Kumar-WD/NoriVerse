"""App icon for the Cooking game (user approved one Meshy image, nano-banana-pro, 9 credits, 2026-10-28).

Reference: the existing Nori bust (identity only). Output: Tools/ArtPipeline/meshy_raw/icon/icon_v2.png
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import meshy_image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
REF = os.path.join(ROOT, "Assets/_Project/Art/Characters/Nori/bust/nori_bust_big_smile.png")
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "meshy_raw", "icon", "icon_v2.png")

PROMPT = (
    "Mobile game app icon artwork. The exact same raccoon character from the reference image, Nori: same face, same "
    "fur colours and mask markings, same white chef hat, same yellow goggles on the hat, same red neckerchief and teal "
    "apron with a wooden spoon and whisk. Keep her design identical, only re-frame it. Show her happy open-mouth smile "
    "looking at the viewer, head and upper chest centered and large, filling about 80 percent of the square, with a "
    "comfortable margin around the chef hat so nothing is near the edges. Plain flat background, no objects, no text, "
    "no letters, no border, no frame. Glossy soft 3D cartoon style matching the reference, clean and readable at "
    "small sizes."
)

if __name__ == "__main__":
    meshy_image.generate(OUT, PROMPT, refs=[REF], aspect="1:1")
