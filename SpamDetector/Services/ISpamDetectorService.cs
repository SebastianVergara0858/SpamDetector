namespace SpamDetector.Services
{
    public class SpamAnalysisResult
    {
        public bool IsSpam { get; set; }
        public int Score { get; set; }
        public List<string> Reasons { get; set; } = new();
    }

    public interface ISpamDetectorService
    {
        Task<bool> IsSpamAsync(string ipAddress);
        SpamAnalysisResult AnalyzeText(string content);
    }
}