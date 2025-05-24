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

    public virtual DbSet<Должность> Должностьs { get; set; }

    public virtual DbSet<ЖурналДействий> ЖурналДействийs { get; set; }

    public virtual DbSet<ЖурналПриложения> ЖурналПриложенияs { get; set; }

    public virtual DbSet<КупленныеПодписки> КупленныеПодпискиs { get; set; }

    public virtual DbSet<Модератор> Модераторs { get; set; }

    public virtual DbSet<МодерацияСообщений> МодерацияСообщенийs { get; set; }

    public virtual DbSet<НеактивныеПользователи> НеактивныеПользователиs { get; set; }

    public virtual DbSet<Подписка> Подпискаs { get; set; }

    public virtual DbSet<Пользователи> Пользователиs { get; set; }

    public virtual DbSet<ПользователиНазванияУвлечений> ПользователиНазванияУвлеченийs { get; set; }

    public virtual DbSet<ПринятыеПредложения> ПринятыеПредложенияs { get; set; }

    public virtual DbSet<ПросмотрСообщенийПользователей> ПросмотрСообщенийПользователейs { get; set; }

    public virtual DbSet<Сообщения> Сообщенияs { get; set; }

    public virtual DbSet<СообщенияПользователей> СообщенияПользователейs { get; set; }

    public virtual DbSet<ТипВзаимодействия> ТипВзаимодействияs { get; set; }

    public virtual DbSet<ТипСообщения> ТипСообщенияs { get; set; }

    public virtual DbSet<ТипыОпераций> ТипыОперацийs { get; set; }

    public virtual DbSet<Увлечения> Увлеченияs { get; set; }

    public virtual DbSet<УлучшеннаяМодерацияСообщений> УлучшеннаяМодерацияСообщенийs { get; set; }

    public virtual DbSet<ЧертыПользователя> ЧертыПользователяs { get; set; }

    public virtual DbSet<ЧертыХарактера> ЧертыХарактераs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=OG-BARBAROSS;Database=Erimeev_1415_TheMatch;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Должность>(entity =>
        {
            entity.HasKey(e => e.IdДолжности).HasName("PK__Должност__9A158B4728052D68");
        });

        modelBuilder.Entity<ЖурналДействий>(entity =>
        {
            entity.HasKey(e => new { e.IdМодератора, e.IdПользователя, e.IdТипОперации, e.ДатаИВремя }).HasName("PK_Журнал_действий_ID_Модератора_ID_Пользователя_ID_Тип_операции_Дата_и_время");

            entity.ToTable("Журнал_действий", tb => tb.HasTrigger("CheckOperationPermission"));

            entity.HasOne(d => d.IdМодератораNavigation).WithMany(p => p.ЖурналДействийs).HasConstraintName("FK_Журнал_действий_ID_Модератора");

            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.ЖурналДействийs).HasConstraintName("FK_Журнал_действий_ID_Пользователя");

            entity.HasOne(d => d.IdТипОперацииNavigation).WithMany(p => p.ЖурналДействийs).HasConstraintName("FK_Журнал_действий_ID_Тип_операции");
        });

        modelBuilder.Entity<ЖурналПриложения>(entity =>
        {
            entity.ToTable("Журнал_приложения", tb => tb.HasTrigger("TriggerApplicationLog"));

            entity.HasOne(d => d.IdПользователя1Navigation).WithMany(p => p.ЖурналПриложенияIdПользователя1Navigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Журнал_приложения_ID_Отправителя");

            entity.HasOne(d => d.IdПользователя2Navigation).WithMany(p => p.ЖурналПриложенияIdПользователя2Navigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Журнал_приложения_ID_Получателя");

            entity.HasOne(d => d.IdТипВзаимодействияNavigation).WithMany(p => p.ЖурналПриложенияs).HasConstraintName("FK_Журнал_приложения_ID_Тип_взаимодействия");
        });

        modelBuilder.Entity<КупленныеПодписки>(entity =>
        {
            entity.HasOne(d => d.IdПодпискиNavigation).WithMany(p => p.КупленныеПодпискиs).HasConstraintName("FK_Купленные_подписки_ID_Подписки");

            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.КупленныеПодпискиs).HasConstraintName("FK_Купленные_подписки_ID_Пользователя");
        });

        modelBuilder.Entity<Модератор>(entity =>
        {
            entity.HasKey(e => e.IdМодератора).HasName("PK__Модерато__C9E47C63EC68AE29");

            entity.Property(e => e.IdДолжности).HasDefaultValue((byte)4);

            entity.HasOne(d => d.IdДолжностиNavigation).WithMany(p => p.Модераторs).HasConstraintName("FK_Модератор_ID_Должности");
        });

        modelBuilder.Entity<МодерацияСообщений>(entity =>
        {
            entity.HasKey(e => new { e.IdМодератора, e.IdСообщения }).HasName("PK_Модерация_сообщений_ID_Модератора_ID_Сообщения");

            entity.ToTable("Модерация_сообщений", tb => tb.HasTrigger("DeleteMessage"));

            entity.HasOne(d => d.IdМодератораNavigation).WithMany(p => p.МодерацияСообщенийs).HasConstraintName("FK_Модерация_сообщений_ID_Модератора");

            entity.HasOne(d => d.IdСообщенияNavigation).WithMany(p => p.МодерацияСообщенийs).HasConstraintName("FK_Модерация_сообщений_ID_Сообщения");
        });

        modelBuilder.Entity<НеактивныеПользователи>(entity =>
        {
            entity.ToView("НеактивныеПользователи");

            entity.Property(e => e.IdПользователя).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Подписка>(entity =>
        {
            entity.HasKey(e => e.IdПодписки).HasName("PK__Подписка__ED5BA477FAA83526");
        });

        modelBuilder.Entity<Пользователи>(entity =>
        {
            entity.ToTable("Пользователи", tb => tb.HasTrigger("SetUserDescription"));

            entity.HasMany(d => d.IdУвлеченияs).WithMany(p => p.IdПользователяs)
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

        modelBuilder.Entity<ПринятыеПредложения>(entity =>
        {
            entity.ToView("Принятые_предложения");
        });

        modelBuilder.Entity<ПросмотрСообщенийПользователей>(entity =>
        {
            entity.ToView("Просмотр_Сообщений_Пользователей");
        });

        modelBuilder.Entity<Сообщения>(entity =>
        {
            entity.HasKey(e => e.IdСообщения).HasName("PK__Сообщени__DA881CE4F00E48E1");
        });

        modelBuilder.Entity<СообщенияПользователей>(entity =>
        {
            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.СообщенияПользователейs).HasConstraintName("FK_Сообщения_пользователей_ID_Пользователя");

            entity.HasOne(d => d.IdСообщенияNavigation).WithMany(p => p.СообщенияПользователейs).HasConstraintName("FK_Сообщения_пользователей_ID_Сообщения");

            entity.HasOne(d => d.IdТипаСообщенияNavigation).WithMany(p => p.СообщенияПользователейs).HasConstraintName("FK_Сообщения_пользователей_ID_Типа_сообщения");
        });

        modelBuilder.Entity<ТипВзаимодействия>(entity =>
        {
            entity.HasKey(e => e.IdТипВзаимодействия).HasName("PK__Тип_взаи__05423EF0B6C94186");
        });

        modelBuilder.Entity<ТипСообщения>(entity =>
        {
            entity.HasKey(e => e.IdТипаСообщения).HasName("PK__Тип_сооб__010D129F5896BDC8");
        });

        modelBuilder.Entity<ТипыОпераций>(entity =>
        {
            entity.HasKey(e => e.IdТипОперации).HasName("PK__Типы_опе__C1FB718649048F38");
        });

        modelBuilder.Entity<Увлечения>(entity =>
        {
            entity.HasKey(e => e.IdУвлечения).HasName("PK__Увлечени__123964A1E522696A");

            entity.Property(e => e.IdУвлечения).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<УлучшеннаяМодерацияСообщений>(entity =>
        {
            entity.ToView("УлучшеннаяМодерацияСообщений");
        });

        modelBuilder.Entity<ЧертыПользователя>(entity =>
        {
            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.ЧертыПользователяs).HasConstraintName("FK_Черты_пользователя_ID_Пользователя");

            entity.HasOne(d => d.IdЧертыХарактераNavigation).WithMany(p => p.ЧертыПользователяs).HasConstraintName("FK_Черты_пользователя_ID_ЧертыХарактера");
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
