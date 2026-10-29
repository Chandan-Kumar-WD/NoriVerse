using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using NoriVerse.Audio;

namespace NoriVerse.Core
{
    /// <summary>
    /// Plays the splash intro and announces when it has finished. The timeline itself lives in the
    /// SplashIntro animation clip; the clip calls <see cref="OnIntroFinished"/> through an animation event.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class SplashController : MonoBehaviour
    {
        [Tooltip("Scene to load once the splash has faded out. Leave empty until the next screen exists.")]
        [SerializeField] private string nextSceneName = "";

        [SerializeField] private UnityEvent onSplashComplete = new UnityEvent();

        [Header("Audio cues (seconds from start)")]
        [Tooltip("Nori calls the name while the logo letters pop in.")]
        [SerializeField] private float brandCallAt = 0.8f;
        [Tooltip("Nori eats from the chopsticks.")]
        [SerializeField] private float eatAt = 6.35f;

        public UnityEvent OnSplashComplete => onSplashComplete;
        public bool IsFinished { get; private set; }

        private void Start()
        {
            AudioDirector.Music(SoundId.MusSplash, false);
            StartCoroutine(VoiceCues());
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        private IEnumerator VoiceCues()
        {
            yield return new WaitForSeconds(brandCallAt);
            AudioDirector.Voice(SoundId.VoBrand);
            yield return new WaitForSeconds(Mathf.Max(0f, eatAt - brandCallAt));
            AudioDirector.Voice(SoundId.VoNom);
        }

        /// <summary>Called by the animation event at the end of the fade-to-white.</summary>
        public void OnIntroFinished()
        {
            if (IsFinished) return;
            IsFinished = true;
            onSplashComplete.Invoke();
            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
        }
    }
}
