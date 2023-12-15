using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using Microsoft.Win32;

namespace WPFContactManager
{
    public partial class MainWindow : Window
    {
        private int currentContactIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            InitializeDatabase();
        }

        public static SQLiteConnection GetFileDatabaseConnection()
        {
            SQLiteConnection connection = new SQLiteConnection("Data Source=rolodex.db");
            
            connection.Open();

            return connection;
        }

        private void InitializeDatabase()
        {
            using (var connection = GetFileDatabaseConnection())
            {
                string sqlStatement = @"SELECT count(name) 
                                        FROM sqlite_master
                                        WHERE (type = 'table' AND name = 'Contacts')";
                var cmdCheck = new SQLiteCommand(sqlStatement, connection);

                if ((long)cmdCheck.ExecuteScalar() == 0)
                {
                    using (var cmd = new SQLiteCommand(connection))
                    {
                        sqlStatement = @"CREATE TABLE Contacts (
                                        ID INTEGER PRIMARY KEY,
                                        FirstName TEXT,
                                        LastName TEXT,
                                        PhoneNumber TEXT,
                                        Email TEXT)";

                        cmd.CommandText = sqlStatement;
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            LoadContacts();
        }

        private void LoadContacts()
        {
            ContactList.Items.Clear();

            using (var connection = GetFileDatabaseConnection())
            {
                var command = new SQLiteCommand(@"SELECT * FROM Contacts", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ContactList.Items.Add(reader["FirstName"].ToString() + " " + reader["LastName"].ToString());

                    }
                }
            }

            if (ContactList.Items.Count > 0)
            {
                ContactList.SelectedIndex = 0;
            }
        }

        private void AddContact(string FirstName, string LastName, string PhoneNumber, string Email)
        {
            using (var connection = GetFileDatabaseConnection())
            {
                string sqlStatement = @"INSERT INTO Contacts (FirstName, LastName, PhoneNumber, email)
                                        VALUES (@FirstName, @LastName, @PhoneNumber, @Email)";

                using (var cmd = new SQLiteCommand(sqlStatement, connection))
                {
                    cmd.Parameters.AddWithValue("@FirstName", FirstName);
                    cmd.Parameters.AddWithValue("@LastName", LastName);
                    cmd.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                    cmd.Parameters.AddWithValue("@Email", Email);

                    cmd.ExecuteNonQuery();
                }
            }

            LoadContacts();
        }

        private void CheckSubmit(string FirstName, string LastName, string PhoneNumber, string Email)
        {
            if (FirstName != "" && LastName != "" && PhoneNumber != "" && Email != "")
            {
                AddContact(FirstName, LastName, PhoneNumber, Email);
            }
        }

        private void SubmitContact_Click(object sender, RoutedEventArgs e)
        {
            CheckSubmit(FirstName.Text, LastName.Text, PhoneNumber.Text, Email.Text);
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (currentContactIndex > 0)
            {
                currentContactIndex--;
                ContactList.SelectedIndex = currentContactIndex;
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ContactList.SelectedIndex >= 0)
            {
                using (var connection = GetFileDatabaseConnection())
                {
                    var command = new SQLiteCommand(@"SELECT * FROM Contacts WHERE ID = @id", connection);
                    command.Parameters.AddWithValue("@id", GetContactId(ContactList.SelectedIndex));

                    int contactId = 0;
                    string FirstName = "";
                    string LastName = "";
                    string PhoneNumber = "";
                    string Email = "";

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            contactId = Convert.ToInt32(reader["ID"]);

                            var editWindow = new EditContactWindow(reader["FirstName"].ToString(), reader["LastName"].ToString(), reader["PhoneNumber"].ToString(), reader["Email"].ToString());
                            editWindow.ShowDialog();

                            FirstName = editWindow.FirstNameTextBox.Text;
                            LastName = editWindow.LastNameTextBox.Text;
                            PhoneNumber = editWindow.PhoneNumberTextBox.Text;
                            Email = editWindow.EmailTextBox.Text;
                        } 
                    }

                    string sqlStatement = @"UPDATE Contacts
                                SET FirstName = @FirstName, 
                                    LastName = @LastName, 
                                    PhoneNumber = @PhoneNumber, 
                                    Email = @Email
                                WHERE ID = @ID";

                    using (var cmd = new SQLiteCommand(sqlStatement, connection))
                    {
                        cmd.Parameters.AddWithValue("@ID", contactId);  // ID is a primary key, so it's required for updating a specific row
                        cmd.Parameters.AddWithValue("@FirstName", FirstName);
                        cmd.Parameters.AddWithValue("@LastName", LastName);
                        cmd.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                        cmd.Parameters.AddWithValue("@Email", Email);

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadContacts();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (currentContactIndex < ContactList.Items.Count - 1)
            {
                currentContactIndex++;
                ContactList.SelectedIndex = currentContactIndex;
            }
        }

        private void DeleteContact(int contactId)
        {
            using (var connection = GetFileDatabaseConnection())
            {
                string sqlStatement = "DELETE FROM Contacts WHERE ID = @contactId";
                using (var cmd = new SQLiteCommand(sqlStatement, connection))
                {
                    cmd.Parameters.AddWithValue("@contactId", contactId);
                    cmd.ExecuteNonQuery();
                }
            }

            LoadContacts();
        }

        private int GetContactId(int selectedIndex)
        {
            using (var connection = GetFileDatabaseConnection())
            {
                var command = new SQLiteCommand("SELECT ID FROM Contacts LIMIT 1 OFFSET @index", connection);
                command.Parameters.AddWithValue("@index", selectedIndex);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ContactList.SelectedIndex >= 0)
            {
                int contactId = GetContactId(ContactList.SelectedIndex);

                DeleteContact(contactId);

                if (currentContactIndex >= ContactList.Items.Count)
                {
                    currentContactIndex = ContactList.Items.Count - 1;
                }

                ContactList.SelectedIndex = currentContactIndex;
            }
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            if (ContactList.SelectedIndex >= 0)
            {
                using (var connection = GetFileDatabaseConnection())
                {
                    var command = new SQLiteCommand(@"SELECT * FROM Contacts WHERE ID = @id", connection);
                    command.Parameters.AddWithValue("@id", GetContactId(ContactList.SelectedIndex));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var viewWindow = new ViewWindow(reader["FirstName"].ToString(), reader["LastName"].ToString(), reader["PhoneNumber"].ToString(), reader["Email"].ToString());
                            viewWindow.ShowDialog();
                        }
                    }
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files|*.csv",
                Title = "Save Database",
                DefaultExt = "csv",
            };

            if(saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                ExportDatabaseToText(filePath);

                MessageBox.Show("Database exported successfully.", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportDatabaseToText(string filePath)
        {
            using (var connection = GetFileDatabaseConnection())
            {
                var command = new SQLiteCommand("SELECT * FROM Contacts", connection);

                using (var reader = command.ExecuteReader())
                {
                    using (var writer = new System.IO.StreamWriter(filePath))
                    {
                        writer.WriteLine("ID,First Name,Last Name,Phone,Email");

                        while(reader.Read())
                        {
                            writer.WriteLine($"{reader["ID"]},{reader["FirstName"]},{reader["LastName"]},{reader["PhoneNumber"]},{reader["Email"]}");
                        }
                    }
                }
            }
        }
    }
}
