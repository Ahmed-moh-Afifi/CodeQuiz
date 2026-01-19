namespace CodeQuizDesktop.Models
{
    public class SupportedLanguage
    {
        public required string Name { get; set; }
        public required string Extension { get; set; }
        public override string ToString() => Name;
    }
}
