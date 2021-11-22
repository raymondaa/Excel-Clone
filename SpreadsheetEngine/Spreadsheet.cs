using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace CptS321
{
    public class Spreadsheet
    {
        // 2D array of Cells to represent spreadsheet
        private Cell[,] spreadsheet;

        // keeps track of location for easy access
        private Dictionary<string, int> location = new Dictionary<string, int>();

        // get and set column count
        public int ColumnCount { get; set; }

        // get and set row count
        public int RowCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // Stack that keeps track of undos and redos to the spreadsheet
        private Stack<ICommand> UndoStack = new Stack<ICommand>();
        private Stack<ICommand> RedoStack = new Stack<ICommand>();

        public Spreadsheet(int numRows, int numCols)
        {
            // Create spreadsheet 2D array and initialize rows and columns
            this.spreadsheet = new SpreadSheetCell[numRows, numCols];
            this.ColumnCount = numCols;
            this.RowCount = numRows;

            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    this.spreadsheet[row, col] = new SpreadSheetCell(row, col);

                    this.spreadsheet[row, col].PropertyChanged += this.CellPropertyChanged;
                }
            }

            // add all key values to dictionary for easy reference
            int k = 0;
            for (int i = 65; i < 91; i++)
            {
                this.location.Add(((char)i).ToString(), k);
                ++k;
            }
        }

        // returns cell with desired row and column if row or column out of bounds returns null
        public Cell GetCell(int row, int col)
        {
            if (row > this.RowCount || col > this.ColumnCount)
            {
                return null;
            }

            return this.spreadsheet[row, col];
        }

        // updates the cell text and calls event handler to alert changes
        public void CellPropertyChanged(object sender, EventArgs e)
        {
            var cell = sender as SpreadSheetCell;

            PropertyChangedEventArgs E = e as PropertyChangedEventArgs;

            // If the value is changed in a cell than it must be updated in the spreadsheet. The rest of this function can be ignored in this case.
            if (E.PropertyName == "Value")
            {
                // send info to form for updating spreadsheet
                this.PropertyChanged(this, new PropertyChangedEventArgs(cell.RowIndex.ToString() + "," + cell.ColumnIndex.ToString() + "," + cell.Value)); // Send information to form to update dataviewgrid.
                return;
            }

            // Cell's color is changed
            if (E.PropertyName == "Color")
            {
                String str = "";
                this.PropertyChanged(str, new PropertyChangedEventArgs(cell.RowIndex.ToString() + "," + cell.ColumnIndex.ToString() + "," + cell.BGColor));
            }

            // cell does not start with '=' value must be set to the text
            if (cell.Text.Length == 0 || cell.Text[0] != '=')
            {
                cell.Value = cell.Text;

                // Must unsubsribe to avoid update after hitting undo
                if (cell.varNames.Count > 0)
                {
                    foreach (KeyValuePair<string, double> var in cell.varNames.ToList())
                    {
                        cell.UnSubscribeExpressionTreeToCell(this.FindCellFromName(var.Key));
                    }
                }

                this.PropertyChanged(cell, new PropertyChangedEventArgs("Text"));
            }

            // Compute the value from the formula after the '='
            else
            {
                cell.NewExpressionTree(cell.Text.Substring(1));
                if (this.CheckForReference(cell))
                {
                    foreach (KeyValuePair<string, double> var in cell.varNames.ToList())
                    {
                        cell.SubscribeExpressionTreeToCell(this.FindCellFromName(var.Key));
                    }

                    cell.Value = cell.EvaluateExpression();
                }

                // update spreadsheet when cell property is changed
                this.PropertyChanged(this, new PropertyChangedEventArgs(cell.RowIndex.ToString() + "," + cell.ColumnIndex.ToString() + "," + cell.Value)); // Send information to form to update dataviewgrid.
            }
        }

        // takes the row number and column letter and creates name ex. A12
        public static string CreateCellName(int rowIndex, int columnIndex)
        {
            string name = ((char)(columnIndex + 65)).ToString();
            return name + (rowIndex + 1);
        }

        // Grabs the text from a cell using it's coordinates.
        public string GetTextFromCell(string cellName)
        {
            // Gets the text from a cell using the location dictionary
            return this.GetCell(Convert.ToInt32(cellName[2].ToString()) - 1, this.location[cellName[1].ToString()]).Text;
        }

        // Finds and returns desired cell from the cell name
        public Cell FindCellFromName(string cellName)
        {
            int col = this.location[cellName[0].ToString()];
            int row = Convert.ToInt32(cellName.Substring(1)) - 1;
            return this.GetCell(row, col);
        }

        // Push new undo command to the undo stack
        public void PushToUndoStack(ICommand undoCommand)
        {
            this.UndoStack.Push(undoCommand);
            this.PropertyChanged(undoCommand, new PropertyChangedEventArgs("UndoStack NE"));
        }

        // Perform undo command
        public void Undo()
        {
            this.UndoStack.Peek().UnDo();
            this.RedoStack.Push(this.UndoStack.Pop());
            this.PropertyChanged(this.RedoStack.Peek(), new PropertyChangedEventArgs("RedoStack NE"));

            // undo stack is not empty
            if (this.UndoStack.Count > 0)
            {
                this.PropertyChanged(this.RedoStack.Peek(), new PropertyChangedEventArgs("UndoStack NE"));
            }
            else // undo stack is empty
            {
                this.PropertyChanged(this.RedoStack.Peek(), new PropertyChangedEventArgs("UndoStack E"));
            }
        }

        // Perform the redo command
        public void Redo()
        {
            this.RedoStack.Peek().Execute();
            this.UndoStack.Push(this.RedoStack.Pop());
            this.PropertyChanged(this.UndoStack.Peek(), new PropertyChangedEventArgs("UndoStack NE"));

            // Redo stack is not empty
            if (this.RedoStack.Count > 0)
            {
                this.PropertyChanged(this.UndoStack.Peek(), new PropertyChangedEventArgs("RedoStack NE"));
            }
            else // Redo stack is empty
            {
                this.PropertyChanged(this.UndoStack.Peek(), new PropertyChangedEventArgs("RedoStack E"));
            }
        }

        // Performs saving of the spreadsheet data
        public void Save(Stream saveStream)
        {
            // allows for indentation and new lines in xml file
            var indent = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "    "
            };

            XmlWriter writer = XmlWriter.Create(saveStream, indent);

            writer.WriteStartDocument();

            // start text of saved spreadsheet data
            writer.WriteStartElement("spreadsheet");

            for (int i = 0; i < this.RowCount; i++)
            {
                for (int j = 0; j < this.ColumnCount; j++)
                {
                    Cell temp = this.GetCell(i, j);
                    if (temp.Text != string.Empty || temp.BGColor != 0xFFFFFFFF)
                    {
                        // start of specific cell data
                        writer.WriteStartElement("cell");

                        // write cell name to file
                        writer.WriteStartElement("name");
                        writer.WriteString(temp.IndexName);
                        writer.WriteEndElement();

                        // write color to file
                        writer.WriteStartElement("bgcolor");
                        writer.WriteString(temp.BGColor.ToString());
                        writer.WriteEndElement();

                        // write cell's contents to file
                        writer.WriteStartElement("text");
                        writer.WriteString(temp.Text);
                        writer.WriteEndElement();

                        // end cell data write
                        writer.WriteEndElement();
                    }
                }
            }

            // end of data
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        // Performs Loading of spreadsheet
        public void Load(Stream loadStream)
        {
            // send ignoring of whitespace setting to XmlReader
            var whiteSpace = new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            };

            XmlReader reader = XmlReader.Create(loadStream, whiteSpace);

            // temporary holders for values as the spreadsheet's info is saved
            string temp;
            Cell tempCell;

            // start of save file
            reader.ReadStartElement("spreadsheet");

            while (reader.Name == "cell")
            {
                // start of cell data
                reader.ReadStartElement("cell");

                // Get name of Cell
                reader.ReadStartElement("name");
                temp = reader.ReadContentAsString();
                tempCell = this.FindCellFromName(temp);
                reader.ReadEndElement();

                // Get color of cell
                reader.ReadStartElement("bgcolor");
                temp = reader.ReadContentAsString();
                uint.TryParse(temp, out uint result);
                tempCell.BGColor = result;
                reader.ReadEndElement();

                // Get text of cell
                reader.ReadStartElement("text");
                temp = reader.ReadContentAsString();
                tempCell.Text = temp;
                reader.ReadEndElement();

                // end of cell
                reader.ReadEndElement();
            }

            // end of load file
            reader.ReadEndElement();
        }

        // checks that users input does not cause one of the three references. Self, bad (cell does not exist), and circular
        private bool CheckForReference(Cell cell)
        {
            foreach (KeyValuePair<string, double> cellName in cell.varNames.ToList())
            {
                // a cell name in the variable dictionary matches the cell name the user is inputting, self reference
                if (cellName.Key == cell.IndexName)
                {
                    cell.Value = "!(self reference)";
                    return false;
                }

                // The index of the cell in the dictionary does not exist in the spreadsheet
                else if (!this.CheckCellExists(cellName.Key))
                {
                    cell.Value = "!(bad reference)";
                    return false;
                }

                // A circular index if the variable dictionary cell references the cell the user inputted
                else if (this.CheckCircularReference(cellName.Key, cell.IndexName))
                {
                    cell.Value = "!(circular reference)";
                    return false;
                }
            }

            // the inputted cell breaks none of the reference rules
            return true;
        }

        private bool CheckCellExists(string name)
        {
            char col;
            int row, intCol;

            // get Letter for column and number for row value
            col = name[0];
            int.TryParse(name.Substring(1), out row);

            // convert the column number
            intCol = Convert.ToInt32(col) - 65;

            // check whether cell is a valid cell within the spreadsheet
            if (intCol < 0 || intCol > this.ColumnCount || row < 0 || row > this.RowCount)
            {
                return false;
            }

            // The cell is valid
            return true;
        }

        // finds whether the cell entered into the spreadsheet creates a circular reference
        private bool CheckCircularReference(string varName, string cellName)
        {
            Cell tempCell;

            tempCell = FindCellFromName(varName);

            foreach (KeyValuePair<string, double> name in tempCell.varNames.ToList())
            {
                // circular reference if cell is subscribed to one of the cells in its variable list
                if (name.Key == cellName)
                {
                    return true;
                }

                // checks for all other keys for circular references
                return this.CheckCircularReference(name.Key, cellName);
            }

            // No circular reference
            return false;
        }
    }
}
