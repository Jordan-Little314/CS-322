/* Jordan Little -- 11349968 - CS322 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using CptS322;

namespace Spreadsheet_JLittle
{
    public partial class Form1 : Form
    {
        private Spreadsheet JLitExcel;

        public Form1()
        {
            JLitExcel = new Spreadsheet(50, 26);
            JLitExcel.CellPropertyChanged += OnCellPropertyChanged;

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            for (char c = 'A'; c <= 'Z'; c++)
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.Name = c.ToString();
                col.HeaderText = col.Name;
                dataGridView1.Columns.Add(col);
            }

            for (int i =1; i <= 50; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.HeaderCell.Value = Convert.ToString(i);
                dataGridView1.Rows.Add(row);
            }

            dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);

            initSheet();
        }

         void initSheet()
        {
            dataGridView1.SelectAll();
            foreach (DataGridViewCell c in dataGridView1.SelectedCells)
            {
                c.Value = "";
                c.Style.BackColor = Color.White;
            }
            dataGridView1.ClearSelection();
        }

        private void OnCellPropertyChanged(Object sender, PropertyChangedEventArgs info)
        {
            Cell c = (Cell)sender;
            if (dataGridView1.Rows[c._RowIndex].Cells[c._ColumnIndex].Value.ToString() != c.Value)                    /* if the value is different, update the value */
            {
                dataGridView1.Rows[c._RowIndex].Cells[c._ColumnIndex].Value = c.Value;
            }
            if (dataGridView1.Rows[c._RowIndex].Cells[c._ColumnIndex].Style.BackColor != Color.FromArgb(c.Color))
            {
                dataGridView1.Rows[c._RowIndex].Cells[c._ColumnIndex].Style.BackColor = Color.FromArgb(c.Color);
            }
        }

        private void dataGridView1_CellBeginEdit(Object sender, DataGridViewCellCancelEventArgs e)
        {
            // Editing a cell shows the actual text, not the value
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = JLitExcel.GetCell(e.RowIndex, e.ColumnIndex).Text;
        }

        private void dataGridView1_CellEndEdit(Object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == JLitExcel.GetCell(e.RowIndex, e.ColumnIndex).Text)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == JLitExcel.GetCell(e.RowIndex, e.ColumnIndex).Value)
                    return;
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = JLitExcel.GetCell(e.RowIndex, e.ColumnIndex).Value;
                return;
            }

            Cell c = JLitExcel.GetCell(e.RowIndex, e.ColumnIndex);

            Stack<Cell> curCell = new Stack<Cell>();
            Stack<Cell> prevCell = new Stack<Cell>();

            Stack<string> curText = new Stack<string>();
            Stack<string> prevText = new Stack<string>();

            prevText.Push(c.Text);

            // Save current and previous states
            curCell.Push(c);
            prevCell.Push(c);

            // Cell updates to new state
            c.Text = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            curText.Push(c.Text);

            RestoreText cmd = new RestoreText(prevCell, curCell, prevText, curText, "Cell Text Change");
            JLitExcel.AddUndo(cmd);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

     
        }

        private void button1_Click(object sender, EventArgs e)
        {
            JLitExcel.RunSim();
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            if (cd.ShowDialog() != DialogResult.OK)
                return;

            Stack<Cell> curCell = new Stack<Cell>();
            Stack<Cell> prevCell = new Stack<Cell>();

            Stack<int> curColor = new Stack<int>();
            Stack<int> prevColor = new Stack<int>();

            foreach(DataGridViewCell c in dataGridView1.SelectedCells)
            {
                Cell cell = JLitExcel.GetCell(c.RowIndex, c.ColumnIndex);
                prevCell.Push(cell);
                curCell.Push(cell);
                prevColor.Push(cell.Color);
                cell.Color = cd.Color.ToArgb();
                curColor.Push(cell.Color);
            }

            RestoreColor cmd = new RestoreColor(prevCell, curCell, prevColor, curColor, "Cell Color Change");
            JLitExcel.AddUndo(cmd);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (JLitExcel.CanUndo() == false)
                return;

            JLitExcel.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (JLitExcel.CanRedo() == false)
                return;

            JLitExcel.Redo();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (JLitExcel.CanUndo() == false)
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[0]).Enabled = false;

            else
            {
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[0]).Enabled = true;
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[0]).Text = "Undo " + JLitExcel.GetUndoCommand();
            }

            if (JLitExcel.CanRedo() == false)
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[1]).Enabled = false;

            else
            {
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[1]).Enabled = true;
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[1]).Text = "Redo " + JLitExcel.GetRedoCommand();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                JLitExcel = new Spreadsheet(50, 26);
                JLitExcel.CellPropertyChanged += OnCellPropertyChanged;

                initSheet();

                FileStream infile = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read);
                JLitExcel.load(infile);
                infile.Close();
                infile.Dispose();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileStream outfile = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write);
                JLitExcel.save(outfile);
                outfile.Close();
                outfile.Dispose();
            }
        }
    }
}
