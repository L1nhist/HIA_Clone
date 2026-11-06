namespace HIA.Data.Base;

public interface IMapable<T>
{
    T? MapTo();

    void MapFrom(T? item);
}