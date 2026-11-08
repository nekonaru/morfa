#!/usr/bin/env python3
"""Generates src/Morfa.Desktop/Assets/Sample/sample.png: a friendly photo-like sample
the first-run UI offers to convert, so users can try Morfa with something real.

Run from the repository root: python build/tools/make-sample.py
"""
import math
import os

from PIL import Image, ImageDraw, ImageFont

W, H = 1200, 800

def linear_vertical(w, h, top, bottom):
    img = Image.new("RGB", (w, h))
    d = ImageDraw.Draw(img)
    for y in range(h):
        t = y / (h - 1)
        d.line([(0, y), (w, y)], fill=tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)))
    return img

img = linear_vertical(W, H, (142, 197, 252), (248, 210, 166))  # peach sky
d = ImageDraw.Draw(img)
# Sun
d.ellipse([880, 110, 1080, 310], fill=(255, 242, 194))
# Far hills + near land
d.polygon([(0, 520), (300, 360), (620, 520), (0, 600)], fill=(108, 154, 91))
d.polygon([(520, 520), (840, 380), (1200, 520), (1200, 580), (520, 580)], fill=(63, 110, 74))
d.rectangle([0, 600, W, H], fill=(79, 124, 172))
# Reflection strokes on water
for i in range(14):
    y = 640 + i * 11
    x0 = 480 + int(40 * math.sin(i * 1.7))
    d.line([(x0, y), (x0 + 190, y)], fill=(148, 196, 232), width=3)
# Sailboat
hull = (122, 78, 45)
white = (242, 242, 242)
d.polygon([(1030, 480), (990, 460), (1050, 460)], fill=hull)
d.polygon([(1005, 465), (1030, 330), (1050, 460)], fill=white)
d.polygon([(1032, 465), (1085, 375), (1095, 460)], fill=(255, 180, 84))
# Brand strip bottom-left
d.rectangle([0, 700, W, H], fill=(10, 110, 110))
font = None
for candidate in (
    "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
    "C:\\Windows\\Fonts\\seguisb.ttf",
    "C:\\Windows\\Fonts\\arialbd.ttf",
):
    if os.path.exists(candidate):
        font = ImageFont.truetype(candidate, 34)
        break
if font:
    d.text((36, 724), "Morfa sample", font=font, fill=(255, 255, 255))
    small = ImageFont.truetype(font.path, 20)
    d.text((280, 732), "Drag me onto a format", font=small, fill=(215, 245, 242))

out = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))),
                   "src", "Morfa.Desktop", "Assets", "Sample", "sample.png")
os.makedirs(os.path.dirname(out), exist_ok=True)
img.save(out, optimize=True)
print("Wrote", out, f"({os.path.getsize(out)} bytes)")
