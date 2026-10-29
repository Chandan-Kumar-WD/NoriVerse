using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    [DefaultExecutionOrder(-1100)]
    [RequireComponent(typeof(Collider2D))]
    public class SceneBackButton : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private string destination = "Kitchen";
        private Collider2D hit;
        public void Configure(Camera cam, string scene = "Kitchen") { sceneCamera = cam; destination = scene; }
        private void Awake() => hit = GetComponent<Collider2D>();
        private void Update()
        {
            if (SceneTransition.IsBusy || ExitConfirmation.IsOpen) return;
            var p = PointerInput.Read(sceneCamera);
            if (p.phase != PointerPhase.Began) return;
            // The corner anchor can move after physics updates (and the dialog pauses physics).
            Physics2D.SyncTransforms();
            if (hit.OverlapPoint(p.world))
            {
                AudioDirector.Play(SoundId.UiTap);
                ExitConfirmation.Show(sceneCamera, destination);
            }
        }
    }
}
