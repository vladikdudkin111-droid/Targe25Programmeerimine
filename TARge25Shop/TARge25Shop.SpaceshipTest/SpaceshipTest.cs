using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using Xunit;

namespace TARge25Shop.SpaceshipTest
{
    // Viis testi kontrollivad kosmoselaeva loomist, otsimist, kustutamist ja muutmist.
    public class SpaceshipTest : TestBase
    {
        [Fact]
        // Kosmoselaeva lisamisel peab teenus tagastama objekti ja salvestama selle andmebaasi.
        public async Task ShouldNot_AddEmptySpaceShip_WhenResultIsReturned()
        {
            // Ülesseade: valmistame ette uue kosmoselaeva andmed.
            var dto = MockSpaceshipData();

            // Tegevus: kutsume päris ApplicationServices teenust.
            var result = await Svc<ISpaceshipServices>().Create(dto);

            // Kontroll: teenus tagastab objekti koos mittetühja tunnusega.
            Assert.NotNull(result);
            Assert.True(result.Id.HasValue);
            Assert.NotEqual(Guid.Empty, result.Id.GetValueOrDefault());

            // Uus kontekst tõestab, et tulemus jõudis ka andmebaasi.
            using var verificationScope = CreateScope();
            var context = verificationScope.ServiceProvider.GetRequiredService<TARge25ShopContext>();
            var saved = await context.Spaceships.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == result.Id);

            Assert.NotNull(saved);
            Assert.Equal(dto.Name, saved.Name);
            Assert.Equal(dto.ShipType, saved.ShipType);
            Assert.Equal(dto.Crew, saved.Crew);
            Assert.Equal(dto.EnginePower, saved.EnginePower);
            Assert.NotEqual(default(DateTime), saved.CreatedAt);
            Assert.NotEqual(default(DateTime), saved.UpdatedAt);
        }

        [Fact]
        // Vale ID järgi otsimine peab tagastama null ka siis, kui andmebaasis on teine laev.
        public async Task ShouldNot_GetSpaceShipByID_WhenIDNotEqual()
        {
            // Ülesseade: loome ühe kirje ja valime sellest erineva puuduva ID.
            var added = await Svc<ISpaceshipServices>().Create(MockSpaceshipData());
            var wrongGuid = Guid.NewGuid();
            Assert.NotEqual(wrongGuid, added.Id.GetValueOrDefault());

            // Tegevus: küsime teenuselt vale ID-ga kosmoselaeva.
            using var queryScope = CreateScope();
            var result = await queryScope.ServiceProvider
                .GetRequiredService<ISpaceshipServices>().DetailAsync(wrongGuid);

            // Kontroll: hindame teenuse vastust, mitte kahe kohaliku GUID-i erinevust.
            Assert.Null(result);
        }

