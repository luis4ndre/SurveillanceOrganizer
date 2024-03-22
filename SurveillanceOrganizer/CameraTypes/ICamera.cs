
namespace SurveillanceOrganizer.CameraTypes
{
    public interface ICamera
    {
        DateTime? GetDateFromFileName(string? lastFolder);
    }
}