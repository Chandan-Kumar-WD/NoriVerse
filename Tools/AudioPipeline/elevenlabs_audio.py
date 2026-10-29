"""Minimal ElevenLabs client for NoriVerse voice and sound effects.

Reads the API key from the ELEVENLABS_API_KEY environment variable (never printed).
Direct REST, so it works even when the MCP server cannot see the key.

Usage:
    python3 Tools/AudioPipeline/elevenlabs_audio.py check            # free: plan + character balance
    python3 Tools/AudioPipeline/elevenlabs_audio.py voices           # free: list available voices
    python3 Tools/AudioPipeline/elevenlabs_audio.py tts --voice VOICE_ID --text "Yummy!" --out OUT.mp3
    python3 Tools/AudioPipeline/elevenlabs_audio.py sfx --text "soft oven ding" --seconds 1.5 --out OUT.mp3

tts, sfx and music spend ElevenLabs credits. gen_audio.py holds the NoriVerse job list.
"""
import argparse
import json
import os
import sys
import urllib.error
import urllib.request

API_BASE = "https://api.elevenlabs.io/v1"


def _request(method, path, body=None, raw=False):
    key = os.environ.get("ELEVENLABS_API_KEY")
    if not key:
        sys.exit("ELEVENLABS_API_KEY is not set")
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(f"{API_BASE}{path}", data=data, method=method, headers={
        "xi-api-key": key, "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            payload = resp.read()
    except urllib.error.HTTPError as exc:
        sys.exit(f"ElevenLabs {method} {path} failed: HTTP {exc.code} {exc.read()[:300].decode(errors='replace')}")
    return payload if raw else json.loads(payload)


def _save(out, audio):
    os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
    with open(out, "wb") as f:
        f.write(audio)
    print(f"saved {out} ({len(audio)} bytes)")


def check():
    sub = _request("GET", "/user/subscription")
    print(f"tier={sub.get('tier')} characters used={sub.get('character_count')} of {sub.get('character_limit')}")


def voices():
    for v in _request("GET", "/voices").get("voices", []):
        labels = ", ".join(f"{k}:{val}" for k, val in (v.get("labels") or {}).items())
        print(f"{v['voice_id']}  {v['name']}  [{labels}]")


def tts(voice, text, out, model, stability=None):
    body = {"text": text, "model_id": model}
    if stability is not None:
        body["voice_settings"] = {"stability": stability, "similarity_boost": 0.75}
    _save(out, _request("POST", f"/text-to-speech/{voice}?output_format=mp3_44100_128", body, raw=True))


def sfx(text, seconds, out, loop=False, influence=0.5):
    body = {"text": text, "prompt_influence": influence}
    if seconds:
        body["duration_seconds"] = seconds
    if loop:
        body["loop"] = True
        body["model_id"] = "eleven_text_to_sound_v2"
    _save(out, _request("POST", "/sound-generation?output_format=mp3_44100_128", body, raw=True))


def music(prompt, seconds, out):
    body = {"prompt": prompt, "music_length_ms": int(seconds * 1000), "force_instrumental": True}
    _save(out, _request("POST", "/music?output_format=mp3_44100_128", body, raw=True))


def main():
    parser = argparse.ArgumentParser()
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("check")
    sub.add_parser("voices")
    p = sub.add_parser("tts")
    p.add_argument("--voice", required=True)
    p.add_argument("--text", required=True)
    p.add_argument("--out", required=True)
    p.add_argument("--model", default="eleven_multilingual_v2")
    p = sub.add_parser("sfx")
    p.add_argument("--text", required=True)
    p.add_argument("--seconds", type=float)
    p.add_argument("--out", required=True)
    args = parser.parse_args()
    if args.command == "check":
        check()
    elif args.command == "voices":
        voices()
    elif args.command == "tts":
        tts(args.voice, args.text, args.out, args.model)
    else:
        sfx(args.text, args.seconds, args.out)


if __name__ == "__main__":
    main()
