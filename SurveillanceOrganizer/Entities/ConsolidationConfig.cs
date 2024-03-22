using SurveillanceOrganizer.CameraTypes;

namespace SurveillanceOrganizer.Entities;
public class ConsolidationConfig
{
    public int Id { get; set; }
    public string? Parent { get; set; }
    public string? Alias { get; set; }
    public int DaysDiff { get; set; }
    public int DaysKeep { get; set; }
    public string? DaysOfWeek { get; set; }
    public CameraType CameraType { get; set; }
    public string Extension { get; set; }
    

    public IEnumerable<DayOfWeek>? GetDaysOfWeek() => this.DaysOfWeek?.Split('|').Select(s => (DayOfWeek)Convert.ToInt32(s));
}
