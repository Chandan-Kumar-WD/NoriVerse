import sys,numpy as np
from scipy.io import wavfile
from scipy.signal import stft
from PIL import Image,ImageDraw
for n in sys.argv[1:]:
    sr,x=wavfile.read(f"audio/{n}.wav");x=x.astype(np.float32)/32768
    f,t,Z=stft(x,sr,nperseg=1024,noverlap=1024-441)  # 20ms cols
    M=20*np.log10(np.abs(Z)+1e-7);M=M[f<8000];M=np.clip((M+90)/70,0,1)
    img=(255*M[::-1]).astype(np.uint8)  # rows=freq
    rows=30  # seconds per row
    cps=50;W=rows*cps;H=img.shape[0]
    nrow=int(np.ceil(img.shape[1]/W))
    out=Image.new("L",(W,nrow*(H+18)),0);d=ImageDraw.Draw(out)
    for r in range(nrow):
        seg=img[:,r*W:(r+1)*W];out.paste(Image.fromarray(seg),(0,r*(H+18)+18))
        for s in range(rows+1):
            xx=s*cps;d.line([(xx,r*(H+18)+12),(xx,r*(H+18)+18)],fill=255)
            if s%2==0:d.text((xx+1,r*(H+18)),str(r*rows+s),fill=255)
    out.save(f"audio/{n}_spec.png");print(n,out.size)
