using System;
using PigBattle.Model;

namespace PigBattle.Persistence
{
    public class PigBattleTable
    {
        #region Fields

        private FieldType[,] _tableContent; //Játéktábla tartalma
        private RobotPig[] _players; //Játékosok

        #endregion

        #region Properites

        /// <summary>
        /// Játékosok lekérdezése.
        /// </summary>
        public RobotPig[] Players { get { return _players; } }

        /// <summary>
        /// Első játékos lekérdezése.
        /// </summary>
        public RobotPig PlayerOne { get { return _players[0]; } }

        /// <summary>
        /// Másodk játékos lekérdezése.
        /// </summary>
        public RobotPig PlayerTwo { get { return _players[1]; } }

        /// <summary>
        /// Játéktábla lekérdezése.
        /// </summary>
        public FieldType[,] TableContent { get { return _tableContent; } }

        #endregion

        #region Constructors

        /// <summary>
        /// PigBattleTable példányosítása.
        /// </summary>
        /// <param name="tableSize">Játéktábla mérete.</param>
        public PigBattleTable(TableSize tableSize = TableSize.Medium)
        {
            //Táblatábla generálása és beállítása
            Int32 size = (Int32)tableSize;
            _tableContent = new FieldType[size, size];

            for (Int32 i = 0; i < size; ++i)
                for (Int32 j = 0; j < size; ++j)
                {
                    _tableContent[i, j] = FieldType.Empty;
                }

            //Játékosos létrehozása és pozíciójuk beállítása a táblán
            _players = new RobotPig[2];
            Int32 playerX = size / 2;
            Int32 playerY = 0;

            _tableContent[playerX, playerY] = FieldType.Player;
            _players[0] = new RobotPig(playerX, playerY, Direction.Right);

            playerX = playerX - 1;
            playerY = size - 1;

            _tableContent[playerX, playerY] = FieldType.Player;
            _players[1] = new RobotPig(playerX, playerY, Direction.Left);
        }


