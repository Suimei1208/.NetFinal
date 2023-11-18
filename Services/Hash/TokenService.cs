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
				new Claim(ClaimTypes.Email, email)
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

				// Trích xuất giá trị claim "email" từ token
				var emailClaim = principal.FindFirst(ClaimTypes.Email);
				return emailClaim?.Value;
			}
			catch (Exception)
			{
				// Token không hợp lệ
				return null;
			}
		}
	}
}
