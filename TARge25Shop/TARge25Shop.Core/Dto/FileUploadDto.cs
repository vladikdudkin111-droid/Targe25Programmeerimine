namespace TARge25Shop.Core.Dto
{
    public class FileUploadDto
    {
        public const int MaxFileSize = 5 * 1024 * 1024;
        public const int MaxFileCount = 10;
        public const int MaxTotalSize = 20 * 1024 * 1024;
        public const int MaxRequestSize = 25 * 1024 * 1024;

        public string FileName { get; set; } = string.Empty;
        public byte[] Data { get; set; } = [];
    }
}
