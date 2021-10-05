using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CptS321;

namespace Spreadsheet_Aaron_Raymond
{
    public partial class Form1 : Form
    {

        private Spreadsheet sheet;

        // constructor
        public Form1()
        {
            this.InitializeComponent();
        }

        // The event of the form loading.
        private void Form1_Load(object sender, EventArgs e)
        {
            // column set up
            int columns = 0;
            this.dataGridView1.ColumnCount = 26;
            for (int i = 65; i < 91; i++)
            {
                this.dataGridView1.Columns[columns].Name = ((char)i).ToString();
                ++columns;
            }

            // row set up
            this.dataGridView1.RowCount = 50;
            for (int rows = 0; rows < 50; rows++)
            {
                this.dataGridView1.Rows[rows].HeaderCell.Value = (rows + 1).ToString();
            }

            // create new spreadsheet and update cells
            this.sheet = new Spreadsheet(50, 26);
            this.sheet.PropertyChanged += this.SpreadSheetCellPropertyChanged;
        }

        // handles the event when the cell property is changed
        private void SpreadSheetCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender.GetType() == typeof(Spreadsheet))
            {
                string temp = e.PropertyName;
                string[] values = temp.Split(','); // parse for row,col, and new value

                //                       row location                      column location w/ old value        new text
                this.dataGridView1.Rows[Convert.ToInt32(values[0])].Cells[Convert.ToInt32(values[1])].Value = values[2];
            }

            // If the sender is a command than the undo / redo button should become available if the corresponding stack is empty or not.
            else if (sender is ICommand)
            {
                string temp = e.PropertyName;

                if (temp == "UndoStack NE")
                {
                    this.toolStripMenuItem1.Enabled = true;
                }
                else if (temp == "UndoStack E")
                {
                    this.toolStripMenuItem1.Enabled = false;
                }
                else if (temp == "RedoStack NE")
                {
                    this.toolStripMenuItem2.Enabled = true;
                }
                else if (temp == "RedoStack E")
                {
                    this.toolStripMenuItem2.Enabled = false;
                }
            }

