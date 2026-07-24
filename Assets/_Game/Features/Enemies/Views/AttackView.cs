using Code.Common.View;
using Code.Features.Stats;
using TMPro;
using UnityEngine;

namespace _Game.Features.Enemies
{
    public partial class AttackView : EntityChildView
    {
        [SerializeField] private TMP_Text _attackLabel;

        protected override void PostBind()
        {
            if (Entity.Has<Attack>())
            {
                var currentValue = Entity.Read<Attack>().Rounded();
                SetAttack(currentValue, false);
            }
        }

        public void SetAttack(float attack, bool animated = true)
        {
            _attackLabel.text = attack.ToString();
        }
    }
}