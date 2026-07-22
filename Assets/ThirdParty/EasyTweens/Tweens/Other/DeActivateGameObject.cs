using System;
using UnityEngine;

namespace EasyTweens
{
    
    [Serializable]
    public class DeActivateGameObject : TweenBase, ITargetSetter<GameObject>
    {
        [ExposeInEditor]
        public GameObject GameObject;
        
        public override void UpdateTween(float time, float deltaTime)
        {
            if (deltaTime > 0 && (time - deltaTime) <= TotalDelay && time >= TotalDelay)
            {
                GameObject.SetActive(false);
            }

            if (deltaTime < 0 && (time - deltaTime) >= TotalDelay && time <= TotalDelay)
            {
                GameObject.SetActive(true);
            }
        }


        public void SetTarget(GameObject target)
        {
            GameObject = target;
        }
    }
}