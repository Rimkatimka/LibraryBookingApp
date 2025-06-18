using Microsoft.Data.Sqlite;
using LibraryBookingApp.Services;
using Xunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Assert = Xunit.Assert;

namespace LibraryBookingApp.Tests
{
    public class RegistrationServiceTests
    {
        private SqliteConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            // Создаём таблицу User
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE User (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    LastName TEXT,
                    FirstName TEXT,
                    MiddleName TEXT,
                    ReaderNumber TEXT,
                    Address TEXT,
                    PhoneNumber TEXT
                )";
            command.ExecuteNonQuery();

            return connection;
        }

        [Fact]
        public void REG_01_SuccessfulRegistration_ShouldRegisterUser()
        {
            // Arrange
            using var connection = CreateInMemoryDatabase();
            var service = new RegistrationService();
            string username = "testuser";
            string password = "testpass";
            string lastName = "Test";
            string firstName = "User";
            string middleName = "";
            string address = "123 Street";
            string phoneNumber = "1234567890";
            string readerNumber = "0001234";

            // Act
            bool result = service.RegisterUser(username, password, lastName, firstName, middleName, address, phoneNumber, readerNumber);

            // Assert
            Assert.True(result);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM User WHERE Username = @Username AND ReaderNumber = @ReaderNumber";
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@ReaderNumber", readerNumber);
            int count = Convert.ToInt32(command.ExecuteScalar());
            Assert.Equal(1, count);
        }

        [Fact]
        public void REG_03_ExistingUsername_ShouldFailRegistration()
        {
            // Arrange
            using var connection = CreateInMemoryDatabase();
            var service = new RegistrationService();
            string username = "testuser";
            string password = "testpass";
            string lastName = "Test";
            string firstName = "User";
            string middleName = "";
            string address = "123 Street";
            string phoneNumber = "1234567890";
            string readerNumber = "0001234";

            // Регистрируем пользователя первый раз
            service.RegisterUser(username, password, lastName, firstName, middleName, address, phoneNumber, readerNumber);

            // Act
            bool result = service.RegisterUser(username, password, lastName, firstName, middleName, address, phoneNumber, "0005678");

            // Assert
            Assert.False(result);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM User WHERE Username = @Username";
            command.Parameters.AddWithValue("@Username", username);
            int count = Convert.ToInt32(command.ExecuteScalar());
            Assert.Equal(1, count); // Только одна запись
        }

        [Fact]
        public void REG_04_UniqueReaderNumber_ShouldGenerateDifferentNumbers()
        {
            // Arrange
            using var connection = CreateInMemoryDatabase();
            var service = new RegistrationService();

            // Act
            string readerNumber1 = service.GenerateUniqueReaderNumber(connection);
            string readerNumber2 = service.GenerateUniqueReaderNumber(connection);

            // Assert
            Assert.NotEqual(readerNumber1, readerNumber2);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM User WHERE ReaderNumber = @ReaderNumber";
            command.Parameters.AddWithValue("@ReaderNumber", readerNumber1);
            int count1 = Convert.ToInt32(command.ExecuteScalar());
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@ReaderNumber", readerNumber2);
            int count2 = Convert.ToInt32(command.ExecuteScalar());
            Assert.Equal(0, count1); // Номера не сохранены, но уникальны
            Assert.Equal(0, count2);
        }
    }
}