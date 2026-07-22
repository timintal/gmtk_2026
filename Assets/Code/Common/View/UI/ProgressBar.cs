using DG.Tweening;
using EasyTweens;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Code.Common.View.UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _mask;
        [SerializeField] private float _fullSize;
        [SerializeField] TweenAnimation _fillAnimation;
        [SerializeField] private TMP_Text _label;

        float _percent;

        private int _currValue;
        private int _maxValue;

        public void SetState(int currValue, int maxValue)
        {
            if (currValue != _currValue || maxValue != _maxValue)
            {
                _currValue = currValue;
                _maxValue = maxValue;
                _label.text = $"{currValue}/{maxValue}";
                SetPercent(currValue / (float)maxValue);
            }
        }

        public void SetPercent(float percent, bool animated = true)
        {
            if (animated)
            {
                _mask.DOSizeDelta(new Vector2(_fullSize * percent, _mask.sizeDelta.y), 0.2f);
                _fillAnimation.Play();
            }
            else
            {
                _mask.sizeDelta = new Vector2(_fullSize * percent, _mask.sizeDelta.y);
            }
            _percent = percent;
        }

        [Button]
        void Set(float percent)
        {
            SetPercent(percent, false);
        }
    }
}