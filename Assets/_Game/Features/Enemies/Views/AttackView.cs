using Code.Common.View;
using TMPro;
using UnityEngine;

namespace _Game.Features.Enemies
{
    public partial class AttackView : EntityChildView
    {
        [SerializeField] private TMP_Text _attackLabel;

        public override void PostBind()
        {
            if (Entity.Has<Attack>())
            {
                var currentValue = Entity.Read<Attack>().CurrentValue;
                SetAttack(currentValue, false);
            }
        }

        public void SetAttack(float attack, bool animated = true)
        {
            _attackLabel.text = attack.ToString();
        }
    }
}