using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Common.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Proto/AudioConfig", order = 1)]
    public class AudioConfig : ScriptableObject
    {
        [ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true, ListElementLabelName = "Type")]
        public SoundEntry[] Sounds = Array.Empty<SoundEntry>();

        public AudioClip GetClip(SoundType type)
        {
            for (var i = 0; i < Sounds.Length; i++)
            {
                if (Sounds[i].Type == type)
                {
                    return Sounds[i].Clip;
                }
            }

            return null;
        }

        [Serializable]
        public struct SoundEntry
        {
            public SoundType Type;
            public AudioClip Clip;
        }
    }
}
