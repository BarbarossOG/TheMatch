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

    public virtual DbSet<УвлеченияПользователя> УвлеченияПользователяs { get; set; }

    public virtual DbSet<ИзображенияПрофиля> ИзображенияПрофиля { get; set; }

    
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

        modelBuilder.Entity<УвлеченияПользователя>(entity =>
        {
            entity.HasKey(e => new { e.ID_Пользователя, e.ID_Увлечения });
            entity.ToTable("Увлечения_пользователя");

            entity.HasOne(e => e.Пользователь)
                .WithMany(u => u.УвлеченияПользователяs)
                .HasForeignKey(e => e.ID_Пользователя)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Увлечение)
                .WithMany(h => h.UserHobbies)
                .HasForeignKey(e => e.ID_Увлечения)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ИзображенияПрофиля>(entity =>
        {
            entity.ToTable("ИзображенияПрофиля");
            entity.HasKey(e => e.ID_Изображения);
            entity.Property(e => e.Ссылка).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Основное).HasDefaultValue(false);
            entity.HasOne(e => e.Пользователь)
                .WithMany(u => u.ИзображенияПрофиля)
                .HasForeignKey(e => e.ID_Пользователя)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Изображения_Пользователь");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
