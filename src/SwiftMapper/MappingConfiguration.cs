using System;

namespace SwiftMapper
{
    internal class MappingConfiguration<TSource, TDestination> : IMappingConfiguration
    {
        public Type Source => typeof(TSource);
        public Type Destination => typeof(TDestination);
    }
}