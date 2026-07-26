using System;
using FFS.Libraries.StaticEcs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Features.Blessings
{
    public struct BlessingE : IEntityType
    {
        public byte Id() => 1;
    }
    public enum BlessingType
    {
        None,
        AddValue,
        MultiplyValue,
        RerollDice,
        DrawCard
    }

    public enum BlessingAffectRule
    {
        None,
        Odd,
        Even,
        SameValueDice,
        AllDice
    }

    [Serializable]
    public class BlessingsConfig
    {
        public string BlessingId;
        public string Title;
        [Multiline]
        public string Description;
        public BlessingType Type;
        public BlessingAffectRule[] AffectRules;
        public float Value;
        public int LevelLock = 1;
        public float Probability = 100;
        public Color CardBackColor;

        public Action<BlessingsConfig> OnDebugCreateBlessing;

        [Button]
        void DebugCreateBlessing() => OnDebugCreateBlessing?.Invoke(this);

        public W.Entity GetDiceBlessingEntity()
        {
            var entity = W.NewEntity<BlessingE>();

            entity.Set<Blessing>();
            entity.Set(new BlessingId
            {
                Value = BlessingId
            });
            entity.Set(new BlessingValue
            {
                Value = Value
            });
            switch (Type)
            {
                case BlessingType.AddValue:
                    entity.Set<DiceBlessing>();
                    entity.Set<AddValueBlessing>();
                    break;
                case BlessingType.MultiplyValue:
                    entity.Set<DiceBlessing>();
                    entity.Set<MultiplyValueBlessing>();
                    break;
                case BlessingType.RerollDice:
                    entity.Set<RerollBlessing>();
                    break;
                case BlessingType.DrawCard:
                    entity.Set<DrawBlessing>();
                    break;
            }

            foreach (var rule in AffectRules)
            {
                switch (rule)
                {
                    case BlessingAffectRule.Odd:
                        entity.Set<OddBlessing>();
                        break;
                    case BlessingAffectRule.Even:
                        entity.Set<EvenBlessing>();
                        break;
                    case BlessingAffectRule.SameValueDice:
                        entity.Set<AffectSameValueDicesBlessing>();
                        break;
                    case BlessingAffectRule.AllDice:
                        entity.Set<AffectAllBlessing>();
                        break;
                    default:
                        break;
                }
            }

            return entity;
        }
    }
}