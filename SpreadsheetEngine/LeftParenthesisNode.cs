using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    internal class LeftParenthesisNode : OperatorNode
    {
        // Constructor for LeftParenthesisNode
        public LeftParenthesisNode() : base('(')
        {
        }

        //getter for precedence
        public int Precedence { get; } = 2;

        // overridden Evaluate function
        public override double Evaluate(double left, double right)
        {
            return right;
        }
    }
}
