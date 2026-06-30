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
    public partial class StartScreen : Form
    {
        public StartScreen()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int columns = Convert.ToInt32(ColumnsCount.Text);
            int rows = Convert.ToInt32(RowsCount.Text);

            GameField gameField = new GameField(columns, rows);
            gameField.Show();
            this.Hide();
        }

        private void btnLoadGame_Click(object sender, EventArgs e)
        {
            if (!File.Exists("savegame.txt"))
            {
                MessageBox.Show("Файл сохранения не найден!", "Ошибка");
                return;
            }

            StreamReader reader = new StreamReader("savegame.txt");
            string[] metaData = reader.ReadLine().Split(';');
            int savedRows = Convert.ToInt32(metaData[0]);
            int savedColumns = Convert.ToInt32(metaData[1]);
            bool savedTurn = Convert.ToBoolean(metaData[2]);

            int[,] savedField = new int[savedRows, savedColumns];
            for (int r = 0; r < savedRows; r++)
            {
                string[] cells = reader.ReadLine().Split(',');
                for (int c = 0; c < savedColumns; c++)
                {
                    savedField[r, c] = Convert.ToInt32(cells[c]);
                }
            }
            reader.Close();

            GameField gameField = new GameField(savedColumns, savedRows, savedField, savedTurn);
            gameField.Show();
            this.Hide();
        }
    }
}
