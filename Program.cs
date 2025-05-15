using ConsoleAdoDotNet;

string? mySqlConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");


var employeeService = new EmployeeService(mySqlConnectionString!);

while (true)
{
    Console.WriteLine("\nEmployee Management System");
    Console.WriteLine("1. Add Employee");
    Console.WriteLine("2. View All Employees");
    Console.WriteLine("3. View Employee by ID");
    Console.WriteLine("4. Update Employee");
    Console.WriteLine("5. Delete Employee");
    Console.WriteLine("6. Exit");
    Console.Write("Enter your choice: ");

    if (!int.TryParse(Console.ReadLine(), out int choice))
    {
        Console.WriteLine("Invalid input. Please enter a number.");
        continue;
    }

    switch (choice)
    {
        case 1:
            await employeeService.AddEmployeeAsync();
            break;
        case 2:
            await employeeService.ViewAllEmployeesAsync();
            break;
        case 3:
            await employeeService.ViewEmployeeByIdAsync();
            break;
        case 4:
            await employeeService.UpdateEmployeeAsync();
            break;
        case 5:
            await employeeService.DeleteEmployeeAsync();
            break;
        case 6:
            Environment.Exit(0);
            break;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
}