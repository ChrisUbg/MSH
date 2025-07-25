using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

class Program
{
    static void Main()
    {
        var passwordHasher = new PasswordHasher<object>();
        var hash = passwordHasher.HashPassword(null, "Admin123!");
        Console.WriteLine($"Password Hash: {hash}");
    }
} 