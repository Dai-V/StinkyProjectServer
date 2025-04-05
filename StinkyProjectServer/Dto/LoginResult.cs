namespace StinkyProjectServer.Dto
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required string Token { get; set; }   
    }
}
