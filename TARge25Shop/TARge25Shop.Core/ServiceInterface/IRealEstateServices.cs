using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Liides määrab samad CRUD toimingud nagu ISpaceshipServices.
    public interface IRealEstateServices
    {
        // Loob uue kinnisvaraobjekti.
        Task<RealEstate> Create(RealEstateDto dto);

        // Muudab olemasolevat objekti; puuduva kirje korral tagastab null.
        Task<RealEstate?> Update(RealEstateDto dto);

        // Leiab ühe objekti ID järgi või tagastab null.
        Task<RealEstate?> DetailAsync(Guid id);

        // Kustutab objekti või tagastab puuduva kirje korral null.
        Task<RealEstate?> Delete(Guid id);
    }
}
