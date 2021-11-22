using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    internal class OperatorNodeFactory
    {
        public static OperatorNode CreateOperatorNode(char op)
        {
            switch (op)
            {
                case '-':
                    return new MinusOperatorNode();
                case '+':
                    return new PlusOperatorNode();
                case '*':
                    return new MultiplyOperatorNode();
                case '/':
                    return new DivideOperatorNode();
                case '(':
                    return new LeftParenthesisNode();
                case ')':
                    return new RightParenthesisNode();
            }

            return null;
        }

        public static bool IsValidOperator(char op)
        {
            char[] operators = { '-', '+', '*', '/', '(', ')'};

            if (op.ToString().IndexOfAny(operators) != -1)
            {
                return true;
            }

            return false;
        }


    }
}
