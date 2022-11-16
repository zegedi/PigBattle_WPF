using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using PigBattle.Model;
using System.Windows.Threading;

namespace PigBattle.WPF.ViewModel
{
    public class PigBattleViewModel : ViewModelBase
    {
        #region Fields

        private PigBattleGameModel _model; //modell
        private String[]? _commands = null!;
        private Boolean _controlsEnabled = true;
        private String _gameInstructions = "Első játékos! Adj meg 5 utasítást! (előre, hátra, balra, jobbra, fordulás balra, fordulás jobbra, tűz, ütés)";

        #endregion

        #region Properties

        /// <summary>
        /// Új játék kezdése parancs lekérdezése.
        /// </summary>
        public DelegateCommand NewGameCommand { get; private set; }

        /// <summary>
        /// Játék betöltése parancs lekérdezése.
        /// </summary>
        public DelegateCommand LoadGameCommand { get; private set; }

        /// <summary>
        /// Játék mentése parancs lekérdezése.
        /// </summary>
        public DelegateCommand SaveGameCommand { get; private set; }

        /// <summary>
        /// Kilépés parancs lekérdezése.
        /// </summary>
        public DelegateCommand ExitCommand { get; private set; }

        /// <summary>
        /// Parancs bevitel lekérdezése.
        /// </summary>
        public DelegateCommand InputCommand { get; private set; }

        /// <summary>
        /// Játékmező gyűjtemény lekérdezése.
        /// </summary>
        public ObservableCollection<PigBattleField>? Fields { get; set; }

        public String GameRoundCount { get { return Convert.ToString(_model.RoundCount) + ". Kör"; } }

        public Int32 TableDimension { get { return (Int32)_model.TableSize; } }

        public Boolean ControlsEnabled
        {
            get { return _controlsEnabled; }
            set
            {
                if (_controlsEnabled == value)
                    return;

                _controlsEnabled = value;
                OnPropertyChanged(nameof(ControlsEnabled));
            }
        }

        public String GameInstructions
        {
            get { return _gameInstructions; }
            set
            {
                if (_gameInstructions == value)
                    return;

                _gameInstructions = value;
                OnPropertyChanged(nameof(GameInstructions));
            }
        }

        /// <summary>
        /// Kicsi táblaméret állapotának lekérdezése és beállítása.
        /// </summary>
        public Boolean IsTableSizeSmall
        {
            get { return _model.TableSize == TableSize.Small; }
            set
            {
                if (_model.TableSize == TableSize.Small)
                    return;

                _model.TableSize = TableSize.Small;
                OnPropertyChanged(nameof(IsTableSizeSmall));
                OnPropertyChanged(nameof(IsTableSizeMedium));
                OnPropertyChanged(nameof(IsTableSizeLarge));
            }
        }

        /// <summary>
        /// Közepes táblaméret állapotának lekérdezése és beállítása.
        /// </summary>
        public Boolean IsTableSizeMedium
        {
            get { return _model.TableSize == TableSize.Medium; }
            set
            {
                if (_model.TableSize == TableSize.Medium)
                    return;

                _model.TableSize = TableSize.Medium;
                OnPropertyChanged(nameof(IsTableSizeSmall));
                OnPropertyChanged(nameof(IsTableSizeMedium));
                OnPropertyChanged(nameof(IsTableSizeLarge));
            }
        }

