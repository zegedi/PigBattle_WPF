using System;

namespace PigBattle.Persistence
{

    /// <summary>
    /// Megadja a RobotPig állását
    /// </summary>
    public enum Direction { Left, Right, Up, Down }

    public class RobotPig
    {
        #region Fields

        private Int32 _x;
        private Int32 _y;
        private Int32 _health;
        private Direction _direction;

        #endregion

        #region Properties

        public Int32 X
        {
            get { return _x; }
            set { _x = value; }
        }

        public Int32 Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public Int32 Health
        {
            get { return _health; }
        }

        public Direction Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        #endregion

        #region Constructors

        public RobotPig(Int32 x, Int32 y, Direction direction, Int32 health = 3)
        {
            if (health < 0)
                throw new ArgumentException("The health is negative!");

            _x = x;
            _y = y;
            _health = health;
            _direction = direction;
        }

        #endregion

        #region Public methods

        public void DecrementHealth()
        {
            --_health;
        }

        #endregion
    }
}
