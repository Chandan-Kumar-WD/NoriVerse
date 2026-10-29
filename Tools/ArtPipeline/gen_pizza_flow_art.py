"""Video 003 art plan: 16 images, 144 documented Meshy credits; run only after batch approval."""
import json
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from meshy_image import generate

ROOT=Path(__file__).parent
OUT=ROOT/'meshy_raw'/'pizza_flow'
STYLE=('Premium preschool cooking game, soft rendered cartoon illustration, rounded clean forms, smooth dimensional shading, '
       'warm cheerful colours, consistent with the NoriVerse kitchen art. ')
ISOLATED=('Orthographic directly overhead top-down view, centred, entire object visible, generous empty margins, '
          'no perspective tilt, no text, no extra objects, no cast shadow, solid flat pure magenta background (#FF00FF). ')
JOBS={
 'wood_background':dict(aspect='16:9',background=True,prompt=STYLE+'A pale honey wooden kitchen tabletop seen directly from above. Subtle horizontal wood grain. At the far lower LEFT corner only, a folded pale yellow checkered cloth and a small silver flour sieve; at the far upper RIGHT corner only, the partial rim of a pale wooden flour bowl and a few scattered flour grains. Centre area completely empty for a large rolling board. No rolling board, no dough, no rolling pin, no text. Surface fills every edge.'),
 'rolling_board':dict(aspect='4:3',prompt=STYLE+ISOLATED+'One empty orange honey-coloured rounded rectangular kneading board, width about 1.35 times height, smooth flat amber working surface, thick rounded darker golden-orange border with a subtle inset rounded border. No handles, no food, no tools.'),
 'dough_small':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One small thick mound of soft pale buttery-cream raw pizza dough, gently flattened into an irregular oval, smooth soft surface, organic rounded edge, very subtle thickness, not bread and not baked. No crust, no flour dust.'),
 'dough_medium':dict(aspect='4:3',prompt=STYLE+ISOLATED+'One partially rolled sheet of pale buttery-cream raw pizza dough, thin but soft, broad irregular rounded shape about 1.2 times as wide as high, organic gently scalloped edges, smooth unmarked surface. No crust, not baked, no flour dust.'),
 'dough_rolled':dict(aspect='4:3',prompt=STYLE+ISOLATED+'One fully rolled thin sheet of pale buttery-cream raw pizza dough, broad almost rectangular rounded shape about 1.38 times as wide as high, very gently irregular scalloped edges, smooth unmarked surface. No crust, not baked, no flour dust.'),
 'rolling_pin':dict(aspect='16:9',prompt=STYLE+ISOLATED+'One wooden rolling pin lying horizontally, a wide straight cylindrical pale honey wood barrel with rounded ends and one short rounded handle on each end. Light wood grain, friendly smooth shape. Width about 5.5 times total height. No dough.'),
 'pizza_base':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One perfectly ROUND raw pizza dough base, pale cream centre with a softly raised thick golden-cream dough rim, directly overhead, entire circular outline visible. Empty plain centre for sauce. Not baked, no plate, no board, no toppings, no sauce, no hearts.'),
 'prep_board':dict(aspect='4:3',prompt=STYLE+ISOLATED+'One empty cream-white rounded rectangular kitchen preparation board, width about 1.35 times height, subtle warm golden border and a broad smooth flat ivory working surface. No handles, no tools, no food.'),
 'sauce_disc':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One perfectly ROUND full disc of thick rich red tomato pizza sauce, gentle broad glossy spiral swirls and tiny tomato texture, bright appetising red, consistent lighting. Just sauce, flat directly overhead, no plate, no crust, no dough, no toppings, no splashes outside the circular edge.'),
 'cheese_shred':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One single short gently curved strip of pale golden shredded mozzarella cheese, rounded edges and visible soft thickness, a small curved cheese shred for sprinkling on pizza. No other pieces, no plate.'),
 'tomato_slice':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One round red ripe tomato slice, glossy red flesh, clear pale seeds arranged around its centre and a thin darker red skin. Entire circular slice, directly overhead, no leaf, no plate.'),
 'cheese_piece':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One chunky golden yellow Swiss cheese slice, roughly square with slightly irregular rounded edges and several rounded holes, smooth appetising surface, directly overhead, no plate.'),
 'salami_slice':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One ROUND pink-red salami slice with a thin deep rose edge and little pale pink fat speckles, child-friendly appetising cartoon food, directly overhead, no plate, no herbs.'),
 'basil_sprig':dict(aspect='1:1',prompt=STYLE+ISOLATED+'One small fresh green basil sprig of three plump bright rounded leaves with delicate visible veins on a tiny short stem. Directly overhead, no pot, no plate.'),
 'sorting_background':dict(aspect='16:9',background=True,prompt=STYLE+'Flat game table background directly overhead. Upper 63 percent is a cheerful turquoise surface (#24CDC9) with very subtle wide vertical stripes. Lower 37 percent is bright warm yellow (#FFE16B) with faint little round dots. A perfectly straight horizontal boundary separates the two surfaces. Fill every edge. NO objects, no tray, no plates, no food, no writing, no gradients to white at the edges.'),
 'sorting_tray':dict(aspect='16:9',prompt=STYLE+ISOLATED+'One EMPTY pale ice-blue ceramic rectangular serving tray, wide horizontal orientation, width about 2.6 times height. Very rounded corners, white glossy empty interior, thin soft cyan raised rim and one short rounded loop handle on each of its left and right sides. Directly overhead, no food, no ridges, no objects.')
}

def run(item):
 name,job=item
 OUT.mkdir(parents=True,exist_ok=True)
 path=OUT/(name+'.png')
 if path.exists():return name+' already exists'
 task=generate(str(path),job['prompt'],text_only=True,aspect=job['aspect'],keep_background=True)
 (OUT/(name+'.task')).write_text(task)
 return name+' '+task

if __name__=='__main__':
 import sys
 names=sys.argv[1:] or list(JOBS)
 with ThreadPoolExecutor(max_workers=3) as pool:
  for message in pool.map(run,[(n,JOBS[n]) for n in names]):print(message,flush=True)
