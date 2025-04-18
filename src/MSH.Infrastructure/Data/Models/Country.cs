namespace MSH.Infrastructure.Data.Models
{
    public class Country  
    {
        public int Id { get; set; }
        public string CountryCode { get; set; } = string.Empty;  // ISO-Alpha-2 (z. B. "DE")
        public string CountryName { get; set; } = string.Empty;

        public ICollection<Address>? Addresses { get; set; }  // Navigation property
    }
}