            // The color of the cell is being changed
            else if (sender.GetType() == typeof(String))
            {
                string temp = e.PropertyName; // string parsed for cell location and the value it holds.
                string[] values = temp.Split(','); // parse for row,col, and new Color
                if (uint.TryParse(values[2], out uint result))
                {
                    // grid               row location                      column location         current color             new color
                    this.dataGridView1.Rows[Convert.ToInt32(values[0])].Cells[Convert.ToInt32(values[1])].Style.BackColor = Color.FromArgb(unchecked((int)result));
                }
            }
        }

        // demo button click event
        private void button1_Click(object sender, EventArgs e)
        {
            // Have to reinitialize these variables in every loop to get a "truly" random number.
            var cell = sender as Cell;
            Random rand = new Random();

            // random tests
            for (int num = 0; num < 49; num++)
            {
                sheet.GetCell(rand.Next(0, 49), rand.Next(0, 25)).Text = "Hello World!";
            }

            // Loop for column B
            for (int j = 0; j < 50; j++)
            {
                this.dataGridView1.Rows[j].Cells[1].Value = "This is cell B" + (j + 1).ToString();
            }

            // Loop for column A
            for (int k = 0; k < 50; k++)
            {
                this.dataGridView1.Rows[k].Cells[0].Value = "=B" + (k + 1).ToString();
            }
        }

        // event for user changing the contents of the cell
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != -1)
            {
                // Sets the text in the cell in the sheet to the text in the corresponding dataGridView cell
                if (this.sheet.GetCell(e.RowIndex, e.ColumnIndex).Value != this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString())
                {
                    Cell cell = this.sheet.GetCell(e.RowIndex, e.ColumnIndex);
                    TextChange command = new TextChange(cell, cell.Text, this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                    this.sheet.PushToUndoStack(command);

                    // set text of spreadsheet cell to the text in the data grid box
                    this.sheet.GetCell(e.RowIndex, e.ColumnIndex).Text = this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                }
            }
        }

        // user starts editing cell, value should change to text property of cell
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // set the cell text to the spreadsheet cell text
            string cellText = this.sheet.GetCell(e.RowIndex, e.ColumnIndex).Text;

            // if the cell text starts with '=' then we want to set this cell text to the spreadsheet cell value
            if (cellText.Length > 0 && cellText[0] == '=')
            {
                this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cellText;
            }
        }

        // When the user finishes editing it must go back to show the value
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string msg = this.sheet.GetCell(e.RowIndex, e.ColumnIndex).Value;

            // helps not crash for empty cell editing
            if (msg.Length > 0)
            {
                this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = msg;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // Undo
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.sheet.Undo();
        }

        // Redo
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.sheet.Redo();
        }

        // Change the background color of cell
        private void changeCellBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Cell> cells = new List<Cell>();
            List<uint> previousColors = new List<uint>();
            ColorDialog userDialog = new ColorDialog();

            // Allow user to select color
            userDialog.AllowFullOpen = true;

            // Update the text box color once the user selects okay
            if (userDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (DataGridViewTextBoxCell cell in this.dataGridView1.SelectedCells)
                {
                    cells.Add(this.sheet.GetCell(cell.RowIndex, cell.ColumnIndex));
                    previousColors.Add(this.sheet.GetCell(cell.RowIndex, cell.ColumnIndex).BGColor);
                    this.sheet.GetCell(cell.RowIndex, cell.ColumnIndex).BGColor = this.ColorToUInt(userDialog.Color);
                }

                ColorChange command = new ColorChange(cells, previousColors, this.ColorToUInt(userDialog.Color));
                this.sheet.PushToUndoStack(command);
            }
        }

        // Changes Color into unsigned int version
        private uint ColorToUInt(Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                          (color.G << 8) | (color.B << 0));
        }

        // Save command called when user clicks save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create a new save file dialog
            SaveFileDialog saveSpreadSheetFileDialog = new SaveFileDialog();
            saveSpreadSheetFileDialog.Filter = "XML File|*.xml";
            saveSpreadSheetFileDialog.Title = "Save an XML File";
            saveSpreadSheetFileDialog.ShowDialog();

            // load the xml file to the spreadsheet if file name not empty
            if (saveSpreadSheetFileDialog.FileName != string.Empty)
            {
                // load the file from the filestream
                FileStream stream = (System.IO.FileStream)saveSpreadSheetFileDialog.OpenFile();

                this.sheet.Save(stream);

                stream.Close();
            }
        }

        // Load command called when the user clicks load
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create a new file dialog
            OpenFileDialog openFileLoadDialog = new OpenFileDialog();
            openFileLoadDialog.Filter = "XML File|*.xml";
            openFileLoadDialog.Title = "Load an XML File";

            // show the file dialog to user
            openFileLoadDialog.ShowDialog();

            // load the xml file to the spreadsheet if file name not empty
            if (openFileLoadDialog.FileName != string.Empty)
            {
                // load the file from the filestream
                FileStream stream = (System.IO.FileStream)openFileLoadDialog.OpenFile();

                // clear current spreadsheet before loading
                this.ClearSpreadSheet();
                this.sheet.Load(stream);

                stream.Close();
            }
        }

        // Clears the current spreadsheet's data, will be used for load
        private void ClearSpreadSheet()
        {
            // Clear the data grid view
            this.dataGridView1.Rows.Clear();
            this.dataGridView1.Columns.Clear();

            // redraws the blank data grid view
            this.dataGridView1.Refresh();

            // disable the undo and redo buttons because redo/undo stack will be cleared
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem2.Enabled = false;

            // reinitialize the form to base values
            this.Form1_Load(this, new EventArgs());

        }
    }
}
