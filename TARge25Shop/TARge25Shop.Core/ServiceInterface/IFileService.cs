using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    public interface IFileService
    {
        List<KindergartenImage> PrepareImages(IReadOnlyCollection<FileUploadDto> files);
        Task<List<KindergartenImageDto>> GetImagesAsync(Guid kindergartenId);
        Task<KindergartenImage?> GetImageAsync(Guid kindergartenId, Guid imageId);
        Task<bool> DeleteImageAsync(Guid kindergartenId, Guid imageId);
    }
}
