using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TARge25Shop.Data.Migrations
{
    // Migratsioon lisab andmebaasi tabeli, milles hoitakse üleslaaditud failide seoseid.
    public partial class AddFileToApi : Migration
    {
        // Up meetod rakendatakse siis, kui migratsioon andmebaasi lisatakse.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Loome FileToApis tabeli õpetaja näites kasutatud kolme väljaga.
            migrationBuilder.CreateTable(
                name: "FileToApis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExistingFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpaceshipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    // Id on FileToApis tabeli primaarvõti.
                    table.PrimaryKey("PK_FileToApis", x => x.Id);
                });

            // Indeks kiirendab ühe kosmoselaeva failide otsimist SpaceshipId järgi.
            migrationBuilder.CreateIndex(
                name: "IX_FileToApis_SpaceshipId",
                table: "FileToApis",
                column: "SpaceshipId");
        }

        // Down meetod võtab migratsiooni tagasi ja eemaldab loodud tabeli.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileToApis");
        }
    }
}
