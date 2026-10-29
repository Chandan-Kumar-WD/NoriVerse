"""Key, crop and size Video 003 artwork; preserve food proportions and merge import layout."""
from pathlib import Path
import json,math
import numpy as np
import cv2
from PIL import Image, ImageFilter, ImageOps
import process_kitchen_art as art
from process_served_dishes import key_exterior

RAW=Path(__file__).parent/'meshy_raw/pizza_flow'
WIDTHS={'rolling_board':12.7,'dough_small':5.0,'dough_medium':7.5,'dough_rolled':11.5,'rolling_pin':8.0,
        'pizza_base':8.45,'prep_board':12.8,'sauce_disc':7.05,'cheese_shred':0.35,
        'tomato_slice':1.50,'cheese_piece':1.45,'salami_slice':1.55,'basil_sprig':1.40,'sorting_tray':12.8}

def clean_edges(im):
 arr=np.array(im)
 mask=(arr[...,3]>128).astype('uint8')
 core=cv2.erode(mask,np.ones((7,7),dtype='uint8'))
 # Replace reflected pink RGB at the cut boundary with the nearest reliable
 # interior colour, including for pink salami, without changing its interior.
 _,labels=cv2.distanceTransformWithLabels(1-core,cv2.DIST_L2,5,labelType=cv2.DIST_LABEL_PIXEL)
 lut=np.zeros((int(labels.max())+1,3),dtype='uint8')
 lut[labels[core>0]]=arr[core>0,:3]
 edge=core==0
 arr[edge,:3]=lut[labels[edge]]
 inset=cv2.erode(mask,np.ones((3,3),dtype='uint8'))
 arr[...,3]=np.array(Image.fromarray(inset*255).filter(ImageFilter.GaussianBlur(0.65)))
 return art.clean(Image.fromarray(arr))

def main():
 path=Path(art.ART)/'Kitchen/layout.json'
 art.layout.update({s['path']:{k:v for k,v in s.items() if k!='path'} for s in json.loads(path.read_text())['sprites']})
 for name,width in WIDTHS.items():
  im=key_exterior(Image.open(RAW/(name+'.png')))
  if name=='sorting_tray':
   arr=np.array(im);rgb=arr[...,:3].astype(float)
   pink=(np.minimum(rgb[...,0],rgb[...,2])-rgb[...,1]>35)&(rgb[...,0]>130)&(rgb[...,2]>100)
   arr[pink,3]=0;im=art.clean(Image.fromarray(arr))
  im=art.trim(clean_edges(im))
  if name=='sauce_disc':
   side=max(im.size);can=Image.new('RGBA',(side,side));can.alpha_composite(im,((side-im.width)//2,(side-im.height)//2));im=can
  art.register('PizzaFlow/'+name+'.png',im,width_units=width)
  if name in ('tomato_slice','cheese_piece','salami_slice','basil_sprig'):
   grey=ImageOps.grayscale(im).convert('RGBA');grey.putalpha(im.getchannel('A'))
   art.register('PizzaFlow/'+name+'_silhouette.png',grey,width_units=width)
  print(name,im.size,'units',width,round(im.height/im.width*width,2))
 wood=Image.open(RAW/'wood_background.png').convert('RGBA')
 art.register('PizzaFlow/wood_background.png',wood,width_units=24)
 bg=Image.open(RAW/'sorting_background.png').convert('RGBA')
 rows=np.median(np.array(bg)[...,:3],axis=1)
 candidates=np.where((rows[:,0]>rows[:,2]+35)&(np.arange(bg.height)>bg.height*.35))[0]
 split=int(candidates[0]) if len(candidates) else int(bg.height*.63)
 upper=bg.crop((0,0,bg.width,split));lower=bg.crop((0,min(bg.height-2,split+24),bg.width,bg.height-16))
 art.register('PizzaFlow/sort_upper.png',upper,width_units=24,pivot=(0.5,0))
 art.register('PizzaFlow/sort_lower.png',lower,width_units=24,pivot=(0.5,1))
 yellow=Image.new('RGBA',(bg.width,int(bg.width/2.2)))
 for y in range(0,yellow.height,lower.height):yellow.alpha_composite(lower,(0,y))
 art.register('PizzaFlow/yellow_background.png',yellow,width_units=24)
 data={'sprites':[{'path':k,**v} for k,v in art.layout.items()]}
 path.write_text(json.dumps(data,indent=1))
 (Path(art.ART)/'PizzaFlow/layout.json').write_text(json.dumps({'sprites':[s for s in data['sprites'] if s['path'].startswith('PizzaFlow/')]},indent=1))
 print('background split',split,bg.size)

if __name__=='__main__':main()
