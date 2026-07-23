using Code.Common.View;
using EasyTweens;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Game.Features.Dice.View
{
    public partial class DieView : EntityChildView
    {
        [SerializeField] TMP_Text _value;
        [SerializeField] GameObject[] _faces;
        [SerializeField] TweenAnimation _rollAnimation;

        [Button]
        public void SetValue(int value, bool animated = true)
        {
            _value.text = value.ToString();
            foreach (var face in _faces)
            {
                face.SetActive(false);
            }

            if (value > 0 && value <= _faces.Length)
            {
                _faces[value - 1].SetActive(true);
                _value.gameObject.SetActive(false);
            }
            else
            {
                _value.gameObject.SetActive(true);
            }
            if (animated)
                _rollAnimation.Play();
        }

        public override void PostBind()
        {
            var entity = Entity;
            
            if (entity != default && entity.Has<DiceValue>())
                SetValue(entity.Read<DiceValue>().Value, false);
            else
                SetValue(1, false);
        }
    }
}