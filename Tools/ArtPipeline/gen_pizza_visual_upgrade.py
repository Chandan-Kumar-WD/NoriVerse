"""Approved 2026-10-28: 14 nano-banana-pro images, maximum 126 credits; no extra variants."""
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from meshy_image import generate
ROOT=Path(__file__).parent
OUT=ROOT/'meshy_raw/pizza_upgrade'
STYLE=('Premium preschool cooking game production sprite, richly polished soft 2.5D rendered cartoon illustration, rounded sculpted shapes, beautifully controlled bevels, substantial volume, warm key light from upper left and gentle ambient fill. Consistent with the reference NoriVerse art, not flat vector, not photorealistic. ')
CUT=('Isolated on uniform pure magenta #FF00FF, entire object visible with 8 percent empty margin on every side. No text, letters, watermark, hands, face or extra objects. Orthographic directly overhead camera; no perspective tilt. ')
NORI=('Use the reference as an exact identity and composition lock. The SAME fluffy Nori chef raccoon, identical face shape, fur mask, eye colour, white chef hat with yellow goggles, red scarf, white jacket, teal apron. Same front-facing bust with complete two resting dark paws at the lower edge, same body position, same head size, hat top and nose position, same camera and lighting, generous margins. No counter, food, extra arms or objects. Solid white background. Change ONLY facial expression, retaining soft fur detail, sculpted cheeks and natural volume: ')
JOBS={
 'prep_board_premium':dict(aspect='4:3',ref=ROOT/'meshy_raw/pizza_flow/prep_board.png',prompt=STYLE+CUT+'A single elegant warm ivory glazed ceramic preparation board. Same broad rectangular rounded silhouette as the reference, but beautifully manufactured with a thick raised softly bevelled edge, subtle warm cream gold rim and soft reflected light. Large EMPTY smooth pale cream central surface occupying at least 88 percent of the board, no divisions, no decorations, no handles. It must read as a solid substantial ceramic preparation board with real depth, not a flat sheet. Centre entirely empty for a large pizza overlay.'),
 'sauce_holder':dict(aspect='1:1',prompt=STYLE+CUT+'One small round thick-rimmed ceramic ramekin filled with appetising rich red tomato pizza sauce, gentle shallow spoon swirl, fine tomato texture and moist highlights. Pearl-ivory glazed outer rim, turquoise inner rim and softly shaded exterior lip. Consistent directly overhead circular outline, visible small rim thickness and natural volume. No spoon, no saucer, no food outside bowl. Food fills the vessel naturally.'),
 'cheese_holder':dict(aspect='1:1',prompt=STYLE+CUT+'One small round thick-rimmed ceramic ramekin, pearl-ivory glazed outer rim, turquoise inner rim, softly shaded exterior lip. Filled with a generous natural loose pile of short pale golden mozzarella cheese shreds, overlapping thin soft strips with visible depth and soft contact shadows. Not French fries, not spaghetti, not Swiss cheese cubes, no holes. Match a premium tomato sauce ramekin from the same dishware set. No spoon or saucer.'),
 'salami_clean':dict(aspect='1:1',ref=ROOT/'meshy_raw/pizza_flow/salami_slice.png',prompt=STYLE+CUT+'One single perfectly intact round slice of pink cooked salami/ham for a child cooking game. Smooth continuous rounded perimeter, subtle shallow visible thickness and attractive soft pink meat surface with ONLY a few tiny softly integrated cream marbling flecks. NO punched holes, no missing parts, no white circles cut through it, no loose specks, no debris outside it, no mold, no black dots, no torn rim. Clean appetising food, soft gloss, sculpted rim, natural surface shading. Preserve the reference size, camera and pink colour family but improve depth and surface quality.'),
 'oven_premium':dict(aspect='4:3',ref=ROOT/'meshy_raw/pizza_finish/oven_body.png',prompt=STYLE+CUT+'A luxurious rounded rectangular cocoa-brown and deep cherry enamel countertop pizza oven seen DIRECTLY overhead, square-on like reference. Entire frame and right control area must remain visible. LEFT 78 percent is ONE VERY LARGE empty deep dark-brown cavity, a broad rounded rectangle occupying almost the full height inside a NARROW glossy enamel frame and a thin polished silver bevel. Cavity is empty, clear and unobstructed to accept a large round pizza and a separate rack. RIGHT 22 percent is a dimensional dark chocolate control recess, completely EMPTY smooth panel. Strong but tasteful sculpted enamel highlights, subtle layered thickness and soft inner shadows make it look tactile and 2.5D. NO rack, bars, rails across cavity, glass reflections across cavity, food, pizza, buttons, lights, knobs, symbols, numbers or text. No thick frame swallowing the cavity.'),
 'oven_rack_premium':dict(aspect='1:1',prompt=STYLE+CUT+'One SINGLE polished pale silver oven wire rack, directly overhead. Rounded rectangular outer wire frame, six evenly spaced thin parallel vertical bars and TWO horizontal support bars. Actual rounded wire thickness, chrome highlights and soft lower-edge shading create sculpted depth. Empty spaces between bars are pure magenta background. No duplicate rack, no oven body, food, pan or handles. Entire wire outline visible.'),
 'reward_star_empty':dict(aspect='1:1',prompt=STYLE+CUT+'One large five-point reward star for a premium preschool game UI. Rounded sculpted tips, a substantial gleaming warm gold beveled outer rim, recessed softly shaded muted amber/cream centre indicating an UNLIT not-yet-earned star. Rich 2.5D volume, soft upper-left creamy highlight and darker lower-right golden edge. Directly front-on rather than overhead object perspective. Symmetric upright five point star, top point at twelve o clock, centred and fully visible. No face, symbols, glows, sparkles or other objects outside the star.'),
 'reward_star_gold':dict(aspect='1:1',prompt=STYLE+CUT+'One large earned five-point golden reward star for a premium preschool game UI. Rounded sculpted tips, substantial gleaming warm gold beveled outer rim, softly inflated brilliant GOLD centre with creamy upper-left reflection and softly shaded orange-gold lower right edge. Rich soft 2.5D volume, tactile polished toy material. Directly front-on rather than overhead object perspective. Symmetric upright five point star, top point at twelve o clock, centred and fully visible. No face, symbols, glows, sparkles or other objects outside the star.'),
}
expressions={
 'nori_anticipate':'eager happy anticipation, bright eyes subtly focused toward food in front, lips parted in a small excited smile, relaxed eyebrows.',
 'nori_bite_open':'a happy naturally wide OPEN mouth ready for a bite, visible small pink tongue, bright delighted eyes, cheek fur full and soft. Keep the mouth exactly centred at the same location.',
 'nori_bite_close':'eyes gently squinting with pleasure, mouth closing into a small pleased curve as a bite is taken, gently full cheeks, soft relaxed eyebrows.',
 'nori_chew_left':'closed small mouth subtly offset to the LEFT, left cheek just a little fuller with a delicious bite, friendly open eyes. Subtle natural chewing expression, do not distort the face.',
 'nori_chew_right':'closed small mouth subtly offset to the RIGHT, right cheek just a little fuller with a delicious bite, eyes softly half closed in contentment. Subtle natural chewing expression, do not distort the face.',
 'nori_savour':'happy softly closed crescent eyes, warm satisfied smile after swallowing, subtle rosy cheeks and relaxed eyebrows, a lovely pleased expression.'}
