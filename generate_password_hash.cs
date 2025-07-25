using System;
using Microsoft.AspNetCore.Identity;

class Program
{
    static void Main()
    {
        var passwordHasher = new PasswordHasher<IdentityUser>();
        var user = new IdentityUser { UserName = "admin@msh.local" };
        
        string password = "Admin123!";
        string hash = passwordHasher.HashPassword(user, password);
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
    }
} 