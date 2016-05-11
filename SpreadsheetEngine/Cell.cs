/* Jordan Little -- 11349968 - CS322 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CptS322
{
    
    public abstract class Cell : INotifyPropertyChanged
    {
        public readonly int _RowIndex;
        public readonly int _ColumnIndex;
        protected String _Text;
        protected String _Value;
        protected int _BGColor;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected Cell(int rowIndex, int ColIndex)
        {
            _RowIndex = rowIndex;
            _ColumnIndex = ColIndex;
            _Text = "";
            _Value = "";
            _BGColor = -1;
        }

        public String Text
        {
            get { return _Text; }
            set
            {
                // If the strings aren't equal, fire the PropertyChanged event
                if (this._Value != value)
                {
                    _Text = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }

        public int Color
        {
            get { return _BGColor; }
            set
            {
                // If not changing to same color, fire PropChanged event
                if (this._BGColor != value)
                {
                    _BGColor = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Color"));
                }
            }
        }

        public String Value
        {
            get { return _Value; }
        }

        internal void setValue (String Val)
        {
            _Value = Val;
        }
        
    }
}
