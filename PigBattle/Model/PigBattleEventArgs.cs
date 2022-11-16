using System;

namespace PigBattle.Model
{
    public class PigBattleEventArgs : EventArgs
    {
        private Int32 _roundCount;
        private Int32 _playerIndex;
        private Boolean _roundOver;

        public Boolean RoundOver { get { return _roundOver; } }
        public Int32 RoundCount { get { return _roundCount; } }
        public Int32 PlayerIndex { get { return _playerIndex; } }

        public PigBattleEventArgs(Int32 roundCount, Int32 playerIndex, Boolean roundOver)
        {
            _roundCount = roundCount;
            _playerIndex = playerIndex;
            _roundOver = roundOver;
        }
    }
}
