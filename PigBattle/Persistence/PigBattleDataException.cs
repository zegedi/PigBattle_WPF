using System;

namespace PigBattle.Persistence
{
    public class PigBattleDataException : Exception
    {
        public PigBattleDataException() { }
        public PigBattleDataException(String message) : base(message) { }
    }
}
