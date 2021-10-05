using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    public class ColorChange : ICommand
    {
        private uint newColor;
        private List<uint> previousColor;
        private List<Cell> cells;

        // Constuctor keeps track of the cell being modified, with the new color and cells previous color
        public ColorChange(List<Cell> cells, List<uint> previousColor, uint newColor)
        {
            this.previousColor = previousColor;
            this.newColor = newColor;
            this.cells = cells;
        }

        // Execute changes the color of all cells to the new color
        public void Execute()
        {
            foreach (Cell cell in this.cells)
            {
                cell.BGColor = this.newColor;
            }
        }

        // Undo changes all cells back to their previous color.
        public void UnDo()
        {
            for (int i = 0; i < this.cells.Count; i++)
            {
                this.cells[i].BGColor = this.previousColor[i];
            }
        }

    }
}
