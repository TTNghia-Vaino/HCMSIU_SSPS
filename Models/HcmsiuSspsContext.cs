using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HCMSIU_SSPS.Models;

public partial class HcmsiuSspsContext : DbContext
{
    public HcmsiuSspsContext()
    {
    }

    public HcmsiuSspsContext(DbContextOptions<HcmsiuSspsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PrintJob> PrintJobs { get; set; }

    public virtual DbSet<Printer> Printers { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=nghia\\sqlexpress;Initial Catalog=HCMSIU_SSPS;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrintJob>(entity =>
        {
            entity.HasKey(e => e.PrintJobId).HasName("PK__PrintJob__180135C02457328B");

            entity.Property(e => e.PrintJobId)
                .ValueGeneratedNever()
                .HasColumnName("PrintJobID");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PrinterId).HasColumnName("PrinterID");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Printer).WithMany(p => p.PrintJobs)
                .HasForeignKey(d => d.PrinterId)
                .HasConstraintName("FK__PrintJobs__Print__4F7CD00D");

            entity.HasOne(d => d.User).WithMany(p => p.PrintJobs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__PrintJobs__UserI__4E88ABD4");
        });

        modelBuilder.Entity<Printer>(entity =>
        {
            entity.HasKey(e => e.PrinterId).HasName("PK__Printers__D452AB219C290B39");

            entity.Property(e => e.PrinterId)
                .ValueGeneratedNever()
                .HasColumnName("PrinterID");
            entity.Property(e => e.Brand)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsEnable)
                .HasDefaultValue(1)
                .HasColumnName("isEnable");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Model)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PrinterName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__SystemSe__54372AFD7684E650");

            entity.Property(e => e.SettingId)
                .ValueGeneratedNever()
                .HasColumnName("SettingID");
            entity.Property(e => e.LastUpdated).HasColumnType("datetime");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SettingValue)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A4B8407E51B");

            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("TransactionID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Timestamp).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Transacti__UserI__52593CB8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACCCC69E06");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PageBalance).HasDefaultValue(0);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
