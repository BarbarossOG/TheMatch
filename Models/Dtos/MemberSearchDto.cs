namespace TheMatch.Models.Dtos
{
    public class MemberSearchDto
    {
        public string? City { get; set; } = "";
        public int AgeMin { get; set; }
        public int AgeMax { get; set; }
        public int HeightMin { get; set; }
        public int HeightMax { get; set; }
        public List<string> Interests { get; set; } = new();
    }
} 