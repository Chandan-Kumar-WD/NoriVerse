"""NoriVerse audio job list (Docs/Audio_Reference_Breakdown.md). Approved 2026-10-28: generate all 38 clips.

Skips any clip whose raw file already exists, so re-running never re-spends credits.
Usage: python3 Tools/AudioPipeline/gen_audio.py [voice|sfx|music|all]
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import elevenlabs_audio as el

RAW = os.path.join(os.path.dirname(os.path.abspath(__file__)), "raw")
NORI_VOICE = "FleaHIqMqAL0jx5Znrdf"   # "Nori - Spoonderful"

VOICE = {
    "vo_brand": "[excited] Noh-ree... Verse!",
    "vo_giggle": "[giggles] Hee-hee!",
    "vo_hmm_worried": "[softly, a little worried] Hmmm...",
    "vo_yawn": "[yawns] Aaahhh...",
    "vo_ooh": "[excited] Ooh!",
    "vo_yay": "[cheerful] Yay!",
    "vo_wow": "[amazed] Wow!",
    "vo_mmm_anticipate": "[hungry, delighted] Mmmm!",
    "vo_nom": "[munching happily] Nom nom nom!",
    "vo_chew": "[chewing happily] Mm-mm-mm.",
    "vo_yummy": "[satisfied] Mmm, yummy!",
    "vo_hum_happy": "[happy humming] Hmm-hmm-hmm.",
}

STYLE = "cute cartoon sound effect for a toddler cooking game, soft, clean, no music, no voice"
SFX = {  # id: (prompt, seconds or None, loop)
    "ui_tap": ("soft bubbly button pop click", 0.5, False),
    "dish_locked_wobble": ("short wobbly rubbery boing, gentle 'not yet' bounce", 0.7, False),
    "scene_whoosh": ("light airy sparkle whoosh transition", 0.9, False),
    "item_pop_in": ("tiny bright pop as a small food item appears", 0.5, False),
    "item_pickup": ("very short light pluck as a food item is picked up", 0.5, False),
    "drop_plop_bowl": ("soft plop of a food ingredient landing in a ceramic mixing bowl", 0.6, False),
    "drop_return": ("quick soft slide-back swish, item returning to its place", 0.5, False),
    "whisk_fly_in": ("descending cartoon whoosh with a light metal whisk jingle at the end", 1.0, False),
    "whisk_stir_loop": ("gentle continuous whisking of thick batter in a bowl, soft swishing", 3.0, True),
    "success_arpeggio": ("rising magical glockenspiel arpeggio with sparkles, task complete", 1.3, False),
    "rolling_pin_loop": ("wooden rolling pin rolling smoothly over soft dough on a board, continuous", 3.0, True),
    "dough_cut_trace": ("soft continuous pastry cutter gliding through dough", 2.0, True),
    "cut_complete_sparkle": ("short twinkly sparkle burst", 1.0, False),
    "sauce_squish_loop": ("soft squishy spreading of tomato sauce with a spoon, gentle and continuous", 3.0, True),
    "cheese_sprinkle": ("tiny soft patter of grated cheese sprinkling onto pizza", 0.5, False),
    "sort_correct_ding": ("cheerful single bell ding, correct answer", 0.8, False),
    "topping_place_pop": ("soft squishy pat as a pizza topping is placed", 0.5, False),
    "confirm_button": ("bright happy button click followed by a tiny chime", 0.8, False),
    "oven_rack_slide": ("metal oven rack sliding in smoothly, light rattle", 1.0, False),
    "oven_power_click": ("toy oven knob click then soft gas ignition whoomp", 1.0, False),
    "oven_bake_loop": ("warm gentle oven hum with soft crackle, continuous", 4.0, True),
    "oven_done_ding": ("cute kitchen timer bell ding ding", 1.2, False),
    "star_ding": ("magical sparkling star chime", 0.9, False),
    "confetti_pop": ("party popper pop with confetti flutter and sparkles", 1.5, False),
}

MUSIC = {
    "mus_splash": ("Cheerful children's game brand intro jingle, G major, about 150 BPM, glockenspiel, ukulele and "
                   "light hand percussion, playful and bouncy, ending on a sparkling bell flourish. Instrumental.", 9),
    "mus_gameplay_loop": ("Gentle, happy background music for a toddler cooking game, A major, about 145 BPM, soft "
                          "marimba, ukulele, pizzicato and light shaker, simple repeating melody, calm and "
                          "unobtrusive, steady energy with no intro or ending so it can loop. Instrumental.", 60),
}


def out(kind, name):
    return os.path.join(RAW, kind, f"{name}.mp3")


def run(kind):
    if kind in ("voice", "all"):
        for name, text in VOICE.items():
            if not os.path.exists(out("voice", name)):
                el.tts(NORI_VOICE, text, out("voice", name), "eleven_v3", stability=0.5)
    if kind in ("sfx", "all"):
        for name, (prompt, seconds, loop) in SFX.items():
            if not os.path.exists(out("sfx", name)):
                el.sfx(f"{prompt}. {STYLE}", seconds, out("sfx", name), loop=loop)
    if kind in ("music", "all"):
        for name, (prompt, seconds) in MUSIC.items():
            if not os.path.exists(out("music", name)):
                el.music(prompt, seconds, out("music", name))


if __name__ == "__main__":
    run(sys.argv[1] if len(sys.argv) > 1 else "all")
