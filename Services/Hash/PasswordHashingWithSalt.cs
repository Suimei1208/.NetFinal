using System.Security.Cryptography;
using System.Text;

namespace NetTechnology_Final.Services.Hash
{
	public class PasswordHashingWithSalt
	{
		private const string Key = "jkhasdhkashjkasdhkasdhjksadhjk";

		public static string HashPasswordWithKey(string password)
		{
			string saltedPassword = password + Key;
			// Thực hiện băm mật khẩu với giá trị salt (Key) cố định
			return HashPassword(saltedPassword);
		}

		public static bool VerifyPasswordWithKey(string enteredPassword, string storedHash)
		{
			string saltedEnteredPassword = enteredPassword + Key;
			// Băm mật khẩu đã nhập với giá trị salt (Key) cố định và so sánh với chuỗi hash đã lưu trữ
			return HashPassword(saltedEnteredPassword) == storedHash;
		}

		private static string HashPassword(string password)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < hashedBytes.Length; i++)
				{
					stringBuilder.Append(hashedBytes[i].ToString("x2"));
				}

				return stringBuilder.ToString();
			}
		}
	}
}
