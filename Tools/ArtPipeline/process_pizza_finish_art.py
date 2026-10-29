"""Prepare Video 004 props and aligned Nori expression/portrait variants."""
from pathlib import Path
import json
import cv2
import numpy as np
from PIL import Image,ImageDraw,ImageChops
import process_kitchen_art as art
from process_served_dishes import key_exterior
from process_pizza_flow_art import clean_edges

RAW=Path(__file__).parent/'meshy_raw/pizza_finish'

def key_all_pink(im):
 a=np.array(im.convert('RGBA')).astype(float)
 excess=np.minimum(a[...,0],a[...,2])-a[...,1]
 a[...,3]*=np.clip((65-excess)/45,0,1)
 return art.clean(Image.fromarray(a.astype('uint8')))

def hat_metrics(im):
 a=np.array(im);m=(a[...,3]>128)&(a[...,0]>205)&(a[...,1]>205)&(a[...,2]>205)
 m[310:]=False;m[:,:280]=False;m[:,960:]=False
 ys,xs=np.where(m)
 return ((float(xs.mean()),float(ys.mean())),float(np.percentile(xs,98)-np.percentile(xs,2)))

def portrait(im):
 im=im.crop((145,0,855,800));im.thumbnail((446,462),Image.Resampling.LANCZOS)
 canvas=Image.new('RGBA',(512,512));canvas.alpha_composite(im,((512-im.width)//2,35))
 mask=Image.new('L',(512,512));ImageDraw.Draw(mask).ellipse((8,8,504,504),fill=255)
 canvas.putalpha(ImageChops.multiply(canvas.getchannel('A'),mask));return canvas

def expand_oven_window(im):
 # Nine-slice the empty cavity so the rack and full pizza fit the reference's
 # large window while the generated frame, corner shading and panel remain intact.
 w,h=im.size;left=int(w*0.13);right=int(w*0.68);extra_x=int(w*0.365)
 horizontal=Image.new('RGBA',(w+extra_x,h))
 horizontal.alpha_composite(im.crop((0,0,left,h)),(0,0))
 horizontal.alpha_composite(im.crop((left,0,right,h)).resize((right-left+extra_x,h),Image.Resampling.LANCZOS),(left,0))
 horizontal.alpha_composite(im.crop((right,0,w,h)),(right+extra_x,0))
 top=int(h*0.19);bottom=int(h*0.80);extra_y=int(h*0.82);nw=horizontal.width
 result=Image.new('RGBA',(nw,h+extra_y))
 result.alpha_composite(horizontal.crop((0,0,nw,top)),(0,0))
 result.alpha_composite(horizontal.crop((0,top,nw,bottom)).resize((nw,bottom-top+extra_y),Image.Resampling.LANCZOS),(0,top))
 result.alpha_composite(horizontal.crop((0,bottom,nw,h)),(0,bottom+extra_y))
 return result

def main():
 path=Path(art.ART)/'Kitchen/layout.json'
 art.layout.update({s['path']:{k:v for k,v in s.items() if k!='path'} for s in json.loads(path.read_text())['sprites']})
 for name,width in [('oven_body',12.2),('oven_rack',8.4),('baked_base',8.45),('sun_cloud',2.6)]:
  raw=Image.open(RAW/(name+'.png')).convert('RGBA')
  keyed=key_all_pink(raw) if name=='oven_rack' else key_exterior(raw)
  im=art.trim(clean_edges(keyed))
  if name=='oven_body':im=expand_oven_window(im)
  art.register('PizzaFinish/'+name+'.png',im,width_units=width)
  print(name,im.size)
 art.register('PizzaFinish/oven_background.png',Image.open(RAW/'oven_background.png').convert('RGBA'),width_units=24)
 ref=Image.open(Path(art.RAW)/'peek_smile.png').convert('RGBA');(rx,ry),rw=hat_metrics(ref)
 for suffix in ['open','bite','chew','satisfied']:
  im=Image.open(RAW/('nori_eat_'+suffix+'.png')).convert('RGBA')
  if im.size!=ref.size:im=im.resize(ref.size,Image.Resampling.LANCZOS)
  (x,y),w=hat_metrics(im);scale=rw/w
  mat=np.array([[scale,0,rx-scale*x],[0,scale,ry-scale*y]],dtype='float32')
  im=Image.fromarray(cv2.warpAffine(np.array(im),mat,ref.size,flags=cv2.INTER_CUBIC,borderMode=cv2.BORDER_CONSTANT,borderValue=(0,0,0,0)))
  bust=art.clean(im.crop(art.PEEK_CROP))
  art.register('PizzaFinish/nori_eat_'+suffix+'.png',bust,width_units=bust.width/150,pivot=(0.5,0))
  print('aligned',suffix,round(scale,4),round(rx-x,2),round(ry-y,2))
  if suffix=='satisfied':art.register('PizzaFinish/avatar_satisfied.png',portrait(bust),width_units=2.8)
 for name,source in [('smile','peek_smile'),('blink','peek_blink')]:
  im=Image.open(Path(art.RAW)/(source+'.png')).convert('RGBA').crop(art.PEEK_CROP)
  art.register('PizzaFinish/avatar_'+name+'.png',portrait(im),width_units=2.8)
 data={'sprites':[{'path':k,**v} for k,v in art.layout.items()]};path.write_text(json.dumps(data,indent=1))
 (Path(art.ART)/'PizzaFinish/layout.json').write_text(json.dumps({'sprites':[s for s in data['sprites'] if s['path'].startswith('PizzaFinish/')]},indent=1))

if __name__=='__main__':main()
