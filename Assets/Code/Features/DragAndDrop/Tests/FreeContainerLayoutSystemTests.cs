using Code.Common;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UnityEngine;

namespace Code.Features.DragAndDrop.Tests
{
    public sealed class FreeContainerLayoutSystemTests
    {
        [SetUp]
        public void SetUp()
        {
            if (W.Status != WorldStatus.NotCreated)
            {
                W.Destroy();
            }

            W.Create(WorldConfig.Default());
            W.Types().RegisterAll();
            W.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (W.Status != WorldStatus.NotCreated)
            {
                W.Destroy();
            }
        }

        [Test]
        public void OverlappingMembersAreSeparatedAndKeptCentered()
        {
            var container = W.NewEntity<Default>().Set(
                new DragContainer(),
                new Position { Value = Vector2.zero },
                new FreeContainerLayout
                {
                    Bounds = new Vector2(10f, 10f),
                    ItemSize = new Vector2(2f, 2f),
                    OverlapTolerance = 0f,
                    RelaxIterations = 8
                });
            container.Set<ContainerLayoutDirty>();

            var first = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = Vector2.zero });
            DragContainerRelations.PlaceInContainer(first, container.GID);

            var second = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = new Vector2(0.5f, 0f) });
            DragContainerRelations.PlaceInContainer(second, container.GID);

            new FreeContainerLayoutSystem().Update();

            var a = first.Read<Position>()!.Value;
            var b = second.Read<Position>()!.Value;

            Assert.That(Mathf.Abs(a.x - b.x), Is.EqualTo(2f).Within(0.0001f));
            Assert.That((a.x + b.x) * 0.5f, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(a.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(b.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(container.Has<ContainerLayoutDirty>(), Is.False);
        }

        [Test]
        public void AnimatedLayoutSpreadsOverMultipleFramesAndSettles()
        {
            W.SetResource(new DeltaTime { Value = 1f / 60f });

            var container = W.NewEntity<Default>().Set(
                new DragContainer(),
                new Position { Value = Vector2.zero },
                new FreeContainerLayout
                {
                    Bounds = new Vector2(10f, 10f),
                    ItemSize = new Vector2(2f, 2f),
                    OverlapTolerance = 0f,
                    AnimationSpeed = 6f
                });
            container.Set<ContainerLayoutDirty>();

            var first = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = Vector2.zero });
            DragContainerRelations.PlaceInContainer(first, container.GID);

            var second = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = new Vector2(0.5f, 0f) });
            DragContainerRelations.PlaceInContainer(second, container.GID);

            var system = new FreeContainerLayoutSystem();

            // A single frame must not fully separate the items (animation, not snap).
            system.Update();
            var afterFirstFrame = Mathf.Abs(second.Read<Position>()!.Value.x - first.Read<Position>()!.Value.x);
            Assert.That(afterFirstFrame, Is.LessThan(2f));
            Assert.That(container.Has<ContainerLayoutDirty>(), Is.True);

            var frames = 1;
            while (container.Has<ContainerLayoutDirty>() && frames < 1000)
            {
                system.Update();
                frames++;
            }

            Assert.That(frames, Is.GreaterThan(1));
            Assert.That(container.Has<ContainerLayoutDirty>(), Is.False);

            var a = first.Read<Position>()!.Value;
            var b = second.Read<Position>()!.Value;
            Assert.That(Mathf.Abs(a.x - b.x), Is.EqualTo(2f).Within(0.01f));
            Assert.That((a.x + b.x) * 0.5f, Is.EqualTo(0.25f).Within(0.01f));
        }

        [Test]
        public void MembersAreClampedInsideBounds()
        {
            var container = W.NewEntity<Default>().Set(
                new DragContainer(),
                new Position { Value = Vector2.zero },
                new FreeContainerLayout
                {
                    Bounds = new Vector2(10f, 10f),
                    ItemSize = new Vector2(2f, 2f),
                    OverlapTolerance = 0f,
                    RelaxIterations = 8
                });
            container.Set<ContainerLayoutDirty>();

            var outside = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = new Vector2(100f, 0f) });
            DragContainerRelations.PlaceInContainer(outside, container.GID);

            new FreeContainerLayoutSystem().Update();

            Assert.That(outside.Read<Position>()!.Value, Is.EqualTo(new Vector2(4f, 0f)));
            Assert.That(container.Has<ContainerLayoutDirty>(), Is.False);
        }
    }
}