        /// <summary>
        /// Nagy táblaméret állapotának lekérdezése és beállítása.
        /// </summary>
        public Boolean IsTableSizeLarge
        {
            get { return _model.TableSize == TableSize.Large; }
            set
            {
                if (_model.TableSize == TableSize.Large)
                    return;

                _model.TableSize = TableSize.Large;
                OnPropertyChanged(nameof(IsTableSizeSmall));
                OnPropertyChanged(nameof(IsTableSizeMedium));
                OnPropertyChanged(nameof(IsTableSizeLarge));
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Új játék eseménye.
        /// </summary>
        public event EventHandler? NewGame;

        /// <summary>
        /// Játék betöltésének eseménye.
        /// </summary>
        public event EventHandler? LoadGame;

        /// <summary>
        /// Játék mentésének eseménye.
        /// </summary>
        public event EventHandler? SaveGame;

        /// <summary>
        /// Játékból való kilépés eseménye.
        /// </summary>
        public event EventHandler? ExitGame;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler? InputError;

        #endregion

        #region Constructors

        public PigBattleViewModel(PigBattleGameModel model)
        {
            // játék csatlakoztatása
            _model = model;
            _model.GameAdvanced += new EventHandler<PigBattleEventArgs>(Model_GameAdvanced);
            _model.GameCreated += new EventHandler(Model_GameCreated);

            // parancsok kezelése
            NewGameCommand = new DelegateCommand(param => OnNewGame());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());
            InputCommand = new DelegateCommand(param => OnInput(param));

            // játéktábla létrehozása
            GenerateTable();
            RefreshTable();
        }

        #endregion

        #region Private Methods

        private void GenerateTable()
        {
            if (Fields != null)
                Fields.Clear();
            else
                Fields = new ObservableCollection<PigBattleField>();

            // Lekérdezzük és értesítünk a táblaméret változásról.
            Int32 size = TableDimension;
            OnPropertyChanged(nameof(TableDimension));

            for (Int32 i = 0; i < size; ++i) // inicializáljuk a mezőket
            {
                for (Int32 j = 0; j < size; ++j)
                {
                    Fields.Add(new PigBattleField
                    {
                        X = i,
                        Y = j
                    });
                }
            }
        }

        /// <summary>
		/// Tábla frissítése.
		/// </summary>
		private void RefreshTable()
        {
            if (Fields == null) 
                throw new ArgumentNullException(nameof(Fields));

            foreach (PigBattleField field in Fields)
            {
                field.Text = _model.TablePositionString(field.X, field.Y);
                switch (_model.TablePositionFieldType(field.X, field.Y))
                {
                    case FieldType.Empty:
                        field.Color = Color.FromRgb(202, 253, 202);
                        break;

                    case FieldType.Punch:
                    case FieldType.Laser:
                        field.Color = Color.FromRgb(247, 53, 53);
                        break;

                    case FieldType.Player:
                        field.Color = field.Text.Substring(0, 1) == "1" ? Color.FromRgb(251, 153, 153) : Color.FromRgb(253, 202, 228);
                        break;
                }
                //OnPropertyChanged(nameof(field.Text));
                //OnPropertyChanged(nameof(field.Color));
            }

            //OnPropertyChanged(nameof(Fields));
        }

        #endregion

        #region Game event handlers

        /// <summary>
        /// Játék előrehaladásának eseménykezelője.
        /// </summary>
        private void Model_GameAdvanced(object? sender, PigBattleEventArgs e)
        {
            RefreshTable();
            Task.Delay(1000).Wait();

            //Ha vége van a körnek
            if (e.RoundOver)
            {
                ControlsEnabled = true;
                GameInstructions = "Első játékos! Adj meg 5 utasítást! (előre, hátra, balra, jobbra, fordulás balra, fordulás jobbra, tűz, ütés)";
                OnPropertyChanged(nameof(GameRoundCount));
            }
        }

        /// <summary>
        /// Játék létrehozásának eseménykezelője.
        /// </summary>
        private void Model_GameCreated(object? sender, EventArgs e)
        {
            ControlsEnabled = true;
            GameInstructions = "Első játékos! Adj meg 5 utasítást! (előre, hátra, balra, jobbra, fordulás balra, fordulás jobbra, tűz, ütés)";
            
            OnPropertyChanged(nameof(GameRoundCount));
            GenerateTable();
            RefreshTable();
        }

        #endregion

        #region Event methods

        /// <summary>
		/// Új játék indításának eseménykiváltása.
		/// </summary>
		private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játék betöltése eseménykiváltása.
        /// </summary>
        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játék mentése eseménykiváltása.
        /// </summary>
        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játékból való kilépés eseménykiváltása.
        /// </summary>
        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Bement érkezése.
        /// </summary>
        private void OnInput(object? param)
        {
            // Frissítjük a parancsokat
            String input = param as String ?? String.Empty;

            // Ha helytelen a program, akkor InputError esemény kiváltás és visszatérés.
            if (!_model.IsValidProgram(input))
            {
                InputError?.Invoke(this, EventArgs.Empty);
                return;
            }

            //Megnézzük, hogy az első játékos adott-e már meg programot
            Int32 index = 1;
            if (_commands == null)
            {
                GameInstructions = "Második játékos! Adj meg 5 utasítást! (előre, hátra, balra, jobbra, fordulás balra, fordulás jobbra, tűz, ütés)";
                _commands = new String[2];
                index = 0;
            }

            //Ha helyes volt a program akkor eltároljuk a megfelelő helyen
            _commands[index] = input;

            //Ha mindkét játékos megadta a programot, akkor elindul a kör
            if (index == 1 && !String.IsNullOrEmpty(_commands[0]) && !String.IsNullOrEmpty(_commands[1]))
            {
                //Vezérlők átállítása
                GameInstructions = "Harc folyamatban...";
                ControlsEnabled = false;

                //Kör indítása és parancsok törlése
                _model.PlayRound(_commands[0], _commands[1]);
                _commands = null;
            }
        }

        #endregion
    }
}
