using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TARge25Shop.Data.Migrations
{
    // V16 eemaldab kinnisvara pildid EF mudelist; CRUD kasutab ainult RealEstates tabelit.
    public partial class RealEstateWithoutImages : Migration
    {
        // Mudeli muudatus salvestatakse selle migratsiooni Designerisse ja Snapshoti.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // V15 migratsiooni ei kirjutata ümber, sest see võib olla juba rakendatud.
            // Vana RealEstateFiles tabel jääb andmete säilitamiseks andmebaasi,
            // kuid rakendus seda enam ei kaardista ega kasuta. SQL muudatusi ei ole.
        }

        // Eelmise versiooni kood saab kasutada alles hoitud tabelit.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Vana tabel on alles, seega ei ole vaja seda uuesti luua.
        }
    }
}
