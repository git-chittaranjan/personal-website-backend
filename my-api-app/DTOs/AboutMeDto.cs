namespace my_api_app.DTOs
{
    public class AboutMeDto
    {
        public string? Name { get; set; }
        public int? Age { get; set; } // With or without ? does not throw any warnings but string shows warning
        public string? Gender { get; set; } //With or withou ? it return null, but ? only suppress warning (if mendatory then write default!
        public string? Role { get; set; }
        public int? ExperienceYears { get; set; } //With ? - It will retun 'null' and Without ? it returns '0' in the response if value is not assigned
        public List<string>? Skills { get; set; }
        public string? Location { get; set; }
        public List<string>? Interests { get; set; }
    }
}
