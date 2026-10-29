"""Decode every raw clip, report duration/peak/RMS/leading+trailing silence; transcribe voice clips with local Whisper."""
import glob,os,subprocess,sys,json
import numpy as np, imageio_ffmpeg
FF=imageio_ffmpeg.get_ffmpeg_exe()
def load(p,sr=22050):
    raw=subprocess.run([FF,"-v","error","-i",p,"-ac","1","-ar",str(sr),"-f","f32le","-"],capture_output=True,check=True).stdout
    return np.frombuffer(raw,np.float32)
root=sys.argv[1];rows=[]
for p in sorted(glob.glob(f"{root}/*/*.mp3")):
    x=load(p);sr=22050;a=np.abs(x);thr=10**(-45/20)
    nz=np.where(a>thr)[0];lead=nz[0]/sr if len(nz) else -1;trail=(len(x)-nz[-1])/sr if len(nz) else -1
    rms=20*np.log10(np.sqrt(np.mean(x**2))+1e-9);peak=20*np.log10(a.max()+1e-9)
    rows.append((os.path.relpath(p,root),len(x)/sr,peak,rms,lead,trail))
    print(f"{rows[-1][0]:34s} {len(x)/sr:6.2f}s peak {peak:6.1f} rms {rms:6.1f} lead {lead:5.2f} trail {trail:5.2f}")
if "--whisper" in sys.argv:
    from faster_whisper import WhisperModel
    from scipy.signal import resample_poly
    m=WhisperModel("small",device="cpu",compute_type="int8")
    for p in sorted(glob.glob(f"{root}/voice/*.mp3")):
        a=load(p,16000);segs,info=m.transcribe(a,language="en")
        print(f"{os.path.basename(p):24s} -> {' '.join(s.text.strip() for s in segs)!r}")
