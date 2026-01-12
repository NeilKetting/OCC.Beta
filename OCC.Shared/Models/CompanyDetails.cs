namespace OCC.Shared.Models
{
    public class CompanyDetails
    {
        public string CompanyName { get; set; } = "Orange Circle Construction (PTY) Ltd";
        public string RegistrationNumber { get; set; } = "2007/004038/23";
        public string VatNumber { get; set; } = "4510254610";
        public string Website { get; set; } = string.Empty;

        public List<DepartmentEmail> DepartmentEmails 
        { 
            get => Branches.ContainsKey(Branch.JHB) ? Branches[Branch.JHB].DepartmentEmails : new List<DepartmentEmail>(); 
            set { if (Branches.ContainsKey(Branch.JHB)) Branches[Branch.JHB].DepartmentEmails = value; } 
        }

        public string Email
        {
            get => Branches.ContainsKey(Branch.JHB) ? Branches[Branch.JHB].Email : string.Empty;
            set { if (Branches.ContainsKey(Branch.JHB)) Branches[Branch.JHB].Email = value; }
        }

        public string Phone
        {
            get => Branches.ContainsKey(Branch.JHB) ? Branches[Branch.JHB].Phone : string.Empty;
            set { if (Branches.ContainsKey(Branch.JHB)) Branches[Branch.JHB].Phone = value; }
        }

        public string Fax
        {
            get => Branches.ContainsKey(Branch.JHB) ? Branches[Branch.JHB].Fax : string.Empty;
            set { if (Branches.ContainsKey(Branch.JHB)) Branches[Branch.JHB].Fax = value; }
        }

        public string Address
        {
            get => Branches.ContainsKey(Branch.JHB) ? $"{Branches[Branch.JHB].AddressLine1}, {Branches[Branch.JHB].City}, {Branches[Branch.JHB].PostalCode}" : string.Empty;
        }

        public string FullAddress => Address;

        public string BankName { get; set; } = "FNB";
        public string AccountName { get; set; } = "Orange Circle Construction";
        public string AccountNumber { get; set; } = "62123456789";
        public string BranchCode { get; set; } = "250655";
        public string AccountType { get; set; } = "Business Account";

        public string AddressLine1
        {
            get => Branches.ContainsKey(Branch.JHB) ? Branches[Branch.JHB].AddressLine1 : string.Empty;
            set { if (Branches.ContainsKey(Branch.JHB)) Branches[Branch.JHB].AddressLine1 = value; }
        }

        public string AddressLine2
        {
            get => Branches.ContainsKey(Branch.JHB) ? Branches[Branch.JHB].AddressLine2 : string.Empty;
            set { if (Branches.ContainsKey(Branch.JHB)) Branches[Branch.JHB].AddressLine2 = value; }
        }

        public string City
        {
            get => Branches.ContainsKey(Branch.JHB) ? Branches[Branch.JHB].City : string.Empty;
            set { if (Branches.ContainsKey(Branch.JHB)) Branches[Branch.JHB].City = value; }
        }

        public string PostalCode
        {
            get => Branches.ContainsKey(Branch.JHB) ? Branches[Branch.JHB].PostalCode : string.Empty;
            set { if (Branches.ContainsKey(Branch.JHB)) Branches[Branch.JHB].PostalCode = value; }
        }

        public Dictionary<Branch, BranchDetails> Branches { get; set; } = new()
        {
            { 
                Branch.JHB, new BranchDetails 
                { 
                    Phone = "+27 11 391-1340",
                    Fax = "(011) 972-6958",
                    Email = "info@orange-circle.co.za",
                    AddressLine1 = "No. 58, Rd5, Wydan Business Park",
                    AddressLine2 = "Brentwood Park",
                    City = "Benoni",
                    PostalCode = "1510",
                    DepartmentEmails = new()
                    {
                        new DepartmentEmail { Department = "Buying", EmailAddress = "jackie@orange-circle.co.za" },
                        new DepartmentEmail { Department = "Accounts", EmailAddress = "anthia@orange-circle.co.za" }
                    }
                } 
            },
            { 
                Branch.CPT, new BranchDetails 
                { 
                    Phone = "TBD",
                    Fax = "TBD",
                    Email = "TBD",
                    AddressLine1 = "TBD",
                    AddressLine2 = "TBD",
                    City = "Cape Town",
                    PostalCode = "TBD",
                    DepartmentEmails = new()
                    {
                        new DepartmentEmail { Department = "Buying", EmailAddress = "TBD" },
                        new DepartmentEmail { Department = "Accounts", EmailAddress = "TBD" }
                    }
                } 
            }
        };
    }

    public class BranchDetails
    {
        public string Phone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "South Africa";

        public List<DepartmentEmail> DepartmentEmails { get; set; } = new();

        public string FullAddress => $"{AddressLine1}, {City}, {PostalCode}";
    }

    public class DepartmentEmail
    {
        public string Department { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}