        /// <summary>
        /// PigBattleTable példányosítása meglévő játékosok és tábla esetén.
        /// </summary>
        /// <param name="players">Játékosok 2 hosszzú tömbje.</param>
        /// <param name="tableContent">Játéktábla n*n-es tömbje.</param>
        public PigBattleTable(RobotPig[] players, FieldType[,] tableContent)
        {
            if (players == null || players.Length != 2 || tableContent.GetLength(0) != tableContent.GetLength(1))
                throw new ArgumentException();

            _players = players;
            _tableContent = tableContent;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Megajda, hogy a játéktábla (x,y)-dik pozíciója üres-e (pozíció bal felülről számítva).
        /// </summary>
        /// <param name="x">X-edik sor.</param>
        /// <param name="y">Y-edik oszlop.</param>
        public Boolean IsEmpty(Int32 x, Int32 y)
        {
            if (x < 0 || x >= _tableContent.GetLength(0) || y < 0 || y >= _tableContent.GetLength(1))
                throw new IndexOutOfRangeException();

            return _tableContent[x, y] == FieldType.Empty;
        }

        /// <summary>
        /// Visszaadja a győztes játékos sorszámát.
        /// </summary>
        public Int32 WinnerPlayerNumber()
        {
            Boolean playerOneIsDead = _players[0].Health <= 0;
            Boolean playerTwoIsDead = _players[1].Health <= 0;

            if (playerOneIsDead && playerTwoIsDead) return 3;
            else if (playerOneIsDead) return 2;
            else if (playerTwoIsDead) return 1;
            else return 0;
        }

        /// <summary>
        /// Ellenőrzi, hogy egy adott játékos léphet-e az adott pozícióra.
        /// </summary>
        /// <param name="x">X-edik célsor.</param>
        /// <param name="y">Y-adik céloszlop</param>
        /// <param name="playerIndex">Játékosnak a sorszáma.</param>
        public Boolean CheckStep(Int32 x, Int32 y, Int32 playerIndex)
        {
            --playerIndex;

            Int32 distX = Math.Abs(x - _players[playerIndex].X);
            Int32 distY = Math.Abs(y - _players[playerIndex].Y);

            return IndexesInRange(x, y) &&
                   ((distX == 1 && distY == 0) || (distX == 0 && distY == 1)) &&
                   _tableContent[x, y] == FieldType.Empty;
        }

        /// <summary>
        /// Lépteti az adott játékos azt adott pozícióra, amennyiben lehetséges.
        /// </summary>
        /// <param name="x">X-edik célsor.</param>
        /// <param name="y">Y-adik céloszlop.</param>
        /// <param name="playerIndex">Játékos sorszáma.</param>
        public void StepPlayer(Int32 x, Int32 y, Int32 playerIndex)
        {
            if (!CheckStep(x, y, playerIndex))
                throw new Exception("Inproper step!");

            --playerIndex;

            _tableContent[x, y] = FieldType.Player;
            _tableContent[_players[playerIndex].X, _players[playerIndex].Y] = FieldType.Empty;
            _players[playerIndex].X = x;
            _players[playerIndex].Y = y;
        }

        /// <summary>
        /// Beállítja a magadott játékosnak az irányát.
        /// </summary>
        /// <param name="direction">Célirány.</param>
        /// <param name="playerIndex">Játékos sorszáma.</param>
        public void RotatePlayer(Direction direction, Int32 playerIndex)
        {
            --playerIndex;
            _players[playerIndex].Direction = direction;
        }

        /// <summary>
        /// Módosítja a táblát annak megfelelően, hogy melyik játékos lézerezik.
        /// </summary>
        /// <param name="playerIndex">Játékos sorszáma.</param>
        public void Laser(Int32 playerIndex)
        {
            --playerIndex;
            RobotPig player = _players[playerIndex];

            switch (player.Direction)
            {
                case Direction.Left:
                    for (Int32 j = player.Y - 1; j >= 0; --j)
                    {
                        if (_tableContent[player.X, j] == FieldType.Empty)
                            _tableContent[player.X, j] = FieldType.Laser;
                        else if (_tableContent[player.X, j] == FieldType.Player)
                            _players[OtherPlayerIndex(playerIndex)].DecrementHealth();
                    }
                    break;

                case Direction.Right:
                    for (Int32 j = player.Y + 1; j < _tableContent.GetLength(0); ++j)
                    {
                        if (_tableContent[player.X, j] == FieldType.Empty)
                            _tableContent[player.X, j] = FieldType.Laser;
                        else if (_tableContent[player.X, j] == FieldType.Player)
                            _players[OtherPlayerIndex(playerIndex)].DecrementHealth();
                    }
                    break;

                case Direction.Up:
                    for (Int32 i = player.X - 1; i >= 0; --i)
                    {
                        if (_tableContent[i, player.Y] == FieldType.Empty)
                            _tableContent[i, player.Y] = FieldType.Laser;
                        else if (_tableContent[i, player.Y] == FieldType.Player)
                            _players[OtherPlayerIndex(playerIndex)].DecrementHealth();
                    }
                    break;

                case Direction.Down:
                    for (Int32 i = player.X + 1; i < _tableContent.GetLength(0); ++i)
                    {
                        if (_tableContent[i, player.Y] == FieldType.Empty)
                            _tableContent[i, player.Y] = FieldType.Laser;
                        else if (_tableContent[i, player.Y] == FieldType.Player)
                            _players[OtherPlayerIndex(playerIndex)].DecrementHealth();
                    }
                    break;
            }
        }

        /// <summary>
        /// Módosítja a táblát annak megfelelően, hogy melyik játékos üt.
        /// </summary>
        /// <param name="playerIndex">Játékos sorszáma.</param>
        public void Punch(Int32 playerIndex)
        {
            --playerIndex;
            RobotPig player = _players[playerIndex];

            for (Int32 x = player.X - 1; x <= player.X + 1; ++x)
                for (Int32 y = player.Y - 1; y <= player.Y + 1; ++y)
                {
                    if (!IndexesInRange(x, y) || (x == player.X && y == player.Y))
                        continue;

                    if (_tableContent[x, y] == FieldType.Empty)
                    {
                        _tableContent[x, y] = FieldType.Punch;
                    }
                    else if (_tableContent[x, y] == FieldType.Player)
                    {
                        _players[OtherPlayerIndex(playerIndex)].DecrementHealth();
                    }
                }
        }

        /// <summary>
        /// Üresre cseréli azokat a mezőket a táblán, amiken nem játékos tartózkodik.
        /// </summary>
        public void EmptyNonplayerFields()
        {
            for (Int32 i = 0; i < _tableContent.GetLength(0); ++i)
                for (Int32 j = 0; j < _tableContent.GetLength(1); ++j)
                {
                    if (_tableContent[i, j] != FieldType.Player && _tableContent[i, j] != FieldType.Empty)
                        _tableContent[i, j] = FieldType.Empty;
                }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Ellenőrzi, hogy az adott indexek a tábla egy érvényes pozíciójára mutatnak-e.
        /// </summary>
        /// <param name="x">X pozíció.</param>
        /// <param name="y">Y pozíció.</param>
        private Boolean IndexesInRange(Int32 x, Int32 y)
        {
            return 0 <= x && x < _tableContent.GetLength(0) &&
                   0 <= y && y < _tableContent.GetLength(1);
        }

        /// <summary>
        /// Visszaadja a másik játékos indexét
        /// </summary>
        /// <param name="index">Az egyik játékos indexe</param>
        private Int32 OtherPlayerIndex(Int32 index)
        {
            return (index + 1) % 2;
        }

        #endregion

    }
}
