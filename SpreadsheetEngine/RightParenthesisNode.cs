using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    internal class RightParenthesisNode : OperatorNode
    {
        // Constructor for RightParenthesisNode
        public RightParenthesisNode()
            : base(')')
        {
        }

        // the precendence of the operator to be used in computation.
        public ushort Precedence { get; } = 2;

        // overridden Evaluate function
        public override double Evaluate(double left, double right)
        {
            return left;
        }
    }

}
