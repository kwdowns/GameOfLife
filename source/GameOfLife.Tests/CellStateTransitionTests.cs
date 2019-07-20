using Xunit;

namespace GameOfLife.Tests
{
    public class CellStateTransitionTests
    {
        [Fact]
        public void LonelyCellDies()
        {
            Assert.True(LifeGrid.NextCellState(CellState.Alive, 0) == CellState.Dead);
        }

        [Fact]
        public void HealthyCellLives()
        {
            Assert.True(LifeGrid.NextCellState(CellState.Alive, 2) == CellState.Alive);
            Assert.True(LifeGrid.NextCellState(CellState.Alive, 3) == CellState.Alive);
        }

        [Fact]
        public void CrowdedCellDies()
        {
            Assert.True(LifeGrid.NextCellState(CellState.Alive, 4) == CellState.Dead);
        }

        [Fact]
        public void ReproducingCellLives()
        {
            Assert.True(LifeGrid.NextCellState(CellState.Dead, 3) == CellState.Alive);
        }

        [Fact]
        public void DeadCellStaysDead()
        {
            Assert.True(LifeGrid.NextCellState(CellState.Dead, 0) == CellState.Dead);
            Assert.True(LifeGrid.NextCellState(CellState.Dead, 2) == CellState.Dead);
            Assert.True(LifeGrid.NextCellState(CellState.Dead, 4) == CellState.Dead);
        }
    }
}
