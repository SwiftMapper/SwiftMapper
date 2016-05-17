using System;

namespace SwiftMapper
{
    internal interface IMappingConfiguration
    {
        Type Source { get; }
        Type Destination { get; }
    }
}