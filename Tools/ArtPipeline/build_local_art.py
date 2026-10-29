"""Builds the locally-made NoriVerse splash art: logo letters, tagline, ground shadow and fade pixel.

Nori, props and hearts are generated with Meshy (see gen_splash_art.py).

Run from the project root:  python3 Tools/ArtPipeline/build_local_art.py  (needs pillow)
"""
import io
import json
import math
import os

from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ART = os.path.join(ROOT, "Assets", "_Project", "Art")

DESIGN_PX_PER_UNIT = 200
RENDER_SCALE = 1.5
PIXELS_PER_UNIT = int(DESIGN_PX_PER_UNIT * RENDER_SCALE)

TITLE_FONT = "/System/Library/Fonts/Supplemental/Arial Rounded Bold.ttf"
TAGLINE_FONT = "/System/Library/Fonts/Avenir Next.ttc"
TAGLINE_FONT_INDEX = 2  # Avenir Next Demi Bold

# Colours sampled from the reference video, plus Nori's established palette.
TITLE_BLUE = (87, 182, 202, 255)
TITLE_GREEN = (126, 202, 78, 255)
TAGLINE_GRAY = (106, 106, 106, 255)

def out(*parts):
    path = os.path.join(ART, *parts)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    return path


def build_shadow():
    import math
    w, h = 400, 80
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = img.load()
    for y in range(h):
        for x in range(w):
            d = math.hypot((x - w / 2) / (w / 2 - 4), (y - h / 2) / (h / 2 - 4))
            if d < 1:
                px[x, y] = (0, 0, 0, int(46 * (1 - d) ** 0.8))
    img.save(out("FX", "ground_shadow.png"))


def build_white():
    Image.new("RGBA", (16, 16), (255, 255, 255, 255)).save(out("FX", "white_pixel.png"))


# --------------------------------------------------------------------------
# Logo: one sprite per letter so each can pop in on its own
# --------------------------------------------------------------------------
TITLE_CAP_UNITS = 1.35
TAGLINE_CAP_UNITS = 0.44


def build_logo():
    layout = {"pixelsPerUnit": PIXELS_PER_UNIT, "letters": [], "tagline": {}}
    cap_px = TITLE_CAP_UNITS * PIXELS_PER_UNIT
    probe = ImageFont.truetype(TITLE_FONT, 1000)
    cap_ratio = (probe.getbbox("N")[3] - probe.getbbox("N")[1]) / 1000.0
    font = ImageFont.truetype(TITLE_FONT, int(cap_px / cap_ratio))
    ascent, descent = font.getmetrics()
    baseline_y = ascent
    letters = [(c, TITLE_BLUE) for c in "Nori"] + [(c, TITLE_GREEN) for c in "Verse"]
    pen, centers = 0.0, []
    for i, (ch, col) in enumerate(letters):
        adv = font.getlength(ch)
        pad = 12
        img = Image.new("RGBA", (int(adv) + pad * 2, ascent + descent), (0, 0, 0, 0))
        ImageDraw.Draw(img).text((pad, 0), ch, font=font, fill=col)
        fname = f"logo_{i:02d}_{ch}.png"
        img.save(out("UI", "Logo", fname))
        # pivot at the letter's centre on the cap-height middle line
        piv_y = 1 - (baseline_y - cap_px / 2) / img.height
        centers.append((fname, pen + adv / 2, piv_y))
        pen += adv
        if i == 3:
            pen += cap_px * 0.12  # small gap between "Nori" and "Verse", like the reference
    for fname, cx, piv_y in centers:
        layout["letters"].append({"sprite": fname, "x": round((cx - pen / 2) / PIXELS_PER_UNIT, 4),
                                  "pivotY": round(piv_y, 4)})

    tcap = TAGLINE_CAP_UNITS * PIXELS_PER_UNIT
    tprobe = ImageFont.truetype(TAGLINE_FONT, 1000, index=TAGLINE_FONT_INDEX)
    tratio = (tprobe.getbbox("P")[3] - tprobe.getbbox("P")[1]) / 1000.0
    tfont = ImageFont.truetype(TAGLINE_FONT, int(tcap / tratio), index=TAGLINE_FONT_INDEX)
    text = "Play. Learn. Explore."
    bbox = tfont.getbbox(text)
    img = Image.new("RGBA", (bbox[2] + 24, bbox[3] + 24), (0, 0, 0, 0))
    ImageDraw.Draw(img).text((12, 0), text, font=tfont, fill=TAGLINE_GRAY)
    img.save(out("UI", "Logo", "tagline.png"))
    layout["tagline"] = {"sprite": "tagline.png"}
    with open(out("UI", "Logo", "logo_layout.json"), "w") as f:
        json.dump(layout, f, indent=2)


def main():
    build_shadow()
    build_white()
    build_logo()
    print("Local art written to", ART)


if __name__ == "__main__":
    main()
