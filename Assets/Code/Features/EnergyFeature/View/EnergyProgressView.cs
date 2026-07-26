using Code.Common.View;
using DG.Tweening;
using EasyTweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Features.EnergyFeature.View
{
    public partial class EnergyProgressView : EntityChildView
    {
        const string EnergyTutorialKey = "EnergyTutorial";
        
        [SerializeField] private Image _fill;
        [SerializeField] TweenAnimation _fillAnimation;
        [SerializeField] private TMP_Text _label;

        protected override void PostBind()
        {
            if (_entity.TryUnpack<WT>(out var entity) &&
                entity.Has<Energy>() && entity.Has<MaxEnergy>())
            {
                SetEnergy(entity.Read<Energy>().Value, entity.Read<MaxEnergy>().Value, false);
            }
        }

        public void SetEnergy(int energy, int maxEnergy, bool animate = true, float delay= 0)
        {
            if (animate)
            {
                CheckTutorial(energy, maxEnergy);
                
                _fillAnimation.Play().SetDelay(delay);
                DOVirtual.Float(_fill.fillAmount, (float)energy / maxEnergy, 0.5f, value =>
                    {
                        _fill.fillAmount = value;
                        _label.text = $"{Mathf.RoundToInt(value * maxEnergy)}/{maxEnergy}";
                    }).SetEase(Ease.InOutSine).SetDelay(delay)
                    .onComplete += () =>
                {
                    _label.text = $"{energy}/{maxEnergy}";
                };
            }
            else
            {
                _fill.fillAmount = (float)energy / maxEnergy;
                _label.text = $"{energy}/{maxEnergy}";
            }
            
        }
        
        private void CheckTutorial(int energy, int maxEnergy)
        {
            if (energy < maxEnergy && PlayerPrefs.GetInt(EnergyTutorialKey, 0) == 0)
            {
                PlayerPrefs.SetInt(EnergyTutorialKey, 1);
                PlayerPrefs.Save();
                
            }
        }
    }
}