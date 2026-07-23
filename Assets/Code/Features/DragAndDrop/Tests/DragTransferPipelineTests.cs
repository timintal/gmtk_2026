using Code.Common;
using Code.Features.DragAndDrop;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UnityEngine;

namespace Code.Features.DragAndDrop.Tests
{
    public sealed class DragTransferPipelineTests
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
        public void PlaceInContainerKeepsBidirectionalLinks()
        {
            var container = W.NewEntity<Default>().Set(new DragContainer());
            var draggable = W.NewEntity<Default>().Set(new Draggable());

            DragContainerRelations.PlaceInContainer(draggable, container);

            Assert.That(draggable.Read<W.Link<InDragContainer>>().Value, Is.EqualTo(container.GID));
            Assert.That(container.Has<W.Links<DragContainerItems>>(), Is.True);
            Assert.That(container.Read<W.Links<DragContainerItems>>().Length, Is.EqualTo(1));
            Assert.That(container.Read<W.Links<DragContainerItems>>()[0].Value, Is.EqualTo(draggable.GID));
        }

        [Test]
        public void CoreValidationRejectsMissingTarget()
        {
            var source = W.NewEntity<Default>().Set(new DragContainer());
            var draggable = W.NewEntity<Default>().Set(new Draggable());
            var request = W.NewEntity<Default>().Set(new DragTransferRequest
            {
                Draggable = draggable.GID,
                SourceContainer = source.GID,
                TargetContainer = default,
                HasSourceContainer = true,
                HasTargetContainer = false,
            });

            new DragTransferCoreValidationSystem().Update();

            Assert.That(request.Has<DragTransferRejected>(), Is.True);
            Assert.That(request.Read<DragTransferRejected>().Reason, Is.EqualTo(DragTransferRejectReason.NoTargetContainer));
        }

        [Test]
        public void CoreValidationRejectsFullContainer()
        {
            var source = W.NewEntity<Default>().Set(new DragContainer());
            var target = W.NewEntity<Default>().Set(new DragContainer { Capacity = 1 });
            DragContainerRelations.PlaceInContainer(W.NewEntity<Default>().Set(new Draggable()), target.GID);
            var draggable = W.NewEntity<Default>().Set(new Draggable());
            DragContainerRelations.PlaceInContainer(draggable, source.GID);
            var request = W.NewEntity<Default>().Set(new DragTransferRequest
            {
                Draggable = draggable.GID,
                SourceContainer = source.GID,
                TargetContainer = target.GID,
                HasSourceContainer = true,
                HasTargetContainer = true,
            });

            new DragTransferCoreValidationSystem().Update();

            Assert.That(request.Has<DragTransferRejected>(), Is.True);
            Assert.That(request.Read<DragTransferRejected>().Reason, Is.EqualTo(DragTransferRejectReason.ContainerFull));
        }

        [Test]
        public void ResolveApprovedRequestUpdatesContainerLink()
        {
            var source = W.NewEntity<Default>().Set(new DragContainer());
            var target = W.NewEntity<Default>().Set(new DragContainer());
            var draggable = W.NewEntity<Default>().Set(
                new Draggable(),
                new Dragging
                {
                    PointerId = 0,
                    SourceContainer = source.GID,
                    HasSourceContainer = true,
                    OriginPosition = Vector2.one,
                    PointerOffset = Vector2.zero,
                    UsesScreenSpace = false
                });
            DragContainerRelations.PlaceInContainer(draggable, source.GID);
            var request = W.NewEntity<Default>().Set(new DragTransferRequest
            {
                Draggable = draggable.GID,
                SourceContainer = source.GID,
                TargetContainer = target.GID,
                HasSourceContainer = true,
                HasTargetContainer = true,
            });
            var requestGid = request.GID;

            new DragTransferResolveSystem().Update();

            Assert.That(draggable.Read<W.Link<InDragContainer>>().Value, Is.EqualTo(target.GID));
            Assert.That(target.Has<W.Links<DragContainerItems>>(), Is.True);
            Assert.That(target.Read<W.Links<DragContainerItems>>().Length, Is.EqualTo(1));
            Assert.That(draggable.Has<Dragging>(), Is.False);
            Assert.That(source.Has<ContainerLayoutDirty>(), Is.True);
            Assert.That(target.Has<ContainerLayoutDirty>(), Is.True);
            Assert.That(requestGid.TryUnpack<WT>(out _), Is.False);
        }

