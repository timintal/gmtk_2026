using Code.Common.View;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Game.Features.Enemies
{

    public partial class EnemyCountdownView : EntityChildView
    {
        [SerializeField] private TMP_Text countdownText;

        int _currentCountdown;
        private Tweener _countdownTween;

        protected override void PostBind()
        {
            if (Entity.Has<EnemyCountdown>())
            {
                SetCountdown(Entity.Read<EnemyCountdown>().Value, false);
            }
        }

        public void SetCountdown(int countdown, bool animated = true)
        {
            _countdownTween?.Kill(true);
            if (!animated)
            {
                _currentCountdown = countdown;
                countdownText.text = countdown.ToString();
            }
            else
            {
                _countdownTween = DOVirtual.Float(_currentCountdown, countdown, 0.5f, value =>
                {
                    _currentCountdown = Mathf.RoundToInt(value);
                    countdownText.text = _currentCountdown.ToString();
                });
            }
        }
    }
}