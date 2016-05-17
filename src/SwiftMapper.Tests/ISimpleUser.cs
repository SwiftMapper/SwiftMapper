using System;

namespace SwiftMapper.Tests
{
    public interface ISimpleUser
    {
        string Name { get; set; }
        int Age { get; set; }
        DateTime Joined { get; set; }
        Gender Gender { get; set; }
        decimal Score { get; set; }
        string ToString();
    }
}