for name,expression in expressions.items():
 JOBS[name]=dict(aspect='4:3',character=True,ref=ROOT/'meshy_raw/peek_smile.png',prompt=STYLE+NORI+expression)

def run(item):
 name,job=item;OUT.mkdir(parents=True,exist_ok=True);path=OUT/(name+'.png')
 if path.exists():return name+' already exists'
 kwargs=dict(out=str(path),prompt=job['prompt'],aspect=job['aspect'],keep_background=not job.get('character',False))
 if job.get('ref'):kwargs['refs']=[str(job['ref'])]
 else:kwargs['text_only']=True
 # Generated first: using the same star contour gives the award transition a reliable silhouette.
 if name=='reward_star_gold' and (OUT/'reward_star_empty.png').exists():kwargs['refs']=[str(OUT/'reward_star_empty.png')];kwargs.pop('text_only',None);kwargs['prompt']+=' Preserve the exact silhouette, scale and centre of the provided unlit star; change only the material into fully earned luminous gold.'
 task=generate(**kwargs);(OUT/(name+'.task')).write_text(task);return name+' '+task
if __name__=='__main__':
 import sys
 names=sys.argv[1:] or list(JOBS)
 independent=[n for n in names if n!='reward_star_gold']
 with ThreadPoolExecutor(max_workers=3) as pool:
  for result in pool.map(run,[(n,JOBS[n]) for n in independent]):print(result,flush=True)
 if 'reward_star_gold' in names:print(run(('reward_star_gold',JOBS['reward_star_gold'])),flush=True)
