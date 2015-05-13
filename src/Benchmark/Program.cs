using System;
using System.Collections.Generic;
using Benchmark.Flattening;

namespace Benchmark
{
    using System.Reflection;
    using System.Reflection.Emit;
    using AutoMapper;

    public class Program
	{
	    private class Foo
	    {
            public Foo() { }

	        public Foo(string blarg, int blerg)
	        {
	            
	        }
	        public string Blarg { get; set; }
	        public int Blerg { get; set; }
	        public string Blirg;
	        public int Blorg;

	        public string Blurg()
	        {
	            return "Blurg";
	        }

	        public int Blyrg()
	        {
	            return 5;
	        }
	    }

        private void Aasdf(object target, object value)
        {
            ((Foo) target).Blarg = (string) value;
        }

        private void Aasdf2(object target, object value)
        {
            ((Foo) target).Blerg = (int) value;
        }

        private object Aasdf3(object target)
        {
            return ((Foo) target).Blarg;
        }

        private object Aasdf4(object target)
        {
            return ((Foo) target).Blerg;
        }

        private void Aaasdf(object target, object value)
        {
            ((Foo) target).Blirg = (string) value;
        }

        private void Aaasdf2(object target, object value)
        {
            ((Foo) target).Blorg = (int) value;
        }

        private object Aaasdf3(object target)
        {
            return ((Foo) target).Blirg;
        }

        private object Aaasdf4(object target)
        {
            return ((Foo) target).Blorg;
        }

        private object Aasdf5(object target)
        {
            return ((Foo) target).Blurg();
        }

        private object Aasdf6(object target)
        {
            return ((Foo) target).Blyrg();
        }

        private object Aasdf7(params object[] parameters)
        {
            return new Foo((string)parameters[0], (int)parameters[1]);
        }

		public static void Main(string[] args)
		{
            var method = new DynamicMethod("cheat", typeof(object),
            new[] { typeof(object) }, typeof(Foo), true);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, typeof(Foo));
            il.Emit(OpCodes.Callvirt, typeof(Foo).GetProperty("Blarg",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                ).GetGetMethod(true));
            il.Emit(OpCodes.Ret);
            var func = (LateBoundPropertyGet)method.CreateDelegate(
                typeof(LateBoundPropertyGet));

            var obj = new Foo { Blarg = "Blarg"};
            Console.WriteLine(func(obj));

            DynamicMethod method2 = new DynamicMethod("Setter", typeof(void), new[] { typeof(object), typeof(object) }, true);
            var ilgen = method2.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Castclass, typeof(Foo));
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Castclass, typeof(string));
            ilgen.Emit(OpCodes.Callvirt, typeof(Foo).GetProperty("Blarg").GetSetMethod());
            ilgen.Emit(OpCodes.Ret);
            var action = (LateBoundPropertySet)method2.CreateDelegate(typeof(LateBoundPropertySet));
		    action(obj, "Blerg");
            Console.WriteLine(obj.Blarg);

            var mappers = new Dictionary<string, IObjectToObjectMapper[]>
				{
					{ "Flattening", new IObjectToObjectMapper[] { new FlatteningMapper(), new ManualMapper() } },
					{ "Ctors", new IObjectToObjectMapper[] { new CtorMapper(), new ManualCtorMapper(),  } }
				};
		

			foreach (var pair in mappers)
			{
				foreach (var mapper in pair.Value)
				{
					new BenchEngine(mapper, pair.Key).Start();
				}
			}
		}
	}
}
