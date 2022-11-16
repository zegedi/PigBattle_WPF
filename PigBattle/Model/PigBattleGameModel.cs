using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PigBattle.Persistence;

namespace PigBattle.Model
{
    /// <summary>
    /// Megadja a tábla méretét.
    /// </summary>
    public enum TableSize { Small = 4, Medium = 6, Large = 8 };

    /// <summary>
    /// Megadja a játéktábla pozíciójának típusát.
    /// </summary>
    public enum FieldType { Empty, Player, Laser, Punch };

    public class PigBattleGameModel
    {
        #region Fields

        private IPigBattleDataAccess? _dataAccess; // Adatelérés
        private TableSize _tableSize; // Játéktábla mérete
        private PigBattleTable _table; // Játéktábla
        private Int32 _roundCount; // Kör sorszáma

        #endregion

        #region Properties

        /// <summary>
        /// Táblaméret lekérdezése, beállítása.
        /// </summary>
        public TableSize TableSize
        {
            get { return _tableSize; }
            set { _tableSize = value; }
        }

        /// <summary>
        /// Kör sorszámának lekérdezése.
        /// </summary>
        public Int32 RoundCount { get { return _roundCount; } }

        /// <summary>
        /// Játéktábla lekérdezése.
        /// </summary>
        public PigBattleTable Table { get { return _table; } }

        #endregion

        #region Events

        /// <summary>
        /// Játék előrehaladásának eseménye.
        /// </summary>
        public event EventHandler<PigBattleEventArgs>? GameAdvanced;

        /// <summary>
        /// Játék végének eseménye.
        /// </summary>
        public event EventHandler<PigBattleEventArgs>? GameOver;

        public event EventHandler? GameCreated;

        #endregion

        #region Constructors


