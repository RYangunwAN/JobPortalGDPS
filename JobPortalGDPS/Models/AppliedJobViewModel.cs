namespace JobPortalGDPS.Models
{
    public class AppliedJobViewModel
    {
        public int ApplicationId { get; set; }
        public string JobTitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[] ResumeFile { get; set; }
    }
}
