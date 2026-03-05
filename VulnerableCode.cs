using System;
using System.Data.SqlClient;

public class VulnerableApp
{
    public void ProcessUserInput()
    {
        // Source
        string input = Console.ReadLine();
        
        // Taint propagation
        string query = input;
        
        // Sink
        ExecuteSql(query);
    }

    public void SafeProcessing()
    {
        // Source
        string input = Console.ReadLine();
        
        // Sanitization (simulated, simplistic DFA might not catch this without explicit logic, 
        // but current logic clears taint on assignment if right side isn't tainted, wait.
        // If I assign a constant or clean var, it clears.
        string safe = "Fixed String";
        
        ExecuteSql(safe);
    }

    private void ExecuteSql(string command)
    {
        Console.WriteLine("Executing SQL: " + command);
    }
}
