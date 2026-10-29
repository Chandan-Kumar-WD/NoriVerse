"""Turns the raw Meshy images for Video 002 into Unity-ready sprites.

Run from the project root:  python3 Tools/ArtPipeline/process_kitchen_art.py   (needs pillow + numpy)

* Images generated on a magenta background are chroma-keyed (white-on-white subjects can't be cut out by Meshy).
* Everything is cropped to its content; backgrounds keep their full frame.
* Nori's moods share the exact crop box of the peek set, so cross-fading between them stays pixel-aligned.
* Writes Assets/_Project/Art/Kitchen/layout.json: for every sprite its pixels-per-unit (so it imports at its on-screen size
  in Unity units, camera size 5) and pivot.
"""
import json
import os

import numpy as np
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
RAW = os.path.join(HERE, "meshy_raw")
KIT = os.path.join(RAW, "kitchen")
ART = os.path.abspath(os.path.join(HERE, "..", "..", "Assets", "_Project", "Art"))

# Keep the established face anchor, with enough horizontal space for the raised paws.
PEEK_CROP = (113, 0, 1113, 896)
layout = {}


def out(rel):
    path = os.path.join(ART, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    return path


def register(rel, img, width_units=None, height_units=None, pivot=(0.5, 0.5)):
    """Saves img and records its import settings; the size is fixed by width or height in Unity units."""
    img.save(out(rel))
    ppu = img.width / width_units if width_units else img.height / height_units
    layout[rel.replace("\\", "/")] = {"ppu": round(ppu, 3), "pivotX": pivot[0], "pivotY": pivot[1]}


def key_magenta(im):
    """Keys out the flat background colour sampled from the image corners (Meshy's 'magenta' varies per image)."""
    arr = np.array(im.convert("RGBA")).astype(float)
    h, w = arr.shape[:2]
    corners = np.concatenate([arr[:12, :12, :3].reshape(-1, 3), arr[:12, -12:, :3].reshape(-1, 3),
                              arr[-12:, :12, :3].reshape(-1, 3), arr[-12:, -12:, :3].reshape(-1, 3)])
    bg = np.median(corners, axis=0)
    dist = np.sqrt(((arr[..., :3] - bg) ** 2).sum(axis=2))
    alpha = np.clip((dist - 40) / 60.0, 0, 1)
    # de-spill: pull edge pixels away from the background colour
    edge = (alpha > 0) & (alpha < 1)
    for c in range(3):
        ch = arr[..., c]
        ch[edge] = np.clip((ch[edge] - bg[c] * (1 - alpha[edge])) / np.maximum(alpha[edge], 0.2), 0, 255)
        arr[..., c] = ch
    arr[..., 3] = np.minimum(arr[..., 3], alpha * 255)
    return clean(Image.fromarray(arr.astype("uint8")))


def clean(im):
    arr = np.array(im)
    arr[arr[..., 3] == 0, :3] = 0
    return Image.fromarray(arr)


def trim(im, pad=4, thresh=8):
    a = np.array(im.getchannel("A"))
    ys, xs = np.where(a > thresh)
    box = (max(xs.min() - pad, 0), max(ys.min() - pad, 0), min(xs.max() + pad + 1, im.width), min(ys.max() + pad + 1, im.height))
    return im.crop(box)


def load(name, keyed=False):
    im = Image.open(os.path.join(KIT, name + ".png")).convert("RGBA")
    return key_magenta(im) if keyed else im


def load_root(name):
    return Image.open(os.path.join(RAW, name + ".png")).convert("RGBA")


# --------------------------------------------------------------------------
def nori_bust():
    sources = {
        "smile": load_root("peek_smile"), "blink": load_root("peek_blink"), "big_smile": load_root("peek_talk"),
        "surprised": load_root("peek_ooh"),
        "half_blink": load("nori_half_blink"), "worried": load("nori_worried"), "worried_left": load("nori_worried_left"),
        "worried_right": load("nori_worried_right"), "worried_closed": load("nori_worried_closed"),
        "surprised_arms": load("nori_surprised_arms"), "yawn": load("nori_yawn"), "sleepy": load("nori_sleepy"),
        "arms_half": load("nori_arms_half"), "yawn_half": load("nori_yawn_half"), "smile_wide": load("nori_smile_wide"),
    }
    for name, im in sources.items():
        crop = clean(im.crop(PEEK_CROP))
        # Face width ~600 px -> 4.0 u: 150 px per unit. Bottom-centre pivot sits on the counter.
        register(f"Characters/Nori/bust/nori_bust_{name}.png", crop, width_units=crop.width / 150.0, pivot=(0.5, 0.0))
        print("bust", name)


def bust_paws():
    """Dedicated complete paw silhouettes, without torso remnants from a rectangular bust crop."""
    arr = np.array(load("counter_paws_clean")).astype(float)
    # This image's pink backdrop is shaded. Key its hue rather than one corner
    # colour; avoid inverse colour unmixing, which made dark cuff edges green.
    excess = (arr[..., 0] + arr[..., 2]) * 0.5 - arr[..., 1]
    alpha = np.clip((65 - excess) / 45, 0, 1)
    edge = alpha < 1
    for c in (0, 2):
        arr[..., c][edge] = np.minimum(arr[..., c][edge], arr[..., 1][edge] + 12)
    arr[..., 3] *= alpha
    im = clean(Image.fromarray(arr.astype("uint8")))
    for name, half in (("L", (0, 0, im.width // 2, im.height)),
                       ("R", (im.width // 2, 0, im.width, im.height))):
        sprite = trim(im.crop(half))
        register(f"Characters/Nori/bust/nori_bust_paw{name}.png", sprite, width_units=1.05)


def pot():
    body = trim(load("pot_body"))
    register("Kitchen/Pot/pot_body.png", body, width_units=3.3)
    lid = trim(load("pot_lid"))
    register("Kitchen/Pot/pot_lid.png", lid, width_units=1.9)
    print("pot", body.size, lid.size)


def counter_asset():
    counter = load("counter")
    cw, ch = counter.size
    strip = counter.crop((int(cw * 0.03), int(ch * 0.04), int(cw * 0.97), int(ch * 0.04 + cw * 0.94 / 5.6)))
    rows = np.median(np.array(strip)[..., :3], axis=1)
    cream_start = next((i for i, rgb in enumerate(rows) if rgb[0] > rgb[2] + 12 and rgb[0] > 220), 0)
    strip = strip.crop((0, cream_start, strip.width, strip.height))
    register("Kitchen/Menu/counter.png", strip, width_units=24.0, pivot=(0.5, 1.0))


def menu():
    wall = load("wall", keyed=False)
    m = int(wall.width * 0.04), int(wall.height * 0.06)
    wall = wall.crop((m[0], m[1], wall.width - m[0], wall.height - m[1]))
    register("Kitchen/Menu/wall.png", wall, width_units=24.0)
    counter_asset()

    plants = trim(load("plants_shelf"))
    # draw the white wall shelf under the plants (the generated image has none)
    shelf_h = int(plants.height * 0.10)
    canvas = Image.new("RGBA", (int(plants.width * 1.08), plants.height + shelf_h), (0, 0, 0, 0))
    d = ImageDraw.Draw(canvas)
    sx0, sx1 = 0, canvas.width
    y0 = plants.height - int(shelf_h * 0.35)
    d.rounded_rectangle((sx0, y0, sx1, y0 + shelf_h), radius=shelf_h // 2, fill=(236, 246, 250, 255), outline=(176, 214, 226, 255), width=3)
    canvas.alpha_composite(plants, ((canvas.width - plants.width) // 2, 0))
    register("Kitchen/Menu/plants_shelf.png", canvas, width_units=3.9)

    register("Kitchen/Menu/window.png", trim(load("window")), width_units=5.0)

    plate = trim(load("dish_plate", keyed=True))
    register("Kitchen/Menu/dish_plate.png", plate, width_units=2.9)
    foods = {"pancakes": (True, 1.95), "cheese_toast": (True, 1.7), "heart_pizza": (True, 2.0), "cookie_pizza": (True, 2.1),
             "pear_tart": (False, 2.5), "star_fruit": (False, 2.55), "veggie_pizza": (False, 2.5)}
    for name, (keyed, width) in foods.items():
        register(f"Kitchen/Menu/food_{name}.png", trim(load("food_" + name, keyed=keyed)), width_units=width)
    register("Kitchen/Menu/padlock.png", trim(load("padlock")), width_units=0.95)
    register("Kitchen/Menu/btn_back.png", trim(load("btn_back")), width_units=1.14)
    register("Kitchen/Menu/btn_hands.png", trim(load("btn_hands")), width_units=1.35)
    register("Kitchen/Menu/btn_chef.png", trim(load("btn_chef")), width_units=1.35)
    print("menu done")


def cooking():
    bg = load("cook_bg")
    mx, my = int(bg.width * 0.03), int(bg.height * 0.04)       # trim the soft white border Meshy leaves
    bg = bg.crop((mx, my, bg.width - mx, bg.height - my))
    register("Kitchen/Cooking/cook_bg.png", bg, width_units=24.0)
    register("Kitchen/Cooking/bowl.png", trim(load("bowl")), width_units=7.4)
    register("Kitchen/Cooking/tray.png", trim(load("tray", keyed=True)), height_units=9.2)
    register("Kitchen/Cooking/butter_curl.png", trim(load("butter_curl")), width_units=1.8)
    register("Kitchen/Cooking/flour_pile.png", trim(load("flour_pile", keyed=True)), width_units=2.4)
    register("Kitchen/Cooking/sugar_crystals.png", trim(load("sugar_crystals")), width_units=1.5)
    whisk = trim(load("whisk"))
    register("Kitchen/Cooking/whisk.png", whisk, width_units=3.6, pivot=(0.17, 0.5))   # pivot on the whisk head
    hand = trim(load("hand_point"))
    register("Kitchen/Cooking/hand_point.png", hand, width_units=1.7, pivot=(0.12, 0.93))  # pivot on the fingertip
    for i in range(1, 5):
        register(f"Kitchen/Cooking/batter_{i}.png", trim(load(f"batter_{i}")), width_units=5.9)
    print("cooking done")


def shelf_assets():
    """Approved illustrated shelves; herb pots remain a separately grounded layer."""
    register("Kitchen/Menu/herb_pots.png", trim(load("plants_shelf")), width_units=3.6, pivot=(0.5, 0))
    for name, width in (("dish_display_shelf", 22.2), ("herb_wall_shelf", 4.1)):
        register(f"Kitchen/Menu/{name}.png", trim(load(name, keyed=True)), width_units=width)


if __name__ == "__main__":
    manifest = out("Kitchen/layout.json")
    if os.path.exists(manifest):
        layout.update({s["path"]: {k: v for k, v in s.items() if k != "path"}
                       for s in json.load(open(manifest))["sprites"]})
    nori_bust()
    bust_paws()
    pot()
    menu()
    cooking()
    shelf_assets()
    json.dump({"sprites": [{"path": k, **v} for k, v in layout.items()]}, open(out("Kitchen/layout.json"), "w"), indent=1)
    print("wrote", len(layout), "sprites")
    if os.path.isdir(os.path.join(KIT, "served")):
        from process_served_dishes import process as process_served
        process_served()
