using System;
using System.IO;

namespace HurryKingProbe
{
    public static class GameStorage
    {
        private const string SaveFileName = "savegame.txt";

        public static void SaveGameToFile(GameEngine engine)
        {
            using (StreamWriter writer = new StreamWriter(SaveFileName))
            {
                writer.WriteLine($"{engine.rowsCount};{engine.columnsCount};{engine.firstPlayerTurn}");
                for (int r = 0; r < engine.rowsCount; r++)
                {
                    string[] rowCells = new string[engine.columnsCount];
                    for (int c = 0; c < engine.columnsCount; c++)
                    {
                        rowCells[c] = ((int)engine.gameField[r, c]).ToString();
                    }
                    writer.WriteLine(string.Join(",", rowCells));
                }
            }
        }

        public static GameEngine LoadGameFromFile()
        {
            if (!File.Exists(SaveFileName))
                return null;

            using (StreamReader reader = new StreamReader(SaveFileName))
            {
                string[] metaData = reader.ReadLine().Split(';');
                int rows = int.Parse(metaData[0]);
                int columns = int.Parse(metaData[1]);
                bool currentTurn = bool.Parse(metaData[2]);

                CellState[,] loadedField = new CellState[rows, columns];

                for (int r = 0; r < rows; r++)
                {
                    string[] cells = reader.ReadLine().Split(',');
                    for (int c = 0; c < columns; c++)
                    {
                        loadedField[r, c] = (CellState)int.Parse(cells[c]);
                    }
                }
                return new GameEngine(columns, rows, loadedField, currentTurn);
            }
        }

        public static bool CheckSaveFileExists()
        {
            return File.Exists(SaveFileName);
        }
    }
}
