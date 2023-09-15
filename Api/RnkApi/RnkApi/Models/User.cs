namespace login.model
{
    public class User
    {
        public Guid id { get; set; }
        public string? fristName { get; set; }
        public string? lastName { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string? token { get; set; }
        public string? role { get; set; }
        public string? email { get; set; }
        public string? refreshToken { get; set; }
        public DateTime? refreshTokenExpiryTime { get; set; }

    

        //reseat password
        public string? ResetPasswordToken { get; set; }
        public DateTime ResetPasswordExpiry { get; set; }


    }
}
