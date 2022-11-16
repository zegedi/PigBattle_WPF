using System;
using System.Threading.Tasks;

namespace PigBattle.Persistence
{
    /// <summary>
    /// Sudoku fájl kezelő felülete.
    /// </summary>
    public interface IPigBattleDataAccess
    {
        /// <summary>
        /// Fájl betöltése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A fájlból beolvasott játéktábla.</returns>
        Task<PigBattleTable> LoadAsync(String path);

        /// <summary>
        /// Fájl mentése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <param name="table">A fájlba kiírandó játéktábla.</param>
        Task SaveAsync(String path, PigBattleTable table);
    }
}