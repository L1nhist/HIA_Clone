--Dm ko theo benh vien
select * from DMBVTuyenTinh
select * from DMDacBiet
select * from DMDichVuDaDung
select * from DMGioiTinh
select * from [DMKetQuaDieuTri]
select * from [dbo].[DMKhac]
select * from [dbo].[DMKhoa]
select * from [dbo].[DMLyDoVaoVien]
select * from [dbo].[DMNhom]
select * from [dbo].[DMNhomBenh]
select * from [dbo].[DMTuyenBV]
select * from [dbo].[DMTaiNan]
select * from [dbo].[DMPhauThuat]
select * from [dbo].[DMTinhTrangRaVien]
select * from [dbo].[DMYHDTICD10]
select * from [dbo].[HDSDDichVu]
select * from [dbo].[HDSDThuoc]
select * from [dbo].DMBenh
select * from [dbo].DMBenhPhanLoai
select * from [dbo].DMError
select * from PhacDoDieuTri

--danh mucj theo user
select * from DMBacSi


--dm theo benh vien
select MA_COSOKCB,count(*) from DMDichVu group by MA_COSOKCB
select * from [dbo].[DMKhoaBV]
select * from [dbo].[DMThuoc]
select * from  [dbo].[DMVTYT]




select * from  xml1
select * from  XML2
select * from  XML3
select * from  XML4
select * from  XML5
delete from XML1

select x2.MA_THUOC,x2.MA_BENH,hd.MA_BENH from  XML1Error e 
join DMError dm on dm.ErrorCode = e.XEErrorCode 
join xml1 x1 on x1.XML1ID = e.XEXML1ID 
join HDSDThuoc hd on hd.MA_LOI = dm.ErrorCode
join XML2 x2 on x2.XML1ID = x1.XML1ID and x2.STT = e.XEErrorRow

select XEErrorCode,count(*) from  XML1Error group by XEErrorCode order by count(*) desc 

select * from  XML1Error where XEErrorCode in ('11111','22222','33333','44444','55555')


select * from  HDSDThuoc

select * from  DMError where ErrorCode not in (select MA_LOI from  HDSDThuoc) and
ErrorCode not in (select MA_LOI from  HDSDDichVu) order by ErrorAtXMLFile, ErrorAtXMLTag

select * from DMError where  ErrorCode =251 

select * from  HDSDThuoc where MA_LOI = 318

select * from  [User]

select * from  HDSDDichVu

select * from  ImportError

delete from ImportError

select * from  [User]
select * from  UserRole
select * from  [Role]
select * from  Hospital

select * from  XML1Error












