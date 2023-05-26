using Microsoft.Extensions.Configuration;

namespace ProjectAPI.Middlewares
{
    public class JwtOptions
    {
        public string SigningKey { get; set; }
        public double ExpiredToken { get; set; }
        public JwtOptions()
        {
            ExpiredToken = 5.0;
        }

        public JwtOptions(IConfiguration configuration)
        {
            SigningKey = configuration.GetValue<string>("JwtOptions:SigningKey") ?? string.Empty;
            ExpiredToken = configuration.GetValue<double?>("JwtOptions:ExpiredToken") ?? 5.0;
        }
        public JwtOptions(string signingKey, double expiredToken)
        {
            SigningKey = signingKey;
            ExpiredToken = expiredToken;
        }
    }
}