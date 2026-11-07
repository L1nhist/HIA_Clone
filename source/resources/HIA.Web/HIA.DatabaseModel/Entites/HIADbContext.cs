using System;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Logging;


#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class HIADbContext : DbContext
    {
        public HIADbContext()
        {
        }

        public HIADbContext(DbContextOptions<HIADbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CheckThe4210> CheckThe4210s { get; set; }
        public virtual DbSet<ChiDinhDongThoi> ChiDinhDongThois { get; set; }
        public virtual DbSet<DichVuMaMay> DichVuMaMays { get; set; }
        public virtual DbSet<DMBVTuyenTinh> DMBVTuyenTinhs { get; set; }
        public virtual DbSet<DMBacSi> DMBacSis { get; set; }
        public virtual DbSet<DMBenh> DMBenhs { get; set; }
        public virtual DbSet<DMBenhPhanLoai> DMBenhPhanLoais { get; set; }
        public virtual DbSet<DMCacLoai> DMCacLoais { get; set; }
        public virtual DbSet<DMChung130> DMChung130s { get; set; }
        public virtual DbSet<DMDacBiet> DMDacBiets { get; set; }
        public virtual DbSet<DMDichVu> DMDichVus { get; set; }
        public virtual DbSet<DMDVThuCong> DMDVThuCongs { get; set; }
        public virtual DbSet<DMError130> DMError130s { get; set; }
        public virtual DbSet<DMErrorMHTC> DMErrorMHTCs { get; set; }
        public virtual DbSet<DMGiuong> DMGiuongs { get; set; }
        public virtual DbSet<DMDVGiuong> DMDVGiuongs { get; set; }
        public virtual DbSet<DMMaGiuong> DMMaGiuongs { get; set; }
        public virtual DbSet<DMCSKCB> DMCSKCBs { get; set; }
        public virtual DbSet<DMKhac> DMKhacs { get; set; }
        public virtual DbSet<DMKhoa> DMKhoas { get; set; }
        public virtual DbSet<DMKhoaBV> DMKhoaBVs { get; set; }
        public virtual DbSet<DMNhomBenh> DMNhomBenhs { get; set; }
        public virtual DbSet<DMPhauThuat> DMPhauThuats { get; set; }
        public virtual DbSet<DMThuoc> DMThuocs { get; set; }
        public virtual DbSet<DMTuyenBV> DMTuyenBVs { get; set; }
        public virtual DbSet<DMThietBi> DMThietBis { get; set; }
        public virtual DbSet<DMVTYT> DMVTYTs { get; set; }
        public virtual DbSet<DMYHDTICD10> DMYHDTICD10s { get; set; }
        public virtual DbSet<HDSDDichVu> HDSDDichVus { get; set; }
        public virtual DbSet<HDSDThuoc> HDSDThuocs { get; set; }
        public virtual DbSet<Hospital> Hospitals { get; set; }
        public virtual DbSet<PhacDoDieuTri> PhacDoDieuTris { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<SysHistoryLog> SysHistoryLogs { get; set; }
        public virtual DbSet<ThongKeBenh> ThongKeBenhs { get; set; }
        public virtual DbSet<ThongKeNgay> ThongKeNgays { get; set; }
        public virtual DbSet<User> Users { get; set; }
        //public virtual DbSet<UserKhoa> UserKhoas { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<XML01> XML01s { get; set; }
        public virtual DbSet<XML02> XML02s { get; set; }
        public virtual DbSet<XML03> XML03s { get; set; }
        public virtual DbSet<XML04> XML04s { get; set; }
        public virtual DbSet<XML05> XML05s { get; set; }
        public virtual DbSet<XML06> XML06s { get; set; }
        public virtual DbSet<XML07> XML07s { get; set; }
        public virtual DbSet<XML08> XML08s { get; set; }
        public virtual DbSet<XML09> XML09s { get; set; }
        public virtual DbSet<XML10> XML10s { get; set; }
        public virtual DbSet<XML11> XML11s { get; set; }
        public virtual DbSet<XML12> XML12s { get; set; }
        public virtual DbSet<XML13> XML13s { get; set; }
        public virtual DbSet<XML14> XML14s { get; set; }
        public virtual DbSet<XML15> XML15s { get; set; }
        public virtual DbSet<XMLState> XML00s { get; set; }
        //public virtual DbSet<XMLCheckIn> XMLCheckIns { get; set; }
        //public virtual DbSet<XMLResult> XMLResults { get; set; }
        public virtual DbSet<XMLError> XMLErrors { get; set; }
        //public virtual DbSet<XMLState> XMLStates { get; set; }
        public virtual DbSet<XMLValidate> XMLValidates { get; set; }
        //public virtual DbSet<XMLVerify> XMLVerifies { get; set; }

        private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "Logs", "ef-log.txt");
            builder
                .AddProvider(new FileLoggerProvider(logPath))
                .AddFilter((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name && level >= LogLevel.Information);
        });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {

                var logPath = Path.Combine(AppContext.BaseDirectory, "ef-log.txt");
                optionsBuilder.UseSqlServer("Server=172.16.101.188,1433;Initial Catalog=HIA_NEO;Trusted_Connection=True;Persist Security Info=True")
                                 .UseLoggerFactory(_loggerFactory)
                                .EnableSensitiveDataLogging(); // hiển thị giá trị tham số như @__maCSKCB_0
            }

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<CheckThe4210>(entity =>
            {
                entity.HasKey(e => e.ID);

                entity.ToTable("CheckThe4210");

                entity.Property(e => e.GT_THE_DEN).HasMaxLength(250);

                entity.Property(e => e.GT_THE_TU).HasMaxLength(250);

                entity.Property(e => e.HO_TEN).HasMaxLength(100);

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MA_DKBD).HasMaxLength(250);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_THE_BHYT).HasMaxLength(250);

                entity.Property(e => e.MA_KHUVUC).HasMaxLength(15);

                entity.Property(e => e.NGAY_SINH).HasColumnType("datetime");

                entity.Property(e => e.TG_CHECK_THE).HasColumnType("datetime");
            });

            modelBuilder.Entity<ChiDinhDongThoi>(entity =>
            {
                entity.HasKey(e => e.CDDTID)
                    .HasName("PK__ChiDinhD__AE8D30533ABC5111");

                entity.ToTable("ChiDinhDongThoi");

                entity.Property(e => e.CHI_DINH).HasDefaultValueSql("((0))");

                entity.Property(e => e.DICH_VU_KHAC).HasDefaultValueSql("('')");

                entity.Property(e => e.HIEU_LUC).HasDefaultValueSql("((1))");

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MA_DICH_VU)
                    .HasMaxLength(1000)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MA_LOI).HasDefaultValueSql("('')");

                entity.Property(e => e.VAT_TU_BHYT).HasDefaultValueSql("('')");

                entity.Property(e => e.VAT_TU_KO_BH).HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<DichVuMaMay>(entity =>
            {
                entity.HasKey(e => new { e.DVMMID, e.MA_CSKCB });
                entity.Property(e => e.DVMMID).ValueGeneratedOnAdd();

                entity.ToTable("DichVuMaMay");

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MA_DICH_VU)
                    .HasMaxLength(3000)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MA_MAY).HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<DMBVTuyenTinh>(entity =>
            {
                entity.HasKey(e => e.BVTID)
                    .HasName("PK__DMBVTuye__2AC6104BA0CB21A2");

                entity.ToTable("DMBVTuyenTinh");

                entity.Property(e => e.BVTCode).HasMaxLength(50);

                entity.Property(e => e.BVTName).HasMaxLength(255);
            });

            modelBuilder.Entity<DMBacSi>(entity =>
            {
                entity.HasKey(e => new { e.BSID, e.MA_CSKCB })
                    .HasName("PK__DMBacSi__F7FD4BB37759E85B");

                entity.ToTable("DMBacSi");

                entity.Property(e => e.BSID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CHUC_DANH)
                    .HasMaxLength(250)
                    .HasComment("des: Chức danh");

                entity.Property(e => e.VI_TRI)
                    .HasMaxLength(250)
                    .HasComment("des: Vị trí");

                //entity.Property(e => e.CHUCDANH_NN).HasMaxLength(250);

                //entity.Property(e => e.CHUNGCHI_KHAC)
                //    .HasMaxLength(1000)
                //    .HasComment("des: Chứng chỉ khác");

                entity.Property(e => e.CSKCB_CGKT).HasMaxLength(30);

                entity.Property(e => e.CSKCB_KHAC).HasMaxLength(30);

                //entity.Property(e => e.DIA_CHI)
                //    .HasMaxLength(1000)
                //    .HasComment("des: Địa chỉ");

                //entity.Property(e => e.DIEN_THOAI)
                //    .HasMaxLength(50)
                //    .HasComment("des: Điện thoại");

                entity.Property(e => e.DVKT_KHAC).HasMaxLength(3000);

                //entity.Property(e => e.EMAIL)
                //    .HasMaxLength(250)
                //    .HasComment("des: Email");

                entity.Property(e => e.GIOI_TINH).HasComment("des: Giới tính");

                //entity.Property(e => e.HOCHAM_HOCVI)
                //    .HasMaxLength(250)
                //    .HasComment("des: Học hàm học vị");

                entity.Property(e => e.HO_TEN).HasMaxLength(150);

                //entity.Property(e => e.ID).HasMaxLength(250);

                //entity.Property(e => e.KHOA_BOPHAN)
                //    .HasMaxLength(50)
                //    .HasComment("des: Khoa - Bộ phận");

                //entity.Property(e => e.LOAI)
                //    .HasMaxLength(250)
                //    .HasComment("des: Loại");

                entity.Property(e => e.MA_CCHN)
                    .HasMaxLength(50)
                    .HasComment("des: Mã CCHN");

                //entity.Property(e => e.MANHANVIEN)
                //    .HasMaxLength(50)
                //    .HasComment("des: Mã nhân viên");

                entity.Property(e => e.MA_BHXH).HasMaxLength(50);

                //entity.Property(e => e.MA_CHUYENNGANH)
                //    .HasMaxLength(50)
                //    .HasComment("des: Mã chuyên ngành");

                entity.Property(e => e.MA_KHOA)
                    .HasMaxLength(350)
                    .HasComment("des: Mã khoa");

                //entity.Property(e => e.MA_LOAI_KCB).HasMaxLength(250);

                entity.Property(e => e.NGAYCAP_CCHN)
                    .HasMaxLength(20)
                    .HasComment("des: Ngày cấp CCHN");

                //entity.Property(e => e.NGAY_SINH)
                //    .HasMaxLength(50)
                //    .HasComment("des: Ngày sinh");

                entity.Property(e => e.NOICAP_CCHN)
                    .HasMaxLength(250)
                    .HasComment("des: Nơi cấp CCHN");

                entity.Property(e => e.PHAMVI_CM).HasMaxLength(250);

                entity.Property(e => e.PHAMVI_CMBS).HasMaxLength(500);

                entity.Property(e => e.QD_CGKT).HasMaxLength(250);

                //entity.Property(e => e.STT).HasMaxLength(250);

                entity.Property(e => e.TEN_KHOA).HasMaxLength(1000);

                //entity.Property(e => e.TEN_NHANVIEN)
                //    .HasMaxLength(250)
                //    .HasComment("des: Họ và tên;title");

                //entity.Property(e => e.TEN_VIETTAT)
                //    .HasMaxLength(250)
                //    .HasComment("des: Tên viết tắt");

                entity.Property(e => e.TG_DANGKY)
                    .HasMaxLength(50)
                    .HasComment("des: Thời gian đăng ký");

                entity.Property(e => e.TG_NGAY)
                    .HasMaxLength(500)
                    .HasComment("des: Thời gian ngày");

                entity.Property(e => e.TG_TUAN)
                    .HasMaxLength(300)
                    .HasComment("des: Thời gian tuần");

                entity.Property(e => e.TU_NGAY)
                    .HasMaxLength(50)
                    .HasComment("des: Từ ngày");

                entity.Property(e => e.DEN_NGAY)
                    .HasMaxLength(50)
                    .HasComment("des: Đến ngày");

                entity.Property(e => e.SO_CCCD)
                    .IsUnicode(false)
                    .HasMaxLength(20);

                entity.Property(e => e.VB_PHANCONG).HasMaxLength(300);
            });

            modelBuilder.Entity<DMBenh>(entity =>
            {
                entity.HasKey(e => e.BenhID)
                    .HasName("PK__DMBenh__145E310CFCCAD67D");

                entity.ToTable("DMBenh");

                entity.Property(e => e.BenhCode).HasMaxLength(255);

                entity.Property(e => e.BenhLBCode).HasMaxLength(255);

                entity.Property(e => e.BenhMaChiTiet).HasMaxLength(255);

                entity.Property(e => e.BenhMaGocBS_MCT).HasMaxLength(255);

                entity.Property(e => e.BenhMaNhomBC_BYT).HasMaxLength(255);

                entity.Property(e => e.TenBenh_Modify).HasMaxLength(255);

                entity.Property(e => e.TenBenh_Original_EN).HasMaxLength(255);

                entity.Property(e => e.TenBenh_Original_VN).HasMaxLength(255);

                entity.HasOne(d => d.BenhLB)
                    .WithMany(p => p.DMBenhs)
                    .HasForeignKey(d => d.BenhLBID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DMBenh_DMBenhPhanLoai");
            });

            modelBuilder.Entity<DMBenhPhanLoai>(entity =>
            {
                entity.HasKey(e => e.LBID)
                    .HasName("PK__DMBenhPh__6B4D18589D3D83C5");

                entity.ToTable("DMBenhPhanLoai");

                entity.Property(e => e.LBCode).HasMaxLength(255);

                entity.Property(e => e.LBTenLoai_EN).HasMaxLength(255);

                entity.Property(e => e.LBTenLoai_VN).HasMaxLength(255);

                entity.Property(e => e.NBCode).HasMaxLength(255);
            });

            modelBuilder.Entity<DMCacLoai>(entity =>
            {
                entity.HasKey(e => e.DMID)
                    .HasName("PK__DMCacLoa__28BFED6140E09D0B");

                entity.ToTable("DMCacLoai");

                entity.Property(e => e.DMCode).HasMaxLength(50);

                entity.Property(e => e.DMName).HasMaxLength(255);

                entity.Property(e => e.DMType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DMChung130>(entity =>
            {
                entity.HasKey(e => e.ID);

                entity.ToTable("DMChung130");

                entity.Property(e => e.MA).HasMaxLength(50);

                entity.Property(e => e.TEN).HasMaxLength(500);

                entity.Property(e => e.LOAI).HasMaxLength(15);
            });

            modelBuilder.Entity<DMDacBiet>(entity =>
            {
                entity.ToTable("DMDacBiet");

                entity.Property(e => e.DMDacBietName)
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.DMDacBietValue).HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<DMDichVu>(entity =>
            {
                entity.HasKey(e => new { e.DVID, e.MA_CSKCB })
                    .HasName("PK__DMDichVu__2168A14671934E0F");

                entity.ToTable("DMDichVu");

                entity.Property(e => e.DVID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MANHOM_5937).HasMaxLength(50);

                entity.Property(e => e.MANHOM_9324)
                    .HasMaxLength(50);

                entity.Property(e => e.MA_DICH_VU).HasMaxLength(250);

                entity.Property(e => e.MA_GIA)
                    .HasMaxLength(250);

                entity.Property(e => e.QUYET_DINH)
                    .HasMaxLength(250);

                entity.Property(e => e.TEN_DICH_VU).HasMaxLength(1000);
            });

            modelBuilder.Entity<DMDVThuCong>(entity =>
            {
                entity.HasKey(e => new { e.DVTCID, e.MA_CSKCB });

                entity.ToTable("DMDVThuCong");

                entity.Property(e => e.DVTCID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MANHOM_5937).HasMaxLength(50);

                entity.Property(e => e.MANHOM_9324)
                    .HasMaxLength(50);

                entity.Property(e => e.MA_DICH_VU).HasMaxLength(250);

                entity.Property(e => e.MA_GIA)
                    .HasMaxLength(250);

                entity.Property(e => e.QUYET_DINH)
                    .HasMaxLength(250);

                entity.Property(e => e.TEN_DICH_VU).HasMaxLength(1000);
            });

            modelBuilder.Entity<DMError130>(entity =>
            {
                entity.HasKey(e => e.ErrorId)
                    .HasName("PK_DMError130");

                entity.ToTable("DMError130");

                entity.Property(e => e.ErrorId).ValueGeneratedOnAdd();

                entity.Property(e => e.ErrorCode).HasComment("des: Mã lỗi");

                entity.Property(e => e.ErrorFile)
                    .HasMaxLength(10)
                    .HasComment("des: Tệp XML");

                entity.Property(e => e.ErrorTag)
                    .HasMaxLength(50)
                    .HasComment("des: Thẻ XML");

                entity.Property(e => e.Comment)
                    .HasMaxLength(500)
                    .HasComment("des: Ghi chú");

                entity.Property(e => e.Message)
                    .HasMaxLength(1000)
                    .HasComment("des: Nội dung");

                entity.Property(e => e.ErrorCatetory)
                    .HasMaxLength(500);

                entity.Property(e => e.ErrorExplanation)
                    .HasMaxLength(500);

                entity.Property(e => e.ErrorKeyword)
                    .HasMaxLength(500);

                entity.Property(e => e.FixSugesstion)
                    .HasMaxLength(500);

                entity.Property(e => e.HospitalScope)
                    .HasMaxLength(500);

                entity.Property(e => e.ApplicableDecision)
                    .HasMaxLength(500);

                entity.Property(e => e.ApplicableLink)
                    .HasMaxLength(500);

                entity.Property(e => e.ReferenceExcept)
                    .HasMaxLength(500);

                entity.Property(e => e.RelatedField)
                    .HasMaxLength(500);

                entity.Property(e => e.Response)
                    .HasMaxLength(500);

                entity.Property(e => e.TriggerCondition)
                    .HasMaxLength(500);

                entity.Property(e => e.ErrorType).HasComment("des: Loại lỗi");
            });

            modelBuilder.Entity<DMErrorMHTC>(entity =>
            {
                entity.HasKey(e => new { e.EMID, e.MA_CSKCB })
                    .HasName("PK__DMErrorM__895E8407CAF82C8F");

                entity.ToTable("DMErrorMHTC");

                entity.Property(e => e.EMID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.EMCreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("default");

                entity.Property(e => e.EMCreator)
                    .HasMaxLength(100)
                    .HasComment("curentuserid;readonly");

                entity.Property(e => e.EMEnabled).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<DMGiuong>(entity =>
            {
                entity.HasKey(e => new { e.GiuongID, e.MA_CSKCB })
                    .HasName("PK__DMGiuong__436CF770DFA040F5");

                entity.ToTable("DMGiuong");

                entity.Property(e => e.GiuongID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.GiuongClass).HasComment("La Loai giuong (1,2,3,4)");

                entity.Property(e => e.GiuongCode).HasMaxLength(50);

                entity.Property(e => e.GiuongKhoa).HasDefaultValueSql("((0))");

                entity.Property(e => e.GiuongMaKhoa).HasMaxLength(50);

                entity.Property(e => e.GiuongName).HasMaxLength(255);

                entity.Property(e => e.GiuongNameTT37).HasMaxLength(255);

                entity.Property(e => e.GiuongType)
                    .HasDefaultValueSql("((0))")
                    .HasComment("1: Giuong Ngoai, 2: Giuong Noi, 3: Giuong Hoi suc");
            });

            modelBuilder.Entity<DMKhac>(entity =>
            {
                entity.HasKey(e => e.STDVID)
                    .HasName("PK__DMKhac__5358B51A0F302DBD");

                entity.ToTable("DMKhac");

                entity.Property(e => e.STDVCode).HasMaxLength(150);

                entity.Property(e => e.STDVCreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("default");

                entity.Property(e => e.STDVIsBlood).HasDefaultValueSql("((0))");

                entity.Property(e => e.STDVName).HasMaxLength(500);
            });

            modelBuilder.Entity<DMKhoa>(entity =>
            {
                entity.HasKey(e => e.KhoaID)
                    .HasName("PK__DMKhoa__42EDDF149DBCC93E");

                entity.ToTable("DMKhoa");

                entity.Property(e => e.KhoaCode)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.KhoaName)
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<DMKhoaBV>(entity =>
            {
                entity.HasKey(e => new { e.KBVID, e.MA_CSKCB })
                    .HasName("PK__DMKhoaBV__46C9A8C14008D6C4");

                entity.ToTable("DMKhoaBV");

                entity.Property(e => e.KBVID)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.DEN_NGAY)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MA_KHOA)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TEN_KHOA)
                    .HasMaxLength(1000)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TU_NGAY)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<DMNhomBenh>(entity =>
            {
                entity.HasKey(e => e.NBID)
                    .HasName("PK__DMNhomBe__D983E09E3D789B5D");

                entity.ToTable("DMNhomBenh");

                entity.Property(e => e.NBCode).HasMaxLength(255);

                entity.Property(e => e.NBCode_Sub_1).HasMaxLength(255);

                entity.Property(e => e.NBCode_Sub_2).HasMaxLength(255);

                entity.Property(e => e.NBTenNhom_EN).HasMaxLength(255);

                entity.Property(e => e.NBTenNhom_Phu_1_EN).HasMaxLength(255);

                entity.Property(e => e.NBTenNhom_Phu_1_VN).HasMaxLength(255);

                entity.Property(e => e.NBTenNhom_Phu_2_EN).HasMaxLength(255);

                entity.Property(e => e.NBTenNhom_Phu_2_VN).HasMaxLength(255);

                entity.Property(e => e.NBTenNhom_VN).HasMaxLength(255);
            });

            modelBuilder.Entity<DMPhauThuat>(entity =>
            {
                entity.HasKey(e => e.PhauThuatID)
                    .HasName("PK__DMPhauTh__E5656C3E337BED17");

                entity.ToTable("DMPhauThuat");

                entity.Property(e => e.PhauThuatChuyenKhoa).HasMaxLength(255);

                entity.Property(e => e.PhauThuatCode).HasMaxLength(250);

                entity.Property(e => e.PhauThuatLoai).HasMaxLength(10);

                entity.Property(e => e.PhauThuatMaChuong).HasMaxLength(255);

                entity.Property(e => e.PhauThuatName).HasMaxLength(255);

                entity.Property(e => e.PhauThuatNameTT15).HasMaxLength(255);

                entity.Property(e => e.PhauThuatTuyen).HasMaxLength(1);
            });

            modelBuilder.Entity<DMThuoc>(entity =>
            {
                entity.HasKey(e => new { e.THID, e.MA_CSKCB })
                    .HasName("PK__DMThuoc__078C72B3170B33EA");

                entity.ToTable("DMThuoc");

                entity.Property(e => e.THID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DANG_BAO_CHE).HasMaxLength(250);

                entity.Property(e => e.DEN_NGAY).HasMaxLength(250);

                entity.Property(e => e.DON_VI_TINH)
                    .HasMaxLength(50);

                entity.Property(e => e.DUONG_DUNG)
                    .HasMaxLength(100);

                entity.Property(e => e.HAM_LUONG)
                    .HasMaxLength(1024);

                entity.Property(e => e.HT_THAU).HasMaxLength(250);

                entity.Property(e => e.LOAI_THAU)
                    .HasMaxLength(50);

                entity.Property(e => e.LOAI_THUOC)
                    .HasMaxLength(50);

                entity.Property(e => e.MANHOM_5937).HasMaxLength(50);

                entity.Property(e => e.MANHOM_9324)
                    .HasMaxLength(50);

                entity.Property(e => e.MA_DUONG_DUNG)
                    .HasMaxLength(50);

                entity.Property(e => e.MA_DVKT).HasMaxLength(1000);

                entity.Property(e => e.MA_THUOC).HasMaxLength(250);

                entity.Property(e => e.NHA_SX).HasMaxLength(2500);

                entity.Property(e => e.NHA_THAU)
                    .HasMaxLength(2500);

                entity.Property(e => e.NHOM_THAU)
                    .HasMaxLength(50);

                entity.Property(e => e.NUOC_SX)
                    .HasMaxLength(500);

                entity.Property(e => e.QUY_CACH).HasMaxLength(1000);

                entity.Property(e => e.SO_DANG_KY)
                    .HasMaxLength(500);

                entity.Property(e => e.TEN_HOAT_CHAT).HasMaxLength(1000);

                entity.Property(e => e.TEN_THUOC)
                    .HasMaxLength(1024);

                entity.Property(e => e.TT_THAU).HasMaxLength(250);

                entity.Property(e => e.TU_NGAY).HasMaxLength(50);

                entity.Property(e => e.DEN_NGAY).HasMaxLength(50);

                entity.Property(e => e.NGAY_DE_NGHI).HasMaxLength(50);

                entity.Property(e => e.PP_CHEBIEN).HasMaxLength(300);
            });

            modelBuilder.Entity<DMTuyenBV>(entity =>
            {
                entity.HasKey(e => e.TuyenBVID)
                    .HasName("PK__DMTuyenB__B0CF91784B384A98");

                entity.ToTable("DMTuyenBV");

                entity.Property(e => e.TuyenBVCode).HasMaxLength(50);

                entity.Property(e => e.TuyenBVCreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("default");

                entity.Property(e => e.TuyenBVCreator).HasComment("curentuserid;readonly");

                entity.Property(e => e.TuyenBVName).HasMaxLength(250);
            });

            modelBuilder.Entity<DMThietBi>(entity =>
            {
                entity.HasKey(e => e.TBID);

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.ToTable("DMThietBi");
            });

            modelBuilder.Entity<DMVTYT>(entity =>
            {
                entity.HasKey(e => new { e.VTID, e.MA_CSKCB })
                    .HasName("PK__DMVTYT__707C2112463F913A");

                entity.ToTable("DMVTYT");

                entity.Property(e => e.VTID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DEN_NGAY).HasMaxLength(250);

                entity.Property(e => e.DON_VI_TINH)
                    .HasMaxLength(250);

                entity.Property(e => e.NHA_SX)
                    .HasMaxLength(500);

                entity.Property(e => e.HT_THAU).HasMaxLength(250);

                entity.Property(e => e.LOAI_THAU)
                    .HasMaxLength(50);

                entity.Property(e => e.MANHOM_5937).HasMaxLength(50);

                entity.Property(e => e.MANHOM_9324)
                    .HasMaxLength(50);

                entity.Property(e => e.MA_HIEU)
                    .HasMaxLength(1024);

                entity.Property(e => e.MA_VAT_TU).HasMaxLength(250);

                entity.Property(e => e.NHA_THAU)
                    .HasMaxLength(250);

                entity.Property(e => e.NHOM_VAT_TU).HasMaxLength(1000);

                entity.Property(e => e.NUOC_SX)
                    .HasMaxLength(250);

                entity.Property(e => e.QUYET_DINH)
                    .HasMaxLength(250);

                entity.Property(e => e.TEN_VAT_TU).HasMaxLength(1000);

                entity.Property(e => e.TT_THAU).HasMaxLength(250);

                entity.Property(e => e.TU_NGAY).HasMaxLength(250);

                entity.Property(e => e.TYLE_TT_BH).HasMaxLength(250);
            });

            modelBuilder.Entity<DMYHDTICD10>(entity =>
            {
                entity.HasKey(e => e.YHDTID)
                    .HasName("PK__DMYHDTIC__2C12A51586EFD0E8");

                entity.ToTable("DMYHDTICD10");

                entity.Property(e => e.ICD10Code).HasMaxLength(50);

                entity.Property(e => e.YHDTCode).HasMaxLength(255);
            });
            
            modelBuilder.Entity<DMDVGiuong>(entity =>
            {
                entity.HasKey(e => new { e.GID, e.MA_CSKCB });

                entity.Property(e => e.GID).ValueGeneratedOnAdd();

                entity.ToTable("DMDVGiuong");
            });

            modelBuilder.Entity<DMMaGiuong>(entity =>
            {
                entity.HasKey(e => new { e.GID, e.MA_CSKCB });

                entity.Property(e => e.GID).ValueGeneratedOnAdd();

                entity.ToTable("DMMaGiuong");
            });

            modelBuilder.Entity<DMCSKCB>(entity =>
            {
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).ValueGeneratedOnAdd();

                entity.ToTable("DMCSKCB");
            });

            modelBuilder.Entity<HDSDDichVu>(entity =>
            {
                entity.HasKey(e => e.HDSDDVID)
                    .HasName("PK__HDSDDich__8997F67DD949B133");

                entity.ToTable("HDSDDichVu");

                entity.Property(e => e.HDSDDVID).HasComment("des: Hướng dẫn sử dụng dịch vụ");

                entity.Property(e => e.CHI_DINH)
                    .HasDefaultValueSql("((1))")
                    .HasComment("des: Phân loại");

                entity.Property(e => e.CHO_CON_BU)
                    .HasDefaultValueSql("((0))")
                    .HasComment("des: Đang cho con bú");

                entity.Property(e => e.CO_THAI)
                    .HasDefaultValueSql("((0))")
                    .HasComment("des: Phụ nữ có thai");

                entity.Property(e => e.DICH_VU_KHAC)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Dịch vụ khác");

                entity.Property(e => e.HIEU_LUC)
                    .HasDefaultValueSql("((1))")
                    .HasComment("default");

                entity.Property(e => e.MA_BENH)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Mã bệnh");

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MA_DICH_VU)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Mã dịch vụ;title;required");

                entity.Property(e => e.MA_LOI)
                    .HasMaxLength(250)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Mã lỗi");

                entity.Property(e => e.MA_NHOM)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TEN_DICH_VU)
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Tên dịch vụ");

                entity.Property(e => e.THOI_GIAN_THUC_HIEN)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.THOI_GIAN_TOI_THIEU).HasComment("des: Thời gian tối thiểu");

                entity.Property(e => e.THUOC_KHAC)
                    .HasMaxLength(500)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Thuốc dùng kèm");

                entity.Property(e => e.TIEN_SU_BENH)
                    .HasMaxLength(2000)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Tiền sử bệnh");

                entity.Property(e => e.TUOI)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Tuổi");

                entity.Property(e => e.TYLE_TT)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Tỷ lệ thanh toán");
            });

            modelBuilder.Entity<HDSDThuoc>(entity =>
            {
                entity.ToTable("HDSDThuoc");

                entity.Property(e => e.CHI_DINH).HasDefaultValueSql("((1))");

                entity.Property(e => e.CHO_CON_BU).HasDefaultValueSql("((0))");

                entity.Property(e => e.CO_THAI).HasDefaultValueSql("((0))");

                entity.Property(e => e.DICH_VU).HasDefaultValueSql("('')");

                entity.Property(e => e.DUONG_DUNG)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.HIEU_LUC)
                    .HasDefaultValueSql("((1))")
                    .HasComment("default");

                entity.Property(e => e.MA_BENH).HasDefaultValueSql("('')");

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MA_LOI).HasDefaultValueSql("('')");

                entity.Property(e => e.MA_THUOC)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TEN_THUOC)
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.THUOC_KHAC)
                    .HasMaxLength(500)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TIEN_SU_BENH)
                    .HasMaxLength(500)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TUOI)
                    .HasMaxLength(250)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TYLE_TT)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')")
                    .HasComment("des: Tỷ lệ thanh toán");
            });

            modelBuilder.Entity<Hospital>(entity =>
            {
                entity.ToTable("Hospital");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.AccessToken).HasMaxLength(255);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DateTimeNow).HasColumnType("datetime");

                entity.Property(e => e.IdToken).HasMaxLength(255);

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.OfficeTime)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.SuperHospitalId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.Username).HasMaxLength(255);

                entity.HasOne(d => d.SuperHospital)
                   .WithMany(p => p.HospitalCons)
                   .HasForeignKey(d => d.SuperHospitalId)
                   .HasConstraintName("FK_Hospital_Hospital");
            });

            modelBuilder.Entity<PhacDoDieuTri>(entity =>
            {
                entity.HasKey(e => e.PhacDoID)
                    .HasName("PK__PhacDoDi__05421840D474CEF9");

                entity.ToTable("PhacDoDieuTri");

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.PhacDoMaBenh).HasMaxLength(10);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<SysHistoryLog>(entity =>
            {
                entity.ToTable("SysHistoryLog");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.BrowserOsInfo).HasMaxLength(255);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ExecutionTime).HasColumnType("datetime");

                entity.Property(e => e.FunctionName).HasMaxLength(255);

                entity.Property(e => e.HospitalId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.HospitalName).HasMaxLength(255);

                entity.Property(e => e.IpAddress).HasMaxLength(100);

                entity.Property(e => e.UserId)
                    .HasMaxLength(36)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ThongKeBenh>(entity =>
            {
                entity.HasKey(e => new { e.TKID, e.MA_CSKCB })
                    .HasName("PK__ThongKeB__0C605F10AB5D45A0");

                entity.ToTable("ThongKeBenh");

                entity.Property(e => e.TKID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdate).HasColumnType("datetime");

                entity.Property(e => e.MaBenh).HasMaxLength(255);

                entity.Property(e => e.TenBenh).HasMaxLength(255);
            });

            modelBuilder.Entity<ThongKeNgay>(entity =>
            {
                entity.HasKey(e => new { e.TKID, e.MA_CSKCB })
                    .HasName("PK__ThongKeN__0C605F10D21194E0");

                entity.ToTable("ThongKeNgay");

                entity.Property(e => e.TKID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.KhamBenh).HasDefaultValueSql("((0))");

                entity.Property(e => e.NgoaiTru).HasDefaultValueSql("((0))");

                entity.Property(e => e.NoiTru).HasDefaultValueSql("((0))");

                entity.Property(e => e.TheLoai).HasDefaultValueSql("((0))");

                entity.Property(e => e.TongHop).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.FullName).HasMaxLength(255);

                entity.Property(e => e.HospitalId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PhoneNumber).HasMaxLength(255);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Hospital)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.HospitalId)
                    .HasConstraintName("FK_User_Hospital");
            });

            //modelBuilder.Entity<UserKhoa>(entity =>
            //{
            //    entity.ToTable("UserKhoa");

            //    entity.Property(e => e.MaKhoa)
            //        .HasMaxLength(500)
            //        .HasDefaultValueSql("('')");
            //});

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK__UserRole__AF2760AD877177E0");

                entity.ToTable("UserRole");

                entity.Property(e => e.UserId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.RoleId)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_Role");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRole_User");
            });

            modelBuilder.Entity<XML01>(entity =>
            {
                entity.HasKey(e => new { e.XML01ID, e.MA_CSKCB })
                    .HasName("PK__XML01__64D434B8DC71A8D9");

                entity.ToTable("XML01");

                entity.Property(e => e.XML01ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CAN_NANG).HasMaxLength(50);

                entity.Property(e => e.CAN_NANG_CON).HasMaxLength(100);

                entity.Property(e => e.CHAN_DOAN_RV).HasMaxLength(4000);

                entity.Property(e => e.CHAN_DOAN_VAO).HasMaxLength(4000);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreatorID)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.DIA_CHI).HasMaxLength(1200);

                entity.Property(e => e.DIEN_THOAI).HasMaxLength(50);

                entity.Property(e => e.DU_PHONG).HasMaxLength(4000);

                entity.Property(e => e.FileName).HasMaxLength(4000);

                entity.Property(e => e.GHI_CHU).HasMaxLength(4000);

                entity.Property(e => e.GIAY_CHUYEN_TUYEN).HasMaxLength(50);

                entity.Property(e => e.GT_THE_DEN).HasMaxLength(4000);

                entity.Property(e => e.GT_THE_TU).HasMaxLength(4000);

                entity.Property(e => e.HO_TEN).HasMaxLength(300);

                entity.Property(e => e.HasFiles)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HospitalID)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.LY_DO_VNT).HasMaxLength(4000);

                entity.Property(e => e.LY_DO_VV).HasMaxLength(4000);

                entity.Property(e => e.MAHUYEN_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MATINH_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MAXA_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MA_BENH_CHINH).HasMaxLength(50);

                entity.Property(e => e.MA_BENH_KT).HasMaxLength(100);

                entity.Property(e => e.MA_BENH_YHCT).HasMaxLength(300);

                entity.Property(e => e.MA_BN).HasMaxLength(100);

                entity.Property(e => e.MA_DANTOC).HasMaxLength(50);

                entity.Property(e => e.MA_NGHE_NGHIEP).HasMaxLength(50);

                entity.Property(e => e.MA_DKBD).HasMaxLength(4000);

                entity.Property(e => e.MA_DOITUONG_KCB).HasMaxLength(50);

                entity.Property(e => e.MA_HSBA).HasMaxLength(100);

                entity.Property(e => e.MA_KHOA).HasMaxLength(50);

                entity.Property(e => e.MA_KHUVUC).HasMaxLength(50);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_LY_DO_VNT).HasMaxLength(50);

                entity.Property(e => e.MA_NOI_DEN).HasMaxLength(50);

                entity.Property(e => e.MA_NOI_DI).HasMaxLength(50);

                entity.Property(e => e.MA_PTTT_QT).HasMaxLength(300);

                entity.Property(e => e.MA_QUOCTICH).HasMaxLength(50);

                entity.Property(e => e.MA_THE_BHYT).HasMaxLength(4000);

                //Linhist removed 21/01/26
                //entity.Property(e => e.MA_TTDV).HasMaxLength(50);

                entity.Property(e => e.NAM_NAM_LIEN_TUC).HasColumnType("datetime");

                entity.Property(e => e.NGAY_LAP).HasMaxLength(4000);

                entity.Property(e => e.NGAY_MIEN_CCT).HasColumnType("datetime");

                entity.Property(e => e.NGAY_RA).HasColumnType("datetime");

                entity.Property(e => e.NGAY_SINH).HasColumnType("datetime");

                entity.Property(e => e.NGAY_TTOAN).HasColumnType("datetime");

                entity.Property(e => e.NGAY_VAO).HasColumnType("datetime");

                entity.Property(e => e.NGAY_VAO_NOI_TRU).HasColumnType("datetime");

                entity.Property(e => e.PP_DIEU_TRI).HasMaxLength(4000);

                entity.Property(e => e.SO_CCCD).HasMaxLength(50);
            });

            modelBuilder.Entity<XML02>(entity =>
            {
                entity.HasKey(e => new { e.XML02ID, e.MA_CSKCB })
                    .HasName("PK__XML02__66E7C5F66807772E");

                entity.ToTable("XML02");

                entity.Property(e => e.XML02ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CACH_DUNG).HasMaxLength(1200);

                entity.Property(e => e.DANG_BAO_CHE).HasMaxLength(1200);

                entity.Property(e => e.DON_VI_TINH).HasMaxLength(50);

                entity.Property(e => e.DUONG_DUNG).HasMaxLength(50);

                entity.Property(e => e.DU_PHONG).HasMaxLength(4000);

                entity.Property(e => e.HAM_LUONG).HasMaxLength(1200);

                entity.Property(e => e.LIEU_DUNG).HasMaxLength(1200);

                entity.Property(e => e.MA_BAC_SI).HasMaxLength(300);

                entity.Property(e => e.MA_CSKCB_THUOC).HasMaxLength(50);

                entity.Property(e => e.MA_DICH_VU).HasMaxLength(300);

                entity.Property(e => e.MA_KHOA).HasMaxLength(50);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_PP_CHEBIEN).HasMaxLength(300);

                entity.Property(e => e.MA_THUOC).HasMaxLength(300);

                entity.Property(e => e.NGAY_YL).HasColumnType("datetime");

                entity.Property(e => e.SO_DANG_KY).HasMaxLength(300);

                entity.Property(e => e.TEN_THUOC).HasMaxLength(1200);

                entity.Property(e => e.TT_THAU).HasMaxLength(50);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML02s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML02_XML01");
            });

            modelBuilder.Entity<XML03>(entity =>
            {
                entity.HasKey(e => new { e.XML03ID, e.MA_CSKCB })
                    .HasName("PK__XML03__66A1FFFFB85B481C");

                entity.ToTable("XML03");

                entity.Property(e => e.XML03ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DON_VI_TINH).HasMaxLength(50);

                entity.Property(e => e.DU_PHONG).HasMaxLength(4000);

                entity.Property(e => e.GOI_VTYT).HasMaxLength(50);

                entity.Property(e => e.MA_BAC_SI).HasMaxLength(300);

                entity.Property(e => e.MA_BENH).HasMaxLength(150);

                entity.Property(e => e.MA_BENH_YHCT).HasMaxLength(300);

                entity.Property(e => e.MA_DICH_VU).HasMaxLength(50);

                entity.Property(e => e.MA_GIUONG).HasMaxLength(50);

                entity.Property(e => e.MA_HIEU_SP).HasMaxLength(300);

                entity.Property(e => e.MA_KHOA).HasMaxLength(50);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_MAY).HasMaxLength(1200);

                entity.Property(e => e.MA_PTTT_QT).HasMaxLength(300);

                entity.Property(e => e.MA_VAT_TU).HasMaxLength(300);

                entity.Property(e => e.MA_XANG_DAU).HasMaxLength(50);

                entity.Property(e => e.NGAY_KQ).HasColumnType("datetime");

                entity.Property(e => e.NGAY_TH_YL).HasColumnType("datetime");

                entity.Property(e => e.NGAY_YL).HasColumnType("datetime");

                entity.Property(e => e.NGUOI_THUC_HIEN).HasMaxLength(300);

                entity.Property(e => e.TEN_DICH_VU).HasMaxLength(1200);

                entity.Property(e => e.TEN_VAT_TU).HasMaxLength(1200);

                entity.Property(e => e.TT_THAU).HasMaxLength(50);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML03s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML03_XML01");
            });

            modelBuilder.Entity<XML04>(entity =>
            {
                entity.HasKey(e => new { e.XML04ID, e.MA_CSKCB })
                    .HasName("PK__XML04__6667F9E4217FE8F7");

                entity.ToTable("XML04");

                entity.Property(e => e.XML04ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DON_VI_DO).HasMaxLength(50);

                entity.Property(e => e.DU_PHONG).HasMaxLength(4000);

                entity.Property(e => e.GIA_TRI).HasMaxLength(250);

                entity.Property(e => e.KET_LUAN).HasMaxLength(4000);

                entity.Property(e => e.MA_BS_DOC_KQ).HasMaxLength(300);

                entity.Property(e => e.MA_CHI_SO).HasMaxLength(250);

                entity.Property(e => e.MA_DICH_VU).HasMaxLength(50);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MO_TA).HasMaxLength(4000);

                entity.Property(e => e.NGAY_KQ).HasColumnType("datetime");

                entity.Property(e => e.TEN_CHI_SO).HasMaxLength(300);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML04s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML04_XML01");
            });

            modelBuilder.Entity<XML05>(entity =>
            {
                entity.HasKey(e => new { e.XML05ID, e.MA_CSKCB })
                    .HasName("PK__XML05__662183ED9EF3C58F");

                entity.ToTable("XML05");

                entity.Property(e => e.XML05ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DIEN_BIEN_LS).HasMaxLength(4000);

                entity.Property(e => e.DU_PHONG).HasMaxLength(4000);

                entity.Property(e => e.GIAI_DOAN_BENH).HasMaxLength(4000);

                entity.Property(e => e.HOI_CHAN).HasMaxLength(4000);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.NGUOI_THUC_HIEN).HasMaxLength(300);

                entity.Property(e => e.PHAU_THUAT).HasMaxLength(4000);

                entity.Property(e => e.THOI_DIEM_DBLS).HasColumnType("datetime");

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML05s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML05_XML01");
            });

            modelBuilder.Entity<XML06>(entity =>
            {
                entity.HasKey(e => new { e.XML06ID, e.MA_CSKCB })
                    .HasName("PK__XML06__65E6C91B46465101");

                entity.ToTable("XML06");

                entity.Property(e => e.XML06ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.NGAY_SINH).HasColumnType("datetime");

                entity.Property(e => e.BDDT_ARV).HasColumnType("datetime");

                entity.Property(e => e.DU_PHONG).HasMaxLength(4000);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_PHAC_DO_DIEU_TRI).HasMaxLength(300);

                entity.Property(e => e.MA_PHAC_DO_DIEU_TRI_BD).HasMaxLength(300);

                entity.Property(e => e.MA_THE_BHYT).HasMaxLength(4000);

                entity.Property(e => e.MA_TINH_TRANG_DK).HasMaxLength(50);

                entity.Property(e => e.NGAYBD_DTRI_LAO).HasColumnType("datetime");

                entity.Property(e => e.NGAYKD_HIV).HasColumnType("datetime");

                entity.Property(e => e.NGAYKT_DTRI_LAO).HasColumnType("datetime");

                entity.Property(e => e.NGAY_BAT_DAU_DT_CTX).HasColumnType("datetime");

                entity.Property(e => e.NGAY_BAT_DAU_XU_TRI).HasColumnType("datetime");

                entity.Property(e => e.NGAY_KET_THUC_XU_TRI).HasColumnType("datetime");

                entity.Property(e => e.NGAY_KQ_XN_PCR).HasColumnType("datetime");

                entity.Property(e => e.NGAY_KQ_XN_TLVR).HasColumnType("datetime");

                entity.Property(e => e.NGAY_NHAN_TT_MANG_THAI).HasColumnType("datetime");

                entity.Property(e => e.NGAY_XN_PCR).HasColumnType("datetime");

                entity.Property(e => e.NGAY_XN_TLVR).HasColumnType("datetime");

                entity.Property(e => e.NGAY_CHUYEN_PHAC_DO).HasColumnType("datetime");

                entity.Property(e => e.SO_CCCD).HasMaxLength(50);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML06s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML06_XML01");
            });

            modelBuilder.Entity<XML07>(entity =>
            {
                entity.HasKey(e => new { e.XML07ID, e.MA_CSKCB })
                    .HasName("PK__XML07__62A4DF22AE03F06F");

                entity.ToTable("XML07");

                entity.Property(e => e.XML07ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CHAN_DOAN_RV).HasMaxLength(4000);

                entity.Property(e => e.GHI_CHU).HasMaxLength(4000);

                entity.Property(e => e.HO_TEN_CHA).HasMaxLength(300);

                entity.Property(e => e.HO_TEN_ME).HasMaxLength(300);

                entity.Property(e => e.MA_BS).HasMaxLength(300);

                entity.Property(e => e.MA_CHA).HasMaxLength(50);

                entity.Property(e => e.MA_KHOA_RV).HasMaxLength(300);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_ME).HasMaxLength(50);

                entity.Property(e => e.MA_THE_TAM).HasMaxLength(50);

                //Linhist removed 21/01/26
                //entity.Property(e => e.MA_TTDV).HasMaxLength(50);

                entity.Property(e => e.MA_YTE).HasMaxLength(300);

                entity.Property(e => e.NGAY_CT).HasColumnType("datetime");

                entity.Property(e => e.NGAY_RA).HasColumnType("datetime");

                entity.Property(e => e.NGAY_VAO).HasColumnType("datetime");

                entity.Property(e => e.NGOAITRU_DENNGAY).HasColumnType("datetime");

                entity.Property(e => e.NGOAITRU_TUNGAY).HasColumnType("datetime");

                entity.Property(e => e.NGUYENNHAN_DINHCHI).HasMaxLength(4000);

                entity.Property(e => e.PP_DIEUTRI).HasMaxLength(4000);

                entity.Property(e => e.SO_LUU_TRU).HasMaxLength(300);

                entity.Property(e => e.TEN_BS).HasMaxLength(300);

                entity.Property(e => e.THOIGIAN_DINHCHI).HasColumnType("datetime");

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML07s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML07_XML01");
            });

            modelBuilder.Entity<XML08>(entity =>
            {
                entity.HasKey(e => new { e.XML08ID, e.MA_CSKCB })
                    .HasName("PK__XML08__6566B6E9913E8B33");

                entity.ToTable("XML08");

                entity.Property(e => e.XML08ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CHAN_DOAN_RV).HasMaxLength(4000);

                entity.Property(e => e.CHAN_DOAN_VAO).HasMaxLength(4000);

                entity.Property(e => e.DON_VI).HasMaxLength(1200);

                entity.Property(e => e.DU_PHONG).HasMaxLength(4000);

                entity.Property(e => e.GHI_CHU).HasMaxLength(4000);

                entity.Property(e => e.HO_TEN_CHA).HasMaxLength(300);

                entity.Property(e => e.HO_TEN_ME).HasMaxLength(300);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_THE_TAM).HasMaxLength(50);

                //Linhist removed 21/01/26
                //entity.Property(e => e.MA_TTDV).HasMaxLength(50);

                entity.Property(e => e.NGAY_CONCHET).HasColumnType("datetime");

                entity.Property(e => e.NGAY_CT).HasColumnType("datetime");

                entity.Property(e => e.NGAY_RA).HasColumnType("datetime");

                entity.Property(e => e.NGAY_SINHCON).HasColumnType("datetime");

                entity.Property(e => e.NGAY_VAO).HasColumnType("datetime");

                entity.Property(e => e.NGUOI_GIAM_HO).HasMaxLength(300);

                entity.Property(e => e.PP_DIEUTRI).HasMaxLength(4000);

                entity.Property(e => e.QT_BENHLY).HasMaxLength(4000);

                entity.Property(e => e.TOMTAT_KQ).HasMaxLength(4000);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML08s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML08_XML01");
            });

            modelBuilder.Entity<XML09>(entity =>
            {
                entity.HasKey(e => new { e.XML09ID, e.MA_CSKCB })
                    .HasName("PK__XML09__622494B060924F65");

                entity.ToTable("XML09");

                entity.Property(e => e.XML09ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.GHI_CHU).HasMaxLength(4000);

                entity.Property(e => e.HO_TEN_CHA).HasMaxLength(300);

                entity.Property(e => e.HO_TEN_CON).HasMaxLength(300);

                entity.Property(e => e.HO_TEN_NND).HasMaxLength(300);

                entity.Property(e => e.MAHUYEN_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MATINH_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MAXA_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MA_DANTOC_NND).HasMaxLength(50);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_QUOCTICH).HasMaxLength(50);

                entity.Property(e => e.MA_THE_NND).HasMaxLength(50);

                entity.Property(e => e.MA_THE_TAM).HasMaxLength(50);

                //Linhist removed 21/01/26
                //entity.Property(e => e.MA_TTDV).HasMaxLength(50);

                entity.Property(e => e.NGAYCAP_CCCD_NND).HasColumnType("datetime");

                entity.Property(e => e.NGAYSINH_NND).HasColumnType("datetime");

                entity.Property(e => e.NGAY_CT).HasColumnType("datetime");

                entity.Property(e => e.NGAY_SINH_CON).HasColumnType("datetime");

                entity.Property(e => e.NGUOI_DO_DE).HasMaxLength(300);

                entity.Property(e => e.NGUOI_GHI_PHIEU).HasMaxLength(300);

                entity.Property(e => e.NOICAP_CCCD_NND).HasMaxLength(1200);

                entity.Property(e => e.NOI_CU_TRU_NND).HasMaxLength(1200);

                entity.Property(e => e.NOI_SINH_CON).HasMaxLength(1200);

                entity.Property(e => e.QUYEN_SO).HasMaxLength(300);

                entity.Property(e => e.SO).HasMaxLength(300);

                entity.Property(e => e.SO_CCCD_NND).HasMaxLength(50);

                entity.Property(e => e.TINH_TRANG_CON).HasMaxLength(4000);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML09s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML09_XML01");
            });

            modelBuilder.Entity<XML10>(entity =>
            {
                entity.HasKey(e => new { e.XML10ID, e.MA_CSKCB })
                    .HasName("PK__XML10__BECB91F10FE66DB3");

                entity.ToTable("XML10");

                entity.Property(e => e.XML10ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CHAN_DOAN_RV).HasMaxLength(4000);

                entity.Property(e => e.DEN_NGAY).HasColumnType("datetime");

                entity.Property(e => e.DON_VI).HasMaxLength(1200);

                entity.Property(e => e.MA_BS).HasMaxLength(300);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                //Linhist removed 21/01/26
                //entity.Property(e => e.MA_TTDV).HasMaxLength(50);

                entity.Property(e => e.NGAY_CT).HasColumnType("datetime");

                entity.Property(e => e.SO_CT).HasMaxLength(300);

                entity.Property(e => e.SO_SERI).HasMaxLength(300);

                entity.Property(e => e.TEN_BS).HasMaxLength(300);

                entity.Property(e => e.TU_NGAY).HasColumnType("datetime");

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML10s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML10_XML01");
            });

            modelBuilder.Entity<XML11>(entity =>
            {
                entity.HasKey(e => new { e.XML11ID, e.MA_CSKCB })
                    .HasName("PK__XML11__BE8DBA4A9BB19DEF");

                entity.ToTable("XML11");

                entity.Property(e => e.XML11ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CHAN_DOAN_RV).HasMaxLength(4000);

                entity.Property(e => e.DEN_NGAY).HasColumnType("datetime");

                entity.Property(e => e.DON_VI).HasMaxLength(1200);

                entity.Property(e => e.HO_TEN_CHA).HasMaxLength(300);

                entity.Property(e => e.HO_TEN_ME).HasMaxLength(300);

                entity.Property(e => e.MAU_SO).HasMaxLength(50);

                entity.Property(e => e.MA_BHXH).HasMaxLength(50);

                entity.Property(e => e.MA_BS).HasMaxLength(300);

                entity.Property(e => e.MA_LK).HasMaxLength(100);

                entity.Property(e => e.MA_THE_BHYT).HasMaxLength(4000);

                entity.Property(e => e.MA_THE_TAM).HasMaxLength(50);

                entity.Property(e => e.NGAY_CT).HasColumnType("datetime");

                entity.Property(e => e.NGUYENNHAN_DINHCHI).HasMaxLength(4000);

                entity.Property(e => e.PP_DIEUTRI).HasMaxLength(4000);

                entity.Property(e => e.SO_CT).HasMaxLength(300);

                entity.Property(e => e.SO_KCB).HasMaxLength(300);

                entity.Property(e => e.SO_SERI).HasMaxLength(300);

                entity.Property(e => e.TU_NGAY).HasColumnType("datetime");

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML11s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML11_XML01");
            });

            modelBuilder.Entity<XML12>(entity =>
            {
                entity.HasKey(e => new { e.XML12ID, e.MA_CSKCB })
                    .HasName("PK__XML12__BD4BB4438CF91DB4");

                entity.ToTable("XML12");

                entity.Property(e => e.XML12ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DANG_HUONG_CHE_DO).HasMaxLength(50);

                entity.Property(e => e.DE_NGHI).HasMaxLength(4000);

                entity.Property(e => e.DIA_CHI).HasMaxLength(1200);

                entity.Property(e => e.DIEN_THOAI).HasMaxLength(50);

                entity.Property(e => e.DUOC_XACDINH).HasMaxLength(4000);

                entity.Property(e => e.DU_PHONG).HasMaxLength(4000);

                entity.Property(e => e.GIOI_THIEU_CUA).HasMaxLength(1200);

                entity.Property(e => e.HO_TEN).HasMaxLength(300);

                entity.Property(e => e.KET_QUA_KHAM).HasMaxLength(4000);

                entity.Property(e => e.MAHUYEN_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MATINH_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MAXA_CU_TRU).HasMaxLength(50);

                entity.Property(e => e.MA_BHXH).HasMaxLength(50);

                entity.Property(e => e.MA_DOI_TUONG).HasMaxLength(50);

                entity.Property(e => e.MA_DONVI).HasMaxLength(300);

                entity.Property(e => e.MA_THE_BHYT).HasMaxLength(50);

                entity.Property(e => e.NGAY_CAP_CCCD).HasColumnType("datetime");

                entity.Property(e => e.NGAY_CHUNG_TU).HasColumnType("datetime");

                entity.Property(e => e.NGAY_DE_NGHI).HasColumnType("datetime");

                entity.Property(e => e.NGAY_HOP).HasColumnType("datetime");

                entity.Property(e => e.NGAY_SINH).HasColumnType("datetime");

                entity.Property(e => e.NGHE_NGHIEP).HasMaxLength(100);

                entity.Property(e => e.NGUOI_CHU_TRI).HasMaxLength(300);

                entity.Property(e => e.NOI_CAP_CCCD).HasMaxLength(1200);

                entity.Property(e => e.SO_BIEN_BAN).HasMaxLength(300);

                entity.Property(e => e.SO_CCCD).HasMaxLength(50);

                entity.Property(e => e.SO_GIAY_GIOI_THIEU).HasMaxLength(300);

                entity.Property(e => e.SO_VAN_BAN_CAN_CU).HasMaxLength(300);
            });

            modelBuilder.Entity<XML13>(entity =>
            {
                entity.HasKey(e => new { e.XML13ID, e.MA_CSKCB });

                entity.ToTable("XML13");

                entity.Property(e => e.XML13ID).ValueGeneratedOnAdd();

                entity.Property(e => e.NGAY_SINH).HasColumnType("datetime");

                entity.Property(e => e.NGAY_VAO).HasColumnType("datetime");

                entity.Property(e => e.NGAY_VAO_NOI_TRU).HasColumnType("datetime");

                entity.Property(e => e.NGAY_RA).HasColumnType("datetime");

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML13s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML13_XML01");
            });

            modelBuilder.Entity<XML14>(entity =>
            {
                entity.HasKey(e => new { e.XML14ID, e.MA_CSKCB });

                entity.ToTable("XML14");

                entity.Property(e => e.XML14ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.NGAY_SINH).HasColumnType("datetime");

                entity.Property(e => e.NGAY_CT).HasColumnType("datetime");

                entity.Property(e => e.NGAY_HEN_KL).HasColumnType("datetime");

                entity.Property(e => e.NGAY_RA).HasColumnType("datetime");

                entity.Property(e => e.NGAY_VAO).HasColumnType("datetime");

                entity.Property(e => e.NGAY_VAO_NOI_TRU).HasColumnType("datetime");

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML14s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML14_XML01");
            });

            modelBuilder.Entity<XML15>(entity =>
            {
                entity.HasKey(e => new { e.XML15ID, e.MA_CSKCB });

                entity.ToTable("XML15");

                entity.Property(e => e.XML15ID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.XML15s)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XML15_XML01");
            });

            //modelBuilder.Entity<XMLCheckIn>(entity =>
            //{
            //    entity.HasKey(e => new { e.XMLCID, e.MA_CSKCB })
            //        .HasName("PK__XMLCheck__45BD06AAEB249C59");

            //    entity.ToTable("XMLCheckIn");

            //    entity.Property(e => e.XMLCID).ValueGeneratedOnAdd();

            //    entity.Property(e => e.MA_CSKCB)
            //        .HasMaxLength(10)
            //        .IsUnicode(false);

            //    entity.Property(e => e.GT_THE_DEN).HasMaxLength(50);

            //    entity.Property(e => e.GT_THE_TU).HasMaxLength(50);

            //    entity.Property(e => e.HO_TEN).HasMaxLength(300);

            //    entity.Property(e => e.MA_BN).HasMaxLength(100);

            //    entity.Property(e => e.MA_DICH_VU).HasMaxLength(50);

            //    entity.Property(e => e.MA_DKBD).HasMaxLength(50);

            //    entity.Property(e => e.MA_LK).HasMaxLength(100);

            //    entity.Property(e => e.MA_THE_BHYT).HasMaxLength(50);

            //    entity.Property(e => e.NGAY_SINH).HasColumnType("datetime");

            //    entity.Property(e => e.NGAY_VAO).HasColumnType("datetime");

            //    entity.Property(e => e.NGAY_YL).HasColumnType("datetime");

            //    entity.Property(e => e.SO_CCCD).HasMaxLength(50);

            //    entity.Property(e => e.TEN_DICH_VU).HasMaxLength(1200);
            //});

            modelBuilder.Entity<XMLState>(entity =>
            {
                entity.HasKey(e => new { e.XML01ID, e.MA_CSKCB })
                    .HasName("PK__XML00__64D434B8DC71A8D9");

                entity.ToTable("XMLState");

                entity.Property(e => e.XML01ID).ValueGeneratedNever();

                entity.Property(e => e.MA_CSKCB)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.XML01)
                    .WithOne(p => p.State)
                    .HasForeignKey<XMLState>(d => new { d.XML01ID, d.MA_CSKCB });
            });

            modelBuilder.Entity<XMLError>(entity =>
            {
                entity.HasKey(e => new { e.XMLEID, e.MA_CSKCB })
                    .HasName("PK__XMLError__4F6E9F5EDB1E34E4");

                entity.ToTable("XMLError");

                entity.Property(e => e.XMLEID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MA_LOI).HasMaxLength(100);

                entity.HasOne(d => d.XML01)
                    .WithMany(p => p.Errors)
                    .HasForeignKey(d => new { d.XML01ID, d.MA_CSKCB })
                    .HasConstraintName("FK_XMLError_XML01");
            });

            //modelBuilder.Entity<XMLResult>(entity =>
            //{
            //    entity.HasKey(e => e.XML01ID)
            //        .HasName("PK_XMLResult");

            //    entity.ToTable("XMLResult");

            //    entity.Property(e => e.XML01ID).ValueGeneratedNever();

            //    entity.Property(e => e.MA_CSKCB)
            //        .IsRequired()
            //        .HasMaxLength(10)
            //        .IsUnicode(false);
            //});

            //modelBuilder.Entity<XMLState>(entity =>
            //{
            //    entity.HasKey(e => e.XML01ID)
            //        .HasName("PK_XMLState");

            //    entity.ToTable("XMLState");

            //    entity.Property(e => e.XML01ID).ValueGeneratedNever();

            //    entity.Property(e => e.MA_CSKCB)
            //        .IsRequired()
            //        .HasMaxLength(10)
            //        .IsUnicode(false);

            //    entity.HasOne(x => x.XML01)
            //        .WithOne(x => x.State)
            //        .HasForeignKey<XMLState>(x => new { x.XML01ID, x.MA_CSKCB });
            //});

            modelBuilder.Entity<XMLValidate>(entity =>
            {
                entity.HasKey(e => e.XML01ID)
                    .HasName("PK_XMLValidate");

                entity.ToTable("XMLValidate");

                entity.Property(e => e.XML01ID).ValueGeneratedNever();

                entity.Property(e => e.MA_CSKCB)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            //modelBuilder.Entity<XMLVerify>(entity =>
            //{
            //    entity.HasKey(e => e.XML01ID)
            //        .HasName("PK_XMLVerify");

            //    entity.ToTable("XMLVerify");

            //    entity.Property(e => e.XML01ID).ValueGeneratedNever();

            //    entity.Property(e => e.MA_CSKCB)
            //        .IsRequired()
            //        .HasMaxLength(10)
            //        .IsUnicode(false);
            //});

            //modelBuilder.Entity<CheckChanges>(entity =>
            //{
            //    entity.HasKey(e => e.ChgID);

            //    entity.ToTable("CheckChanges");

            //    entity.Property(e => e.ChgID).ValueGeneratedOnAdd();

            //    entity.Property(e => e.MA_CSKCB)
            //        .HasMaxLength(10);

            //    entity.Property(e => e.MA_LK)
            //        .HasMaxLength(100);

            //    entity.Property(e => e.MA_BN)
            //        .HasMaxLength(100);

            //    entity.Property(e => e.MA_THE_BHYT)
            //        .HasMaxLength(4000);

            //    entity.Property(e => e.HO_TEN)
            //        .HasMaxLength(300);

            //    entity.Property(e => e.MA_KHOA)
            //        .HasMaxLength(50);
            //});

            modelBuilder.Entity<UserLogs>(entity =>
            {
                entity.HasKey(e => e.LogID);

                entity.ToTable("UserLogs");

                entity.Property(e => e.LogID).ValueGeneratedOnAdd();

                entity.Property(e => e.MA_CSKCB)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Action)
                    .HasMaxLength(50);

                entity.Property(e => e.UrlAccess)
                    .HasMaxLength(250);

                entity.Property(e => e.UserID)
                    .HasMaxLength(36)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
