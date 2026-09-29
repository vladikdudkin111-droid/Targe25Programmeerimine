// Migratsiooni sihtmudel säilitab selle versiooni tabelite ja seoste kirjelduse.
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TARge25Shop.Data;

#nullable disable

namespace TARge25Shop.Data.Migrations
{
    [DbContext(typeof(TARge25ShopContext))]
    [Migration("20260929090000_AddRealEstate")]
    partial class AddRealEstate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TARge25Shop.Core.Domain.FileToApi", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ExistingFilePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("SpaceshipId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("FileToApis");
                });


            // Kinnisvara põhiandmed ja kümnendarvude täpsus vastavad DbContexti seadistusele.
            modelBuilder.Entity("TARge25Shop.Core.Domain.RealEstate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<decimal>("Area")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Price")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("RoomCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("RealEstates");
                });

            // Failikirjed salvestavad nime ja omaniku tunnuse; indeks toetab piltide päringut.
            modelBuilder.Entity("TARge25Shop.Core.Domain.RealEstateFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ExistingFilePath")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<Guid>("RealEstateId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RealEstateId");

                    b.ToTable("RealEstateFiles");
                });

            modelBuilder.Entity("TARge25Shop.Core.Domain.Spaceship", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Crew")
                        .HasColumnType("int");

                    b.Property<int>("EnginePower")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShipType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Spaceships");
                });

            // Kohustuslik võõrvõti ühendab iga pildi oma kinnisvaraobjektiga.
            modelBuilder.Entity("TARge25Shop.Core.Domain.RealEstateFile", b =>
                {
                    b.HasOne("TARge25Shop.Core.Domain.RealEstate", null)
                        .WithMany()
                        .HasForeignKey("RealEstateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
