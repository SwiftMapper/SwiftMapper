using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SwiftMapper.Tests
{
    [TestClass]
    public class SimpleMapTests
    {
        [TestMethod]
        public void SimpleMapTest()
        {
            var mapper = new SwiftMapper(new MapperConfiguration());
            mapper.RegisterMapping<SimpleUser, SimpleUserDto>();
            mapper.Compile();

            var user = new SimpleUser
            {
                Age = 29,
                Gender = Gender.Male,
                Joined = new DateTime(2016, 05, 17, 18, 52, 34),
                Score = 18.2M,
                Name = "Mike"
            };

            var dto = mapper.Map<SimpleUser, SimpleUserDto>(user);
            Assert.AreEqual(user.ToString(), dto.ToString());

            var dto2 = mapper.Map2<SimpleUser, SimpleUserDto>(user);
            Assert.AreEqual(user.ToString(), dto2.ToString());
        }
    }
}