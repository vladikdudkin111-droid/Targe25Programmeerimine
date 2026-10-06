using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Puuduva kirje korral tagastavad lugemine, muutmine ja kustutamine null.
    public interface IRealEstateServices
    {
        Task<RealEstate> Create(RealEstateDto dto);
        Task<RealEstate?> Update(RealEstateDto dto);
        Task<RealEstate?> DetailAsync(Guid id);
        Task<RealEstate?> Delete(Guid id);
    }
}
