namespace Spiro_Andon.Models
{
    public class Incident_Report
    {
        public int Id { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public DateTime DateOfIncident { get; set; }
        public string IncidentType { get; set; }
        public string Description { get; set; }
    }

}
