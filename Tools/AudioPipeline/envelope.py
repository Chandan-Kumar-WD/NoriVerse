import sys,numpy as np
from scipy.io import wavfile
from scipy.signal import stft
for name in sys.argv[1:]:
    sr,x=wavfile.read(f"audio/{name}.wav");x=x.astype(np.float32)/32768
    hop=int(sr*0.05);n=len(x)//hop
    rms=np.array([np.sqrt(np.mean(x[i*hop:(i+1)*hop]**2)+1e-12) for i in range(n)])
    db=20*np.log10(rms)
    f,t,Z=stft(x,sr,nperseg=2048,noverlap=2048-hop);M=np.abs(Z)
    flux=np.r_[0,np.maximum(0,np.diff(M,axis=1)).sum(0)];flux=flux/(flux.max()+1e-9)
    cent=(f[:,None]*M).sum(0)/(M.sum(0)+1e-9)
    thr=np.median(flux)+3*np.std(flux)
    on=[i for i in range(1,len(flux)-1) if flux[i]>thr and flux[i]>=flux[i-1] and flux[i]>=flux[i+1]]
    print(f"== {name}: {len(x)/sr:.1f}s  median {np.median(db):.1f} dB  p10 {np.percentile(db,10):.1f}  p90 {np.percentile(db,90):.1f}  silent(<-50dB) {np.mean(db<-50)*100:.0f}%")
    # 1-second loudness strip
    sec=[np.max(db[i:i+20]) for i in range(0,n,20)]
    print("  per-sec peak dB:", " ".join(f"{v:.0f}" for v in sec))
    print("  onsets(s):", " ".join(f"{t[i]:.2f}" for i in on[:400]))
    np.savez(f"audio/{name}_feat.npz",db=db,flux=flux,cent=cent,t=t)
