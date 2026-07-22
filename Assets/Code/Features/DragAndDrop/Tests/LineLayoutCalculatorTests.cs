using Code.Features.DragAndDrop;
using NUnit.Framework;
using UnityEngine;

namespace Code.Features.DragAndDrop.Tests
{
    public sealed class LineLayoutCalculatorTests
    {
        [Test]
        public void CenteredLayoutPlacesItemsAroundOrigin()
        {
            var layout = new LineContainerLayout
            {
                Spacing = 2f,
                LocalOffset = Vector2.zero,
                Direction = Vector2.right,
                Centered = true
            };

            Assert.That(LineLayoutCalculator.GetLocalPosition(0, 3, layout), Is.EqualTo(new Vector3(-2f, 0f, 0f)));
            Assert.That(LineLayoutCalculator.GetLocalPosition(1, 3, layout), Is.EqualTo(Vector3.zero));
            Assert.That(LineLayoutCalculator.GetLocalPosition(2, 3, layout), Is.EqualTo(new Vector3(2f, 0f, 0f)));
        }

        [Test]
        public void NonCenteredLayoutStartsFromOffset()
        {
            var layout = new LineContainerLayout
            {
                Spacing = 1.5f,
                LocalOffset = new Vector2(3f, -1f),
                Direction = Vector2.up,
                Centered = false
            };

            Assert.That(LineLayoutCalculator.GetLocalPosition(2, 4, layout), Is.EqualTo(new Vector3(3f, 2f, 0f)));
        }
    }
}
