using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    // change the cell's text from using undo/redo
    public class TextChange : ICommand
    {
        private string previousText;
        private string newText;
        private Cell cell;

        // Constructor gets the cell, the previous text within the cell, and the new text value
        public TextChange(Cell cell, string previousText, string newText)
        {
            this.cell = cell;
            this.previousText = previousText;
            this.newText = newText;
        }

        // Execute sets the text to the new text sent to the constuctor
        public void Execute()
        {
            this.cell.Text = this.newText;
        }

        // UnDo sets the text to the previous text sent to the constructor
        public void UnDo()
        {
            this.cell.Text = this.previousText;
        }

    }
}
