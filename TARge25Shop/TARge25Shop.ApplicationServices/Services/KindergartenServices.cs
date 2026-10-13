using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // Teenus sisaldab lasteaiarühmade salvestamise loogikat.
    public class KindergartenServices : IKindergartenServices
    {
        private readonly TARge25ShopContext _context;
        // DbContext vastutab ankeetide ja piltide andmebaasioperatsioonide eest.
        private readonly IFileService _fileService;
        // Kasutame piltide kontrollimiseks eraldi teenust liidese kaudu.

        public KindergartenServices(TARge25ShopContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
            // Dependency injection annab teenuse konstruktorisse; me ei loo FileService objekti siin ise.
        }

        public async Task<Kindergarten> Create(KindergartenDto dto)
        // Loob uue ankeedi koos piltidega. DTO sisaldab kontrollerist saadud andmeid.
        {
            var images = _fileService.PrepareImages(dto.Files);
            // Kontrollime kõik uued pildid enne salvestamist. Vigase faili korral katkeb meetod enne SaveChangesAsync kutset.
            // ID ja kuupäevad määrame serveris, mitte vormi saadetud väärtuste põhjal.
            var now = DateTime.Now;
            // Kasutame loomisel sama aega nii CreatedAt kui ka UpdatedAt jaoks.
            var kindergarten = new Kindergarten
            {
                Id = Guid.NewGuid(),
                // Uue ankeedi ID luuakse serveris, mitte kasutaja saadetud väärtuse põhjal.
                GroupName = dto.GroupName,
                ChildrenCount = dto.ChildrenCount,
                KindergartenName = dto.KindergartenName,
                TeacherName = dto.TeacherName,
                CreatedAt = now,
                UpdatedAt = now,
                Images = images
                // Seome uued pildiobjektid ankeediga; Entity Framework määrab neile selle ankeedi KindergartenId väärtuse.
            };

            _context.Kindergartens.Add(kindergarten);
            // Märgime uue ankeedi ja selle pildid lisamiseks. Add ise veel SQL-andmeid ei salvesta.
            await _context.SaveChangesAsync();
            // Saadame jälgitavad muudatused andmebaasi ja ootame salvestamise lõppu asünkroonselt.
            return kindergarten;
            // Tagastame loodud või muudetud objekti; Delete korral on see juba andmebaasist eemaldatud objekt.
        }

        public async Task<Kindergarten?> Update(KindergartenDto dto)
        // Muudab olemasolevat ankeeti ja lisab uued pildid; vanu pilte see meetod ei eemalda.
        {
            var kindergarten = await _context.Kindergartens.FindAsync(dto.Id);
            // Otsime ankeedi primaarvõtme järgi. Leitud objekti muudatusi jälgib DbContext.
            if (kindergarten == null)
            {
                return null;
                // Kirjet ei leitud; kontroller saab tagastada NotFound tulemuse.
            }

            var images = _fileService.PrepareImages(dto.Files);
            // Kontrollime kõik uued pildid enne salvestamist. Vigase faili korral katkeb meetod enne SaveChangesAsync kutset.
            foreach (var image in images)
            {
                image.KindergartenId = kindergarten.Id;
                // Seome iga uue pildi olemasoleva ankeediga välisvõtme kaudu.
                _context.KindergartenImages.Add(image);
                // Märgime uue pildi lisamiseks; olemasolevaid pildikirjeid ei muudeta.
            }

            // Loomise aeg jääb muutmata; uuendame ainult muudetavaid välju ja muutmise aega.
            kindergarten.GroupName = dto.GroupName;
            kindergarten.ChildrenCount = dto.ChildrenCount;
            kindergarten.KindergartenName = dto.KindergartenName;
            kindergarten.TeacherName = dto.TeacherName;
            kindergarten.UpdatedAt = DateTime.Now;
            // Uuendame muutmise aega. CreatedAt jääb muutmata.

            await _context.SaveChangesAsync();
            // Saadame jälgitavad muudatused andmebaasi ja ootame salvestamise lõppu asünkroonselt.
            return kindergarten;
            // Tagastame loodud või muudetud objekti; Delete korral on see juba andmebaasist eemaldatud objekt.
        }

        public async Task<Kindergarten?> DetailAsync(Guid id)
        // Tagastab ankeedi põhiandmed; galerii laadib kontroller eraldi FileService kaudu.
        {
            // Puuduva kirje korral tagastame null.
            return await _context.Kindergartens.FindAsync(id);
            // Otsime ID järgi; puuduva ankeedi korral tagastame null.
        }

        public async Task<Kindergarten?> Delete(Guid id)
        // Kustutab kogu ankeedi. Seotud pildid eemaldab andmebaasi kaskaadkustutus.
        {
            var kindergarten = await _context.Kindergartens.FindAsync(id);
            // Otsime kustutatava ankeedi; piltide sisu pole vaja selleks mällu laadida.
            if (kindergarten == null)
            {
                return null;
                // Kirjet ei leitud; kontroller saab tagastada NotFound tulemuse.
            }

            // Võõrvõtme kaskaadkustutus eemaldab pildid ka siis, kui neid pole laaditud.
            _context.Kindergartens.Remove(kindergarten);
            // Märgime ankeedi kustutamiseks. Cascade seos tagab ka seotud pildikirjete eemaldamise.
            await _context.SaveChangesAsync();
            // Saadame jälgitavad muudatused andmebaasi ja ootame salvestamise lõppu asünkroonselt.
            return kindergarten;
            // Tagastame loodud või muudetud objekti; Delete korral on see juba andmebaasist eemaldatud objekt.
        }
    }
}
