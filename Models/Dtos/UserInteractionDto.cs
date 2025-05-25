namespace TheMatch.Models.Dtos
{
    public class UserInteractionDto
    {
        public int TargetUserId { get; set; }
        public byte InteractionTypeId { get; set; } // 2-лайк, 3-дизлайк, 6-блокировка
    }
} 