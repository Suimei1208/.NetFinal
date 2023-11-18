using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NetTechnology_Final.Services.Hash
{
	public class TokenService
	{
		private const string SecretKey = "khongthenaocanbuocduocmagaming";

        public string GenerateToken(string email, int seconds)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
					new Claim(ClaimTypes.Email, email),
					new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddSeconds(seconds).ToString())
                }),
                Expires = DateTime.UtcNow.AddSeconds(seconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public string ValidateToken(string token)
		{
			var tokenHandler = new JwtSecurityTokenHandler();

			try
			{
				var validationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)),
					ValidateIssuer = false,
					ValidateAudience = false
				};

				var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

				var emailClaim = principal.FindFirst(ClaimTypes.Email);
				return emailClaim?.Value;
			}
			catch (Exception)
			{
				return null;
			}
		}

        public bool IsTokenExpired(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

                var expirationDate = jwtToken?.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Expiration)?.Value;

                if (!string.IsNullOrEmpty(expirationDate) && DateTime.TryParse(expirationDate, out var expirationDateTime))
                {
                    return expirationDateTime < DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking token expiration: {ex.Message}");
            }

            return false;
        }
    }
}
