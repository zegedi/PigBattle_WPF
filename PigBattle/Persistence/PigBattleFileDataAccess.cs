using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PigBattle.Model;

namespace PigBattle.Persistence
{
    public class PigBattleFileDataAccess : IPigBattleDataAccess
    {

        public async Task<PigBattleTable> LoadAsync(String path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    //Pálya méretének beolvasása
                    String line = await reader.ReadLineAsync() ?? String.Empty;
                    TableSize tableSize = (TableSize)Convert.ToInt32(line);

                    //Két játékos beolvasása
                    RobotPig[] players = new RobotPig[2];
                    for (Int32 i = 0; i < players.Length; ++i)
                    {
                        line = await reader.ReadLineAsync() ?? String.Empty;
                        String[] data = line.Split(' ');

                        Int32 x = Convert.ToInt32(data[0]);
                        Int32 y = Convert.ToInt32(data[1]);
                        Int32 health = Convert.ToInt32(data[2]);
                        Direction direction = (Direction)Convert.ToInt32(data[3]);

                        players[i] = new RobotPig(x, y, direction, health);
                    }

                    //TableContent beolvasása
                    Int32 size = (Int32)tableSize;
                    FieldType[,] tableContent = new FieldType[size, size];

                    for (Int32 i = 0; i < size; ++i)
                    {
                        line = await reader.ReadLineAsync() ?? String.Empty;
                        String[] data = line.Split(' ');

                        for (Int32 j = 0; j < tableContent.GetLength(1); ++j)
                        {
                            tableContent[i, j] = (FieldType)Convert.ToInt32(data[j]);
                        }
                    }

                    return new PigBattleTable(players, tableContent);
                }
            }
            catch
            {
                throw new PigBattleDataException();
            }
        }

        public async Task SaveAsync(String path, PigBattleTable table)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    //Lekérdezzük a tábla tartalmát és a két játékos állapotát
                    FieldType[,] tableContent = table.TableContent;
                    RobotPig[] players = table.Players;

                    //Mekkora a tábla dimenziója
                    writer.WriteLine(tableContent.GetLength(0));

                    //Játékosok adatainak kiírása soronként
                    for (Int32 i = 0; i < players.Length; ++i)
                    {
                        await writer.WriteLineAsync(
                            players[i].X + " " +
                            players[i].Y + " " +
                            players[i].Health + " " +
                            Convert.ToInt32(players[i].Direction)
                        );
                    }

                    //Játéktábla adatainak kiírása soronként
                    Int32 size = tableContent.GetLength(0);

                    for (Int32 i = 0; i < size; ++i)
                    {
                        StringBuilder stringBuilder = new StringBuilder();

                        for (Int32 j = 0; j < size; ++j)
                        {
                            stringBuilder.Append(Convert.ToInt32(tableContent[i, j]));
                            if (j < size - 1) stringBuilder.Append(" ");
                        }

                        await writer.WriteLineAsync(stringBuilder.ToString());
                    }
                }
            }
            catch
            {
                throw new PigBattleDataException();
            }
        }
    }
}
