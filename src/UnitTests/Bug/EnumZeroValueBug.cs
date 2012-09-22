using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class EnumZeroValueBug
    {
        public class Dummy
        {
            public DummyEnum DummyInt { get; set; }
        }

        public class DummyDto
        {
            public int DummyInt { get; set; }
        }

        public enum DummyEnum
        {
            A = 1,
            B = 2
        }

        [Test]
        public void Example()
        {
            AutoMapper.Mapper.CreateMap<Dummy, DummyDto>();
            AutoMapper.Mapper.CreateMap<DummyDto, Dummy>();

            var dto = new DummyDto
            {
                DummyInt = 1
            };

            var obj = new Dummy
            {
                DummyInt = DummyEnum.B
            };

            var convertedObj = AutoMapper.Mapper.Map<DummyDto, Dummy>(dto);

            var convertedDto = AutoMapper.Mapper.Map<Dummy, DummyDto>(obj);
        }
    }
}