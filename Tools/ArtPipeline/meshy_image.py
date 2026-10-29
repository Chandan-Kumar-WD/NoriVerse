"""Minimal Meshy image-to-image client for NoriVerse art generation.

Reads the API key from the MESHY_API_KEY environment variable (never printed).

Usage:
    python3 Tools/ArtPipeline/meshy_image.py --out OUT.png --prompt "..." \
        --ref ref1.png [--ref ref2.png ...] [--model nano-banana-pro] \
        [--aspect 1:1] [--keep-background]
    python3 Tools/ArtPipeline/meshy_image.py --out OUT.png --prompt "..." --from-task TASK_ID

Each run costs Meshy credits (nano-banana-pro: 9 per image).
"""
import argparse
import base64
import json
import mimetypes
import os
import sys
import time
import urllib.request
import urllib.error

API_BASE = "https://api.meshy.ai/openapi/v1"


def _request(method, url, body=None):
    key = os.environ.get("MESHY_API_KEY")
    if not key:
        sys.exit("MESHY_API_KEY is not set")
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(url, data=data, method=method, headers={
        "Authorization": f"Bearer {key}", "Content-Type": "application/json"})
    for attempt in range(5 if method == "GET" else 1):
        try:
            with urllib.request.urlopen(req, timeout=120) as resp:
                return json.loads(resp.read())
        except (urllib.error.URLError, OSError) as exc:
            if isinstance(exc, urllib.error.HTTPError) and 400 <= exc.code < 500 and exc.code != 429:
                raise
            if method != "GET" or attempt == 4:
                raise
            time.sleep(min(2 ** attempt, 15))


def data_uri(path):
    mime = mimetypes.guess_type(path)[0] or "image/png"
    with open(path, "rb") as f:
        return f"data:{mime};base64,{base64.b64encode(f.read()).decode()}"


def generate(out, prompt, refs=(), from_task=None, model="nano-banana-pro", aspect=None,
             keep_background=False, text_only=False, max_wait=900):
    """Create a text-to-image or image-to-image task, wait for it, save the images.

    Returns the Meshy task id.
    """
    endpoint = f"{API_BASE}/{'text-to-image' if text_only else 'image-to-image'}"
    body = {"ai_model": model, "prompt": prompt}
    if not text_only:
        if from_task:
            body["input_task_id"] = from_task
        else:
            body["reference_image_urls"] = [data_uri(p) for p in refs]
    if aspect:
        body["aspect_ratio"] = aspect
    if not keep_background:
        body["remove_background"] = True

    pending = out + ".pending-task"
    if os.path.exists(pending):
        with open(pending) as f:
            task_id = f.read().strip()
    else:
        task_id = _request("POST", endpoint, body)["result"]
        os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
        with open(pending, "w") as f:
            f.write(task_id)
    print("task", task_id, flush=True)
    delay = 5
    deadline = time.time() + max_wait
    while True:
        if time.time() > deadline:
            raise TimeoutError(f"task {task_id} still {status if 'status' in dir() else 'PENDING'} after {max_wait}s; pending marker kept")
        time.sleep(delay)
        task = _request("GET", f"{endpoint}/{task_id}")
        status = task.get("status")
        if status == "SUCCEEDED":
            break
        if status in ("FAILED", "CANCELED"):
            raise RuntimeError(f"task {task_id} {status}: {task.get('task_error')}")
        delay = min(delay + 3, 20)
    os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
    root, ext = os.path.splitext(out)
    for i, url in enumerate(task.get("image_urls") or []):
        path = out if i == 0 else f"{root}_{i}{ext}"
        urllib.request.urlretrieve(url, path)
        print("saved", path, flush=True)
    if os.path.exists(pending):
        os.remove(pending)
    return task_id


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", required=True)
    ap.add_argument("--prompt", required=True)
    ap.add_argument("--ref", action="append", default=[])
    ap.add_argument("--from-task")
    ap.add_argument("--model", default="nano-banana-pro")
    ap.add_argument("--aspect")
    ap.add_argument("--keep-background", action="store_true")
    ap.add_argument("--text-only", action="store_true", help="text-to-image, no references")
    args = ap.parse_args()
    generate(args.out, args.prompt, args.ref, args.from_task, args.model, args.aspect,
             args.keep_background, args.text_only)


if __name__ == "__main__":
    main()
