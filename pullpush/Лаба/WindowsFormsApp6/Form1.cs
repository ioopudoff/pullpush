using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace WindowsFormsApp6
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        int AmountOfNotes; 

        private void Form1_Load(object sender, EventArgs e)
        {
            AddColumns();

            var connection = Connect();

            var total = GetTotal(connection);            
            
            GetTable(connection, total);

            AmountOfNotes = total;
        }

        private void AddColumns() //add columns
        {
            data.Columns.Add("Code", "Код");
            data.Columns.Add("Surname", "Фамилия");
            data.Columns.Add("Name", "Имя");
            data.Columns.Add("Secondname", "Отчество");
            data.Columns.Add("Street", "Код улицы");
            data.Columns.Add("House", "Номер дома");
            data.Columns.Add("Fraction", "Дробная часть");
            data.Columns.Add("Phone", "Телефон");

            data.Columns[0].ReadOnly = true;
        }

        private OleDbConnection Connect() // connect DB
        {
            return new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source =" + @"БД.mdb");
        }

        private int GetTotal(OleDbConnection connection) //count of notes in DB
        {
            OleDbCommand command = connection.CreateCommand();

            command.CommandText = "select count(*) from Владельцы";

            connection.Open();
            int total = Convert.ToInt32(command.ExecuteScalar());
            connection.Close();

            return total;
        }

        private void GetTable(OleDbConnection connection, int total) //record table from DB in DataGridView
        {
            OleDbCommand command = connection.CreateCommand();

            command.CommandText = "select * from Владельцы";
            connection.Open();
            OleDbDataReader reader = command.ExecuteReader();
            int i = 0;
            string[,] A = new string[8, total];
            try
            {
                while (reader.Read())
                {
                    data.Rows.Add();
                    data[0, i].Value = reader["Код"];
                    data[1, i].Value = reader["Фамилия"];
                    data[2, i].Value = reader["Имя"];
                    data[3, i].Value = reader["Отчество"];
                    data[4, i].Value = reader["Код_улицы"];
                    data[5, i].Value = reader["Номер_дома"];
                    data[6, i].Value = reader["Дробная_часть_номера"];
                    data[7, i].Value = reader["Телефон"];
                    ++i;
                }
            }
            finally
            {
                reader.Close();
                connection.Close();
            }
        }

        private void Add_Click(object sender, EventArgs e) 
        {
            var id = data.CurrentCell.RowIndex;

            if (id < AmountOfNotes)
            {
                MessageBox.Show("Выберите пустую строку!");
                return;
            }

            if (!EmptyCheck(id))
            {
                MessageBox.Show("Не все столбцы заполнены!");
                return;
            }

            if (!ValueCheck(id))
                return;

            AddNote(id);

            MessageBox.Show("Успешно добавлено в таблицу");
        }

        private void Update_Click(object sender, EventArgs e) 
        {
            var id = data.CurrentCell.RowIndex;

            if (id >= AmountOfNotes)
            {
                MessageBox.Show("Выберите заполненную строку!");
                return;
            }

            if (!EmptyCheck(id))
            {
                MessageBox.Show("Заполните все ячейки!");
                return;
            }

            if (!ValueCheck(id))
            
            return;

            UpdateNote(id);

        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var id = data.CurrentCell.RowIndex;

            if (id >= AmountOfNotes)
            {
                MessageBox.Show("Пустая строка! Выберите заполненную");
                return;
            }

            DeleteNote(id);

            MessageBox.Show("Запись была удалена из таблицы");
        }


        private bool EmptyCheck(int rowIndex) //check cells on (zapoln)
        {
            for (int i = 1; i < 7; i++)
                if (data[i, rowIndex].Value == null || data[i, rowIndex].Value.ToString() == "")
                    return false;
            if (data[7, rowIndex].Value == null || data[7, rowIndex].Value.ToString() == "")
                return false;
            return true;
        }

        private bool ValueCheck(int rowIndex) //check on including values
        {
            int[] strNotes = { 1, 2, 3 };
            char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            foreach (int i in strNotes)
            {
                string value = data[i, rowIndex].Value.ToString();

                if (value.Any(symbol => symbol == numbers[0] || symbol == numbers[1] || symbol == numbers[2] || symbol == numbers[3] || symbol == numbers[4] || symbol == numbers[5] || symbol == numbers[6] || symbol == numbers[7] || symbol == numbers[8] || symbol == numbers[9]))
                {
                    MessageBox.Show("Ячейка [" + i + ", " + rowIndex + "] не может содержать цифр!");
                    return false;
                }
            }
            //дописать цикл, где появляется возможность любое нат число записывать( как код улицы, так и номер дома) 
            string street = data[4, rowIndex].Value.ToString();
            string house = data[5, rowIndex].Value.ToString();
            int[] sum = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
            if ((street != "1") && (street != "2") && (street != "3") && (street != "4") && (street != "5") && (street != "6") && (street != "7") && (street != "8") && (street != "9") && (street != "10") && (street != "11") && (street != "12") && (street != "13"))
            {
                MessageBox.Show("Код улицы является числом");
                return false;
            }

            if (!house.Any(symbol => symbol == numbers[0] || symbol == numbers[1] || symbol == numbers[2] || symbol == numbers[3] || symbol == numbers[4] || symbol == numbers[5] || symbol == numbers[6] || symbol == numbers[7] || symbol == numbers[8] || symbol == numbers[9]))
            {
                MessageBox.Show("Номер дома состоит только из цифр!");
                return false;
            }

            return true;
        }

        private void AddNote(int rowIndex)
        {
            var connection = Connect();
            var command = Command(connection);

            int newCode = Convert.ToInt32(data[0, rowIndex - 1].Value) + 1;

            command.CommandText = "insert into Владельцы (Код,Фамилия, Имя, Отчество, Код_улицы, Номер_дома, Дробная_часть_номера, Телефон) values (@cod,@family,@name,@secondname,@codstreet,@numberhouse,@drobnayaChast,@phone)";

            command.Parameters["@cod"].Value = newCode;
            command.Parameters["@family"].Value = data[1, rowIndex].Value.ToString();
            command.Parameters["@name"].Value = data[2, rowIndex].Value.ToString();
            command.Parameters["@secondname"].Value = data[3, rowIndex].Value.ToString();
            command.Parameters["@codstreet"].Value = data[4, rowIndex].Value.ToString();
            command.Parameters["@numberhouse"].Value = data[5, rowIndex].Value.ToString();
            command.Parameters["@drobnayaChast"].Value = data[6, rowIndex].Value.ToString();
            command.Parameters["@phone"].Value = data[7, rowIndex].Value.ToString();

            Request(connection, command);
            AmountOfNotes++;
        }

        private void UpdateNote(int rowIndex)
        {
            var connection = Connect();
            var command = Command(connection);

            int kod = Convert.ToInt32(data[0, rowIndex].Value);

            string[] values = new string[8];
            for (int i = 0; i < 8; i++)
                values[i] = data[i, rowIndex].Value.ToString();

            command.CommandText = "update Владельцы set Фамилия=\'" + values[1] + "\', Имя=\'" + values[2] + "\', Отчество=\'" + values[3] + "\', Код_улицы=\'" + values[4] + "\', Номер_дома=\'" + values[5] + "\', Дробная_часть_номера=\'" + values[6] + "\', Телефон=\'" + values[7] + "\' where Код=@cod";
            command.Parameters["@cod"].Value = kod;

            Request(connection, command);
        }

        private void DeleteNote(int rowIndex)
        {
            int kod = Convert.ToInt32(data[0, rowIndex].Value);
            data.Rows.Remove(data.Rows[rowIndex]);

            var connection = Connect();
            var command = Command(connection);

            command.CommandText = "delete from Владельцы where Код=@cod";
            command.Parameters["@cod"].Value = kod;

            Request(connection, command);

            AmountOfNotes--;
        }

        private void data_CellClick(object sender, DataGridViewCellEventArgs e) //add cells for recording
        {
            if (data.CurrentCell.Value == null)
            {
                data.Rows.Add();
                data[0, AmountOfNotes].Selected = true;
                for (int i = 0; i < 8; i++)
                    data[i, AmountOfNotes].Value = "";
                data.CurrentCell.Selected = false;
            }
        }


        private OleDbCommand Command(OleDbConnection connection)
        {
            OleDbCommand command = connection.CreateCommand();
            command.Parameters.Add("@cod", OleDbType.Integer);
            command.Parameters.Add("@family", OleDbType.VarChar);
            command.Parameters.Add("@name", OleDbType.VarChar);
            command.Parameters.Add("@secondname", OleDbType.VarChar);
            command.Parameters.Add("@codstreet", OleDbType.Integer);
            command.Parameters.Add("@numberhouse", OleDbType.Integer);
            command.Parameters.Add("@drobnayaChast", OleDbType.VarChar);
            command.Parameters.Add("@phone", OleDbType.VarChar);

            return command;
        }

        private void Request(OleDbConnection connection, OleDbCommand command)
        {
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
