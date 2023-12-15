using System.Windows;
using System.Data.SQLite;

namespace WPFContactManager
{
    public partial class EditContactWindow : Window
    {
        public string EditedFirstName { get; private set; }

        public EditContactWindow(string FirstName, string LastName, string PhoneNumber, string Email)
        {
            InitializeComponent();

            FirstNameTextBox.Text = FirstName;
            LastNameTextBox.Text = LastName;
            PhoneNumberTextBox.Text = PhoneNumber;
            EmailTextBox.Text = Email;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}