        [Test]
        public void ResolveRejectedRequestRestoresOriginPositionWithoutTouchingTransform()
        {
            var view = new GameObject("Drag test view");
            try
            {
                var source = W.NewEntity<Default>().Set(new DragContainer());
                var target = W.NewEntity<Default>().Set(new DragContainer());
                var untouchedTransformPosition = new Vector3(10f, 11f, 12f);
                var untouchedTransformRotation = Quaternion.Euler(0f, 0f, 45f);
                view.transform.position = untouchedTransformPosition;
                view.transform.rotation = untouchedTransformRotation;
                var originPosition = new Vector2(1f, 2f);
                var draggable = W.NewEntity<Default>().Set(
                    new Draggable(),
                    new Position { Value = new Vector2(20f, 21f) },
                    new TransformLink { Value = view.transform },
                    new Dragging
                    {
                        PointerId = 0,
                        SourceContainer = source.GID,
                        HasSourceContainer = true,
                        OriginPosition = originPosition,
                        PointerOffset = Vector2.zero,
                        UsesScreenSpace = false
                    });
                DragContainerRelations.PlaceInContainer(draggable, source.GID);
                W.NewEntity<Default>().Set(
                    new DragTransferRequest
                    {
                        Draggable = draggable.GID,
                        SourceContainer = source.GID,
                        TargetContainer = target.GID,
                        HasSourceContainer = true,
                        HasTargetContainer = true,
                    },
                    new DragTransferRejected { Reason = DragTransferRejectReason.Custom });

                new DragTransferResolveSystem().Update();

                Assert.That(draggable.Read<W.Link<InDragContainer>>().Value, Is.EqualTo(source.GID));
                Assert.That(draggable.Read<Position>().Value, Is.EqualTo(originPosition));
                Assert.That(draggable.Has<Dragging>(), Is.False);
                Assert.That(view.transform.position, Is.EqualTo(untouchedTransformPosition));
                Assert.That(view.transform.rotation, Is.EqualTo(untouchedTransformRotation));
                Assert.That(source.Has<ContainerLayoutDirty>(), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(view);
            }
        }

        [Test]
        public void LineLayoutWritesMemberPositions()
        {
            var container = W.NewEntity<Default>().Set(
                new DragContainer(),
                new Position { Value = new Vector2(5f, 10f) },
                new LineContainerLayout
                {
                    Spacing = 2f,
                    LocalOffset = new Vector2(1f, 0f),
                    Direction = Vector2.right,
                    Centered = true
                });
            container.Set<ContainerLayoutDirty>();

            var later = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = Vector2.zero },
                new ContainerSlotIndex { Order = 20 });
            DragContainerRelations.PlaceInContainer(later, container.GID);
            var earlier = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = Vector2.zero },
                new ContainerSlotIndex { Order = 10 });
            DragContainerRelations.PlaceInContainer(earlier, container.GID);

            new LineContainerLayoutSystem().Update();

            Assert.That(earlier.Read<Position>().Value, Is.EqualTo(new Vector2(5f, 10f)));
            Assert.That(later.Read<Position>().Value, Is.EqualTo(new Vector2(7f, 10f)));
            Assert.That(container.Has<ContainerLayoutDirty>(), Is.False);
        }

        [Test]
        public void UiLineLayoutWritesLocalMemberPositions()
        {
            var container = W.NewEntity<Default>().Set(
                new DragContainer(),
                new Position { Value = new Vector2(50f, 60f) },
                new DragContainerHitboxUi(),
                new LineContainerLayout
                {
                    Spacing = 100f,
                    LocalOffset = Vector2.zero,
                    Direction = Vector2.right,
                    Centered = true
                });
            container.Set<ContainerLayoutDirty>();

            var first = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = Vector2.zero },
                new ContainerSlotIndex { Order = 10 });
            DragContainerRelations.PlaceInContainer(first, container.GID);
            var second = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = Vector2.zero },
                new ContainerSlotIndex { Order = 20 });
            DragContainerRelations.PlaceInContainer(second, container.GID);

            new LineContainerLayoutSystem().Update();

            Assert.That(first.Read<Position>().Value, Is.EqualTo(new Vector2(-50f, 0f)));
            Assert.That(second.Read<Position>().Value, Is.EqualTo(new Vector2(50f, 0f)));
            Assert.That(container.Has<ContainerLayoutDirty>(), Is.False);
        }

        [Test]
        public void UiGridLayoutWritesLocalMemberPositions()
        {
            var container = W.NewEntity<Default>().Set(
                new DragContainer(),
                new Position { Value = new Vector2(25f, 35f) },
                new DragContainerHitboxUi(),
                new GridContainerLayout
                {
                    ColumnCount = 2,
                    CellSpacing = new Vector2(100f, 80f),
                    LocalOffset = Vector2.zero,
                    Centered = true
                });
            container.Set<ContainerLayoutDirty>();

            var first = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = Vector2.zero });
            DragContainerRelations.PlaceInContainer(first, container.GID);
            var second = W.NewEntity<Default>().Set(
                new Draggable(),
                new Position { Value = Vector2.zero });
            DragContainerRelations.PlaceInContainer(second, container.GID);

            new GridContainerLayoutSystem().Update();

            Assert.That(first.Read<Position>().Value, Is.EqualTo(new Vector2(-50f, 0f)));
            Assert.That(second.Read<Position>().Value, Is.EqualTo(new Vector2(50f, 0f)));
            Assert.That(container.Has<ContainerLayoutDirty>(), Is.False);
        }
    }
}
