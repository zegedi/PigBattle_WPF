using System;
using System.Windows.Media;
using PigBattle.Model;

namespace PigBattle.WPF.ViewModel
{
    /// <summary>
    /// Sudoku játékmező típusa.
    /// </summary>
    public class PigBattleField : ViewModelBase
    {
        private Color _color = Color.FromRgb(202, 253, 202);
        private String _text = String.Empty;

        /// <summary>
        /// Vízszintes koordináta lekérdezése, vagy beállítása.
        /// </summary>
        public Int32 X { get; set; }

        /// <summary>
        /// Függőleges koordináta lekérdezése, vagy beállítása.
        /// </summary>
        public Int32 Y { get; set; }

        /// <summary>
        /// Szín lekérdezése vagy beállítása.
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        /// <summary>
        /// Felirat lekérdezése, vagy beállítása.
        /// </summary>
        public String Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        /// <summary>
        /// Mezőváltoztató parancs lekérdezése, vagy beállítása.
        /// </summary>
        public DelegateCommand? FieldChangeCommand { get; set; }
    }
}
