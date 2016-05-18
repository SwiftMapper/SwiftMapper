using System.Diagnostics.CodeAnalysis;

namespace SwiftMapper
{
    [SuppressMessage("ReSharper", "TypeParameterCanBeVariant")]
    public interface IMapping<TSource, TDestination>
    {
        TDestination Map(TSource entity);
    }
}