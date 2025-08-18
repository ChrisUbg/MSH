using System;
using Microsoft.AspNetCore.Identity;

class Program
{
    static void Main()
    {
        var hasher = new PasswordHasher<IdentityUser>();
        var hash = hasher.HashPassword(null, "password123");
        Console.WriteLine($"Password hash for 'password123': {hash}");
    }
} 