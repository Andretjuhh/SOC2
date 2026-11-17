using System;
using CoreApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace coreApi.Migrations
{
    [DbContext(typeof(SOC2DBContext))]
    [Migration("20261103091214_InitialAccounts")]
    partial class InitialAccounts
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CoreApi.Domain.Gebruiker", b =>
                {
                    b.Property<Guid>("GebruikerID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gebruikersnaam")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("GenormaliseerdeGebruikersnaam")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("IsActief")
                        .HasColumnType("bit");

                    b.Property<Guid?>("KlantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Naam")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RolID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("WachtwoordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("GebruikerID");

                    b.HasIndex("GenormaliseerdeGebruikersnaam")
                        .IsUnique();

                    b.HasIndex("KlantID");

                    b.HasIndex("RolID");

                    b.ToTable("Gebruikers");
                });

            modelBuilder.Entity("CoreApi.Domain.Klant", b =>
                {
                    b.Property<Guid>("KlantID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SLA")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isActief")
                        .HasColumnType("bit");

                    b.Property<string>("naam")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("telefoonnummer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("KlantID");

                    b.ToTable("Klanten");
                });

            modelBuilder.Entity("CoreApi.Domain.Recht", b =>
                {
                    b.Property<Guid>("RechtID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Beschrijving")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Naam")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("RechtID");

                    b.HasIndex("Naam")
                        .IsUnique();

                    b.ToTable("Rechten");
                });

            modelBuilder.Entity("CoreApi.Domain.Rol", b =>
                {
                    b.Property<Guid>("RolID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Beschrijving")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Naam")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("RolID");

                    b.HasIndex("Naam")
                        .IsUnique();

                    b.ToTable("Rollen");
                });

            modelBuilder.Entity("RechtRol", b =>
                {
                    b.Property<Guid>("RechtenRechtID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RollenRolID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("RechtenRechtID", "RollenRolID");

                    b.HasIndex("RollenRolID");

                    b.ToTable("RechtRol");
                });

            modelBuilder.Entity("CoreApi.Domain.Gebruiker", b =>
                {
                    b.HasOne("CoreApi.Domain.Klant", "Klant")
                        .WithMany("Gebruikers")
                        .HasForeignKey("KlantID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("CoreApi.Domain.Rol", "Rol")
                        .WithMany("Gebruikers")
                        .HasForeignKey("RolID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Klant");

                    b.Navigation("Rol");
                });

            modelBuilder.Entity("RechtRol", b =>
                {
                    b.HasOne("CoreApi.Domain.Recht", null)
                        .WithMany()
                        .HasForeignKey("RechtenRechtID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoreApi.Domain.Rol", null)
                        .WithMany()
                        .HasForeignKey("RollenRolID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CoreApi.Domain.Klant", b =>
                {
                    b.Navigation("Gebruikers");
                });

            modelBuilder.Entity("CoreApi.Domain.Rol", b =>
                {
                    b.Navigation("Gebruikers");
                });
#pragma warning restore 612, 618
        }
    }
}
