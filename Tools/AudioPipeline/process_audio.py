"""Raw ElevenLabs clips -> Unity-ready OGG in Assets/_Project/Audio/{Music,Voice,SFX}.

Trims leading silence, fades tails (one-shots), loudness-normalises per category, and crossfades the
music loop's ending into its start so it loops seamlessly. Loops keep their exact length.
"""
import glob, os, subprocess
import numpy as np, imageio_ffmpeg

FF = imageio_ffmpeg.get_ffmpeg_exe()
HERE = os.path.dirname(os.path.abspath(__file__))
DEST = os.path.join(HERE, "..", "..", "Assets", "_Project", "Audio")
SR = 44100
TARGET_RMS = {"music": -20.0, "voice": -17.0, "sfx": -18.0}   # dBFS; game mixer scales further
LOOPS = {"whisk_stir_loop", "rolling_pin_loop", "dough_cut_trace", "sauce_squish_loop", "oven_bake_loop", "mus_gameplay_loop"}


def load(path):
    raw = subprocess.run([FF, "-v", "error", "-i", path, "-ac", "1", "-ar", str(SR), "-f", "f32le", "-"],
                         capture_output=True, check=True).stdout
    return np.frombuffer(raw, np.float32).copy()


def save(x, path):
    subprocess.run([FF, "-v", "error", "-y", "-f", "f32le", "-ar", str(SR), "-ac", "1", "-i", "-",
                    "-c:a", "libvorbis", "-q:a", "6", path], input=x.astype(np.float32).tobytes(), check=True)


def active_rms(x):
    frames = x[: len(x) // 1024 * 1024].reshape(-1, 1024)
    e = np.sqrt((frames ** 2).mean(1))
    e = e[e > 10 ** (-50 / 20)]
    return 20 * np.log10(np.sqrt((e ** 2).mean()) + 1e-9) if len(e) else -90


def seamless(x, seconds=1.5):
    n = int(SR * seconds)
    head, tail = x[:n], x[-n:]
    k = np.linspace(0, 1, n, dtype=np.float32)
    out = x[n:].copy()
    out[-n:] = tail * (1 - k) + head * k   # end blends into the original start, which is where playback restarts
    return out


def process(kind, path):
    name = os.path.splitext(os.path.basename(path))[0]
    x = load(path)
    loop = name in LOOPS
    if loop:
        x = seamless(x, 1.5 if kind == "music" else 0.4)
    else:
        nz = np.where(np.abs(x) > 10 ** (-45 / 20))[0]
        if len(nz):
            x = x[max(0, nz[0] - int(0.005 * SR)): nz[-1] + int(0.05 * SR)]
        f = min(len(x) // 4, int(0.04 * SR))
        x[-f:] *= np.linspace(1, 0, f, dtype=np.float32)
    x *= 10 ** ((TARGET_RMS[kind] - active_rms(x)) / 20)
    peak = np.abs(x).max()
    if peak > 0.97:
        x *= 0.97 / peak
    folder = os.path.join(DEST, {"music": "Music", "voice": "Voice", "sfx": "SFX"}[kind])
    os.makedirs(folder, exist_ok=True)
    save(x, os.path.join(folder, f"{name}.ogg"))
    print(f"{kind:5s} {name:22s} {len(x) / SR:5.2f}s loop={loop}")


for kind in ("music", "voice", "sfx"):
    for p in sorted(glob.glob(os.path.join(HERE, "raw", kind, "*.mp3"))):
        process(kind, p)
