namespace TheMatch.Models.Dtos
{
    public class DeleteAccountDto
    {
        public string Reason { get; set; }
        public string Description { get; set; }
        public bool Confirm { get; set; }
    }
} 