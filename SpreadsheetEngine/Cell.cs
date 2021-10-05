using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CptS321
{
    public abstract class Cell : INotifyPropertyChanged
    {
        // text within the cell
        protected string text;

        // evaluated value of the cell
        protected string value;

        // expression tree for the this specific cell
        protected ExpressionTree tree;

        // Dictionary to keep track of location
        private Dictionary<int, string> location = new Dictionary<int, string>();

        // Dictionary for all variable names that this cell references
        public Dictionary<string, double> varNames = new Dictionary<string, double>();

        // Represents the color of the Cell
        protected uint bgcolor = 0xFFFFFFFF;

        // Constructor.
        public Cell(int row, int col)
        {
            this.text = string.Empty;
            this.value = string.Empty;
            this.RowIndex = row;
            this.ColumnIndex = col;

            // add all of the key values A-Z to location dictionary
            int key = 0;
            for (int i = 65; i < 91; i++)
            {
                this.location.Add(key, ((char)i).ToString());
                ++key;
            }
        }

        // The event that that handles change of text
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // Get the index of the cell row
        public int RowIndex { get; }

        // Get the index of the cell column
        public int ColumnIndex { get; }

        // The index name of the cell. ex: "A1"
        public string IndexName
        {
            get
            {
                return this.location[this.ColumnIndex].ToString() + (this.RowIndex + 1).ToString();
            }
        }

        // Getter/setter for value
        public string Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;         // if value is the same, no need to update
                }

                this.value = value;     // set to new value

                this.PropertyChanged(this, new PropertyChangedEventArgs("Value")); // alert that property has been changed
            }
        }

        // Getter/setter for text member variable.
        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                if (value == this.text)
                {
                    return;   // text is the same do not need to invoke property changed handler
                }

                // text is different must update value
                this.text = value;

                // call property changed handler
                this.PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public uint BGColor
        {
            get { return this.bgcolor; }

            set
            {
                // No change to the cell color
                if (value == this.bgcolor)
                {
                    return;
                }

                this.bgcolor = value;

                this.PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }

        // subscribes expression tree to the cell
        public void SubscribeExpressionTreeToCell(Cell thisCell)
        {
            this.tree.SubscribeToCell(thisCell);
        }

        // unsubscribe expression tree from cell
        public void UnSubscribeExpressionTreeToCell(Cell thisCell)
        {
            this.tree.UnsubscribeToCell(thisCell);
        }

        // Builds a new expression tree for the cell
        public void NewExpressionTree(string exp)
        {
            this.tree = new ExpressionTree(exp);
            this.varNames = this.tree.Vars;
            this.tree.Owner = this;
        }

        // Deletes the expression tree associated with the cell
        public void DeleteExpressionTree()
        {
            this.tree = null;
            this.varNames = null;
        }

        // computes the expression tree for this cell
        public string EvaluateExpression()
        {
            return this.tree.Evaluate().ToString();
        }

    }
}
