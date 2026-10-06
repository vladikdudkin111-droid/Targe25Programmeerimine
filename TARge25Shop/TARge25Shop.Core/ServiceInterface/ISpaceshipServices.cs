using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Puuduva kirje korral tagastavad lugemine, muutmine ja kustutamine null.
    public interface ISpaceshipServices
    {
        Task<Spaceship> Create(SpaceshipDto dto);
        Task<Spaceship?> Update(SpaceshipDto dto);
        Task<Spaceship?> DetailAsync(Guid id);
        Task<Spaceship?> Delete(Guid id);
    }
}
