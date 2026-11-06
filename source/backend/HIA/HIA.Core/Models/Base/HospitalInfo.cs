namespace HIA.Core.Models.Base;

public class HospitalInfo
{
    public string? MaCSKCB { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public bool IsActived { get; set; }

    public bool IsDeleted { get; set; }

    public HospitalInfo() { }

    public HospitalInfo(HIA.Data.Entities.Hospital hospital)
    {
        MaCSKCB = hospital.MA_CSKCB;
        Name = hospital.Name;
        Address = hospital.Address;
        IsActived = hospital.IsActivated;
        IsDeleted = hospital.IsDeleted;
    }
}
public class HospitalAIInfo
{
    public string? MaCSKCB { get; set; }

    public string? Name { get; set; }

    public string? OfficeTime { get; set; }

    public string? Address { get; set; }

    public bool IsActived { get; set; }

    public bool IsDeleted { get; set; }

    public HospitalAIInfo() { }

    public HospitalAIInfo(HIA.Data.Entities.Hospital hospital)
    {
        MaCSKCB = hospital.MA_CSKCB;
        Name = hospital.Name;
        OfficeTime = hospital.OfficeTime;
        Address = hospital.Address;
        IsActived = hospital.IsActivated;
        IsDeleted = hospital.IsDeleted;
    }
}
