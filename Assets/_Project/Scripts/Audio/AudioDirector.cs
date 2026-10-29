using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoriVerse.Audio
{
    /// <summary>
    /// Plays music, Nori's voice and sound effects. Lives across scene loads and creates itself on first use, so gameplay
    /// code only calls the static methods. Music keeps playing between scenes; Nori says one line at a time; continuous
    /// activity sounds (rolling, stirring, painting) run while <see cref="Sustain"/> is called and fade when it stops.
    /// </summary>
    public class AudioDirector : MonoBehaviour
    {
        private const int EffectVoices = 8;
        private const float RepeatCooldown = 0.06f;
        private const float SustainHold = 0.15f;
        private const float LoopFadeSeconds = 0.18f;

        private static AudioDirector instance;

        /// <summary>Raised for every sound that starts, so tests can check what the player would hear.</summary>
        public static event Action<SoundId> Played;

        private readonly Dictionary<SoundId, AudioLibrary.Entry> library = new Dictionary<SoundId, AudioLibrary.Entry>();
        private readonly Dictionary<SoundId, float> lastStarted = new Dictionary<SoundId, float>();
        private readonly Dictionary<SoundId, AudioSource> loops = new Dictionary<SoundId, AudioSource>();
        private readonly Dictionary<SoundId, float> sustainedUntil = new Dictionary<SoundId, float>();
        private readonly List<SoundId> finishedLoops = new List<SoundId>();
        private AudioSource music;
        private AudioSource voice;
        private AudioSource[] effects;
        private int nextEffect;
        private SoundId currentMusic = SoundId.None;
        private AudioListener listener;

        private static AudioDirector Instance
        {
            get
            {
                if (instance != null) return instance;
                var go = new GameObject("AudioDirector");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<AudioDirector>();
                instance.Build();
                SceneManager.sceneLoaded += instance.OnSceneLoaded;
                return instance;
            }
        }

        public static SoundId CurrentMusic => instance != null ? instance.currentMusic : SoundId.None;

        /// <summary>Starts a music track. Calling it again with the track already playing does nothing.</summary>
        public static void Music(SoundId id, bool loop = true)
        {
            var d = Instance;
            if (d.currentMusic == id && d.music.isPlaying) return;
            if (!d.TryGet(id, out var entry)) return;
            d.currentMusic = id;
            d.music.clip = entry.clip;
            d.music.loop = loop;
            d.music.volume = entry.volume;
            d.music.Play();
            Played?.Invoke(id);
        }

        /// <summary>Nori says a line, replacing anything she was still saying.</summary>
        public static void Voice(SoundId id)
        {
            var d = Instance;
            if (!d.TryGet(id, out var entry)) return;
            d.voice.Stop();
            d.voice.clip = entry.clip;
            d.voice.volume = entry.volume;
            d.voice.Play();
            Played?.Invoke(id);
        }

        /// <summary>One-shot effect. Repeats of the same sound within a few frames are skipped so taps never stack up.</summary>
        public static void Play(SoundId id, float pitch = 1f, float cooldown = RepeatCooldown)
        {
            var d = Instance;
            if (!d.TryGet(id, out var entry)) return;
            // Game time, so repeats follow the game's own pacing; no cooldown while paused (exit dialog buttons).
            float now = Time.time;
            if (Time.timeScale > 0f && d.lastStarted.TryGetValue(id, out float last) && now - last < cooldown) return;
            d.lastStarted[id] = now;
            var source = d.effects[d.nextEffect];
            d.nextEffect = (d.nextEffect + 1) % d.effects.Length;
            source.pitch = pitch;
            source.PlayOneShot(entry.clip, entry.volume);
            Played?.Invoke(id);
        }

        /// <summary>Keeps a looping activity sound going; call every frame while the activity happens.</summary>
        public static void Sustain(SoundId id)
        {
            var d = Instance;
            if (!d.TryGet(id, out var entry)) return;
            d.sustainedUntil[id] = Time.unscaledTime + SustainHold;
            if (d.loops.ContainsKey(id)) return;
            var source = d.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.clip = entry.clip;
            source.volume = 0f;
            source.Play();
            d.loops[id] = source;
            Played?.Invoke(id);
        }

        public static bool IsSustaining(SoundId id) => instance != null && instance.loops.ContainsKey(id);

        private void Build()
        {
            var asset = Resources.Load<AudioLibrary>(AudioLibrary.ResourcePath);
            if (asset != null)
                foreach (var entry in asset.Entries)
                    if (entry.clip != null) library[entry.id] = entry;
            if (asset == null) Debug.LogWarning("AudioDirector: no AudioLibrary found in Resources; the game will be silent.");

            // The scene builders give cameras no AudioListener, so the director brings the one ear every scene needs.
            listener = gameObject.AddComponent<AudioListener>();
            UseOwnListenerUnlessSceneHasOne();

            music = CreateSource();
            voice = CreateSource();
            effects = new AudioSource[EffectVoices];
            for (int i = 0; i < effects.Length; i++) effects[i] = CreateSource();
        }

        private AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        private bool TryGet(SoundId id, out AudioLibrary.Entry entry) => library.TryGetValue(id, out entry);

        private void UseOwnListenerUnlessSceneHasOne()
        {
            bool sceneHasOne = false;
            foreach (var other in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                if (other != listener && other.enabled) sceneHasOne = true;
            listener.enabled = !sceneHasOne;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UseOwnListenerUnlessSceneHasOne();
            // Activity sounds belong to the screen that started them.
            foreach (var pair in loops) Destroy(pair.Value);
            loops.Clear();
            sustainedUntil.Clear();
        }

        private void Update()
        {
            if (loops.Count == 0) return;
            float now = Time.unscaledTime;
            float step = Time.unscaledDeltaTime / LoopFadeSeconds;
            foreach (var pair in loops)
            {
                bool active = sustainedUntil.TryGetValue(pair.Key, out float until) && now < until;
                float target = active ? library[pair.Key].volume : 0f;
                pair.Value.volume = Mathf.MoveTowards(pair.Value.volume, target, step * library[pair.Key].volume);
                if (!active && pair.Value.volume <= 0f) finishedLoops.Add(pair.Key);
            }
            foreach (var id in finishedLoops)
            {
                Destroy(loops[id]);
                loops.Remove(id);
            }
            finishedLoops.Clear();
        }
    }
}
