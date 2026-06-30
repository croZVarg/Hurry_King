using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace HurryKingProbe
{
    public partial class GameField : Form
    {
        int columns;
        int rows;
        int FirstPlayerScore = 0;
        int SecondPlayerScore = 0;
        Random rnd = new Random();
        bool firstPlayerTurn = true;
        bool bonusTurn = false;

        enum CellState
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

        CellState[,] field;

        public GameField(int columns, int rows)
        {
            InitializeComponent();
            this.columns = columns;
            this.rows = rows;
            this.SizeChanged += GameField_SizeChanged;
        }

        public GameField(int columns, int rows, int[,] savedField, bool savedTurn)
        {
            InitializeComponent();
            this.columns = columns;
            this.rows = rows;
            this.firstPlayerTurn = savedTurn;
            this.SizeChanged += GameField_SizeChanged;

            field = new CellState[rows, columns];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    field[r, c] = (CellState)savedField[r, c];
        }

        void InitField()
        {
            if (field == null)
            {
                field = new CellState[rows, columns];
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++)
                        field[r, c] = CellState.Empty;

                field[rows / 2, 1] = CellState.FirstCastle;
                field[rows / 2, columns - 2] = CellState.SecondCastle;
                field[rows / 2, columns / 2] = CellState.Gold;

                for (int i = 0; i < columns / 2; i++)
                {
                    int r = rnd.Next(rows);
                    int c = rnd.Next(columns);
                    if (field[r, c] == CellState.Empty)
                        field[r, c] = CellState.Obstacle;
                }
            }
        }

        void DrawField()
        {
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    var cell = BattleField.Rows[r].Cells[c];
                    cell.Value = null;
                    cell.Style.BackColor = Color.White;

                    switch (field[r, c])
                    {
                        case CellState.Empty:
                            cell.Style.BackColor = Color.White;
                            break;

                        case CellState.Obstacle:
                            cell.Style.BackColor = Color.LightSlateGray;
                            break;

                        case CellState.Gold:
                            cell.Style.BackColor = Color.Khaki;
                            break;

                        case CellState.FirstOwned:
                        case CellState.FirstCastle:
                            cell.Style.BackColor = Color.SteelBlue;
                            break;

                        case CellState.SecondOwned:
                        case CellState.SecondCastle:
                            cell.Style.BackColor = Color.IndianRed;
                            break;

                        case CellState.FirstInfluence:
                            cell.Style.BackColor = Color.PaleGreen;
                            break;

                        case CellState.SecondInfluence:
                            cell.Style.BackColor = Color.LightPink;
                            break;
                    }
                }
            CheckGameEnd();
        }

        void UpdateTitle()
        {
            string turn = firstPlayerTurn ? "Ход первого" : "Ход второго игрока";
            Text = $"{turn} | Первый:{FirstPlayerScore} Второй: {SecondPlayerScore}";
        }

        bool CanBuildCastle(int r, int c)
        {
            if (field[r, c] == CellState.Obstacle || field[r, c] == CellState.Gold ||
                field[r, c] == CellState.FirstCastle || field[r, c] == CellState.SecondCastle ||
                field[r, c] == CellState.FirstOwned || field[r, c] == CellState.SecondOwned)
                return false;

            CellState castle = firstPlayerTurn ? CellState.FirstCastle : CellState.SecondCastle;
            CellState owned = firstPlayerTurn ? CellState.FirstOwned : CellState.SecondOwned;

            for (int dr = -4; dr <= 4; dr++)
                for (int dc = -4; dc <= 4; dc++)
                {
                    int nr = r + dr;
                    int nc = c + dc;
                    if (nr >= 0 && nr < rows && nc >= 0 && nc < columns)
                    {
                        if (field[nr, nc] == castle || field[nr, nc] == owned)
                            return true;
                    }
                }
            return false;
        }

        void BuildCastle(int r, int c, bool first)
        {
            field[r, c] = first ? CellState.FirstCastle : CellState.SecondCastle;

            CellState enemyInfluence = first ? CellState.SecondInfluence : CellState.FirstInfluence;
            CellState myInfluence = first ? CellState.FirstInfluence : CellState.SecondInfluence;

            int convertedEnemyInfluencesCount = 0;

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int nr = r + dr;
                    int nc = c + dc;

                    if (nr >= 0 && nr < rows && nc >= 0 && nc < columns)
                    {
                        if (field[nr, nc] == enemyInfluence && convertedEnemyInfluencesCount < 2)
                        {
                            field[nr, nc] = myInfluence;
                            convertedEnemyInfluencesCount++;
                        }
                    }
                }
            }

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int nr = r + dr;
                    int nc = c + dc;

                    if (nr >= 0 && nr < rows && nc >= 0 && nc < columns)
                    {

                        if (field[nr, nc] == CellState.Empty)
                        {
                            field[nr, nc] = myInfluence;
                        }

                        if (field[nr, nc] == CellState.Gold)
                        {
                            bonusTurn = true;
                        }
                    }
                }
            }
        }

        void ResolveInfluence()
        {
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    if (firstPlayerTurn && field[r, c] == CellState.FirstInfluence)
                        field[r, c] = CellState.FirstOwned;

                    if (!firstPlayerTurn && field[r, c] == CellState.SecondInfluence)
                        field[r, c] = CellState.SecondOwned;
                }
        }

        void CountScore()
        {
            FirstPlayerScore = 0;
            SecondPlayerScore = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    if (field[r, c] == CellState.FirstOwned || field[r, c] == CellState.FirstCastle)
                        FirstPlayerScore++;
                    if (field[r, c] == CellState.SecondOwned || field[r, c] == CellState.SecondCastle)
                        SecondPlayerScore++;
                }
        }

        void BattleField_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            if (!CanBuildCastle(e.RowIndex, e.ColumnIndex))
                return;

            DialogResult result = MessageBox.Show(
                "Построить здесь замок, милорд?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            ResolveInfluence();
            BuildCastle(e.RowIndex, e.ColumnIndex, firstPlayerTurn);

            if (!bonusTurn)
                firstPlayerTurn = !firstPlayerTurn;

            bonusTurn = false;
            UpdateTitle();
            CountScore();
            DrawField();
        }

        void GameField_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < columns; i++)
            {
                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
                imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imageColumn.DefaultCellStyle.NullValue = null;
                imageColumn.Description = "";

                BattleField.Columns.Add(imageColumn);
            }

            BattleField.RowCount = rows;
            BattleField.AllowUserToAddRows = false;
            BattleField.AllowUserToResizeColumns = false;
            BattleField.AllowUserToResizeRows = false;
            BattleField.RowHeadersVisible = false;
            BattleField.ColumnHeadersVisible = false;

            InitField();
            DrawField();
            UpdateTitle();
            BattleField.CellClick += BattleField_CellClick;
        }

        private void saveMenuButton_Click(object sender, EventArgs e)
        {
            StreamWriter writer = new StreamWriter("savegame.txt");
            writer.WriteLine(rows + ";" + columns + ";" + firstPlayerTurn);
            for (int r = 0; r < rows; r++)
            {
                string rowLine = "";
                for (int c = 0; c < columns; c++)
                {
                    rowLine += (int)field[r, c];
                    if (c < columns - 1) rowLine += ",";
                }
                writer.WriteLine(rowLine);
            }
            writer.Close();
            MessageBox.Show("Игра успешно сохранена!", "Сохранение");
        }

        private void menuStrip1_MouseEnter(object sender, EventArgs e)
        {
            menuStrip1.Height = 25;
            menuStrip1.BackColor = Color.White;
        }

        private void menuStrip1_MouseLeave(object sender, EventArgs e)
        {
            menuStrip1.Height = 2;
            menuStrip1.BackColor = Color.FromArgb(245, 245, 245);
        }

        void GameField_SizeChanged(object sender, EventArgs e)
        {
            if (this.ClientSize.Height / rows >= 10)
                BattleField.RowTemplate.Height = this.ClientSize.Height / rows;
            else
                BattleField.RowTemplate.Height = 10;

            foreach (DataGridViewColumn column in BattleField.Columns)
                if (this.ClientSize.Width / columns >= 20)
                    column.Width = this.ClientSize.Width / columns;
                else
                    column.Width = 20;
        }

        void CheckGameEnd()
        {
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    if (field[r, c] == CellState.Empty ||
                        field[r, c] == CellState.FirstInfluence ||
                        field[r, c] == CellState.SecondInfluence)
                        return;
                }
            string winner;
            if (FirstPlayerScore > SecondPlayerScore)
                winner = "Победил Игрок 1";
            else
                if (SecondPlayerScore > FirstPlayerScore)
                winner = "Победил Игрок 2";
            else
                winner = "Ничья";
            MessageBox.Show(winner, "Игра окончена");
            this.Close();
        }
    }
}