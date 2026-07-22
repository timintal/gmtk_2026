using Code.Features.DragAndDrop;
using NUnit.Framework;
using UnityEngine;

namespace Code.Features.DragAndDrop.Tests
{
    public sealed class GridLayoutCalculatorTests
    {
        [Test]
        public void CenteredGridPlacesItemsAroundOrigin()
        {
            var layout = new GridContainerLayout
            {
                ColumnCount = 3,
                CellSpacing = new Vector2(2f, 2f),
                LocalOffset = Vector2.zero,
                Centered = true
            };

            Assert.That(GridLayoutCalculator.GetLocalPosition(0, 4, layout), Is.EqualTo(new Vector3(-2f, 1f, 0f)));
            Assert.That(GridLayoutCalculator.GetLocalPosition(1, 4, layout), Is.EqualTo(new Vector3(0f, 1f, 0f)));
            Assert.That(GridLayoutCalculator.GetLocalPosition(2, 4, layout), Is.EqualTo(new Vector3(2f, 1f, 0f)));
            Assert.That(GridLayoutCalculator.GetLocalPosition(3, 4, layout), Is.EqualTo(new Vector3(0f, -1f, 0f)));
        }

        [Test]
        public void CenteredGridCentersPartialLastRow()
        {
            var layout = new GridContainerLayout
            {
                ColumnCount = 3,
                CellSpacing = new Vector2(2f, 2f),
                LocalOffset = Vector2.zero,
                Centered = true
            };

            Assert.That(GridLayoutCalculator.GetLocalPosition(3, 5, layout), Is.EqualTo(new Vector3(-1f, -1f, 0f)));
            Assert.That(GridLayoutCalculator.GetLocalPosition(4, 5, layout), Is.EqualTo(new Vector3(1f, -1f, 0f)));
        }

        [Test]
        public void NonCenteredGridStartsFromOffset()
        {
            var layout = new GridContainerLayout
            {
                ColumnCount = 2,
                CellSpacing = new Vector2(1.5f, 1f),
                LocalOffset = new Vector2(3f, -1f),
                Centered = false
            };

            Assert.That(GridLayoutCalculator.GetLocalPosition(2, 4, layout), Is.EqualTo(new Vector3(3f, -2f, 0f)));
        }
    }
}
