namespace NetTechnology_Final.Services.EmailService
{
    public class EmailDto
    {
        private string _subject = "Confirm account registration via email";
        public string Body { get; set; } = string.Empty;
        public string Subject
        {
            get => _subject;
            set => _subject = value ?? throw new ArgumentNullException(nameof(Subject), "Email subject is required.");
        }

        public string SetBody(string newBody)
        {
            Body = newBody;
            return Body;
        }

    }
}
