namespace RnkApi.Models
{
    public class Customer
    {
        public Guid id { get; set; }
        public string c_FullName { get; set; }
        public string c_Address { get; set; }
        public string c_ContactNo { get; set; }
        public string c_Nic { get; set; }
        public string modifiedUser { get; set; } 
        public DateTime c_AssignedDate { get; set; }
        public string token { get; set; }
    }
}
