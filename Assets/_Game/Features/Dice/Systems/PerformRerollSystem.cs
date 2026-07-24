using Code.Common;
using Code.Common.Audio;
using Code.Common.Hitbox;
using Code.Configs;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace _Game.Features.Dice.Systems
{
    public class PerformRerollSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in W.Query<All<RerollRequest>>().Entities())
            {
                e.Set<Destroyed>();
                var playerState = W.GetResource<PlayerState>();
                if (playerState.RerollsCount <= 0)
                {
                    continue;
                }
                playerState.RerollsCount--;

                var request = e.Read<RerollRequest>();
                if (request.ClearCurrent)
                {
                    RemoveCurrentDices();
                }
                
                PlayRerollSound();
                var diceCount = request.DiceCount;
                W.Query<All<RolledDicesContainer, DragContainer, Position, Hitbox2D>>().One(out var container);
                
                for (int i = 0; i < diceCount; i++)
                {

                    var prefab = W.GetResource<VisualConfig>().DicePrefab;
                    var position = container.Read<Position>().Value;
                    var bounds = container.Read<Hitbox2D>().Value.bounds;
                    position.x += Random.Range(-bounds.extents.x, bounds.extents.x);
                    position.y += Random.Range(-bounds.extents.y, bounds.extents.y);
                    var provider = Object.Instantiate(prefab, position, Quaternion.identity);
                    var diceEntity = provider.Entity;
                    diceEntity.Set(new DiceValue() { Value = Random.Range(1, 7) });
                    diceEntity.Set(new Position() { Value = position });
                    W.NewEntity<Default>().Set(new DragTransferRequest
                    {
                        Draggable = provider.entityGid,
                        SourceContainer = default,
                        TargetContainer = container,
                        HasSourceContainer = false,
                        HasTargetContainer = true,
                    });
                }
            }
        }

        void PlayRerollSound()
        {
            W.GetResource<SFXAudioSource>().PlayDiceRoll();
        }
        
        private void RemoveCurrentDices()
        {
            foreach (var e in W.Query<All<Dice, W.Link<InDragContainer>>>().Entities())
            {
                if (e.Read<W.Link<InDragContainer>>().Value.TryUnpack<WT>(out var container) &&
                    container.Has<RolledDicesContainer>())
                {
                    e.Set<Destroyed>();
                }
            }
        }
    }
}