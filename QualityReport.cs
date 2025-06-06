namespace Spiro_Andon.Models
{
    public class QualityReport
    {
        public int Id { get; set; }
        public string VIN { get; set; }
        public string CheckedBy { get; set; }
        public string DefectType { get; set; }
        public DateTime? Date { get; set; }

        public string Description { get; set; }
    }

}
