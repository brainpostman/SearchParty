namespace WebApplication1ApiTest.APP.User
{
    public class User(string userid, string fullname, string nickname, string email)
    {
        public string? UserId { get; set; } = userid;

        public string? UserFullName { get; set; } = fullname;

        public string? UserNickname { get; set; } = nickname;

        public string? UserEmail { get; set; } = email;
    }
}
