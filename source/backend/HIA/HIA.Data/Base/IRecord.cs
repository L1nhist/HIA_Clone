namespace HIA.Data.Base;

public interface IRecord
{
    long XML01ID { get; }
    string MA_CSKCB { get; }
    string? MA_LK { get; }
    int? STT { get; }

    void RaiseError(int MA_LOI, string? errDesc = null);
}

public interface IRecord<TXML01> : IRecord
    where TXML01 : IRecord
{
    TXML01 XML01 { get; }

    void AppendTo(TXML01 x01);
}

public interface IXMLFile<TEnt>
    where TEnt : IRecord
{
    TEnt Export();
}

public interface IXMLModel<TEnt>
{
    void Import(TEnt entity);
}