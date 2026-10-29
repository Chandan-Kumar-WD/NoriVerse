"""Approved kitchen shelf replacement batch: two images, 18 credits maximum."""
from pathlib import Path
from meshy_image import generate

OUT = Path(__file__).parent / 'meshy_raw' / 'kitchen'
STYLE = ('Soft rendered cartoon illustration matching a premium preschool cooking game, smooth rounded edges, '
         'warm cream and honey wood, soft highlights, straight-on front view with a slightly visible top surface. ')
JOBS = {
    'dish_display_shelf': STYLE + ('One empty very long horizontal kitchen display ledge, length about 12 times its height, '
        'a broad flat wooden top surface to support seven plates, thick cream front fascia, golden wood lip, '
        'two short rounded support brackets beneath. No plates, no food, no plants, no objects, no text. '
        'Entire shelf visible, isolated centered with margin on solid pure magenta background.'),
    'herb_wall_shelf': STYLE + ('One empty short wall-mounted kitchen shelf, width about four times total height, '
        'a visible broad honey wood top board, thick cream front edge and two clearly visible cream triangular support '
        'brackets underneath, a believable solid shelf for two herb pots. No plants, no pots, no objects, no text. '
        'Entire shelf visible, isolated centered with margin on solid pure magenta background.')
}
if __name__ == '__main__':
    for name, prompt in JOBS.items():
        path = OUT / (name + '.png')
        if not path.exists():
            generate(str(path), prompt, text_only=True, aspect='16:9' if name == 'dish_display_shelf' else '4:3', keep_background=True)
