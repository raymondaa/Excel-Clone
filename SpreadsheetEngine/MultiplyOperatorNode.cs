using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    internal class MultiplyOperatorNode : OperatorNode
    {
        // Constructor for multiplyNode
        public MultiplyOperatorNode() : base('*')
        {
        }

        public int Precedence { get; } = 1;

        // overridden Evaluate function
        public override double Evaluate(double left, double right)
        {
            return right * left;
        }
    }
}
