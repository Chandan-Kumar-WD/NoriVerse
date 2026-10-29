import numpy as np
from scipy.io import wavfile
from scipy.signal import stft,correlate
def load(n):
    sr,x=wavfile.read(f"audio/{n}.wav");return sr,x.astype(np.float32)/32768
# does v5 contain v4? compare envelopes
d4=np.load("audio/v4_feat.npz")["db"];d5=np.load("audio/v5_feat.npz")["db"]
a=(d4-d4.mean())/d4.std();b=(d5-d5.mean())/d5.std()
c=correlate(b,a,mode="valid") if len(b)>=len(a) else correlate(a,b,mode="valid")
print("v4/v5 env xcorr peak",round(c.max()/min(len(a),len(b)),2),"at lag",np.argmax(c)*0.05,"s")
for n in ["v1","v2","v4","v5"]:
    sr,x=load(n);f,t,Z=stft(x,sr,nperseg=8192,noverlap=8192-2205);M=np.abs(Z)
    # tempo from onset envelope
    fl=np.r_[0,np.maximum(0,np.diff(M,axis=1)).sum(0)];fl-=fl.mean()
    ac=np.correlate(fl,fl,"full")[len(fl)-1:];lags=np.arange(len(ac))*0.1
    rng=(lags>0.33)&(lags<1.2);best=lags[rng][np.argmax(ac[rng])]
    # chroma
    pc=np.zeros(12);m=(f>80)&(f<2000)
    for fi,row in zip(f[m],M[m]): pc[int(round(12*np.log2(fi/261.63)))%12]+=row.sum()
    names="C C# D D# E F F# G G# A A# B".split();top=np.argsort(pc)[::-1][:5]
    tonal=np.mean(M[m].max(0)/(M[m].mean(0)+1e-9))
    print(n,"beat period",round(best,2),"s (~",round(60/best),"BPM)  top pitch classes",[names[i] for i in top]," peakiness",round(tonal,1))
