using System;
using System.Collections.Generic;
using System.Linq;

namespace SwiftMapper
{
    public class SwiftMapper : IMapper
    {
        private readonly Dictionary<IMappingConfiguration, IMappingOutput> _compiledMappings = new Dictionary<IMappingConfiguration, IMappingOutput>();
        private readonly IMapperConfiguration _configuration;
        private readonly MappingCompiler _mappingCompiler = new MappingCompiler();
        private readonly Dictionary<IMappingConfiguration, Func<IMappingOutput>> _mappingConfigurations = new Dictionary<IMappingConfiguration, Func<IMappingOutput>>();

        public SwiftMapper(IMapperConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void RegisterMapping<TSource, TDestination>()
        {
            var config = new MappingConfiguration<TSource, TDestination>();
            Func<IMappingOutput> getMapping = () => GetMappingOutput(config);

            _mappingConfigurations.Add(config, getMapping);
        }

        private MappingOutput<TSource, TDestination> GetMappingOutput<TSource, TDestination>(MappingConfiguration<TSource, TDestination> config)
        {
            var expression = _mappingCompiler.GetExpression<TSource, TDestination>();
            var type = _mappingCompiler.GetType(expression);
            var compiled = _mappingCompiler.GetDelegate<TSource, TDestination>(type);

            return new MappingOutput<TSource, TDestination> {Expression = expression, MappingsType = type, Delegate = compiled};
        }

        public void Compile()
        {
            foreach (var mappingConfiguration in _mappingConfigurations)
            {
                _compiledMappings.Add(mappingConfiguration.Key, mappingConfiguration.Value());
            }
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            var compiledMapping = _compiledMappings.SingleOrDefault(m => m.Key.Source == typeof(TSource) && m.Key.Destination == typeof(TDestination));

            var mapper = (Func<TSource, TDestination>) compiledMapping.Value.GetDelegate();

            return mapper(source);
        }
    }
}