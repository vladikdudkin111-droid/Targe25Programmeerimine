using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TARge25Shop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKindergartenImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KindergartenImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KindergartenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KindergartenImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KindergartenImages_Kindergartens_KindergartenId",
                        column: x => x.KindergartenId,
                        principalTable: "Kindergartens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KindergartenImages_KindergartenId",
                table: "KindergartenImages",
                column: "KindergartenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KindergartenImages");
        }
    }
}
