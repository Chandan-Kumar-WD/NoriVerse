import sys,cv2,json
from PIL import Image,ImageDraw
src,out,times=sys.argv[1],sys.argv[2],[float(t) for t in sys.argv[3].split(",")]
rot=sys.argv[4] if len(sys.argv)>4 else ""
c=cv2.VideoCapture(src);ims=[]
for t in times:
    c.set(cv2.CAP_PROP_POS_MSEC,t*1000);ok,f=c.read()
    if not ok: continue
    if rot=="ccw": f=cv2.rotate(f,cv2.ROTATE_90_COUNTERCLOCKWISE)
    if rot=="cw": f=cv2.rotate(f,cv2.ROTATE_90_CLOCKWISE)
    im=Image.fromarray(cv2.cvtColor(f,cv2.COLOR_BGR2RGB));im.thumbnail((420,420))
    ImageDraw.Draw(im).text((4,4),f"{t:.2f}s",fill=(255,0,0));ims.append(im)
w,h=ims[0].size;cols=4;rows=(len(ims)+cols-1)//cols
sheet=Image.new("RGB",(cols*w,rows*h),"white")
for i,im in enumerate(ims):sheet.paste(im,((i%cols)*w,(i//cols)*h))
sheet.save(out);print(out,sheet.size,w,h)
