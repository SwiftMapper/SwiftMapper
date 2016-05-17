using System;

namespace SwiftMapper
{
    internal interface IMappingOutput
    {
        Type MappingsType { get; set; }

        object GetDelegate();
    }
}