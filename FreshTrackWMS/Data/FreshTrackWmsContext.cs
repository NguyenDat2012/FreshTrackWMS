using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FreshTrackWMS.Data;

public partial class FreshTrackWmsContext : DbContext
{
    public FreshTrackWmsContext()
    {
    }

    public FreshTrackWmsContext(DbContextOptions<FreshTrackWmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietKiemKe> ChiTietKiemKes { get; set; }

    public virtual DbSet<ChiTietPhieuHuy> ChiTietPhieuHuys { get; set; }

    public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }

    public virtual DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }

    public virtual DbSet<LoHang> LoHangs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<PhieuHuy> PhieuHuys { get; set; }

    public virtual DbSet<PhieuKiemKe> PhieuKiemKes { get; set; }

    public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; }

    public virtual DbSet<PhieuXuat> PhieuXuats { get; set; }

    public virtual DbSet<ThucPham> ThucPhams { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-8U6RT0C\\SQL2022;Initial Catalog=FreshTrackWMS;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietKiemKe>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuKiemKe, e.MaLo }).HasName("PK__ChiTietK__32C2D2DD1C625FEE");

            entity.ToTable("ChiTietKiemKe");

            entity.HasOne(d => d.MaLoNavigation).WithMany(p => p.ChiTietKiemKes)
                .HasForeignKey(d => d.MaLo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietKie__MaLo__59063A47");

            entity.HasOne(d => d.MaPhieuKiemKeNavigation).WithMany(p => p.ChiTietKiemKes)
                .HasForeignKey(d => d.MaPhieuKiemKe)
                .HasConstraintName("FK__ChiTietKi__MaPhi__5812160E");
        });

        modelBuilder.Entity<ChiTietPhieuHuy>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuHuy, e.MaLo }).HasName("PK__ChiTietP__6019FFB6FFB1DBBE");

            entity.ToTable("ChiTietPhieuHuy");

            entity.HasOne(d => d.MaLoNavigation).WithMany(p => p.ChiTietPhieuHuys)
                .HasForeignKey(d => d.MaLo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietPhi__MaLo__52593CB8");

            entity.HasOne(d => d.MaPhieuHuyNavigation).WithMany(p => p.ChiTietPhieuHuys)
                .HasForeignKey(d => d.MaPhieuHuy)
                .HasConstraintName("FK__ChiTietPh__MaPhi__5165187F");
        });

        modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuNhap, e.MaLo }).HasName("PK__ChiTietP__6602B34EB4F4B97C");

            entity.ToTable("ChiTietPhieuNhap");

            entity.Property(e => e.HanSuDung).HasColumnType("datetime");
            entity.Property(e => e.NgaySanXuat).HasColumnType("datetime");

            entity.HasOne(d => d.MaLoNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.MaLo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietPhi__MaLo__44FF419A");

            entity.HasOne(d => d.MaPhieuNhapNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.MaPhieuNhap)
                .HasConstraintName("FK__ChiTietPh__MaPhi__440B1D61");
        });

        modelBuilder.Entity<ChiTietPhieuXuat>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuXuat, e.MaLo }).HasName("PK__ChiTietP__54B6E9D7E06D6542");

            entity.ToTable("ChiTietPhieuXuat");

            entity.HasOne(d => d.MaLoNavigation).WithMany(p => p.ChiTietPhieuXuats)
                .HasForeignKey(d => d.MaLo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietPhi__MaLo__4BAC3F29");

            entity.HasOne(d => d.MaPhieuXuatNavigation).WithMany(p => p.ChiTietPhieuXuats)
                .HasForeignKey(d => d.MaPhieuXuat)
                .HasConstraintName("FK__ChiTietPh__MaPhi__4AB81AF0");
        });

        modelBuilder.Entity<LoHang>(entity =>
        {
            entity.HasKey(e => e.MaLo).HasName("PK__LoHang__2725C75657ADC711");

            entity.ToTable("LoHang");

            entity.Property(e => e.HanSuDung).HasColumnType("datetime");
            entity.Property(e => e.NgayNhap).HasColumnType("datetime");

            entity.HasOne(d => d.MaThucPhamNavigation).WithMany(p => p.LoHangs)
                .HasForeignKey(d => d.MaThucPham)
                .HasConstraintName("FK__LoHang__MaThucPh__3D5E1FD2");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762AB5FC8E1");

            entity.ToTable("NguoiDung");

            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenTaiKhoan).HasMaxLength(100);
            entity.Property(e => e.VaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK__NhaCungC__53DA9205CFBED8BE");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.DiaChi).HasMaxLength(500);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenNhaCungCap).HasMaxLength(200);
        });

        modelBuilder.Entity<PhieuHuy>(entity =>
        {
            entity.HasKey(e => e.MaPhieuHuy).HasName("PK__PhieuHuy__126BA3C3B48B332F");

            entity.ToTable("PhieuHuy");

            entity.Property(e => e.NgayHuy).HasColumnType("datetime");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.PhieuHuys)
                .HasForeignKey(d => d.NguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuHuy__NguoiT__4E88ABD4");
        });

        modelBuilder.Entity<PhieuKiemKe>(entity =>
        {
            entity.HasKey(e => e.MaPhieuKiemKe).HasName("PK__PhieuKie__40B08EA8BB6312C5");

            entity.ToTable("PhieuKiemKe");

            entity.Property(e => e.NgayKiemKe).HasColumnType("datetime");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.PhieuKiemKes)
                .HasForeignKey(d => d.NguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuKiem__Nguoi__5535A963");
        });

        modelBuilder.Entity<PhieuNhap>(entity =>
        {
            entity.HasKey(e => e.MaPhieuNhap).HasName("PK__PhieuNha__1470EF3B4F047D6F");

            entity.ToTable("PhieuNhap");

            entity.Property(e => e.NgayNhap).HasColumnType("datetime");

            entity.HasOne(d => d.MaNhaCungCapNavigation).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.MaNhaCungCap)
                .HasConstraintName("FK__PhieuNhap__MaNha__403A8C7D");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.NguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuNhap__Nguoi__412EB0B6");
        });

        modelBuilder.Entity<PhieuXuat>(entity =>
        {
            entity.HasKey(e => e.MaPhieuXuat).HasName("PK__PhieuXua__26C4B5A2FE734C49");

            entity.ToTable("PhieuXuat");

            entity.Property(e => e.NgayXuat).HasColumnType("datetime");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.PhieuXuats)
                .HasForeignKey(d => d.NguoiTao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuXuat__Nguoi__47DBAE45");
        });

        modelBuilder.Entity<ThucPham>(entity =>
        {
            entity.HasKey(e => e.MaThucPham).HasName("PK__ThucPham__3E4339C615DF5ECE");

            entity.ToTable("ThucPham");

            entity.Property(e => e.DanhMuc).HasMaxLength(50);
            entity.Property(e => e.DonViTinh).HasMaxLength(20);
            entity.Property(e => e.PhuongThucBaoQuan).HasMaxLength(100);
            entity.Property(e => e.TenThucPham).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
