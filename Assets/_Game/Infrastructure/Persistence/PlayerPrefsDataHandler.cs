using System;
using UnityEngine;

namespace _Game.Infrastructure.Persistence
{
    public sealed class PlayerPrefsDataHandler : IPersistentDataHandler
    {
        private const string KeyPrefix = "persistent-data:";

        public void Load(IPersistentData data)
        {
            var storageKey = GetStorageKey(data);
            if (!PlayerPrefs.HasKey(storageKey))
            {
                data.MarkLoaded();
                return;
            }

            try
            {
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(storageKey), data);
                data.MarkLoaded();
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    $"Could not load persistent data '{data.Key}'. The stored value was left unchanged.",
                    exception));
                data.MarkLoaded();
            }
        }

        public void Save(IPersistentData data)
        {
            PlayerPrefs.SetString(GetStorageKey(data), JsonUtility.ToJson(data));
        }

        public void Flush()
        {
            PlayerPrefs.Save();
        }

        private static string GetStorageKey(IPersistentData data)
        {
            return KeyPrefix + data.Key;
        }
    }
}
