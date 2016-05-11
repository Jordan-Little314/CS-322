using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS322
{
    public class UndoRedoCollection
    {
        protected Stack<Cell> _prevCell;
        protected Stack<Cell> _curCell;

        protected Stack<string> _prevText;
        protected Stack<string> _curText;

        protected Stack<int> _prevColor;
        protected Stack<int> _curColor;

        public string _commmand;

        public virtual void prevOperate() { }
        public virtual void curOperate() { }
    }

    public class RestoreText : UndoRedoCollection
    {
        public RestoreText(Stack<Cell> prevCell, Stack<Cell> curCell, Stack<string> prevText, Stack<string> curText, string command)
        {
            _prevCell = prevCell;
            _curCell = curCell;
            _prevText = prevText;
            _curText = curText;
            _commmand = command;
        }

        public override void prevOperate()
        {
            Stack<Cell> cellStack = new Stack<Cell>(_prevCell);
            Stack<string> textStack = new Stack<string>(_prevText);

            // Reset text to original state
            while (_prevCell.Count != 0)
                _prevCell.Pop().Text = _prevText.Pop();

            _prevCell = cellStack;
            _prevText = textStack;
        }

        public override void curOperate()
        {
            Stack<Cell> cellStack = new Stack<Cell>(_curCell);
            Stack<string> textStack = new Stack<string>(_curText);

            // Reset text to original state
            while (_curCell.Count != 0)
                _curCell.Pop().Text = _curText.Pop();

            _curCell = cellStack;
            _curText = textStack;
        }

    }

    public class RestoreColor : UndoRedoCollection
    {
        public RestoreColor(Stack<Cell> prevCell, Stack<Cell> curCell, Stack<int> prevColor, Stack<int> curColor, string command)
        {
            _prevCell = prevCell;
            _curCell = curCell;
            _prevColor = prevColor;
            _curColor = curColor;
            _commmand = command;
        }

        public override void prevOperate()
        {
            Stack<Cell> cellStack = new Stack<Cell>(_prevCell);
            Stack<int> colorStack = new Stack<int>(_prevColor);

            // Reset color to original state
            while (_prevCell.Count != 0)
                _prevCell.Pop().Color = _prevColor.Pop();

            _prevCell = cellStack;
            _prevColor = colorStack;
        }

        public override void curOperate()
        {
            Stack<Cell> cellStack = new Stack<Cell>(_curCell);
            Stack<int> colorStack = new Stack<int>(_curColor);

            // Reset color to original state
            while (_curCell.Count != 0)
                _curCell.Pop().Color = _curColor.Pop();

            _curCell = cellStack;
            _curColor = colorStack;
        }
    }
}
