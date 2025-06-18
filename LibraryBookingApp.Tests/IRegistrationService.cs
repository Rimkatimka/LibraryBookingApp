using Microsoft.Data.Sqlite;
using System;

namespace LibraryBookingApp.Services
{
    public interface IRegistrationService
    {
        bool RegisterUser(string username, string password, string lastName, string firstName, string middleName, string address, string phoneNumber, string readerNumber);
        string GenerateUniqueReaderNumber(SqliteConnection connection);
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly string _connectionString = @"Data Source=DataBase.db;Version=3;";

        public bool RegisterUser(string username, string password, string lastName, string firstName, string middleName, string address, string phoneNumber, string readerNumber)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) ||
                string.IsNullOrEmpty(address) || string.IsNullOrEmpty(phoneNumber))
            {
                return false; // Имитация ошибки валидации
            }

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    using (var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM User WHERE Username = @Username", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", username);
                        int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (userCount > 0)
                        {
                            return false; // Пользователь существует
                        }
                    }

                    using (var command = new SqliteCommand(
                        @"INSERT INTO User (Username, Password, LastName, FirstName, MiddleName, ReaderNumber, Address, PhoneNumber) 
                          VALUES (@Username, @Password, @LastName, @FirstName, @MiddleName, @ReaderNumber, @Address, @PhoneNumber)", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@MiddleName", middleName);
                        command.Parameters.AddWithValue("@Address", address);
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        command.Parameters.AddWithValue("@ReaderNumber", readerNumber);
                        command.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (SqliteException)
            {
                return false;
            }
        }

        public string GenerateUniqueReaderNumber(SqliteConnection connection)
        {
            Random random = new Random();
            string readerNumber;
            bool isUnique;

            do
            {
                int randomNumber = random.Next(0, 10000000);
                readerNumber = randomNumber.ToString("0000000");

                using (var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM User WHERE ReaderNumber = @ReaderNumber", connection))
                {
                    checkCommand.Parameters.AddWithValue("@ReaderNumber", readerNumber);
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    isUnique = count == 0;
                }
            }
            while (!isUnique);

            return readerNumber;
        }
    }
}