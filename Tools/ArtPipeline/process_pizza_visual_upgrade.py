"""Prepare approved premium props, intact salami and six aligned Nori eating expressions."""
from pathlib import Path
import json
import cv2
import numpy as np
from PIL import Image,ImageDraw,ImageFilter,ImageChops
import process_kitchen_art as art
from process_served_dishes import key_exterior
from process_pizza_flow_art import clean_edges
from process_pizza_finish_art import key_all_pink,hat_metrics,expand_oven_window

RAW=Path(__file__).parent/'meshy_raw/pizza_upgrade'
WIDTHS={'prep_board_premium':12.8,'sauce_holder':1.9,'cheese_holder':1.9,'salami_clean':1.55,
        'oven_premium':12.2,'oven_rack_premium':7.9,'reward_star_empty':1.95,'reward_star_gold':1.95}
POSES=['nori_anticipate','nori_bite_open','nori_bite_close','nori_chew_left','nori_chew_right','nori_savour']

def main():
 path=Path(art.ART)/'Kitchen/layout.json'
 art.layout.update({s['path']:{k:v for k,v in s.items() if k!='path'} for s in json.loads(path.read_text())['sprites']})
 for name,width in WIDTHS.items():
  im=Image.open(RAW/(name+'.png')).convert('RGBA')
  im=key_all_pink(im) if name=='oven_rack_premium' else key_exterior(im)
  im=art.trim(clean_edges(im))
  if name=='oven_premium':im=expand_oven_window(im)
  art.register('PizzaUpgrade/'+name+'.png',im,width_units=width)
  if name=='salami_clean':
   # Keep the intact food disk, discarding isolated reflected-magenta fringe fragments.
   a=np.array(im);mask=(a[...,3]>128).astype('uint8')
   count,labels,stats,_=cv2.connectedComponentsWithStats(mask,8)
   largest=1+int(np.argmax(stats[1:,cv2.CC_STAT_AREA]));solid=(labels==largest).astype('uint8')
   flood=solid.copy();cv2.floodFill(flood,None,(0,0),1);solid[flood==0]=1
   # Fit the generated disk, then use one solid feathered ellipse to prevent
   # pinholes or disconnected anti-aliased rim fragments after chroma keying.
   contours,_=cv2.findContours(solid,cv2.RETR_EXTERNAL,cv2.CHAIN_APPROX_NONE)
   contour=max(contours,key=cv2.contourArea);(cx,cy),(ew,eh),angle=cv2.fitEllipse(contour)
   yy,xx=np.indices(solid.shape);radians=np.deg2rad(angle);dx=xx-cx;dy=yy-cy
   ex=dx*np.cos(radians)+dy*np.sin(radians);ey=-dx*np.sin(radians)+dy*np.cos(radians)
   ellipse=(ex/max(1,ew*.5-6))**2+(ey/max(1,eh*.5-6))**2<=1
   missing=ellipse&(a[...,3]<128)
   if missing.any():a[...,:3]=cv2.inpaint(a[...,:3],missing.astype('uint8')*255,5,cv2.INPAINT_TELEA)
   alpha=Image.fromarray((ellipse*255).astype('uint8')).filter(ImageFilter.GaussianBlur(.65))
   a[...,3]=np.array(alpha);im=art.trim(clean_edges(Image.fromarray(a)))
   art.register('PizzaUpgrade/salami_clean.png',im,width_units=width)
   grey=im.convert('L').convert('RGBA');grey.putalpha(im.getchannel('A'))
   art.register('PizzaUpgrade/salami_clean_silhouette.png',grey,width_units=width)
  print(name,im.size,'units',width,round(im.height/im.width*width,3))
 ref=Image.open(Path(art.RAW)/'peek_smile.png').convert('RGBA');(rx,ry),rw=hat_metrics(ref)
 for name in POSES:
  im=Image.open(RAW/(name+'.png')).convert('RGBA')
  if im.size!=ref.size:im=im.resize(ref.size,Image.Resampling.LANCZOS)
  (x,y),w=hat_metrics(im);scale=rw/w
  matrix=np.array([[scale,0,rx-scale*x],[0,scale,ry-scale*y]],dtype='float32')
  im=Image.fromarray(cv2.warpAffine(np.array(im),matrix,ref.size,flags=cv2.INTER_CUBIC,borderMode=cv2.BORDER_CONSTANT,borderValue=(0,0,0,0)))
  # Lock hat, ears, outfit and paws to the existing generated bust. Only the
  # softly blended facial region changes, so expression changes cannot ghost the body.
  face_mask=Image.new('L',ref.size);ImageDraw.Draw(face_mask).ellipse((280,285,955,666),fill=255)
  face_mask=face_mask.filter(ImageFilter.GaussianBlur(14))
  face_mask=ImageChops.multiply(face_mask,ImageChops.darker(ref.getchannel('A'),im.getchannel('A')))
  im=Image.composite(im,ref,face_mask)
  bust=art.clean(im.crop(art.PEEK_CROP));art.register('PizzaUpgrade/'+name+'.png',bust,width_units=bust.width/150,pivot=(.5,0))
  print('aligned',name,round(scale,4),round(rx-x,2),round(ry-y,2))
 data={'sprites':[{'path':k,**v} for k,v in art.layout.items()]};path.write_text(json.dumps(data,indent=1))
 (Path(art.ART)/'PizzaUpgrade/layout.json').write_text(json.dumps({'sprites':[s for s in data['sprites'] if s['path'].startswith('PizzaUpgrade/')]},indent=1))
if __name__=='__main__':main()
