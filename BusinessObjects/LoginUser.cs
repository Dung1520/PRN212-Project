namespace BusinessObjects
{
    public class LoginUser
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty; // Admin / Teacher / Student
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsDefaultAdmin { get; set; } = false;
    }
}