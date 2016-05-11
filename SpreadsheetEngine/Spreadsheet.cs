/* Jordan Little -- 11349968 - CS322 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;


namespace CptS322
{
    public class Spreadsheet
    {
        private UndoRedo _undoRedo;
        Dictionary<Cell, HashSet<Cell>> _Table;
        public Cell[,] _Spreadsheet;

        private readonly int Columns;
        private readonly int Rows;

        public event PropertyChangedEventHandler CellPropertyChanged = delegate { };

        public Spreadsheet(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            _Table = new Dictionary<Cell, HashSet<Cell>>();

            _Spreadsheet = new Cell[Rows, Columns];
            _undoRedo = new UndoRedo();


            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                {
                    _Spreadsheet[i, j] = new StandardCell(i, j);
                    _Spreadsheet[i, j].PropertyChanged += OnPropertyChanged;
                    _Table[_Spreadsheet[i, j]] = new HashSet<Cell>();
                }
        }

        private string CellValueLookup(string name)
        {
            char c = name[0];
            int i = Convert.ToInt32(c) - 65;
            int j;
            Int32.TryParse(name.Substring(1), out j);
            j -= 1;
            return _Spreadsheet[j, i].Value;
        }

        private Cell GetCell(string name)
        {
            char c = name[0];
            int i = Convert.ToInt32(c) - 65;
            int j;
            Int32.TryParse(name.Substring(1), out j);
            j -= 1;
            return (Cell)GetCell(j, i);
        }

        public Cell GetCell(int row, int col)
        {
                return _Spreadsheet[row, col];
        }

        private void AddTable(string[] values, Cell c)
        {
            foreach (string s in values)
            {
                if (_Table.ContainsKey(GetCell(s)) == false)
                    _Table[GetCell(s)] = new HashSet<Cell>();

                _Table[GetCell(s)].Add(c);
            }

        }

        private void RemoveTable(Cell c)
        {
            Stack<Cell> temp = new Stack<Cell>();
            foreach(Cell cell in _Table.Keys)
            {
                if (_Table[cell].Contains(c))
                    temp.Push(cell);
            }

            while (temp.Count > 0)
                _Table[temp.Pop()].Remove(c);
            
        }

        public int RowCount { get { return Rows; } }
        public int ColumnCount { get { return Columns; } }

        private void Complete(Cell cell)
        {
     

            RemoveTable(cell);

            if (_Table.ContainsKey(cell) == false)
            {
                _Table[cell] = new HashSet<Cell>();
            }

            if (cell.Text == "" || cell.Text[0] != '=')
                cell.setValue(cell.Text);
            else
            {
                Compute((StandardCell)cell);
            }

            Stack<StandardCell> temp = new Stack<StandardCell>();

            foreach (StandardCell link in _Table[cell])
            {
                temp.Push(link);
            }

            while (temp.Count > 0)
            {
                StandardCell c = temp.Pop();
                Complete(c);
                OnCellPropertyChanged(c, new PropertyChangedEventArgs("Text"));
            }

        }

        private void Compute(StandardCell cell)
        {
            ExpTree Tree;
            string[] variables;

            // These try/catch exceptions throw bad input if either parenthesis mismatch
            // or if the input is a bad equation (variables dont match)
            try
            {
                Tree = new ExpTree(cell.Text.Substring(1));
            }
            catch
            {
                cell.value = "BAD INPUT";
                return;
            }


            try
            {
                variables = Tree.GetVar().Keys.ToArray();
            }
            catch
            {
                cell.value = "BAD INPUT";
                return;
            }

            if (DoesRefSelf(variables, cell) == true)
            {
                cell.value = "!(self reference)";
                return;
            }

            foreach (string str in variables)
            {
                double value;

                try
                {
                    if (Double.TryParse(CellValueLookup(str), out value))
                    {
                        Tree.SetVar(str, value);
                    }
                }

                catch
                {
                    cell.value = "!(bad reference)";
                    return;
                }
            }

            if (isCircularRef(variables, cell) == true)
            {
                cell.value = "!(circular reference)";
                return;
            }

            try
            {
                cell.value = Tree.Eval().ToString();
            }
            catch
            {
                cell.value = "BAD INPUT";
                return;
            }

            AddTable(variables, cell);
        }

        private bool DoesRefSelf(string [] str, Cell c)
        {
            string name = Convert.ToChar(c._ColumnIndex + 65).ToString() + (c._RowIndex + 1).ToString();
            return str.Contains(name);

        }

        private bool isCircularRef(string [] str, Cell c)
        {
            foreach (string s in str)
            {
                if (isCircularRef(GetCell(s), c) == true)
                    return true;
            }

            return false;
        }

        private bool isCircularRef(Cell current, Cell c)
        {
            if (_Table[c].Contains(current))
                return true;
            Stack<Cell> temp = new Stack<Cell>();

            foreach(Cell cell in _Table.Keys)
            {
                if (_Table[cell].Contains(current))
                    temp.Push(cell);
            }

            while (temp.Count > 0)
            {
                if (isCircularRef(temp.Pop(), c) == true)
                    return true;
            }

            return false;
        }

        // Push a command onto the undo stack
        public void AddUndo(UndoRedoCollection command)
        {
            _undoRedo.pushUndo(command);
        }

        // Undo a command and send command to redo stack from undo stack
        public void Undo()
        {
            _undoRedo.popUndoPushRedo().prevOperate();
        }
        
        public void Redo()
        {
            _undoRedo.popRedoPushUndo().curOperate();
        }

        public bool CanUndo()
        {
            return _undoRedo.canUndo();
        }

        public bool CanRedo()
        {
            return _undoRedo.canRedo();
        }

        public string GetUndoCommand()
        {
            return _undoRedo.getUndoCommand();
        }

        public string GetRedoCommand()
        {
            return _undoRedo.getRedoCommand();
        }

        public void OnPropertyChanged(Object sender, PropertyChangedEventArgs info)
        {
            StandardCell c = (StandardCell)sender;

            if (info.PropertyName == "Text")
            {
                Complete(c);
            }

            OnCellPropertyChanged(sender, info);
        }

        public void OnCellPropertyChanged(object sender, PropertyChangedEventArgs info)
        {
            if (CellPropertyChanged != null)
            {
                CellPropertyChanged(sender, info);
            }
        }

        public void load(FileStream infile)
        {
            XDocument doc = XDocument.Load(infile);

            foreach(XElement ele in doc.Root.Elements("cell"))
            {
                // This converts the row and column to integers, then looks up the cell
                Cell c = (Cell)GetCell(int.Parse(ele.Element("row").Value.ToString()), int.Parse(ele.Element("col").Value.ToString()));

                // Write saved color and text
                c.Color = int.Parse(ele.Element("color").Value.ToString());
                c.Text = ele.Element("text").Value.ToString();
            }
        }

        public void save(FileStream outfile)
        {
            XmlWriter xWrite = XmlWriter.Create(outfile);
            xWrite.WriteStartElement("spreadsheet");
            foreach (Cell c in _Spreadsheet)
            {
                // Only save cells whose default values are changed
                if (c.Text != "" || c.Value != "" || c.Color != -1) 
                {
                    xWrite.WriteStartElement("cell");
                    xWrite.WriteElementString("col", c._ColumnIndex.ToString());
                    xWrite.WriteElementString("row", c._RowIndex.ToString());
                    xWrite.WriteElementString("color", c.Color.ToString());
                    xWrite.WriteElementString("value", c.Value.ToString());
                    xWrite.WriteElementString("text", c.Text.ToString());
                    xWrite.WriteEndElement();
                }
            }
            xWrite.WriteEndElement();
            xWrite.Close();
        }
        
        public void RunSim ()
        {
            Random rand = new Random();

            for (int i = 0; i < RowCount; i++)
            {
                int j = i + 1;
                _Spreadsheet[i, 1].Text = "This is cell B" + j;
            }

            for (int i = 0; i < RowCount; i++)
            {
                int j = i + 1;
                _Spreadsheet[i, 0].Text = "=B" + j;
            }

            for (int i = 0; i < 50; i++)
            {
                int r = rand.Next(50);
                int c = rand.Next(26);
                if (_Spreadsheet[r, c].Text == "")
                    _Spreadsheet[r, c].Text = "Hello World!";
                else
                    i--;
            }
        }

    }

    public class StandardCell : Cell
    {
        public StandardCell(int rowIndex, int colIndex) : base(rowIndex, colIndex) { }
        public string value
        {
            set { _Value = value; }
        }
    }
}
