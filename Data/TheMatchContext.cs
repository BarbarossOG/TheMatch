using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TheMatch.Models;

namespace TheMatch.Data;

public partial class TheMatchContext : DbContext
{
    public TheMatchContext()
    {
    }

    public TheMatchContext(DbContextOptions<TheMatchContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ЖурналПриложения> ЖурналПриложения { get; set; }

    public virtual DbSet<Пользователи> Пользователи { get; set; }

    public virtual DbSet<ПользователиНазванияУвлечений> ПользователиНазванияУвлечений { get; set; }

    public virtual DbSet<ТипВзаимодействия> ТипВзаимодействия { get; set; }

    public virtual DbSet<Увлечения> Увлечения { get; set; }

    public virtual DbSet<ЧертыПользователя> ЧертыПользователя { get; set; }

    public virtual DbSet<ЧертыХарактера> ЧертыХарактера { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<ЖурналПриложения>(entity =>
        {
            entity.ToTable("Журнал_приложения", tb => tb.HasTrigger("TriggerApplicationLog"));

            entity.HasOne(d => d.IdПользователя1Navigation).WithMany(p => p.ЖурналПриложенияIdПользователя1Navigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Журнал_приложения_ID_Отправителя");

            entity.HasOne(d => d.IdПользователя2Navigation).WithMany(p => p.ЖурналПриложенияIdПользователя2Navigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Журнал_приложения_ID_Получателя");

            entity.HasOne(d => d.IdТипВзаимодействияNavigation).WithMany(p => p.ЖурналПриложения).HasConstraintName("FK_Журнал_приложения_ID_Тип_взаимодействия");
        });


        modelBuilder.Entity<Пользователи>(entity =>
        {
            entity.ToTable("Пользователи", tb => tb.HasTrigger("SetUserDescription"));

            entity.HasMany(d => d.IdУвлечения).WithMany(p => p.IdПользователя)
                .UsingEntity<Dictionary<string, object>>(
                    "УвлеченияПользователя",
                    r => r.HasOne<Увлечения>().WithMany()
                        .HasForeignKey("IdУвлечения")
                        .HasConstraintName("FK_Увлечения_пользователя_ID_Увлечения"),
                    l => l.HasOne<Пользователи>().WithMany()
                        .HasForeignKey("IdПользователя")
                        .HasConstraintName("FK_Увлечения_пользователя_ID_Пользователя"),
                    j =>
                    {
                        j.HasKey("IdПользователя", "IdУвлечения");
                        j.ToTable("Увлечения_пользователя");
                        j.IndexerProperty<int>("IdПользователя").HasColumnName("ID_Пользователя");
                        j.IndexerProperty<byte>("IdУвлечения").HasColumnName("ID_Увлечения");
                    });
        });

        modelBuilder.Entity<ПользователиНазванияУвлечений>(entity =>
        {
            entity.ToView("Пользователи_Названия_Увлечений");
        });

        modelBuilder.Entity<ТипВзаимодействия>(entity =>
        {
            entity.HasKey(e => e.IdТипВзаимодействия).HasName("PK__Тип_взаи__05423EF0B6C94186");
        });

        modelBuilder.Entity<Увлечения>(entity =>
        {
            entity.HasKey(e => e.IdУвлечения).HasName("PK__Увлечени__123964A1E522696A");

            entity.Property(e => e.IdУвлечения).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ЧертыПользователя>(entity =>
        {
            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.ЧертыПользователя).HasConstraintName("FK_Черты_пользователя_ID_Пользователя");

            entity.HasOne(d => d.IdЧертыХарактераNavigation).WithMany(p => p.ЧертыПользователя).HasConstraintName("FK_Черты_пользователя_ID_ЧертыХарактера");
        });

        modelBuilder.Entity<ЧертыХарактера>(entity =>
        {
            entity.HasKey(e => e.IdЧертыХарактера).HasName("PK__ЧертыХар__B20AA3E7C7FD4AEC");

            entity.Property(e => e.IdЧертыХарактера).ValueGeneratedOnAdd();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
