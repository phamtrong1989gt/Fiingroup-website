namespace PT.Domain.Model
{
    public class AuthorizeSettings
    {
       public bool CheckLoginWrongPassword { get; set; }
       public int MaxNumberWrongPasswords { get; set; }
       public int LockLoginInMinutes { get; set; }
       public int LockForgotPasswordInMinutes { get; set; }
       public bool CheckForgotPassword { get; set; }
       public string CapChaDataSitekey { get; set; }
       public string CapChaSecret { get; set; }
       public string CapchaVerifyUrl { get; set; }
    }
}
