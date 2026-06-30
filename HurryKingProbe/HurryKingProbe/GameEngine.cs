using System;

namespace HurryKingProbe
{
    public enum CellState
    {
        Empty,
        FirstCastle,
        SecondCastle,
        FirstOwned,
        FirstInfluence,
        SecondOwned,
        SecondInfluence,
        Obstacle,
        Gold
    }

    public class GameEngine
    {
        public int columnsCount;
        public int rowsCount;
        public int firstPlayerScore = 0;
        public int secondPlayerScore = 0;
        public bool firstPlayerTurn = true;
        public bool bonusTurn = false;

        public CellState[,] gameField;
        private Random randomGenerator = new Random();

        public GameEngine(int columns, int rows)
        {
            this.columnsCount = columns;
            this.rowsCount = rows;
            this.gameField = new CellState[rows, columns];
            InitField();
        }

        public GameEngine(int columns, int rows, CellState[,] loadedField, bool currentTurn)
        {
            this.columnsCount = columns;
            this.rowsCount = rows;
            this.gameField = loadedField;
            this.firstPlayerTurn = currentTurn;
            CountScore();
        }

        private void InitField()
        {
            for (int r = 0; r < rowsCount; r++)
                for (int c = 0; c < columnsCount; c++)
                    gameField[r, c] = CellState.Empty;

            gameField[rowsCount / 2, 1] = CellState.FirstCastle;
            gameField[rowsCount / 2, columnsCount - 2] = CellState.SecondCastle;
            gameField[rowsCount / 2, columnsCount / 2] = CellState.Gold;

            for (int i = 0; i < columnsCount / 2; i++)
            {
                int r = randomGenerator.Next(rowsCount);
                int c = randomGenerator.Next(columnsCount);
                if (gameField[r, c] == CellState.Empty)
                    gameField[r, c] = CellState.Obstacle;
            }
        }

        public bool CanBuildCastle(int r, int c)
        {
            if (gameField[r, c] != CellState.Empty &&
                gameField[r, c] != (firstPlayerTurn ? CellState.FirstOwned : CellState.SecondOwned))
                return false;

            CellState castle = firstPlayerTurn
                ? CellState.FirstCastle
                : CellState.SecondCastle;

            for (int dr = -4; dr <= 4; dr++)
                for (int dc = -4; dc <= 4; dc++)
                {
                    int nr = r + dr;
                    int nc = c + dc;
                    if (nr >= 0 && nr < rowsCount && nc >= 0 && nc < columnsCount && gameField[nr, nc] == castle)
                        return true;
                }
            return false;
        }

        public void ProcessPlayerTurn(int rowIndex, int columnIndex)
        {
            BuildCastle(rowIndex, columnIndex, firstPlayerTurn);
            ResolveInfluence();

            if (!bonusTurn)
                firstPlayerTurn = !firstPlayerTurn;

            bonusTurn = false;
            CountScore();
        }

        private void BuildCastle(int r, int c, bool first)
        {
            gameField[r, c] = first
                ? CellState.FirstCastle
                : CellState.SecondCastle;

            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = r + dr;
                    int nc = c + dc;
                    if (nr >= 0 && nr < rowsCount && nc >= 0 && nc < columnsCount)
                    {
                        if (gameField[nr, nc] == CellState.Empty)
                        {
                            gameField[nr, nc] = first
                                ? CellState.FirstInfluence
                                : CellState.SecondInfluence;
                        }
                        if (gameField[nr, nc] == CellState.Gold)
                        {
                            bonusTurn = true;
                        }
                    }
                }
        }

        private void ResolveInfluence()
        {
            for (int r = 0; r < rowsCount; r++)
                for (int c = 0; c < columnsCount; c++)
                {
                    if (gameField[r, c] == CellState.FirstInfluence)
                        gameField[r, c] = CellState.FirstOwned;
                    else if (gameField[r, c] == CellState.SecondInfluence)
                        gameField[r, c] = CellState.SecondOwned;
                }
        }

        public void CountScore()
        {
            firstPlayerScore = 0;
            secondPlayerScore = 0;
            for (int r = 0; r < rowsCount; r++)
                for (int c = 0; c < columnsCount; c++)
                {
                    if (gameField[r, c] == CellState.FirstOwned || gameField[r, c] == CellState.FirstCastle)
                        firstPlayerScore++;
                    if (gameField[r, c] == CellState.SecondOwned || gameField[r, c] == CellState.SecondCastle)
                        secondPlayerScore++;
                }
        }

        public bool IsFieldFull()
        {
            for (int r = 0; r < rowsCount; r++)
                for (int c = 0; c < columnsCount; c++)
                {
                    if (gameField[r, c] == CellState.Empty)
                        return false;
                }
            return true;
        }
    }
}

//taskkill /f /im HurryKingProbe.exe
