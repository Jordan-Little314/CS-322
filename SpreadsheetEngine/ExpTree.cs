using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS322
{
    public class ExpTree
    {
        public Dictionary<string, double> _Table = new Dictionary<string, double>();
        string _Expression;
        private double _Result;
        private Node _Root;

        public ExpTree()
        {
            _Expression = "";
            _Result = 0.0;
            _Root = null;
        }

        public ExpTree(string expression)
        {
            int num = 0;
            string temp = "";
            foreach (string s in expression.Split(' '))
                temp += s;

            foreach (char c in temp)
            {
                if (c == '(')
                    num++;
                if (c == ')')
                    num--;
            }

            if (num != 0)
                throw (new Exception("Parenthesis don't match!"));

            _Expression = temp;
            _Root = Compile(temp);
        }

        public string Expression
        {
            get { return _Expression; }
        }

        public void SetVar(string varName, double varValue)
        {
            _Table[varName] = varValue;
        }
        public Dictionary<string, double> GetVar()
        {
            return _Table;
        }

        public double Eval()
        {
            _Result = EvalHelper(_Root);
            return _Result;
        }

        /*
            This is a helper function the the Eval() function. It determines the value of the node
            by checking to see what type of node it is.

            If the node is an operator node, it recursively calls until at the bottom of the tree
            and then sets the value of the node to the operation that is evaluated on the
            left and right children.
        */
        private double EvalHelper(Node root)
        {
            if (root.Type == "Constant")
                return root.Value;
            if (root.Type == "Variable")
            {
                try
                {
                    return _Table[root.Symbol];
                }

                catch (System.Collections.Generic.KeyNotFoundException e)
                {
                    Console.WriteLine("Not all variables accouted for, " + root.Symbol + " set to zero.");
                    SetVar(root.Symbol, 0.0);
                    return 0;
                }
            }

            if (root.Type == "Operator")
            {
                if (root.Symbol == "+")
                    root.Value = EvalHelper(((OpNode)root)._Left) + EvalHelper(((OpNode)root)._Right);
                else if (root.Symbol == "-")
                    root.Value = EvalHelper(((OpNode)root)._Left) - EvalHelper(((OpNode)root)._Right);
                else if (root.Symbol == "*")
                    root.Value = EvalHelper(((OpNode)root)._Left) * EvalHelper(((OpNode)root)._Right);
                else if (root.Symbol == "/")
                    root.Value = EvalHelper(((OpNode)root)._Left) / EvalHelper(((OpNode)root)._Right);

                return root.Value;
            }

            return 0.0;
        }

        /*
            This function creates the expression tree by creating substrings on operators
            and then recursively calling the substring. Once a substring has no operators,
            it is put into either a constant node or a variable node.
        */

        private Node Compile(string exp)
        {
            if (exp == "")
                return null;

            int index = getOp(ref exp);
            if (index == -1)
                return MakeSimple(exp);

            Node Left = Compile(exp.Substring(0, index));
            Node Right = Compile(exp.Substring(index + 1));

            return new OpNode(exp[index], Left, Right);
        }

        /*
            This function returns the index of the right-most operatior in the passed string
        */


        private int getOp(ref string exp)
        {
            int index = -1;
            int precedence = 5;
            int par = 0;

            exp = getOpHelper(exp);

            for (int i = exp.Length - 1; i >= 0 ; i--)
            {
                if (exp[i] == '(')
                    par--;
                else if (exp[i] == ')')
                    par++;

                if (par == 0)
                {
                    if (exp[i] == '*')
                        if (precedence > 3)
                        {
                            precedence = 3;
                            index = i;
                        }

                    if (exp[i] == '/')
                        if (precedence > 3)
                        {
                            precedence = 3;
                            index = i;
                        }

                    if (exp[i] == '+')
                    {
                        precedence = 1;
                        index = i;
                    }

                    if (exp[i] == '-')
                    {
                        precedence = 1;
                        index = i;
                    }

                    if (precedence == 1)
                        return index;
                }
            }

            if (precedence != -1)
                return index;

            return -1;
        }


        private string getOpHelper(string exp)
        {
            int par = 0;
            bool occurance = false;

            for (int i = exp.Length - 1; i >= 0; i--)
            {
                if (exp[i] == '(')
                {
                    par--;
                    if (par == 0)
                        occurance = true;
                }
                if (exp[i] == ')')
                {
                    par++;
                }

                if ((i != 0) && (occurance == true))
                {
                    return exp;
                }

                else if ((i == 0) && (occurance == true))
                {
                    exp = exp.Substring(1, exp.Length - 2);
                }

                else if (occurance == false && par == 0)
                    return exp;
            }

            return getOpHelper(exp);
        }

        /*
            This function was given to us by Evan, but it creates a constant node if the 
            string value can be evaluated to a double, otherwise it creates a variable node.
        */

        private Node MakeSimple(string s)
        {
            double num;
            if (double.TryParse(s, out num))
                return new ConstNode(num);
            else
                SetVar(s, 0);
            return new VarNode(s);
        }


        /* 
                Everything below here deals with the Node class. There is a abstract base class
                which the three types of nodes inherit from. Each node has at minimum three properties
                Tpye - A string denoting what type of node it is. (Op, Var, Const).
                Symbol - This property is either a operation (+,-,*,/) or a variable.
                Value - This property contains the a double which is either constant or evaluated 

                The OpNode class also contains left and right node children, because it's the only
                node class that would actually need them, thus it's not in the base class.
        */

        private abstract class Node
        {
            protected string _Type;
            protected double _Value;
            protected string _Symbol;

            public virtual string Type
            {
                get { return _Type; }
            }

            public virtual double Value
            {
                get { return _Value; }
                set { _Value = value; }
            }

            public virtual string Symbol
            {
                get { return _Symbol; }
                set { _Symbol = value; }
            }
        }

        private class OpNode : Node
        {
            public Node _Left, _Right;

            public OpNode(char Op, Node left, Node right)
            {
                Symbol = Op.ToString();
                _Left = left;
                _Right = right;
                _Type = "Operator";
            }
        }

        private class VarNode : Node
        {
            public VarNode(string symbol)
            {
                Symbol = symbol;
                _Value = 0.0;
                _Type = "Variable";
            }
        }

        class ConstNode : Node
        {
            public ConstNode(double num)
            {
                _Value = num;
                _Type = "Constant";
            }
        }
    }
}
