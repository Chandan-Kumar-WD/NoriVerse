"""Seven replacement plated dishes requested by the user (7 x documented 9 credits)."""
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from meshy_image import generate

ROOT = Path(__file__).parent
OUT = ROOT / 'meshy_raw' / 'kitchen' / 'served'
STYLE = ('Premium preschool cooking game illustration, soft rendered rounded forms with rich appealing volume, '
         'same warm cheerful style as the reference. ONE complete serving of food resting on ONE shallow white '
         'ceramic dinner plate with a pale sky-blue rim. The plate is lying horizontally on an invisible tabletop, '
         'seen from above at a natural three-quarter serving angle, approximately 45 degrees above the tabletop. '
         'The plate has an elliptical outline and a visible rounded front rim; the food has REAL HEIGHT, '
         'thick sides, full rounded toppings and dimensional lighting. Never a flattened top-down image, '
         'never an upright plate, no plate stand. Keep the plate level, horizontal major axis, centred and '
         'fully inside the image with generous empty margin. No table, no counter, no shelf, no letters, '
         'no symbols, no extra objects, no cast shadow. Uniform solid pure magenta background (#FF00FF). ')
RECIPES = {
    'pancakes': 'A tall fluffy stack of three thick golden pancakes, maple syrup flowing down the sides, a generous butter pat and a whole strawberry on top.',
    'cheese_toast': 'One thick slice of golden toast with rounded corners, a generous bubbly melted cheddar topping and tiny green herbs. Show the bread thickness.',
    'pear_tart': 'A full round pear tart with a deep golden fluted pastry crust and overlapping thick pear slices spiralling on a creamy filling. Show the pastry sides.',
    'tomato_pizza': 'One ROUND thick-crust pizza with pillowy golden crust, generous melted mozzarella, plump red tomato slices and fresh green basil. Round pizza, never heart-shaped.',
    'cookie_pizza': 'One thick round chocolate dessert cookie with a generous rounded chocolate topping and chunky dark sandwich cookie pieces. Show the thick cookie side.',
    'star_fruit': 'A generous full mound of juicy orange wedges, kiwi slices, whole blueberries and raspberries arranged attractively on the ROUND plate. Full juicy fruit with rounded volume.',
    'veggie_pizza': 'One ROUND thick-crust vegetable pizza with pillowy golden edges, melted cheese, plump broccoli florets, sliced mushrooms and black olives. Show crust thickness.'
}

def run(item):
    name, recipe = item
    path = OUT / (name + '.png')
    if path.exists():
        return name + ' already exists'
    ref = ROOT / 'meshy_raw' / 'kitchen' / ('food_' + ('veggie_pizza' if name == 'tomato_pizza' else name) + '.png')
    task = generate(str(path), STYLE + recipe, refs=[str(ref)], aspect='4:3', keep_background=True)
    (OUT / (name + '.task')).write_text(task)
    return name + ' ' + task

if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    with ThreadPoolExecutor(max_workers=3) as pool:
        for result in pool.map(run, RECIPES.items()):
            print(result, flush=True)
