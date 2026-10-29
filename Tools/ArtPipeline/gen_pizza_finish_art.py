"""Video 004 art plan: 9 images, 81 documented credits. Requires explicit batch approval."""
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from meshy_image import generate
ROOT=Path(__file__).parent
OUT=ROOT/'meshy_raw/pizza_finish'
STYLE='Premium preschool cooking game, soft rendered cartoon illustration, rounded clean shapes, smooth dimensional shading, cheerful colours consistent with NoriVerse. '
ISOLATED='Orthographic directly overhead view, centred, entire object visible, generous empty margins, no text, no shadow, isolated on a solid pure magenta background (#FF00FF). '
SAME=('The exact same Nori chef raccoon image as the reference. Identical face identity, fluffy fur, head size and position, white chef hat with yellow goggles, red neckerchief, white jacket and teal apron. Identical front-facing bust pose with both dark brown paws resting at the bottom edge, identical framing and scale; do not move or crop the character. No counter or other objects. Preserve the chef hat and complete paws. Change ONLY the facial expression: ')
JOBS={
 'oven_background':dict(aspect='16:9',prompt=STYLE+'A bright turquoise kitchen worktop seen directly from above, subtle tiny speckles. At far RIGHT only, a faint turquoise spatula resting on a folded turquoise napkin; at far lower LEFT only, the partial corner of a turquoise potholder. Centre entirely empty for an oven. Fill every image edge, no oven, no food, no text.',background=True),
 'oven_body':dict(aspect='4:3',prompt=STYLE+ISOLATED+'One rectangular dark reddish cocoa-brown countertop pizza oven seen from directly above. Width about 1.3 times height, very rounded corners and a thick glossy dark-red rim. LEFT 80 percent is one empty dark brown rectangular glass-window cavity with rounded corners, smoothly shaded and completely empty. RIGHT 20 percent is a slightly darker plain smooth control panel, empty. No wire rack, NO bars, no food, no pizza, no buttons, no symbols, no labels, no knobs. The cavity must be large and unobstructed.'),
 'oven_rack':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One single rectangular pale silver oven wire rack seen directly from above, rounded corners, outer rounded rectangular wire frame with seven thin straight parallel vertical silver bars and two thin horizontal support bars. Empty, no food, no pizza, no oven body, no handles.'),
 'baked_base':dict(aspect='1:1',ref=ROOT/'meshy_raw/pizza_flow/pizza_base.png',prompt=STYLE+'Keep the exact circular pizza dough base from the reference, identical top-down view, identical size, centre, raised rim, contour and empty cream centre. Change ONLY the crust from pale raw dough to appetising freshly baked golden-brown crust, with a few subtle toasted patches on the raised outer edge. No sauce, no cheese, no toppings, no plate, no board. Entire circle visible with margins, solid pure magenta background.'),
 'nori_eat_open':dict(aspect='4:3',ref=ROOT/'meshy_raw/peek_smile.png',character=True,prompt=SAME+'a happy open mouth ready to take a bite of pizza, bright eyes looking forward, mouth open wide naturally, small tongue visible. Plain white background.'),
 'nori_eat_bite':dict(aspect='4:3',ref=ROOT/'meshy_raw/peek_smile.png',character=True,prompt=SAME+'taking a delicious bite: eyes gently squinting closed, mouth closing in a pleased little smile, cheeks slightly full, relaxed happy eyebrows. Plain white background.'),
 'nori_eat_chew':dict(aspect='4:3',ref=ROOT/'meshy_raw/peek_smile.png',character=True,prompt=SAME+'chewing with softly puffed cheeks, mouth closed, bright open eyes and a pleased gentle expression. Plain white background.'),
 'nori_eat_satisfied':dict(aspect='4:3',ref=ROOT/'meshy_raw/peek_smile.png',character=True,prompt=SAME+'very satisfied after a delicious meal: happy crescent eyes softly closed, a warm open-mouth smile and very subtle rosy cheeks, cheerful relaxed eyebrows. Plain white background.'),
 'sun_cloud':dict(aspect='4:3',prompt=STYLE+ISOLATED+'One small cheerful golden sun peeking above one fluffy white cloud, warm soft glowing rays, as a child-friendly pleasant day reaction icon. No face, no eyes, no letters, no sky background, no additional objects.')
}

def run(item):
 name,job=item;OUT.mkdir(parents=True,exist_ok=True);path=OUT/(name+'.png')
 if path.exists():return name+' already exists'
 kwargs=dict(out=str(path),prompt=job['prompt'],aspect=job['aspect'],keep_background=not job.get('character',False))
 if 'ref' in job:kwargs['refs']=[str(job['ref'])]
 else:kwargs['text_only']=True
 task=generate(**kwargs);(OUT/(name+'.task')).write_text(task);return name+' '+task

if __name__=='__main__':
 import sys
 names=sys.argv[1:] or list(JOBS)
 with ThreadPoolExecutor(max_workers=3) as pool:
  for result in pool.map(run,[(n,JOBS[n]) for n in names]):print(result,flush=True)
