using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    internal class PlusOperatorNode : OperatorNode
    {
        // Constructor for DivideNode
        public PlusOperatorNode() : base('+')
        {
        }

        public int Precedence { get; } = 0;

        // overridden Evaluate function
        public override double Evaluate(double left, double right)
        {
            return right + left;
        }
    }
}
