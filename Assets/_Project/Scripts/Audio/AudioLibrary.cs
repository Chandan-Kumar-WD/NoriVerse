using System;
using UnityEngine;

namespace NoriVerse.Audio
{
    /// <summary>Clip and volume for each <see cref="SoundId"/>. Built by NoriVerse/Build Audio Library into Resources.</summary>
    public class AudioLibrary : ScriptableObject
    {
        public const string ResourcePath = "NoriVerseAudio";

        [Serializable]
        public struct Entry
        {
            public SoundId id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public Entry[] Entries => entries;

        public void SetEntries(Entry[] value) => entries = value;
    }
}
