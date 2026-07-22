using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class ActivateGameObject : TweenBase, ITargetSetter<GameObject>
    {
        [ExposeInEditor] public GameObject GameObject;

        public void SetTarget(GameObject target)
        {
            GameObject = target;
        }

        public override void UpdateTween(float time, float deltaTime)
        {
            if (deltaTime > 0 && time - deltaTime <= TotalDelay && time >= TotalDelay)
            {
                GameObject.SetActive(true);
            }

            if (deltaTime < 0 && time - deltaTime >= TotalDelay && time <= TotalDelay)
            {
                GameObject.SetActive(false);
            }
        }
    }
}
