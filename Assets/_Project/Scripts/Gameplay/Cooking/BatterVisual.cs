using System.Collections.Generic;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.Cooking
{
    /// <summary>
    /// Shows the batter forming as the child stirs: the loose ingredients dissolve and four batter stages cross-fade
    /// (milky, golden swirl, pale swirl, smooth), turning with the whisk. Progress is 0..1.
    /// </summary>
    public class BatterVisual : MonoBehaviour
    {
        [SerializeField] private BowlArea bowl;
        [SerializeField] private Transform stageRoot;
        [SerializeField] private SpriteRenderer[] stages;

        private readonly List<Ingredient> contents = new List<Ingredient>();
        private static readonly Vector2[] Ranges =
        {
            new Vector2(0.03f, 0.22f), new Vector2(0.18f, 0.42f), new Vector2(0.38f, 0.68f), new Vector2(0.62f, 0.98f)
        };

        public void Configure(BowlArea bowlArea, Transform root, SpriteRenderer[] stageSprites)
        {
            bowl = bowlArea; stageRoot = root; stages = stageSprites;
        }

        private void Awake()
        {
            foreach (var s in stages) Tween.SetAlpha(s, 0f);
        }

        public void RegisterIngredient(Ingredient ingredient) => contents.Add(ingredient);

        public void Apply(float progress, float swirlDegrees)
        {
            for (int i = 0; i < stages.Length; i++)
                Tween.SetAlpha(stages[i], Easing.SmoothStep(Ranges[i].x, Ranges[i].y, progress));

            // spread a little as it thins, and turn with the whisk
            stageRoot.Rotate(0f, 0f, swirlDegrees * 0.6f);
            float scale = Mathf.Lerp(0.94f, 1f, Easing.SmoothStep(0f, 0.6f, progress));
            stageRoot.localScale = new Vector3(scale, scale, 1f);

            float dissolve = Easing.SmoothStep(0.02f, 0.28f, progress);
            foreach (var ing in contents) ing.SetDissolve(dissolve, bowl.Center, swirlDegrees * 0.8f);
        }
    }
}