        [Fact]
        // Õige ID järgi otsimine peab tagastama varem salvestatud kosmoselaeva.
        public async Task Should_GetSpaceshipByID_WhenGuidIsEqual()
        {
            // Ülesseade: salvestame objekti enne selle otsimist.
            var dto = MockSpaceshipData();
            var added = await Svc<ISpaceshipServices>().Create(dto);
            Assert.True(added.Id.HasValue);

            // Tegevus: uus skoop väldib sama jälgitava objekti kasutamist kontrollis.
            using var queryScope = CreateScope();
            var result = await queryScope.ServiceProvider
                .GetRequiredService<ISpaceshipServices>()
                .DetailAsync(added.Id.GetValueOrDefault());

            // Kontroll: teenus leidis õige ID ja õigete andmetega objekti.
            Assert.NotNull(result);
            Assert.Equal(added.Id, result.Id);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.ShipType, result.ShipType);
            Assert.Equal(dto.Crew, result.Crew);
            Assert.Equal(dto.EnginePower, result.EnginePower);
        }

        [Fact]
        // ID järgi kustutamisel tagastatakse õige objekt ja andmebaasist kaob ainult see kirje.
        public async Task Should_SpaceshipDeletedByID_WhenReturnedResultIsEqual()
        {
            // Ülesseade: teine laev peab pärast esimese kustutamist alles jääma.
            var service = Svc<ISpaceshipServices>();
            var added = await service.Create(MockSpaceshipData());
            var other = await service.Create(MockSpaceshipData(isOneOrTwo: true));
            Assert.True(added.Id.HasValue);

            // Tegevus: kustutame esimese kosmoselaeva tema ID järgi.
            var deleted = await service.Delete(added.Id.GetValueOrDefault());

            // Kontroll: tagastatud objekt vastab kustutamiseks valitud objektile.
            Assert.NotNull(deleted);
            Assert.Equal(added.Id, deleted.Id);

            // Uues kontekstis kontrollime tegelikku salvestatud lõpptulemust.
            using var verificationScope = CreateScope();
            var context = verificationScope.ServiceProvider.GetRequiredService<TARge25ShopContext>();
            Assert.False(await context.Spaceships.AsNoTracking().AnyAsync(item => item.Id == added.Id));
            Assert.True(await context.Spaceships.AsNoTracking().AnyAsync(item => item.Id == other.Id));
        }

        [Fact]
        // Õpetaja loogika: võrdleme dto ja domain andmeid pärast Update-meetodi kutsumist.
        public async Task Should_UpdateSpaceshipByID_WhenUpdatingData()
        {
            // Ülesseade: sama ID ühendab andmebaasikirje ja muutmiseks saadetud DTO.
            var guid = new Guid("68eb8abd-086a-4c8b-9695-71234143f709");
            SpaceshipDto dto = MockSpaceshipData();
            dto.Id = guid;

            // Varasemad fikseeritud kuupäevad väldivad testi sõltuvust kella täpsusest.
            dto.CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dto.UpdatedAt = dto.CreatedAt.AddDays(1);

            // Säilitame õpetaja muutuja nime ja väärtused. domain on siin võrdluseks kasutatav DTO.
            // domain hoiab võrdlusvälju ja kuupäevade oodatud väärtusi; dto on Update-meetodi sisend.
            SpaceshipDto domain = new();

            domain.Id = Guid.Parse("68eb8abd-086a-4c8b-9695-71234143f709");
            domain.EnginePower = 10000000;
            domain.Name = "Igor Mang 2";
            domain.ShipType = "püramiid";
            domain.Crew = 420;
            domain.CreatedAt = dto.CreatedAt; // Loomisaega ei muudeta; võtame olemasoleva väärtuse.
            domain.UpdatedAt = DateTime.UtcNow; // Muutmisaeg peab uuendamisel muutuma.

            // Meie teenus vajab olemasolevat kirjet. Salvestame domain andmed testi andmebaasi.
            // Eraldi Spaceship-objekt jätab võrdlusandmed muutmise ajal alles.
            using (var setupScope = CreateScope())
            {
                var setupContext = setupScope.ServiceProvider
                    .GetRequiredService<TARge25ShopContext>();
                setupContext.Spaceships.Add(new Spaceship
                {
                    Id = domain.Id,
                    Name = domain.Name,
                    ShipType = domain.ShipType,
                    Crew = domain.Crew,
                    EnginePower = domain.EnginePower,
                    CreatedAt = domain.CreatedAt,
                    // Andmebaasis on enne Update-kutset varasem muutmisaeg.
                    UpdatedAt = dto.UpdatedAt
                });
                await setupContext.SaveChangesAsync();
            }

            // Tegevus: kutsume õpetaja näite järgi Update(dto) ja jätame tulemuse kontrollimiseks alles.
            var result = await Svc<ISpaceshipServices>().Update(dto);

            // Kontroll: õpetaja näite ID peab sobima ning muudetavate väljade väärtused peavad erinema.
            Assert.Equal(domain.Id, guid);
            Assert.NotEqual(dto.EnginePower, domain.EnginePower);
            Assert.NotEqual(dto.Name, domain.Name);

            // DoesNotMatch otsib regulaaravaldise mustrit tekstist.
            // Säilitame õpetaja Crew.ToString() näite ja lisame sama võrdluse fikseeritud laevatüübile.
            Assert.DoesNotMatch(dto.Crew.ToString(), domain.Crew.ToString());
            Assert.DoesNotMatch(dto.ShipType, domain.ShipType);

            // Loomisaeg jääb samaks; võrdleme omavahel vana ja uut muutmisaega.
            Assert.Equal(dto.CreatedAt, domain.CreatedAt);
            Assert.NotEqual(dto.UpdatedAt, domain.UpdatedAt);

            // Lisaks kinnitame, et teenuse tagastatud objekt sisaldab tõesti dto andmeid.
            Assert.NotNull(result);
            Assert.Equal(dto.Id, result.Id);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.ShipType, result.ShipType);
            Assert.Equal(dto.Crew, result.Crew);
            Assert.Equal(dto.EnginePower, result.EnginePower);
            Assert.Equal(dto.CreatedAt, result.CreatedAt);
            Assert.NotEqual(dto.UpdatedAt, result.UpdatedAt.ToUniversalTime());

            // Uus kontekst kontrollib salvestamist; andmebaasis peab endiselt olema üks kirje.
            using var verificationScope = CreateScope();
            var context = verificationScope.ServiceProvider.GetRequiredService<TARge25ShopContext>();
            var saved = Assert.Single(await context.Spaceships.AsNoTracking().ToListAsync());

            Assert.Equal(dto.Id, saved.Id);
            Assert.Equal(dto.Name, saved.Name);
            Assert.Equal(dto.ShipType, saved.ShipType);
            Assert.Equal(dto.Crew, saved.Crew);
            Assert.Equal(dto.EnginePower, saved.EnginePower);

            // Kontrollime samu kuupäevareegleid ka uuesti andmebaasist loetud kirjel.
            Assert.Equal(domain.CreatedAt, saved.CreatedAt);

            // Teenus kasutab kohalikku aega; UTC teisendus võimaldab võrrelda sama ajavööndi väärtusi.
            Assert.True(saved.UpdatedAt.ToUniversalTime() > dto.UpdatedAt);
            Assert.Equal(result.UpdatedAt, saved.UpdatedAt);
        }

        // Ühine abimeetod annab kaks eristatavat komplekti testiandmeid.
        private static SpaceshipDto MockSpaceshipData(bool isOneOrTwo = false)
        {
            return new SpaceshipDto
            {
                Name = isOneOrTwo ? "RAKETT69" : "X AE a L 12 menuornvöerv",
                ShipType = isOneOrTwo ? "lendav kauss" : "lendav taldrik",
                Crew = isOneOrTwo ? 420 : 67,
                EnginePower = isOneOrTwo ? 999 : 69,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }
}
