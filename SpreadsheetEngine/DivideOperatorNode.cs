using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    internal class DivideOperatorNode : OperatorNode
    {
        // Constructor for DivideNode
        public DivideOperatorNode() : base('/')
        {
        }

        //getter for precedence
        public int Precedence { get; } = 1;

        // overridden Evaluate function
        public override double Evaluate(double left, double right)
        {
            return right / left;
        }
    }
}
