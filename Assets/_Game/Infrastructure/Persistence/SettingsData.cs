using System;
using UnityEngine;

namespace _Game.Infrastructure.Persistence
{
    [Serializable]
    [PersistentData("settings.v1")]
    public sealed partial class SettingsData : PersistentDataBase
    {
        [PersistentField]
        [SerializeField]
        private float _musicVolume = 0.2f;

        [PersistentField]
        [SerializeField]
        private float _sfxVolume = 1f;

        [PersistentField]
        [SerializeField]
        private bool _tutorialCompleted;
    }
}
