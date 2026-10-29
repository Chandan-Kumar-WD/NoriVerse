"""Generates the Meshy-made sprites for Video 002 (kitchen menu + batter cooking screen).

Run from the project root:  python3 Tools/ArtPipeline/gen_kitchen_art.py [name ...]
With no names it generates everything not already in meshy_raw/kitchen/. 9 credits per image (nano-banana-pro).
"""
import json
import os
import sys
from concurrent.futures import ThreadPoolExecutor

sys.path.insert(0, os.path.dirname(__file__))
from meshy_image import generate  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))
RAW = os.path.join(HERE, "meshy_raw")
OUT = os.path.join(RAW, "kitchen")
TASKS = os.path.join(OUT, "tasks.json")
PEEK = os.path.join(RAW, "peek_smile.png")

STYLE = ("soft rendered 3D-like cartoon illustration in the style of a premium kids' cooking game, smooth shading, "
         "soft highlights, clean shapes, bright cheerful colours, ")
ISOLATED = "centred, generous empty margin, plain white background, no shadow, no text. "
MAGENTA = "centred, generous empty margin, isolated on a solid flat pure magenta (#FF00FF) background, no shadow, no text. "
SAME = ("The exact same image as the reference: identical character, identical framing, size, position, cropping at the bottom, "
        "both paws in the same place, same lighting, nothing drawn below the cut line, no ledge, no wall, plain background. "
        "Change ONLY: ")

