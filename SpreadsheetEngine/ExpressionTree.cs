using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS321
{
    public class ExpressionTree
    {
        private Dictionary<string, double> vars = new Dictionary<string, double>(); // dictionary of variables with their corresponding values
        private Node root; // Root of expression tree
        private Stack<Node> postFixStack = new Stack<Node>();
        private Stack<Node> operatorStack = new Stack<Node>();
        public Cell Owner; // cell that this expression tree belongs to

        // getter/ setter for expression
        public string Expression { get; set; }

        // Getter for the expression trees dictionary of variables, needed for testing
        public Dictionary<string, double> Vars
        {
            get { return this.vars; }
        }

        // Constructor for expression tree
        public ExpressionTree(string expression)
        {
            this.CompileExpression(expression);
            this.root = this.postFixStack.Pop();
            this.root = this.MakeTree(this.root);
            this.Expression = expression;
        }

        public void SetVariable(string variableName, double variableValue)
        {
            // if the name is already contained in var dictionary we need to update the value
            if (vars.ContainsKey(variableName))
            {
                vars[variableName] = variableValue;
            }

            // variable name does not exist in the dictionary, add new entry
            else
            {
                vars.Add(variableName, variableValue);
            }
        }

        // Subscribe the expression tree to the cell property change event
        public void SubscribeToCell(Cell subCell)
        {
            subCell.PropertyChanged += this.CellChanged;

            if (this.vars.ContainsKey(subCell.IndexName))
            {
                // sets the variable in the dict to the value of the cell if it is a double or 0 if it is not.
                if (double.TryParse(subCell.Value, out double num))
                {
                    this.vars[subCell.IndexName] = num;
                }
                else
                {
                    this.vars[subCell.IndexName] = 0.0;
                }
            }
        }

        // unsubscribe from cell to avoid expression tree update after undo
        public void UnsubscribeToCell(Cell unSubCell)
        {
            unSubCell.PropertyChanged -= this.CellChanged;
        }

        // event handling for when cell contents have been changed
        private void CellChanged(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(SpreadSheetCell))
            {
                SpreadSheetCell cell = sender as SpreadSheetCell;

                if (this.vars.ContainsKey(cell.IndexName))
                {
                    // sets the variable in the dict to the value of the cell if it is a double or 0 if it is not.
                    if (double.TryParse(cell.Value, out double num))
                    {
                        this.vars[cell.IndexName] = num;
                    }
                    else
                    {
                        this.vars[cell.IndexName] = 0.0;
                    }

                    // The expression has changed so the value of the cell that this tree belongs to must also change.
                    this.Owner.Value = this.Evaluate().ToString();
                }
            }
        }

        // returns the double output of the expression
        public double Evaluate()
        {
            return this.Evaluate(this.root);
        }

        // evaluates the expression tree nodes based on if they are constants, variables, or operators
        private double Evaluate(Node tree)
        {
            // Evaluate left and right nodes of node if it is a operatornode
            if (tree != null && tree is OperatorNode)
            {
                OperatorNode temp = (OperatorNode)tree;

                return temp.Evaluate(this.Evaluate(temp.Left), this.Evaluate(temp.Right));
            }

            // Return the value of the variable node
            if (tree != null && tree is VariableNode)
            {
                VariableNode temp = (VariableNode)tree;
                return this.vars[temp.Name];
            }

            // Return the value of the constantnode
            if (tree != null && tree is ConstantNode)
            {
                ConstantNode temp = (ConstantNode)tree;
                return temp.Value;
            }

            return 0;
        }

        // Converts the given expression to postfix
        private void ConvertToPostfix(string expression)
        {
            double num;
            int operatorIndex = 0;
            string subString = string.Empty;

            // Checks if expression is empty
            if (expression.Length != 0)
            {
                for (int i = 0; i < expression.Length; i++)
                {
                    operatorIndex = i;

                    // Looks for operator within expression string
                    while (operatorIndex < expression.Length && !OperatorNodeFactory.IsValidOperator(expression[operatorIndex]))
                    {
                        operatorIndex++;
                    }

                    // put operator in substring
                    if (operatorIndex == i)
                    {
                        subString = expression[i].ToString();
                    }

                    // finds the correct substring otherwise
                    else
                    {
                        subString = expression.Substring(i, operatorIndex - i);
                        if (operatorIndex - i > 1)
                        {
                            i += operatorIndex - i;
                            i--;
                        }
                    }

                    // Checks if element placed in substring is an operator
                    if (OperatorNodeFactory.IsValidOperator(subString[0]))
                    {
                        // Creates operator node and divides expression accordingly
                        OperatorNode newNode = OperatorNodeFactory.CreateOperatorNode(subString[0]);

                        // a left parentheses, push it on the stack.
                        if (newNode.Operator == '(')
                        {
                            this.operatorStack.Push(newNode);
                        }

                        // If the incoming symbol is a right parenthesis must pop and print stack until the, left parenthesis is found
                        else if (newNode.Operator == ')')
                        {
                            while ((char)this.operatorStack.Peek().GetType().GetProperty("Operator").GetValue(this.operatorStack.Peek()) != '(')
                            {
                                this.postFixStack.Push(this.operatorStack.Pop());
                            }

                            this.operatorStack.Pop();
                        }

                        // if stack empty/ has a left parenthesis, push operator onto stack
                        else if (this.operatorStack.Count == 0 || (char)this.operatorStack.Peek().GetType().GetProperty("Operator").GetValue(this.operatorStack.Peek()) == '(')
                        {
                            this.operatorStack.Push(newNode);
                        }

                        // if the new operator has higher precendence than the operator on top of stack, push new operator to the stack
                        else if (newNode.Precedence > (int)this.operatorStack.Peek().GetType().GetProperty("Precedence").GetValue(this.operatorStack.Peek()) ||
                            (newNode.Precedence == (int)this.operatorStack.Peek().GetType().GetProperty("Precedence").GetValue(this.operatorStack.Peek())))
                        {
                            this.operatorStack.Push(newNode);
                        }

                        // if the new operator has lower precedence than the operator on top of the stack, pop until the the new operator has higher precedence then push it onto stack
                        else if (newNode.Precedence < (ushort)this.operatorStack.Peek().GetType().GetProperty("Precedence").GetValue(this.operatorStack.Peek()) ||
                            (newNode.Precedence == (ushort)this.operatorStack.Peek().GetType().GetProperty("Precedence").GetValue(this.operatorStack.Peek())))
                        {
                            while (this.operatorStack.Count > 0 && ((OperatorNode)this.operatorStack.Peek()).Operator != '(' && ((OperatorNode)this.operatorStack.Peek()).Precedence >= newNode.Precedence)
                            {
                                this.postFixStack.Push(this.operatorStack.Pop());
                            }

                            this.operatorStack.Push(newNode);
                        }
                    }

                    // if element is a number push it to stack 
                    else if (double.TryParse(subString, out num))
                    {
                        // Must create new constantNode
                        ConstantNode newConstNode = new ConstantNode();
                        newConstNode.Value = num;

                        this.postFixStack.Push(newConstNode);
                    }

                    // otherwise the element is a variableNode
                    else
                    {
                        // must create new variable node and update variable dictionary
                        VariableNode newVarNode = new VariableNode();
                        newVarNode.Name = subString;
                        this.vars[subString] = 0.0;

                        this.postFixStack.Push(newVarNode);
                    }
                }
            }
        }

        // Creates the postfix stack for the expression tree to be created with
        private void CompileExpression(string expression)
        {
            this.ConvertToPostfix(expression);

            // pop and print all operators in operatorStack
            while (this.operatorStack.Count > 0)
            {
                this.postFixStack.Push(this.operatorStack.Pop());
            }

        }

        // creates the Expression tree
        private Node MakeTree(Node cur)
        {
            if (cur is OperatorNode)
            {
                OperatorNode temp = (OperatorNode)cur;
                temp.Left = this.MakeTree(this.postFixStack.Pop());
                temp.Right = this.MakeTree(this.postFixStack.Pop());
            }

            return cur;
        }
    }
}