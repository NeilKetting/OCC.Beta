namespace OCC.Shared.Models
{
    public class CompanyDetails
    {
        public string CompanyName { get; set; } = "Orange Circle Construction CC";
        public string RegistrationNumber { get; set; } = "2007/004038/23";
        public string VatNumber { get; set; } = "4510254610";
        public string Phone { get; set; } = "+27 11 391-1340";
        public string Email { get; set; } = "info@orange-circle.co.za";
        public string Website { get; set; } = string.Empty;
        
        // Physical Address
        public string AddressLine1 { get; set; } = "No.58, Rd 5, Wydan Business Park";
        public string AddressLine2 { get; set; } = "Brentwood Park";
        public string City { get; set; } = "Benoni";
        public string PostalCode { get; set; } = "1510";
        public string Country { get; set; } = "South Africa";

        // Banking
        public string BankName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        
        // Department Emails
        public List<DepartmentEmail> DepartmentEmails { get; set; } = new()
        {
            new DepartmentEmail { Department = "Buying", EmailAddress = "jackie@orange-circle.co.za" },
            new DepartmentEmail { Department = "Accounts", EmailAddress = "anthia@orange-circle.co.za" }
        };

        // Helper
        public string FullAddress => $"{AddressLine1}, {City}, {PostalCode}";
    }

    public class DepartmentEmail
    {
        public string Department { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}
