namespace SurveillanceOrganizer.Entities
{
    public class PathStructure
    {
        public PathStructure(string root, string path)
        {
            this.Root = root;
            this.File = new FileStructure(path);

            this.Folders = path.Replace($"{this.Root}/", "").Replace($"/{string.Join(".", this.File.Full())}", "").Split("/");
        }

        public string Root { get; }

        public IList<string> Folders { get; set; }
        public FileStructure File { get; }

        public string FullPath(bool withFile = true)
        {
            if (withFile)
                return $"{this.Root}/{this.FoldersPath()}/{this.File.Full()}";


            return $"{this.Root}/{this.FoldersPath()}";
        }

        public string FoldersPath()
        {
            return $"{string.Join("/", this.Folders)}";
        }
    }

    public class FileStructure
    {
        public FileStructure(string path)
        {
            this.FileExtension = path.Substring(path.LastIndexOf(".") + 1);
            this.FileName = path.Substring(path.LastIndexOf("/") + 1).Replace($".{this.FileExtension}", "");

        }

        public string FileName { get; set; }
        public int FileCount { get; set; }
        public string FileExtension { get; set; }

        public string Full()
        {
            var aux = this.FileCount > 0 ? $" ({this.FileCount})" : "";

            return $"{this.FileName}{aux}.{this.FileExtension}";
        }
    }
}
