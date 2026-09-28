using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Liides määrab kosmoselaevade teenuse avalikud CRUD meetodid.
    public interface ISpaceshipServices
    {
        // Loob uue kosmoselaeva.
        Task<Spaceship> Create(SpaceshipDto dto);

        // Uuendab olemasoleva kosmoselaeva.
        Task<Spaceship> Update(SpaceshipDto dto);

        // Tagastab ühe kosmoselaeva ID järgi või null, kui kirjet ei leitud.
        Task<Spaceship?> DetailAsync(Guid id);

        // Kustutab kosmoselaeva ja tagastab kustutatud objekti või null.
        Task<Spaceship?> Delete(Guid id);
    }
}
