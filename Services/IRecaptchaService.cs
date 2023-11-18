using Newtonsoft.Json;

namespace NetTechnology_Final.Services
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IRecaptchaService
    {
        Task<RecaptchaResponse> Validate(HttpRequest request);
    }

    public class RecaptchaService : IRecaptchaService
    {
        private readonly IConfiguration configuration;

        public RecaptchaService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<RecaptchaResponse> Validate(HttpRequest request)
        {
            var recaptchaSecretKey = configuration["GoogleRecaptcha:SecretKey"];
            var recaptchaResponse = request.Form["g-recaptcha-response"];

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient
                    .PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={recaptchaSecretKey}&response={recaptchaResponse}", null);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var recaptchaResult = JsonConvert.DeserializeObject<RecaptchaResponse>(jsonString);
                    return recaptchaResult;
                }

                return null;
            }
        }
    }

    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        public decimal Score { get; set; }
        public string Action { get; set; }
        public DateTime ChallengeTs { get; set; }
        public string Hostname { get; set; }
        public List<string> ErrorCodes { get; set; }
    }
}