namespace BookShop.Models
{
    public class ApplicationUserOTP
    {
        public string Id { get; set; }
        public string OTP { get; set; }

        public DateTime CreateAt { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsValid { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
