using System;

namespace SwiftMapper.Tests
{
    public class SimpleUserDto : ISimpleUser
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Joined { get; set; }
        public Gender Gender { get; set; }
        public decimal Score { get; set; }

        public override string ToString()
        {
            return $"{Name} - {Gender}; Age: {Age}; Joined: {Joined}; Score: {Score}";
        }
    }
}