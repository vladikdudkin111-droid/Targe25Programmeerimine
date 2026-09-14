using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Interface määrab ära, millised meetodid peavad SpaceshipServices klassis olemas olema.
    public interface ISpaceshipServices
    {
        // Loob uue kosmoselaeva.
        Task<Spaceship> Create(SpaceshipDto dto);

        // Uuendab olemasolevat kosmoselaeva.
        Task<Spaceship> Update(SpaceshipDto dto);

        // Tagastab ühe kosmoselaeva ID järgi.
        Task<Spaceship?> DetailAsync(Guid id);

        // Kustutab kosmoselaeva ID järgi.
        Task<Spaceship?> Delete(Guid id);
    }
}
