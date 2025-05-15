using System.Data.Common;
using ConsoleTables;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;


namespace ConsoleAdoDotNet
{
    public class EmployeeService
    {
        private readonly string _connectionString;

        public EmployeeService(string mySqlConnectionString)
        {
            _connectionString = mySqlConnectionString ?? throw new ArgumentNullException(nameof(mySqlConnectionString));
        }

        public async Task AddEmployeeAsync()
        {
            Console.WriteLine("\nAdd New Employee");

            Console.Write("First Name: ");
            string firstName = Console.ReadLine()!;

            Console.Write("Last Name: ");
            string lastName = Console.ReadLine()!;

            Console.Write("Email: ");
            string email = Console.ReadLine()!;

            Console.Write("Department: ");
            string department = Console.ReadLine()!;

            Console.Write("Hire Date (yyyy-mm-dd): ");

            if (!DateTime.TryParse(Console.ReadLine(), out DateTime hireDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            Console.Write("Salary: ");

            if (!decimal.TryParse(Console.ReadLine(), out decimal salary))
            {
                Console.WriteLine("Invalid salary format.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO Employees (Id,FirstName, LastName, Email, Department, Hired_Date, Salary) " +
                                "VALUES (@Id,@FirstName, @LastName, @Email, @Department, @Hired_Date, @Salary)";


                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", Guid.NewGuid());
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Department", department);
                command.Parameters.AddWithValue("@Hired_Date", hireDate);
                command.Parameters.AddWithValue("@Salary", salary);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee added successfully!");
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task ViewAllEmployeesAsync()
        {
            Console.WriteLine("\nAll Employees");

            ConsoleTable table = new("Name", "Email", "Department", "Hire Date", "Salary");

            using (MySqlConnection connection = new(_connectionString))
            {
                string query = "SELECT * FROM Employees";
                MySqlCommand command = new(query, connection);

                try
                {
                    await connection.OpenAsync();
                    var reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        table.AddRow($"{reader["FirstName"]} {reader["LastName"]}", reader["Email"], reader["Department"], ((DateTime)reader["Hired_Date"]).ToShortDateString(), reader["Salary"]);
                    }

                    table.Write(Format.Default);

                    reader.Close();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task ViewEmployeeByIdAsync()
        {
            Console.Write("\nEnter Employee ID: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employees WHERE Id = @Id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    await connection.OpenAsync();
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();
                        Console.WriteLine("\nEmployee Details");
                        Console.WriteLine($"ID: {reader["Id"]}");
                        Console.WriteLine($"Name: {reader["FirstName"]} {reader["LastName"]}");
                        Console.WriteLine($"Email: {reader["Email"]}");
                        Console.WriteLine($"Department: {reader["Department"]}");
                        Console.WriteLine($"Hire Date: {((DateTime)reader["Hired_Date"]).ToShortDateString()}");
                        Console.WriteLine($"Salary: {reader["Salary"]}");
                    }
                    else
                    {
                        Console.WriteLine("Employee not found.");
                    }
                    reader.Close();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"MySQLError: {ex.Message}");
                }
            }
        }

        public async Task UpdateEmployeeAsync()
        {
            Console.Write("\nEnter Employee ID to update: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            // First check if employee exists
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Employees WHERE Id = @Id";
                MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@Id", id);

                try
                {
                    await connection.OpenAsync();
                    var employeeCount = await checkCommand.ExecuteScalarAsync();
                    int convertedToInt = Convert.ToInt32(employeeCount);
                    if (convertedToInt == 0)
                    {
                        Console.WriteLine("Employee not found.");
                        return;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return;
                }
            }

            Console.WriteLine("\nEnter new details (leave blank to keep current value):");

            Console.Write("First Name: ");
            string firstName = Console.ReadLine()!;

            Console.Write("Last Name: ");
            string lastName = Console.ReadLine()!;

            Console.Write("Email: ");
            string email = Console.ReadLine()!;

            Console.Write("Department: ");
            string department = Console.ReadLine()!;

            Console.Write("Hire Date (yyyy-mm-dd): ");
            string hireDateStr = Console.ReadLine()!;
            DateTime? hireDate = null;

            if (!string.IsNullOrEmpty(hireDateStr) && DateTime.TryParse(hireDateStr, out DateTime parsedDate))
            {
                hireDate = parsedDate;
            }

            Console.Write("Salary: ");
            string salaryStr = Console.ReadLine()!;
            decimal? salary = null;
            if (!string.IsNullOrEmpty(salaryStr) && decimal.TryParse(salaryStr, out decimal parsedSalary))
            {
                salary = parsedSalary;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = "UPDATE Employees SET " +
                               "FirstName = ISNULL(@FirstName, FirstName), " +
                               "LastName = ISNULL(@LastName, LastName), " +
                               "Email = ISNULL(@Email, Email), " +
                               "Department = ISNULL(@Department, Department), " +
                               "HireDate = ISNULL(@Hired_Date, HireDate), " +
                               "Salary = ISNULL(@Salary, Salary) " +
                               "WHERE Id = @Id";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@FirstName", string.IsNullOrEmpty(firstName) ? (object)DBNull.Value : firstName);
                command.Parameters.AddWithValue("@LastName", string.IsNullOrEmpty(lastName) ? (object)DBNull.Value : lastName);
                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                command.Parameters.AddWithValue("@Department", string.IsNullOrEmpty(department) ? (object)DBNull.Value : department);
                command.Parameters.AddWithValue("@Hired_Date", hireDate.HasValue ? (object)hireDate.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Salary", salary.HasValue ? (object)salary.Value : DBNull.Value);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee updated successfully!");
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task DeleteEmployeeAsync()
        {
            Console.Write("\nEnter Employee ID to delete: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = "DELETE FROM Employees WHERE Id = @Id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee deleted successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Employee not found.");
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
