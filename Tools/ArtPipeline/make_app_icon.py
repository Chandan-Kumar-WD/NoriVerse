"""Cooking app icon: Nori waves with one hand and holds the game's tomato-basil pizza with the other, on a bright gradient.
1024x1024 RGB, no alpha, full bleed (iOS rounds the corners).

Built only from existing art: Nori's happy bust (head, body, resting arm), her raised open hand from the
"surprised arms" bust, her resting paw as the gripping hand, and the served pizza. Background, glow, shadows and
sparkles are generated here. Output: Assets/_Project/Art/AppIcon/app_icon_1024.png
"""
import os, math
import numpy as np
from PIL import Image, ImageFilter, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
BUST = os.path.join(ROOT, "Assets/_Project/Art/Characters/Nori/bust")
FOOD = os.path.join(ROOT, "Assets/_Project/Art/Kitchen/Menu/served_tomato_pizza.png")
OUT = os.path.join(ROOT, "Assets/_Project/Art/AppIcon/app_icon_1024.png")
S = 1024
PAD = 170    # extra canvas below the 896 px bust for the plate


def load(name):
    return Image.open(os.path.join(BUST, name + ".png")).convert("RGBA")


def pad(im):
    out = Image.new("RGBA", (im.width, im.height + PAD), (0, 0, 0, 0))
    out.paste(im, (0, 0))
    return out


def mask_shape(size, draw_fn, blur):
    m = Image.new("L", size, 0)
    draw_fn(ImageDraw.Draw(m))
    return m.filter(ImageFilter.GaussianBlur(blur))


smile, arms = pad(load("nori_bust_big_smile")), pad(load("nori_bust_surprised_arms"))
W, H = smile.size

# 1) pose: raised waving hand on the right (from the surprised pose), resting arm on the left, happy head on top
head = mask_shape((W, H), lambda d: (d.rectangle((0, 0, W, 585), fill=255), d.ellipse((165, 330, 835, 655), fill=255)), 10)
left = mask_shape((W, H), lambda d: d.rectangle((0, 560, 455, H), fill=255), 10)
pose = Image.composite(smile, arms, ImageChops_lighter := Image.fromarray(np.maximum(np.array(head), np.array(left))))

# 2) pizza held in front on the left; her gripping fist rests over the plate rim
food = Image.open(FOOD).convert("RGBA")
food = food.crop(food.getchannel("A").getbbox())
fw = 600
food = food.resize((fw, round(food.height * fw / food.width)), Image.LANCZOS)
fx, fy = 95, 745
sh = Image.new("RGBA", food.size, (110, 45, 0, 170))
sh.putalpha(food.getchannel("A").point(lambda v: int(v * 0.55)))
shadow = Image.new("RGBA", (W, H), (0, 0, 0, 0))
shadow.alpha_composite(sh, (fx + 6, fy + 16))
pose.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(14)))
pose.alpha_composite(food, (fx, fy))
paw = load("nori_bust_pawL")
paw = paw.resize((round(paw.width * 0.66), round(paw.height * 0.66)), Image.LANCZOS)
pose.alpha_composite(paw, (62, 770))

# 3) icon: gradient + glow + character
yy, xx = np.mgrid[0:S, 0:S].astype(np.float32)
d = np.clip(np.sqrt((xx - S * 0.5) ** 2 + (yy - S * 0.42) ** 2) / (S * 0.72), 0, 1)[..., None]
bg = (np.array([255, 233, 120], np.float32) * (1 - d) + np.array([255, 150, 60], np.float32) * d).astype(np.uint8)
img = Image.fromarray(bg, "RGB").convert("RGBA")
glow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
ImageDraw.Draw(glow).ellipse((S * 0.10, S * 0.08, S * 0.90, S * 0.88), fill=(255, 255, 255, 120))
img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(70)))

pose = pose.crop(pose.getchannel("A").getbbox())
scale = (S * 0.93) / pose.height
pose = pose.resize((round(pose.width * scale), round(pose.height * scale)), Image.LANCZOS)
x, y = (S - pose.width) // 2, int(S * 0.045)
cs = Image.new("RGBA", pose.size, (120, 50, 0, 150))
cs.putalpha(pose.getchannel("A").point(lambda v: int(v * 0.4)))
cshadow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
cshadow.alpha_composite(cs, (x + 6, y + 10))
img.alpha_composite(cshadow.filter(ImageFilter.GaussianBlur(14)))
img.alpha_composite(pose, (x, y))


def star(cx, cy, r, alpha=235):
    layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    pts = [(cx + (r if i % 2 == 0 else r * 0.26) * math.cos(math.pi / 4 * i - math.pi / 2),
            cy + (r if i % 2 == 0 else r * 0.26) * math.sin(math.pi / 4 * i - math.pi / 2)) for i in range(8)]
    ImageDraw.Draw(layer).polygon(pts, fill=(255, 255, 255, alpha))
    img.alpha_composite(layer.filter(ImageFilter.GaussianBlur(0.8)))


for cx, cy, r in [(0.10, 0.17, 56), (0.90, 0.10, 42), (0.07, 0.50, 28), (0.93, 0.40, 30)]:
    star(S * cx, S * cy, r)

os.makedirs(os.path.dirname(OUT), exist_ok=True)
img.convert("RGB").save(OUT, "PNG")
print("saved", OUT)
