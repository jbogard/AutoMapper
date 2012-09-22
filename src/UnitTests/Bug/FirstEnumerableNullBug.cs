using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.Mappers;
using Should;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
	[TestFixture]
	public class FirstEnumerableItemNullBug : AutoMapperSpecBase
	{
        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
        }

		[Test]
		public void Should_not_throw_errors()
		{
            Mapper.CreateMap<Source, Destination>();

		    var src = new Source
		    {
		        Value = 5
		    };

		    var items = new List<Source>() {null, src};

		    var dest = Mapper.Map<IEnumerable<Source>, List<Destination>>(items);

		    dest[0].ShouldBeNull();
		    dest[1].ShouldNotBeNull();
            dest[1].Value.ShouldEqual(src.Value);
		}
	}
}
