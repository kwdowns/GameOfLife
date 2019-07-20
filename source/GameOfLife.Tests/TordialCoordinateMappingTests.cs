using System;
using Xunit;
using GameOfLife;

namespace GameOfLife.Tests
{
    public class TordialCoordinateMappingTests
    {
/*
(0,0)(1,0)(2,0)
(0,1)(1,1)(2,2)
(0,2)(1,2)(2,2)
*/

        [Fact]
        public void SpillOverTopEdge()
        {
            var mapped = LifeGrid.MapCoordsToTorus((0,-1), 3, 3);
            Assert.True(mapped == (0,2));
        }

        [Fact]
        public void SpillOverLeftEdge()
        {
            var mapped = LifeGrid.MapCoordsToTorus((-1,0), 3,3);
            Assert.True(mapped == (2,0));
        }

        [Fact]
        public void SpillOverRightEdge()
        {
            var mapped = LifeGrid.MapCoordsToTorus((3,0), 3,3);
            Assert.True(mapped == (0,0));
        }

        [Fact]
        public void SpillOverBottomEdge()
        {
            var mapped = LifeGrid.MapCoordsToTorus((0,3), 3,3);
            Assert.True(mapped == (0,0));
        }
    }
}
