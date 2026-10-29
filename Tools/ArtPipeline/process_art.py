"""Turns the raw Meshy images in meshy_raw/ into the aligned sprites Unity imports.

Run from the project root:  python3 Tools/ArtPipeline/process_art.py   (needs pillow + numpy)

Full-body Nori poses are put on one common canvas so swapping poses never makes him jump:
  * same scale (expression variants come back from Meshy on a square canvas at 1200/1024 scale),
  * feet centre at FEET_X, soles at FEET_Y.
The peek set is already framed identically by Meshy; it is only cropped to a common box.
Props are cropped to their content.
"""
import json
import os

import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
RAW = os.path.join(HERE, "meshy_raw")
ART = os.path.join(HERE, "..", "..", "Assets", "_Project", "Art")

CANVAS_W, CANVAS_H = 1100, 1280
FEET_X, FEET_Y = 550, 1240
HEM_CUT_Y = 902    # world y -3.08: safely inside the opaque cloth, above the wavy hem

# raw name -> (output name, scale relative to the 896x1200 base poses)
BODY = {
    "test_stand": ("stand", 1.0),
    "lean_curious": ("lean_curious", 1.0),
    "surprised": ("surprised", 1.0),
    "arms_spread": ("arms_spread", 1.0),
    "hold_chopsticks": ("hold_chopsticks", 1.0),
    "stand_blink": ("stand_blink", 1200 / 1024),
    "eat": ("eat", 1200 / 1024),
    "chew": ("chew", 1200 / 1024),
    "hold_blink": ("hold_blink", 1200 / 1024),
}
PEEK = ["peek_smile", "peek_talk", "peek_ooh", "peek_blink"]
PEEK_CROP = (268, 0, 958, 896)  # x0, y0, x1, y1 around the face centre
PROPS = {"table": "Props/table.png", "cloche": "Props/cloche.png",
         "noodle_bowl": "Props/noodle_bowl.png", "heart": "FX/heart.png"}


def alpha(im):
    return np.array(im.getchannel("A"))


def feet_center_and_bottom(im):
    mask = alpha(im) > 128
    ys, xs = np.where(mask)
    y1 = ys.max()
    y0 = ys.min()
    band = mask[int(y1 - (y1 - y0) * 0.12): y1 + 1]
    fx = np.where(band.any(0))[0]
    return (fx.min() + fx.max()) / 2.0, y1


def clean_edges(im):
    """Zero RGB where alpha is 0 so bilinear filtering never bleeds a halo."""
    arr = np.array(im)
    arr[arr[..., 3] == 0, :3] = 0
    return Image.fromarray(arr)


def out_path(rel):
    path = os.path.join(ART, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    return path


def process_body():
    layout = {}
    for raw, (name, scale) in BODY.items():
        im = Image.open(os.path.join(RAW, raw + ".png")).convert("RGBA")
        if scale != 1.0:
            im = im.resize((round(im.width * scale), round(im.height * scale)), Image.LANCZOS)
        fcx, fbottom = feet_center_and_bottom(im)
        canvas = Image.new("RGBA", (CANVAS_W, CANVAS_H), (0, 0, 0, 0))
        canvas.alpha_composite(im, (round(FEET_X - fcx), round(FEET_Y - fbottom)))
        if canvas.getchannel("A").getbbox() is None:
            raise ValueError(raw)
        clean_edges(canvas).save(out_path(f"Characters/Nori/nori_{name}.png"))
        # copy with everything below the table hem removed (used once the table is in front of him)
        cut = np.array(canvas)
        fade = 6
        cut[HEM_CUT_Y:, :, 3] = 0
        for k in range(fade):
            row = HEM_CUT_Y - fade + k
            cut[row, :, 3] = (cut[row, :, 3].astype(float) * (fade - k) / (fade + 1)).astype("uint8")
        clean_edges(Image.fromarray(cut)).save(out_path(f"Characters/Nori/nori_{name}_cut.png"))
        layout[name] = {"feetCenter": round(float(fcx * scale / scale), 1)}
        print("body", name)
    return layout


def process_peek():
    for name in PEEK:
        im = Image.open(os.path.join(RAW, name + ".png")).convert("RGBA").crop(PEEK_CROP)
        clean_edges(im).save(out_path(f"Characters/Nori/nori_{name}.png"))
        print("peek", name)


HAND_HEIGHT_PX = 600


def process_hands():
    """Raised forearm + paw (Meshy `hand_up_b`), cropped and set to a fixed height. Unity mirrors it for the other hand."""
    im = Image.open(os.path.join(RAW, "hand_up_b.png")).convert("RGBA")
    ys, xs = np.where(alpha(im) > 8)
    im = im.crop((xs.min(), ys.min(), xs.max() + 1, ys.max() + 1))
    im = im.resize((round(im.width * HAND_HEIGHT_PX / im.height), HAND_HEIGHT_PX), Image.LANCZOS)
    clean_edges(im).save(out_path("Characters/Nori/nori_hand_up.png"))
    print("hand", im.size)


def process_props():
    for raw, rel in PROPS.items():
        im = Image.open(os.path.join(RAW, raw + ".png")).convert("RGBA")
        a = alpha(im)
        ys, xs = np.where(a > 8)
        box = (max(xs.min() - 6, 0), max(ys.min() - 6, 0), min(xs.max() + 7, im.width), min(ys.max() + 7, im.height))
        clean_edges(im.crop(box)).save(out_path(rel))
        print("prop", raw, im.crop(box).size)


if __name__ == "__main__":
    process_body()
    process_peek()
    process_hands()
    process_props()
    json.dump({"canvas": [CANVAS_W, CANVAS_H], "feet": [FEET_X, FEET_Y], "peekCrop": PEEK_CROP},
              open(os.path.join(ART, "Characters", "Nori", "layout.json"), "w"), indent=2)
