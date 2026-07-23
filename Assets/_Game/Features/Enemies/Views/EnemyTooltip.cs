using Code.Common.View.UI;
using FFS.Libraries.StaticEcs;
using TMPro;
using UnityEngine;

namespace _Game.Features.Enemies
{
    public class EnemyTooltip : TooltipView
    {
        [SerializeField] private TMP_Text Title;    
        [SerializeField] private TMP_Text BiggerOnlyLabel;
        [SerializeField] private TMP_Text SmallerOnlyLabel;
        [SerializeField] private GameObject EvenOnlyRoot;
        [SerializeField] private GameObject OddOnlyRoot;
        [SerializeField] private GameObject BiggerRoot;
        [SerializeField] private GameObject SmallerRoot;
        
        public override void Bind(World<WT>.Entity entity)
        {
            EvenOnlyRoot.SetActive(false);
            OddOnlyRoot.SetActive(false);
            BiggerRoot.SetActive(false);
            SmallerRoot.SetActive(false);
            
            if (entity.Has<EnemyCountdown>())
            {
                var countdown = entity.Read<EnemyCountdown>();
                Title.text = $"Ticks to Countdown: {countdown.Value}";
                
                if (entity.Has<AcceptOnlyEven>())
                {
                    EvenOnlyRoot.SetActive(true);
                }
                if (entity.Has<AcceptOnlyOdd>())
                {
                    OddOnlyRoot.SetActive(true);
                }
                if (entity.Has<AcceptBigger>())
                {
                    BiggerRoot.SetActive(true);
                    var biggerThan = entity.Read<AcceptBigger>();
                    BiggerOnlyLabel.text = $">{biggerThan.Value}";
                }
                if (entity.Has<AcceptSmaller>())
                {
                    SmallerRoot.SetActive(true);
                    var smallerThan = entity.Read<AcceptSmaller>();
                    SmallerOnlyLabel.text = $"<{smallerThan.Value}";
                }
            }
        }
    }
}