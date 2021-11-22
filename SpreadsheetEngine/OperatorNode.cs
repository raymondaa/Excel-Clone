using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    internal abstract class OperatorNode : Node
    {
        // Constructor for Operator Node
        public OperatorNode(char c)
        {
            this.Operator = c;
            this.Left = this.Right = null;
        }

        // Gets operator value
        public char Operator { get; set; }

        // getter and setter of left child
        public Node Left { get; set; }

        public int Precedence { get; }

        // Getter and setter of right child
        public Node Right { get; set; }

        // Evaluate method must be overidden in all other operators
        public abstract double Evaluate(double left, double right);
    }
}
