using DSR.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace DSR
{
    public static class DatabaseHelper
    {
        private static string DbPath = Path.Combine(FileSystem.AppDataDirectory, "Fitness.db");

        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var cmd = new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    Surname TEXT,
                    Email TEXT UNIQUE,
                    PasswordHash TEXT,
                    DateOfBirth TEXT,
                    Age INTEGER,
                    Country TEXT,
                    EMSO TEXT,
                    PlaceOfBirth TEXT
                );
                CREATE TABLE IF NOT EXISTS Roles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE
                );
                CREATE TABLE IF NOT EXISTS UserRoles (
                    UserId INTEGER,
                    RoleId INTEGER,
                    PRIMARY KEY(UserId, RoleId),
                    FOREIGN KEY(UserId) REFERENCES Users(Id),
                    FOREIGN KEY(RoleId) REFERENCES Roles(Id)
                );
                 CREATE TABLE IF NOT EXISTS Fitness (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    Location TEXT,
                    NumberOfSubscribers INTEGER,
                    AverageNumberOfPeople INTEGER
                );


            ", connection);

            cmd.ExecuteNonQuery();


        }

        public static void InitializeRolesAndAdminUser()
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            // Create roles
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                var roleCmd = new SqliteCommand("INSERT OR IGNORE INTO Roles (Name) VALUES (@Name)", connection);
                roleCmd.Parameters.AddWithValue("@Name", role);
                roleCmd.ExecuteNonQuery();
            }

            // Check if an admin exists and create one if not
            var checkAdminCmd = new SqliteCommand("SELECT COUNT(*) FROM Users INNER JOIN UserRoles ON Users.Id = UserRoles.UserId INNER JOIN Roles ON UserRoles.RoleId = Roles.Id WHERE Roles.Name = 'Admin'", connection);
            int adminCount = Convert.ToInt32(checkAdminCmd.ExecuteScalar());
            if (adminCount == 0)
            {
                // Add admin user
                var adminUserCmd = new SqliteCommand("INSERT INTO Users (Name, Surname, Email, PasswordHash) VALUES ('Admin', 'User', 'admin@example.com', @PasswordHash)", connection);
                adminUserCmd.Parameters.AddWithValue("@PasswordHash", BCrypt.Net.BCrypt.HashPassword("securepassword"));
                adminUserCmd.ExecuteNonQuery();

                int userId = Convert.ToInt32(new SqliteCommand("SELECT last_insert_rowid()", connection).ExecuteScalar());
                int roleId = Convert.ToInt32(new SqliteCommand("SELECT Id FROM Roles WHERE Name = 'Admin'", connection).ExecuteScalar());

                // Assign role to user
                var assignRoleCmd = new SqliteCommand("INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)", connection);
                assignRoleCmd.Parameters.AddWithValue("@UserId", userId);
                assignRoleCmd.Parameters.AddWithValue("@RoleId", roleId);
                assignRoleCmd.ExecuteNonQuery();
            }
        }

        public static async Task<bool> CheckUserEmailExistsAsync(string email)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();

            var cmd = new SqliteCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", connection);
            cmd.Parameters.AddWithValue("@Email", email);
            int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return result > 0;
        }


        public static async Task AddUserAsync(User user)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();

            // Validate that the email is not null or empty
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentNullException(nameof(user.Email), "Email cannot be null or empty");
            }

            // Insert the new user into the Users table
            var cmd = new SqliteCommand("INSERT INTO Users (Name, Surname, Email, PasswordHash, DateOfBirth, Age, Country, EMSO, PlaceOfBirth) VALUES (@Name, @Surname, @Email, @PasswordHash, @DateOfBirth, @Age, @Country, @EMSO, @PlaceOfBirth)", connection);

            cmd.Parameters.AddWithValue("@Name", user.Name ?? "");
            cmd.Parameters.AddWithValue("@Surname", user.Surname ?? "");
            cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? "");
            cmd.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Age", user.Age);
            cmd.Parameters.AddWithValue("@Country", user.Country ?? "");
            cmd.Parameters.AddWithValue("@EMSO", user.EMSO ?? "");
            cmd.Parameters.AddWithValue("@PlaceOfBirth", user.PlaceOfBirth ?? "");

            await cmd.ExecuteNonQueryAsync();

            // Get the last inserted user ID
            int userId = Convert.ToInt32(new SqliteCommand("SELECT last_insert_rowid()", connection).ExecuteScalar());

            // Retrieve the Role ID for the "User" role
            var roleCmd = new SqliteCommand("SELECT Id FROM Roles WHERE Name = 'User'", connection);
            int roleId = Convert.ToInt32(await roleCmd.ExecuteScalarAsync());

            // Assign the "User" role to the new user
            var assignRoleCmd = new SqliteCommand("INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)", connection);
            assignRoleCmd.Parameters.AddWithValue("@UserId", userId);
            assignRoleCmd.Parameters.AddWithValue("@RoleId", roleId);
            await assignRoleCmd.ExecuteNonQueryAsync();
        }



        public static async Task<string> GetUserRoleAsync(string email)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();

            // SQL query to join the Users, UserRoles, and Roles tables
            var cmd = new SqliteCommand(@"
        SELECT Roles.Name
        FROM Users
        INNER JOIN UserRoles ON Users.Id = UserRoles.UserId
        INNER JOIN Roles ON UserRoles.RoleId = Roles.Id
        WHERE Users.Email = @Email", connection);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                // Retrieve and return the role name
                return reader.GetString(0);
            }

            // Return null if the user has no assigned role
            return null;
        }

        public static async Task DeleteUserAsync(User user)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var deleteRolesCmd = new SqliteCommand("DELETE FROM UserRoles WHERE UserId = @UserId", connection, transaction); // Associate with transaction
                deleteRolesCmd.Parameters.AddWithValue("@UserId", user.Id);
                await deleteRolesCmd.ExecuteNonQueryAsync();

                var deleteUserCmd = new SqliteCommand("DELETE FROM Users WHERE Id = @Id", connection, transaction); // Associate with transaction
                deleteUserCmd.Parameters.AddWithValue("@Id", user.Id);
                await deleteUserCmd.ExecuteNonQueryAsync();

                transaction.Commit(); // Commit transaction if all deletions are successful
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Roll back if any errors occurred
                Debug.WriteLine($"Error deleting user: {ex.Message}");
                throw; // Rethrow the exception to handle it further up the call stack or log it appropriately
            }
        }



        // DatabaseHelper.cs

        public static async Task EditUserAsync(User user)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();

            var cmd = new SqliteCommand(@"UPDATE Users SET 
        Name = @Name, 
        Surname = @Surname, 
        Email = @Email, 
        DateOfBirth = @DateOfBirth, 
        Age = @Age, 
        Country = @Country, 
        EMSO = @EMSO, 
        PlaceOfBirth = @PlaceOfBirth 
        WHERE Id = @Id", connection);

            cmd.Parameters.AddWithValue("@Id", user.Id);
            cmd.Parameters.AddWithValue("@Name", user.Name);
            cmd.Parameters.AddWithValue("@Surname", user.Surname);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Age", user.Age);
            cmd.Parameters.AddWithValue("@Country", user.Country);
            cmd.Parameters.AddWithValue("@EMSO", user.EMSO);
            cmd.Parameters.AddWithValue("@PlaceOfBirth", user.PlaceOfBirth);

            await cmd.ExecuteNonQueryAsync();
        }


        public static async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            try
            {
                using var connection = new SqliteConnection($"Data Source={DbPath}");
                await connection.OpenAsync();

                Debug.WriteLine($"Database Path: {DbPath}");

                // Add a WHERE clause to exclude users with NULL DateOfBirth
                var cmd = new SqliteCommand(@"
            SELECT Id, Name, Surname, Email, DateOfBirth, Age, Country, EMSO, PlaceOfBirth 
            FROM Users 
            WHERE DateOfBirth IS NOT NULL", connection);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var user = new User
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Surname = reader.GetString(2),
                        Email = reader.GetString(3),
                        DateOfBirth = DateTime.Parse(reader.GetString(4)),
                        Age = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                        Country = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        EMSO = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        PlaceOfBirth = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                    };
                    users.Add(user);
                }
                Debug.WriteLine($"Loaded {users.Count} users into the collection.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching users: {ex.Message}");
            }
            return users;
        }

        public static async Task<bool> ValidateUserAsync(string email, string password)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();

            // Query the database for the user record by email
            var cmd = new SqliteCommand("SELECT PasswordHash FROM Users WHERE Email = @Email", connection);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                // Retrieve the stored password hash
                string storedHash = reader.GetString(0);

                // Use BCrypt to safely compare the entered password with the stored hash
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }

            // Return false if no user found or password doesn't match
            return false;
        }

        public static async Task AddFitnessAsync(Fitness fitness)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            try
            {
                await connection.OpenAsync();
                var cmd = new SqliteCommand("INSERT INTO Fitness (Name, Location, NumberOfSubscribers, AverageNumberOfPeople) VALUES (@Name, @Location, @NumberOfSubscribers, @AverageNumberOfPeople)", connection);
                cmd.Parameters.AddWithValue("@Name", fitness.Name);
                cmd.Parameters.AddWithValue("@Location", fitness.Location);
                cmd.Parameters.AddWithValue("@NumberOfSubscribers", fitness.NumberOfSubscribers);
                cmd.Parameters.AddWithValue("@AverageNumberOfPeople", fitness.AverageNumberOfPeople);
                await cmd.ExecuteNonQueryAsync();
                Debug.WriteLine("Fitness center added successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding fitness center: {ex.Message}");
            }
        }



        public static async Task UpdateFitnessAsync(Fitness fitness)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();

            var cmd = new SqliteCommand(@"
        UPDATE Fitness SET
            Name = @Name,
            Location = @Location,
            NumberOfSubscribers = @NumberOfSubscribers,
            AverageNumberOfPeople = @AverageNumberOfPeople
        WHERE Id = @Id", connection);

            cmd.Parameters.AddWithValue("@Name", fitness.Name);
            cmd.Parameters.AddWithValue("@Location", fitness.Location);
            cmd.Parameters.AddWithValue("@NumberOfSubscribers", fitness.NumberOfSubscribers);
            cmd.Parameters.AddWithValue("@AverageNumberOfPeople", fitness.AverageNumberOfPeople);
            cmd.Parameters.AddWithValue("@Id", fitness.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task DeleteFitnessAsync(Fitness fitness)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Ensure the command knows about the transaction
                var cmd = new SqliteCommand("DELETE FROM Fitness WHERE Id = @Id", connection, transaction); // Added the transaction here
                cmd.Parameters.AddWithValue("@Id", fitness.Id);
                await cmd.ExecuteNonQueryAsync();
                transaction.Commit(); // Commit if no errors
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Roll back changes on error
                Debug.WriteLine($"Transaction Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete fitness center: " + ex.Message, "OK");
            }
        }



        public static async Task EditFitnessAsync(Fitness fitness)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();
            var command = new SqliteCommand("UPDATE Fitness SET Name=@Name, Location=@Location, NumberOfSubscribers=@Subscribers, AverageNumberOfPeople=@Average WHERE Id=@Id", connection);
            command.Parameters.AddWithValue("@Name", fitness.Name);
            command.Parameters.AddWithValue("@Location", fitness.Location);
            command.Parameters.AddWithValue("@Subscribers", fitness.NumberOfSubscribers);
            command.Parameters.AddWithValue("@Average", fitness.AverageNumberOfPeople);
            command.Parameters.AddWithValue("@Id", fitness.Id);
            await command.ExecuteNonQueryAsync();
        }

        public static async Task<List<Fitness>> GetAllFitnessAsync()
        {
            List<Fitness> fitnessCenters = new List<Fitness>();
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();

            var cmd = new SqliteCommand("SELECT Id, Name, Location, NumberOfSubscribers, AverageNumberOfPeople FROM Fitness", connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                fitnessCenters.Add(new Fitness
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Location = reader.GetString(2),
                    NumberOfSubscribers = reader.GetInt32(3),
                    AverageNumberOfPeople = reader.GetInt32(4)
                });
            }
            return fitnessCenters;
        }



    }
}
