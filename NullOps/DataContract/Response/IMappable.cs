namespace NullOps.DataContract.Response;

public interface IMappable<in TFrom, out TTo>
    where TTo : class
{
    public static abstract TTo MapTo(TFrom from);
}
