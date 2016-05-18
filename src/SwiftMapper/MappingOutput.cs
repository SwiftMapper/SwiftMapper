using System;
using System.Linq.Expressions;

namespace SwiftMapper
{
    internal class MappingOutput<TSource, TDestination> : IMappingOutput
    {
        public Expression<Func<TSource, TDestination>> Expression { get; set; }
        public Func<TSource, TDestination> Delegate { get; set; }
        public IMapping<TSource, TDestination> Mapping { get; set; }
        public Type MappingsType { get; set; }

        public object GetDelegate()
        {
            return Delegate;
        }

        public object GetMapper()
        {
            return Mapping;
        }
    }
}