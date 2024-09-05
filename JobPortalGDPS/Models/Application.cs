namespace JobPortalGDPS.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public int JobId { get; set; }
        public byte[] ResumeFile { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
