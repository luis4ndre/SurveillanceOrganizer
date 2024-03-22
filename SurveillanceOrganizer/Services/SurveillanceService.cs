using Dapper;
using MySqlConnector;
using SurveillanceOrganizer.CameraTypes;
using SurveillanceOrganizer.Entities;
using System.Text.Json;

namespace SurveillanceOrganizer.Services;

public class SurveillanceService
{
    private IConfiguration _configuration;
    private Xiaomi360 _xiaomi360;
    private TpLinkC100 _tpLinkC100;

    public SurveillanceService(IConfiguration configuration, Xiaomi360 xiaomi360, TpLinkC100 tpLinkC100)
    {
        this._configuration = configuration;
        this._xiaomi360 = xiaomi360;
        this._tpLinkC100 = tpLinkC100;
    }

    public async Task StartAsync(string root, string destfolder)
    {
        Console.WriteLine($"--- Start SurveillanceService ---");

        Console.WriteLine($"root DIR: {string.Join(", ", Directory.GetDirectories(root))}");

        try
        {
            var consolidationConfigs = await this.GetConfigurations();

            Console.WriteLine($"CONFIG: {JsonSerializer.Serialize(consolidationConfigs)}");

            foreach (var config in consolidationConfigs)
            {
                var path = $"{root}/{config.Parent}";

                if (Directory.Exists(path))
                {
                    Console.WriteLine($"Path ({path}) found!");

                    var files = ProcessDirectory(root, path, config.DaysDiff, config.Extension);

                    Console.WriteLine($"FILES COUNT: {files.Count}");

                    Parallel.ForEach(
                        files,
                        new ParallelOptions { MaxDegreeOfParallelism = 5 },
                        file => { ProcessFile(file, root, destfolder, config.Alias, config.CameraType); }
                    );

                    //RemoveOldFiles(root, destfolder, config.DaysKeep, config.GetDaysOfWeek());

                    Console.WriteLine($"--- Start RemoveEmptyDirectory ---");

                    RemoveEmptyDirectory(root);

                    Console.WriteLine($"--- Finish RemoveEmptyDirectory ---");
                }
                else
                    Console.WriteLine($"Path ({path}) NOT found!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR MSG: {ex.Message}");
            Console.WriteLine($"ERROR: {JsonSerializer.Serialize(ex)}");
        }
        finally
        {
            Console.WriteLine($"--- Finish SurveillanceService ---");
        }
    }

    private void RemoveOldFiles(string root, string path, int daysDiff, string extension, IEnumerable<DayOfWeek>? enumerable)
    {
        Console.WriteLine($"--- Start RemoveOldFiles ---");

        var files = ProcessDirectory(root, path, daysDiff, extension, enumerable);

        Parallel.ForEach(
            files,
            new ParallelOptions { MaxDegreeOfParallelism = 5 },
            file => { File.Delete(file.FullPath()); }
        );

        Console.WriteLine($"--- Finish RemoveOldFiles ---");
    }

    private void ProcessFile(PathStructure path, string root, string destfolder, string? alias, CameraType cameraType)
    {
        var fileDate = GetDateFromFileName(path, cameraType);

        if (fileDate.HasValue)
        {
            var destination = $"{root}/{destfolder}/{alias}/{GetDestinationPath(fileDate.Value)}";

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            var destFile = $"{destination}/{path.File.Full()}";

            if (cameraType.Equals(CameraType.Xiaomi360))
                destFile = $"{destination}/{fileDate.Value:HH}H{path.File.Full()}";

            File.Move(path.FullPath(), destFile, true);
            Console.WriteLine($"{path.FullPath()} --> {destFile}");
        }
    }

    private void RemoveEmptyDirectory(string startLocation)
    {
        foreach (var directory in Directory.GetDirectories(startLocation))
        {
            RemoveEmptyDirectory(directory);
            if (Directory.GetFiles(directory).Length == 0 &&
                Directory.GetDirectories(directory).Length == 0)
            {
                Directory.Delete(directory, false);
            }
        }
    }

    private async Task<IEnumerable<ConsolidationConfig>> GetConfigurations()
    {
        using (var conexao = new MySqlConnection(Environment.GetEnvironmentVariable("MySqlConn")))
        {
            return await conexao.QueryAsync<ConsolidationConfig>("SELECT * FROM consolidation_config");
        }
    }

    private static IList<PathStructure> ProcessDirectory(string root, string path, int daysDiff, string extension, IEnumerable<DayOfWeek>? daysOfWeek = null)
    {
        var files = new List<PathStructure>();

        //Process the list of files found in the directory.
        var fileEntries = Directory.GetFiles(path, $"*.{extension}");

        foreach (string fileName in fileEntries)
        {
            var creationTimeUtc = File.GetCreationTimeUtc(fileName);

            if (creationTimeUtc.Date < DateTime.UtcNow.AddDays(-daysDiff).Date && (daysOfWeek is null || !daysOfWeek.Any() || !daysOfWeek.Contains(creationTimeUtc.Date.DayOfWeek)))
                files.Add(new PathStructure(root, fileName));
        }

        //Recurse into subdirectories of this directory.
        var subdirectoryEntries = Directory.GetDirectories(path);
        foreach (string subdirectory in subdirectoryEntries)
            files.AddRange(ProcessDirectory(root, subdirectory, daysDiff, extension));

        return files;
    }

    private DateTime? GetDateFromFileName(PathStructure path, CameraType cameraType)
    {
        switch (cameraType)
        {
            case CameraType.Xiaomi360:
                return this._xiaomi360.GetDateFromFileName(path.Folders.LastOrDefault());
            case CameraType.TpLinkC100:
                return this._tpLinkC100.GetDateFromFileName(path.File.FileName);
            default:
                return null;
        }
    }

    private static string? GetDestinationPath(DateTime fileDate)
    {
        return $"{fileDate:yyyy}/{fileDate:yyyy}-{fileDate:MM}/{fileDate:yyyy}-{fileDate:MM}-{fileDate:dd}";
    }
}
