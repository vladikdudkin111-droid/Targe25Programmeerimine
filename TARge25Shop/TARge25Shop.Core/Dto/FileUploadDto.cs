namespace TARge25Shop.Core.Dto
{
    // Veebikihist teenusekihti edastatav fail.
    public class FileUploadDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
