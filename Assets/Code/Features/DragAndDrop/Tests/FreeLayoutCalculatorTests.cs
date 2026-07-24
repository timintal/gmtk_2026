using Code.Features.DragAndDrop;
using NUnit.Framework;
using UnityEngine;

namespace Code.Features.DragAndDrop.Tests
{
    public sealed class FreeLayoutCalculatorTests
    {
        private static readonly Rect Bounds = new(0f, 0f, 10f, 10f);

        [Test]
        public void NonOverlappingItemsStayInPlace()
        {
            var items = new[]
            {
                Item(2f, 2f),
                Item(8f, 8f)
            };

            FreeLayoutCalculator.Resolve(Bounds, items, overlapTolerance: 0f, iterations: 8);

            Assert.That(items[0].Center, Is.EqualTo(new Vector2(2f, 2f)));
            Assert.That(items[1].Center, Is.EqualTo(new Vector2(8f, 8f)));
        }

        [Test]
        public void OverlappingItemsShareThePushEqually()
        {
            var items = new[]
            {
                Item(5f, 5f),
                Item(5.5f, 5f)
            };

            FreeLayoutCalculator.Resolve(Bounds, items, overlapTolerance: 0f, iterations: 8);

            // Midpoint (5.25) preserved, separated to the required distance of 2.
            Assert.That(items[0].Center, Is.EqualTo(new Vector2(4.25f, 5f)));
            Assert.That(items[1].Center, Is.EqualTo(new Vector2(6.25f, 5f)));
        }

        [Test]
        public void SingleItemIsClampedInsideBounds()
        {
            var items = new[]
            {
                Item(9.5f, 5f)
            };

            FreeLayoutCalculator.Resolve(Bounds, items, overlapTolerance: 0f, iterations: 8);

            Assert.That(items[0].Center, Is.EqualTo(new Vector2(9f, 5f)));
        }

        [Test]
        public void OverlapWithinToleranceIsNotResolved()
        {
            var items = new[]
            {
                Item(5f, 5f),
                Item(6.5f, 5f)
            };

            FreeLayoutCalculator.Resolve(Bounds, items, overlapTolerance: 0.5f, iterations: 8);

            Assert.That(items[0].Center, Is.EqualTo(new Vector2(5f, 5f)));
            Assert.That(items[1].Center, Is.EqualTo(new Vector2(6.5f, 5f)));
        }

        [Test]
        public void CoincidentItemsSeparateDeterministically()
        {
            var items = new[]
            {
                Item(5f, 5f),
                Item(5f, 5f)
            };

            FreeLayoutCalculator.Resolve(Bounds, items, overlapTolerance: 0f, iterations: 8);

            Assert.That(items[0].Center, Is.EqualTo(new Vector2(6f, 5f)));
            Assert.That(items[1].Center, Is.EqualTo(new Vector2(4f, 5f)));
        }

        [Test]
        public void AllItemsStayInsideBoundsAfterResolving()
        {
            var items = new[]
            {
                Item(9f, 5f),
                Item(9f, 5f)
            };

            FreeLayoutCalculator.Resolve(Bounds, items, overlapTolerance: 0f, iterations: 16);

            foreach (var item in items)
            {
                Assert.That(item.Center.x, Is.InRange(1f, 9f));
                Assert.That(item.Center.y, Is.InRange(1f, 9f));
            }
        }

        [Test]
        public void CircleShapeSeparatesAlongCenterLine()
        {
            var items = new[]
            {
                Item(5f, 5f),
                Item(5.6f, 5.8f)
            };

            FreeLayoutCalculator.Resolve(Bounds, items, overlapTolerance: 0f, iterations: 16, shape: FreeLayoutShape.Circle);

            var separation = (items[1].Center - items[0].Center).magnitude;
            Assert.That(separation, Is.EqualTo(2f).Within(0.001f));

            // Midpoint preserved (equal push shared along the center line).
            var midpoint = (items[0].Center + items[1].Center) * 0.5f;
            Assert.That(midpoint.x, Is.EqualTo(5.3f).Within(0.001f));
            Assert.That(midpoint.y, Is.EqualTo(5.4f).Within(0.001f));
        }

        [Test]
        public void CircleShapeIgnoresDiagonalCornerOverlap()
        {
            // Boxes overlap on both axes, but the circles (r = 1) do not touch (distance > 2).
            var items = new[]
            {
                Item(5f, 5f),
                Item(6.5f, 6.5f)
            };

            FreeLayoutCalculator.Resolve(Bounds, items, overlapTolerance: 0f, iterations: 8, shape: FreeLayoutShape.Circle);

            Assert.That(items[0].Center, Is.EqualTo(new Vector2(5f, 5f)));
            Assert.That(items[1].Center, Is.EqualTo(new Vector2(6.5f, 6.5f)));
        }

        private static FreeLayoutItem Item(float x, float y)
        {
            return new FreeLayoutItem
            {
                Center = new Vector2(x, y),
                HalfSize = new Vector2(1f, 1f)
            };
        }
    }
}