# name -> dict(prompt, aspect, kind: nori|text, keep_background)
JOBS = {
    # ---- Nori behind the counter: extra moods (base is peek_smile, already generated) ------------------
    "nori_half_blink": dict(kind="nori", prompt=SAME + "both eyes half closed, eyelids halfway down, mouth a gentle smile."),
    "nori_worried": dict(kind="nori", prompt=SAME + "a worried expression: eyebrows raised and tilted up in the middle, mouth a small downturned frown, eyes wide open looking at the viewer."),
    "nori_worried_left": dict(kind="nori", prompt=SAME + "a worried expression: eyebrows raised and tilted up in the middle, small downturned frown, eyes looking down and to the viewer's left."),
    "nori_worried_right": dict(kind="nori", prompt=SAME + "a worried expression: eyebrows raised and tilted up in the middle, small downturned frown, eyes looking down and to the viewer's right."),
    "nori_worried_closed": dict(kind="nori", prompt=SAME + "both eyes tightly closed in a worried blink, eyebrows tilted up in the middle, small downturned frown."),
    "nori_surprised_arms": dict(kind="nori", prompt=SAME + "a surprised 'oh!' expression with eyes wide open and a small round open mouth, and both paws lifted up off the bottom edge to shoulder height with fingers spread."),
    "nori_yawn": dict(kind="nori", prompt=SAME + "a big yawn: eyes closed, mouth wide open, tongue visible."),
    "nori_sleepy": dict(kind="nori", prompt=SAME + "a sleepy expression: eyes mostly closed with heavy eyelids, mouth slightly open, drowsy."),
    # ---- in-between frames so big expression changes can pass through a middle step ---------------------
    "nori_arms_half": dict(kind="nori", prompt=SAME + "a worried-surprised expression with eyes wide and a small open mouth, and both paws lifted only halfway up off the bottom edge to about chest height, fingers slightly spread."),
    "nori_yawn_half": dict(kind="nori", prompt=SAME + "the start of a yawn: eyes squinting nearly closed, mouth opening about halfway, tongue just visible."),
    "nori_smile_wide": dict(kind="nori", prompt=SAME + "a wider happy smile, mouth open a little with a small tongue, eyes open and bright, cheeks lifted."),
    # ---- Pot (from the end of the splash) -------------------------------------------------------
    "pot_body": dict(kind="text", aspect="1:1", prompt="A bright sky-blue enamel cooking pot with two side handles, no lid, filled with colourful vegetables peeking out of the top (carrot, broccoli, tomato, onion), front view. " + STYLE + ISOLATED),
    "pot_lid": dict(kind="text", aspect="1:1", prompt="A bright sky-blue enamel pot lid with a round knob on top, seen from the front tilted slightly open as if resting at an angle. " + STYLE + ISOLATED),
    # ---- Kitchen menu ----------------------------------------------------------------------------
    "wall": dict(kind="text", aspect="16:9", keep_background=True, prompt="A plain bright turquoise (#6BD3D6) kitchen wall surface with a very subtle soft round-dot pattern, flat game background, no objects, no text, fills the whole image edge to edge. " + STYLE),
    "plants_shelf": dict(kind="text", aspect="4:3", prompt="Two potted green herb plants (basil and mint) in terracotta pots sitting on a small white wall shelf, front view. " + STYLE + ISOLATED),
    "window": dict(kind="text", aspect="4:3", prompt="A kitchen window with a rounded turquoise frame showing a sunny blue sky with white clouds, green trees and a pink house outside, front view. " + STYLE + ISOLATED),
    "counter": dict(kind="text", aspect="16:9", keep_background=True, prompt="A bright yellow (#FFD23F) kitchen counter top surface seen straight on, with a lighter rim line along the top edge and a subtle soft round-dot pattern, flat game background, fills the whole image edge to edge, no objects, no text. " + STYLE),
    "dish_plate": dict(kind="text", aspect="1:1", keep_background=True, prompt="An empty round white glossy dinner plate with a light blue glossy rim, top-down view. " + STYLE + MAGENTA),
    "food_pancakes": dict(kind="text", aspect="1:1", keep_background=True, prompt="A stack of three fluffy golden pancakes topped with butter and maple syrup and a strawberry, no plate, top-down view. " + STYLE + MAGENTA),
    "food_cheese_toast": dict(kind="text", aspect="1:1", keep_background=True, prompt="A square slice of cheesy toast with melted cheese and herbs, no plate, top-down view. " + STYLE + MAGENTA),
    "food_pear_tart": dict(kind="text", aspect="1:1", prompt="A round dark plate with a round pear tart with sliced pears and cream, top-down view. " + STYLE + ISOLATED),
    "food_heart_pizza": dict(kind="text", aspect="1:1", keep_background=True, prompt="A heart-shaped pizza with tomato, basil and cheese, no plate, top-down view. " + STYLE + MAGENTA),
    "food_cookie_pizza": dict(kind="text", aspect="1:1", keep_background=True, prompt="A big round chocolate cookie covered in dark chocolate sandwich cookie pieces, no plate, top-down view. " + STYLE + MAGENTA),
    "food_star_fruit": dict(kind="text", aspect="1:1", prompt="A star-shaped plate with sliced kiwi, orange and berries arranged in a star, top-down view. " + STYLE + ISOLATED),
    "food_veggie_pizza": dict(kind="text", aspect="1:1", prompt="A round plate with a pizza topped with broccoli, mushrooms, olives and cheese, top-down view. " + STYLE + ISOLATED),
    "padlock": dict(kind="text", aspect="1:1", prompt="A shiny golden padlock, closed, front view. " + STYLE + ISOLATED),
    "btn_back": dict(kind="text", aspect="1:1", prompt="A round golden-orange button with a white rim and a white left-pointing arrow in the middle, front view. " + STYLE + ISOLATED),
    "btn_hands": dict(kind="text", aspect="1:1", prompt="A rounded-square light blue button with a white rim showing two white cartoon hands swapping, front view. " + STYLE + ISOLATED),
    "btn_chef": dict(kind="text", aspect="1:1", prompt="A rounded-square purple button with a white rim showing a white chef hat icon, front view. " + STYLE + ISOLATED),
    # ---- Cooking screen --------------------------------------------------------------------------
    "cook_bg": dict(kind="text", aspect="16:9", keep_background=True, prompt="A top-down kitchen table scene background: a bright yellow (#FFE066) table surface with a wide orange (#F4A94E) table runner across the horizontal middle with faint triangle and circle patterns and a darker orange border line along its top and bottom edges. Empty, no objects, flat game background that fills the whole image edge to edge. " + STYLE),
    "bowl": dict(kind="text", aspect="1:1", prompt="A large round very pale ice-blue and nearly white glossy ceramic mixing pot seen from directly above, with a thin light-cyan outline on the rim, a soft pale-blue empty inside, and two small loop handles at the top and bottom. Colours are very pale and soft, not saturated. " + STYLE + ISOLATED),
    "tray": dict(kind="text", aspect="3:4", keep_background=True, prompt="A tall rectangular glossy tray in pale ice-blue (#D9EEFA) with rounded corners, a thin light-cyan outline on the raised rim and subtle horizontal ridges inside, seen from directly above, empty, opaque and solid. Colours are very pale and soft. " + STYLE + MAGENTA),
    "butter_curl": dict(kind="text", aspect="1:1", prompt="A single golden-orange butter curl, a rolled ruffled butter shell seen from above, glossy. " + STYLE + ISOLATED),
    "flour_pile": dict(kind="text", aspect="1:1", keep_background=True, prompt="A single fluffy round pile of white flour with soft puffs around it, seen from above. " + STYLE + MAGENTA),
    "sugar_crystals": dict(kind="text", aspect="1:1", prompt="A small clump of pale blue-white sugar crystals, sparkly cubes, seen from above. " + STYLE + ISOLATED),
    "whisk": dict(kind="text", aspect="4:3", prompt="A balloon whisk with pale blue wire loops and an orange wooden handle seen from directly above, lying horizontally with the whisk head on the left and the handle to the right. " + STYLE + ISOLATED),
    "hand_point": dict(kind="text", aspect="1:1", prompt="A white cartoon glove hand with the index finger pointing up-left, with a small cuff, as a tutorial tap cursor, soft shading. " + STYLE + ISOLATED),
    "batter_1": dict(kind="text", aspect="1:1", prompt="A round disc of liquid seen from directly above: pale milky yellow batter just beginning to mix, with a few translucent soft blobs of white flour and orange butter dissolving in it. Only the round disc, with a softly wavy edge. " + STYLE + ISOLATED),
    "batter_2": dict(kind="text", aspect="1:1", prompt="A round disc of liquid seen from directly above: vivid golden-yellow batter with a bright golden swirl ring in the middle and a few small pale lumps. Only the round disc, with a softly wavy edge. " + STYLE + ISOLATED),
    "batter_3": dict(kind="text", aspect="1:1", prompt="A round disc of liquid seen from directly above: pale golden pancake batter with soft gentle swirl lines spiralling outwards. Only the round disc, with a softly wavy edge. " + STYLE + ISOLATED),
    "batter_4": dict(kind="text", aspect="1:1", prompt="A round disc of liquid seen from directly above: perfectly smooth warm pale-golden pancake batter with a soft glossy highlight and tiny bubbles. Only the round disc, with a softly wavy edge. " + STYLE + ISOLATED),
}


def load_tasks():
    return json.load(open(TASKS)) if os.path.exists(TASKS) else {}


def run(name):
    job = JOBS[name]
    out = os.path.join(OUT, f"{name}.png")
    kwargs = dict(out=out, prompt=job["prompt"], keep_background=job.get("keep_background", False))
    if job["kind"] == "nori":
        kwargs.update(refs=[PEEK], aspect="4:3")
    else:
        kwargs.update(text_only=True, aspect=job.get("aspect"))
    task_id = generate(**kwargs)
    tasks = load_tasks()
    tasks[name] = task_id
    json.dump(tasks, open(TASKS, "w"), indent=2)
    print(f"[done] {name} {task_id}", flush=True)


def main():
    os.makedirs(OUT, exist_ok=True)
    explicit = sys.argv[1:]
    names = explicit or [n for n in JOBS if not os.path.exists(os.path.join(OUT, f"{n}.png"))]
    with ThreadPoolExecutor(max_workers=4) as pool:
        for n, f in [(n, pool.submit(run, n)) for n in names]:
            try:
                f.result()
            except Exception as exc:
                print(f"[FAILED] {n}: {exc}", flush=True)


if __name__ == "__main__":
    main()
