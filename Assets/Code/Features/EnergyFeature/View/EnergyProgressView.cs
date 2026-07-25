using Code.Common.View;
using DG.Tweening;
using EasyTweens;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Features.EnergyFeature.View
{
    public partial class EnergyProgressView : EntityChildView
    {
        [SerializeField] private Image _fill;
        [SerializeField] TweenAnimation _fillAnimation;

        protected override void PostBind()
        {
            if (_entity.TryUnpack<WT>(out var entity) &&
                entity.Has<Energy>() && entity.Has<MaxEnergy>())
            {
                SetEnergy(entity.Read<Energy>().Value, entity.Read<MaxEnergy>().Value, false);
            }
        }

        public void SetEnergy(int energy, int maxEnergy, bool animate = true)
        {
            if (animate)
            {
                _fillAnimation.Play();
                _fill.DOFillAmount((float)energy / maxEnergy, 0.5f).SetEase(Ease.InOutSine);
            }
            else
            {
                _fill.fillAmount = (float)energy / maxEnergy;
            }
        }
    }
}