using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS322
{
    interface IUndoRedoCmd
    {
        IUndoRedoCmd Exec();
    }

    class UndoRedo
    {
        private Stack<UndoRedoCollection> _undo;
        private Stack<UndoRedoCollection> _redo;

        public UndoRedo()
        {
            _undo = new Stack<UndoRedoCollection>();
            _redo = new Stack<UndoRedoCollection>();
        }

        // This clears the redo stack when a new action is performed
        public void pushUndo(UndoRedoCollection command)
        {
            _undo.Push(command);
            _redo.Clear();
        }

        public UndoRedoCollection popRedoPushUndo()
        {
            UndoRedoCollection temp = _redo.Pop();
            _undo.Push(temp);
            return temp;
        }

        public UndoRedoCollection popUndoPushRedo()
        {
            UndoRedoCollection temp = _undo.Pop();
            _redo.Push(temp);
            return temp;
        }

        public bool canUndo()
        {
            if (_undo.Count < 1)
                return false;
            else
                return true;
        }

        public bool canRedo()
        {
            if (_redo.Count < 1)
                return false;
            else
                return true;
        }

        public string getUndoCommand()
        {
            return _undo.Peek()._commmand;
        }

        public string getRedoCommand()
        {
            return _redo.Peek()._commmand;
        }
    }
}
