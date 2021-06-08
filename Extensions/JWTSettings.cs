namespace aspnetIdentity.Extensions
{
    public class JWTSettings
    {
        public string Secret { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public int FinalExpiration { get; set; }
    }
}