using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TARge25Shop.Data.Migrations
{
    // Lisame kinnisvara tabelid; varasemad Spaceshipi tabelid jäävad alles.
    public partial class AddRealEstate : Migration
    {
        // Up rakendatakse versioonile V15 üleminekul üks kord.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RealEstates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RoomCount = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstates", item => item.Id);
                });

            // Iga failikirje kuulub olemasolevale kinnisvaraobjektile.
            migrationBuilder.CreateTable(
                name: "RealEstateFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExistingFilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RealEstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstateFiles", item => item.Id);
                    table.ForeignKey(
                        name: "FK_RealEstateFiles_RealEstates_RealEstateId",
                        column: item => item.RealEstateId,
                        principalTable: "RealEstates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indeks kiirendab ühe kinnisvaraobjekti piltide otsimist.
            migrationBuilder.CreateIndex(
                name: "IX_RealEstateFiles_RealEstateId",
                table: "RealEstateFiles",
                column: "RealEstateId");
        }

        // Down eemaldab selle migratsiooni tabelid ainult tahtliku tagasipööramise korral.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Kõigepealt eemaldame sõltuva tabeli, seejärel kinnisvara tabeli.
            migrationBuilder.DropTable(name: "RealEstateFiles");
            migrationBuilder.DropTable(name: "RealEstates");
        }
    }
}