        /// <summary>
        /// PigBattleModell példányosítása.
        /// </summary>
        /// <param name="dataAccess">Az adatelérés.</param>
        public PigBattleGameModel(IPigBattleDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _tableSize = TableSize.Medium;
            _roundCount = 1;
            _table = new PigBattleTable(_tableSize);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Új játék kezdése.
        /// </summary>
        public void NewGame()
        {
            _roundCount = 1;
            _table = new PigBattleTable(_tableSize);
            OnGameCreated();
        }

        /// <summary>
        /// Játék betöltése.
        /// </summary>
        /// <param name="path">A mentési fájl elérési útvonala.</param>
        public async Task LoadGameAsync(String path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            _table = await _dataAccess.LoadAsync(path);
            _tableSize = (TableSize)_table.TableContent.GetLength(0);
            _roundCount = 1;

            OnGameCreated();
        }

        /// <summary>
        /// Játék mentése.
        /// </summary>
        /// <param name="path">A mentési fájl elérési útvonala.</param>
        public async Task SaveGameAsync(String path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            await _dataAccess.SaveAsync(path, _table);
        }

        /// <summary>
        /// Vezérli a játék köreinek menetét.
        /// </summary>
        /// <param name="inp_01">Első játékos programja.</param>
        /// <param name="inp_02">Második játékos programja.</param>
        public void PlayRound(String inp_01, String inp_02)
        {
            ++_roundCount;

            String[] prog_01 = inp_01.Split(',');
            String[] prog_02 = inp_02.Split(',');

            for (Int32 i = 0; i < prog_01.Length; ++i)
            {
                RunProgram(prog_01[i], prog_02[i]);

                OnGameAdvanced();

                // Ha vége van a játéknak.
                if (_table.WinnerPlayerNumber() != 0)
                {
                    OnGameOver(_table.WinnerPlayerNumber());
                    break;
                }

                _table.EmptyNonplayerFields();
            }

            OnGameAdvanced(true);
        }


        /// <summary>
        /// Megadja, hogy a bemeneti string helyes program-e
        /// </summary>
        /// <param name="program">Egy vesszővel tagolt string</param>
        /// <returns>True - ha 5 helyes parancsból áll a program</returns>
        public Boolean IsValidProgram(String program)
        {
            if (String.IsNullOrEmpty(program)) return false;

            String[] commands = program.Split(',');

            if (commands.Length != 5) return false;

            for (Int32 i = 0; i < 5; ++i)
            {
                try
                {
                    GetActionNumber(commands[i]);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Visszaadja a játéktábla adott pozíciójának a tartalmát.
        /// </summary>
        /// <param name="x">X-pozíció</param>
        /// <param name="y">Y-pozíció</param>
        public FieldType TablePositionFieldType(Int32 x, Int32 y)
        {
            return _table.TableContent[x, y];
        }

        public String TablePositionString(Int32 x, Int32 y)
        {
            StringBuilder result = new StringBuilder(String.Empty);

            if (_table.TableContent[x, y] == FieldType.Player)
            {
                //Melyik játékosról van szó
                RobotPig player = _table.PlayerOne;

                //Ha nem az egyes játékos az akkor kicseréljük
                if (player.X != x || player.Y != y)
                {
                    player = _table.PlayerTwo;
                    result.Append("2. játékos");
                }
                else
                {
                    result.Append("1. játékos");
                }

                //beállítjuk az irányt
                String directionText = "Fel";
                switch (player.Direction)
                {
                    case Direction.Left: directionText = "Bal"; break;
                    case Direction.Right: directionText = "Jobb"; break;
                    case Direction.Down: directionText = "Le"; break;
                }

                result.Append(Environment.NewLine)
                      .Append(" (")
                      .Append(directionText)
                      .Append(" - ")
                      .Append(player.Health)
                      .Append(")");
            }

            return result.ToString();
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Egyszerre lelfuttatja egy-egy utasítást a játékosok malacain.
        /// </summary>
        /// <param name="actStr01">Első játékos utasítása.</param>
        /// <param name="actStr02">Második játékos utasítása.</param>
        private void RunProgram(String actStr01, String actStr02)
        {
            RobotPig playerOne = _table.PlayerOne;
            RobotPig playerTwo = _table.PlayerTwo;
            Int32 act_01 = GetActionNumber(actStr01);
            Int32 act_02 = GetActionNumber(actStr02);

            //Ha valamelyik vagy mindkét játékos lépni akar
            if ((0 <= act_01 && act_01 <= 3) || (0 <= act_02 && act_02 <= 3))
            {
                //Lekérdezzük a következő pozíciójukat
                Int32 playerOne_nextX = GetNextX(playerOne, act_01);
                Int32 playerOne_nextY = GetNextY(playerOne, act_01);
                Int32 playerTwo_nextX = GetNextX(playerTwo, act_02);
                Int32 playerTwo_nextY = GetNextY(playerTwo, act_02);

                //Ha nem ugyanoda akarnak lépnéni, vagy az egyik nem lép rá a másikra
                if (playerOne_nextX != playerTwo_nextX || playerOne_nextY != playerTwo_nextY)
                {
                    //Ha szabad a mező akkor lépnek
                    if (_table.CheckStep(playerOne_nextX, playerOne_nextY, 1))
                        _table.StepPlayer(playerOne_nextX, playerOne_nextY, 1);

                    if (_table.CheckStep(playerTwo_nextX, playerTwo_nextY, 2))
                        _table.StepPlayer(playerTwo_nextX, playerTwo_nextY, 2);
                }
            }

            //Ha valamelyik játékos nem lépni akart, akkor elvégezzük az akcióját.
            for (Int32 playerIndex = 1; playerIndex <= 2; ++playerIndex)
            {
                Boolean firstPlayer = playerIndex == 1;
                Int32 actionNumber = firstPlayer ? act_01 : act_02;
                RobotPig player = firstPlayer ? playerOne : playerTwo;

                switch (actionNumber)
                {
                    //Ha forogni akar
                    case 4:
                    case 5:
                        _table.RotatePlayer(GetNextDirection(player, actionNumber), playerIndex);
                        break;

                    //Ha lézerezni akar
                    case 6:
                        _table.Laser(playerIndex);
                        break;

                    //Ha ütni akar
                    case 7:
                        _table.Punch(playerIndex);
                        break;
                }
            }
        }

        /// <summary>
        /// Megadja a játékos következő X pozítióját a táblán az akciója alapján.
        /// </summary>
        /// <param name="player">Az akciót végrehajtó játékos.</param>
        /// <param name="action">A játékos által végrehajtandó akció.</param>
        private Int32 GetNextX(RobotPig player, Int32 action)
        {
            switch (action)
            {
                case 0:
                    if (player.Direction == Direction.Left || player.Direction == Direction.Right)
                        return player.X;
                    return (player.Direction == Direction.Up) ? player.X - 1 : player.X + 1;

                case 1:
                    if (player.Direction == Direction.Left || player.Direction == Direction.Right)
                        return player.X;
                    return (player.Direction == Direction.Up) ? player.X + 1 : player.X - 1;

                case 2:
                    if (player.Direction == Direction.Up || player.Direction == Direction.Down)
                        return player.X;
                    return (player.Direction == Direction.Left) ? player.X + 1 : player.X - 1;

                case 3:
                    if (player.Direction == Direction.Up || player.Direction == Direction.Down)
                        return player.X;
                    return (player.Direction == Direction.Left) ? player.X - 1 : player.X + 1;

                default: return player.X;
            }
        }

        /// <summary>
        /// Megadja a játékos következő Y pozítióját a táblán az akciója alapján.
        /// </summary>
        /// <param name="player">Az akciót végrehajtó játékos.</param>
        /// <param name="action">A játékos által végrehajtandó akció.</param>
        private Int32 GetNextY(RobotPig player, Int32 action)
        {
            switch (action)
            {
                case 0:
                    if (player.Direction == Direction.Up || player.Direction == Direction.Down)
                        return player.Y;
                    return (player.Direction == Direction.Left) ? player.Y - 1 : player.Y + 1;

                case 1:
                    if (player.Direction == Direction.Up || player.Direction == Direction.Down)
                        return player.Y;
                    return (player.Direction == Direction.Left) ? player.Y + 1 : player.Y - 1;

                case 2:
                    if (player.Direction == Direction.Left || player.Direction == Direction.Right)
                        return player.Y;
                    return (player.Direction == Direction.Up) ? player.Y - 1 : player.Y + 1;

                case 3:
                    if (player.Direction == Direction.Left || player.Direction == Direction.Right)
                        return player.Y;
                    return (player.Direction == Direction.Up) ? player.Y + 1 : player.Y - 1;

                default: return player.Y;
            }
        }

        /// <summary>
        /// Megadja a játékos következő irányát a táblán az akciója alapján.
        /// </summary>
        /// <param name="player">Az akciót végrehajtó játékos.</param>
        /// <param name="action">A játékos által végrehajtandó akció.</param>
        private Direction GetNextDirection(RobotPig player, Int32 action)
        {
            switch (player.Direction)
            {
                case Direction.Left:
                    if (action == 4) return Direction.Down;
                    else if (action == 5) return Direction.Up;
                    break;

                case Direction.Right:
                    if (action == 4) return Direction.Up;
                    else if (action == 5) return Direction.Down;
                    break;

                case Direction.Up:
                    if (action == 4) return Direction.Left;
                    else if (action == 5) return Direction.Right;
                    break;

                case Direction.Down:
                    if (action == 4) return Direction.Right;
                    else if (action == 5) return Direction.Left;
                    break;
            }
            return player.Direction;
        }


        /// <summary>
        /// Akciószámmá konvertáló metódus.
        /// </summary>
        /// <param name="action">Az akció stringje, amit számmá konvertálunk.</param>
        private Int32 GetActionNumber(String action)
        {
            switch (action.Trim().ToLower())
            {
                case "előre": return 0;
                case "hátra": return 1;
                case "balra": return 2;
                case "jobbra": return 3;
                case "fordulás balra": return 4;
                case "fordulás jobbra": return 5;
                case "tűz": return 6;
                case "ütés": return 7;
                default:
                    throw new Exception("Unknown action");
            }
        }

        #endregion

        #region Private Event Methods

        /// <summary>
        /// Játéktábla megváltozás esemény kiváltása
        /// </summary>
        private void OnGameAdvanced(Boolean roundOver = false)
        {
            GameAdvanced?.Invoke(this, new PigBattleEventArgs(_roundCount, 0, roundOver));
        }

        /// <summary>
        /// Játék vége eseményének kiváltása.
        /// </summary>
        /// <param name="playerIndex">Melyik játékos nyerte a játékot</param>
        private void OnGameOver(Int32 playerIndex)
        {
            GameOver?.Invoke(this, new PigBattleEventArgs(_roundCount, playerIndex, true));
        }

        private void OnGameCreated()
        {
            GameCreated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}