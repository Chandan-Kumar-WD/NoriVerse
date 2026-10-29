"""Generates every Meshy-made sprite for the NoriVerse splash (Video 001).

Run from the project root:  python3 Tools/ArtPipeline/gen_splash_art.py [name ...]
With no names it generates everything that is not already in meshy_raw/. Each image costs
9 credits (nano-banana-pro). Task ids are recorded in meshy_raw/tasks.json so expression
variants can be made from their base pose with the same framing.
"""
import json
import os
import sys
from concurrent.futures import ThreadPoolExecutor

sys.path.insert(0, os.path.dirname(__file__))
from meshy_image import generate  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))
RAW = os.path.join(HERE, "meshy_raw")
REF = os.path.join(HERE, "references")
TASKS = os.path.join(RAW, "tasks.json")

STYLE = ("The exact same chef raccoon character Nori from the reference images, same art style "
         "(soft rendered 3D-like cartoon illustration, fluffy fur, white chef hat, yellow goggles resting "
         "on the hat, red neckerchief, white chef jacket with dark buttons, teal apron with front pocket "
         "holding a wooden spoon and whisk, striped tail). Same face, proportions and colours as the "
         "reference. Single character, centred, whole figure inside the frame with generous empty margin "
         "on all sides (nothing cut off, especially the top of the hat), plain white background, "
         "no ground shadow, no text. ")
STYLE_PEEK = STYLE.replace("whole figure inside the frame with generous empty margin on all sides "
                           "(nothing cut off, especially the top of the hat)",
                           "generous empty margin on the left, right and top (the top of the hat must not be cut off)")
VARIANT = ("Same image, identical pose, framing, size, position and colours; change only: ")
PROP = ("soft rendered 3D-like cartoon illustration in the style of a premium kids' game, smooth shading, "
        "soft highlights, front view, centred, generous empty margin, plain white background, no shadow, no text. ")

POSE_REFS = [f"{REF}/nori_ref_wave.png", f"{REF}/nori_ref_celebrate.png", f"{REF}/nori_ref_think.png",
             f"{RAW}/test_stand.png"]

# name -> dict(prompt, refs|from_task name, aspect, text_only)
JOBS = {
    "peek_smile": dict(refs=POSE_REFS, aspect="4:3", prompt=STYLE_PEEK +
        "Only the head and upper chest are visible, peeking up from below the bottom edge of the image; "
        "both dark-brown paws rest on the bottom edge to the left and right of the face, fingers up, "
        "gripping the edge; facing the viewer, happy closed-mouth smile, eyes open. The image is cut "
        "straight horizontally at chest level."),
    "lean_curious": dict(refs=POSE_REFS, aspect="3:4", prompt=STYLE +
        "Leaning forward and slightly to the viewer's right, head tilted down and to the right, looking "
        "down curiously at something on a table in front of him at waist height, small round 'o' mouth, "
        "one paw reaching forward and down."),
    "surprised": dict(refs=POSE_REFS, aspect="3:4", prompt=STYLE +
        "Standing upright, both paws slightly raised in front of the chest, eyes wide open, mouth open in "
        "a delighted round 'oh' of surprise."),
    "arms_spread": dict(refs=POSE_REFS, aspect="3:4", prompt=STYLE +
        "Both arms stretched wide out to the sides, paws open, big happy open-mouth smile, eyes open "
        "looking at the viewer, standing upright."),
    "hold_chopsticks": dict(refs=POSE_REFS, aspect="3:4", prompt=STYLE +
        "Holding a pair of purple chopsticks in the paw on the viewer's left at chest height, chopsticks "
        "pointing horizontally toward the viewer's left; the other paw resting forward at waist height; "
        "happy open smile, looking at the viewer."),
    "stand_blink": dict(base="stand", prompt=VARIANT + "both eyes closed in a happy squint."),
    "peek_talk": dict(base="peek_smile", prompt=VARIANT + "the mouth open wide, happily talking, tongue visible."),
    "peek_ooh": dict(base="peek_smile", prompt=VARIANT + "the mouth a small round 'ooh', eyebrows raised, excited."),
    "peek_blink": dict(base="peek_smile", prompt=VARIANT + "both eyes closed in a happy blink."),
    "eat": dict(base="hold_chopsticks", prompt=VARIANT + "he is now bringing the chopsticks with a few noodles up "
                "to his open mouth, eating."),
    "chew": dict(base="hold_chopsticks", prompt=VARIANT + "chopsticks lowered at chest height, eyes closed "
                 "happily, mouth closed and chewing, cheeks puffed."),
    "hold_blink": dict(base="hold_chopsticks", prompt=VARIANT + "both eyes closed in a blink."),
    "table": dict(text_only=True, aspect="16:9", prompt="A small table covered with a bright green tablecloth "
        "(#8CB51B) with a wavy hem and a subtle darker-green raindrop pattern, short round wooden legs, "
        "straight-on front view. " + PROP),
    "cloche": dict(text_only=True, aspect="1:1", prompt="A closed silver-blue serving dome cloche with a round "
        "knob on top, front view. " + PROP),
    "noodle_bowl": dict(text_only=True, aspect="1:1", prompt="A yellow ceramic bowl full of noodles with green "
        "bok choy leaves, front view, slightly from above. " + PROP),
    "heart": dict(text_only=True, aspect="1:1", prompt="A single soft pink puffy heart. " + PROP),
}
# the approved test pose is the base for `stand`
KNOWN_TASKS = {"stand": "01a17e98-497d-767b-b238-cb28f27fa648"}


def load_tasks():
    tasks = dict(KNOWN_TASKS)
    if os.path.exists(TASKS):
        tasks.update(json.load(open(TASKS)))
    return tasks


def save_task(name, task_id):
    tasks = load_tasks()
    tasks[name] = task_id
    json.dump(tasks, open(TASKS, "w"), indent=2)


def run(name):
    job = JOBS[name]
    out = os.path.join(RAW, f"{name}.png")
    kwargs = dict(out=out, prompt=job["prompt"], aspect=job.get("aspect"))
    if job.get("text_only"):
        kwargs["text_only"] = True
    elif "base" in job:
        kwargs["from_task"] = load_tasks()[job["base"]]
        kwargs["aspect"] = None  # variants keep the base pose's framing
    else:
        kwargs["refs"] = job["refs"]
    task_id = generate(**kwargs)
    save_task(name, task_id)
    print(f"[done] {name} {task_id}", flush=True)


def main():
    names = sys.argv[1:] or list(JOBS)
    names = [n for n in names if sys.argv[1:] or not os.path.exists(os.path.join(RAW, f"{n}.png"))]
    bases = [n for n in names if "base" not in JOBS[n]]
    variants = [n for n in names if "base" in JOBS[n]]
    os.makedirs(RAW, exist_ok=True)
    for group in (bases, variants):
        with ThreadPoolExecutor(max_workers=4) as pool:
            for n, f in [(n, pool.submit(run, n)) for n in group]:
                try:
                    f.result()
                except Exception as exc:  # keep going, report at the end
                    print(f"[FAILED] {n}: {exc}", flush=True)


if __name__ == "__main__":
    main()
