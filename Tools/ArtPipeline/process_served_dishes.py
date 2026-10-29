"""Prepare requested complete plated dishes without changing their proportions."""
import json
from pathlib import Path
import cv2
import numpy as np
from PIL import Image, ImageFilter
import process_kitchen_art as art
from gen_served_dishes import RECIPES

def key_exterior(im):
    arr = np.array(im.convert('RGBA')).astype(float)
    # Only remove pink connected to the exterior. Purple berries inside a plate
    # remain opaque, even when their hue resembles the generated backdrop.
    pink = ((np.minimum(arr[..., 0], arr[..., 2]) - arr[..., 1] > 20)
            & (arr[..., 0] > 70) & (arr[..., 2] > 60)).astype('uint8')
    count, labels = cv2.connectedComponents(pink, connectivity=8)
    outer = np.unique(np.concatenate((labels[0], labels[-1], labels[:, 0], labels[:, -1])))
    outer = outer[outer != 0]
    bg = np.isin(labels, outer)
    alpha = np.array(Image.fromarray((~bg * 255).astype('uint8')).filter(ImageFilter.GaussianBlur(0.45))) / 255.0
    edge = (alpha > 0) & (alpha < 0.99)
    for c in (0, 2):
        arr[..., c][edge] = np.minimum(arr[..., c][edge], arr[..., 1][edge] + 15)
    arr[..., 3] *= alpha
    return art.clean(Image.fromarray(arr.astype('uint8')))

def process():
    path = Path(art.ART) / 'Kitchen/layout.json'
    art.layout.update({s['path']: {k:v for k,v in s.items() if k != 'path'} for s in json.loads(path.read_text())['sprites']})
    for name in RECIPES:
        raw = Path(art.KIT) / 'served' / (name + '.png')
        im = art.trim(key_exterior(Image.open(raw)))
        art.register('Kitchen/Menu/served_' + name + '.png', im, width_units=2.9, pivot=(0.5, 0))
        print(name, im.size)
    art.counter_asset()
    path.write_text(json.dumps({'sprites': [{'path': k, **v} for k,v in art.layout.items()]}, indent=1))

if __name__ == '__main__':
    process()
