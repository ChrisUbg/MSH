namespace MSH.Infrastructure.Data.Models;

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? AddressId { get; set; }  // Foreign key to Address table

    public Address? Address { get; set; }  // Navigation property
}