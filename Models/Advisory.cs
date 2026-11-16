namespace AgriConnect.Models
{
    public class AdvisoryArticle
    {
        public int ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = "RAB Extension Service";
        public string Category { get; set; } = "Crops"; // Crops, Livestock, Weather Alert, Pest Management
        public string Language { get; set; } = "English"; // English, Kinyarwanda, French
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    }

    public class FarmerQuestion
    {
        public int QuestionId { get; set; }
        public int FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string? AnswerText { get; set; }
        public string? AnsweredBy { get; set; }
        public DateTime AskedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AnsweredAt { get; set; }
        public bool IsAnswered => !string.IsNullOrEmpty(AnswerText);
    }
}
