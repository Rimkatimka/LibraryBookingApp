using System.Data.SQLite;
using System;
using System.Windows;
using LibraryBookingApp;
using LibraryBookingApp.Services;
using Microsoft.Data.Sqlite;


namespace LibraryBookingApp
{
    public partial class RegisterWindow : Window
    {
        private readonly IRegistrationService _registrationService;
        private string _generatedReaderNumber;

        public RegisterWindow(IRegistrationService registrationService = null)
        {
            InitializeComponent();
            _registrationService = registrationService ?? new RegistrationService();
            GenerateReaderNumber();
            ReaderNumberTextBox.Text = _generatedReaderNumber;
            ReaderNumberTextBox.IsReadOnly = true;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegUsernameTextBox.Text;
            string password = RegPasswordBox.Password;
            string lastName = LastNameTextBox.Text;
            string firstName = FirstNameTextBox.Text;
            string middleName = MiddleNameTextBox.Text;
            string address = AddressTextBox.Text;
            string phoneNumber = PhoneNumberTextBox.Text;

            bool success = _registrationService.RegisterUser(username, password, lastName, firstName, middleName, address, phoneNumber, _generatedReaderNumber);

            if (!success)
            {
                MessageBox.Show("Ошибка регистрации: проверьте данные или доступ к базе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void GenerateReaderNumber()
        {
            try
            {
                using (var connection = new SqliteConnection(@"Data Source=DataBase.db;Version=3;"))
                {
                    connection.Open();
                    _generatedReaderNumber = _registrationService.GenerateUniqueReaderNumber(connection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации номера читательского билета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _generatedReaderNumber = "Ошибка";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}