
using System.Runtime.CompilerServices;

using VerifyTests;

namespace GeometryFormula.withAttribute.SourceGenerator.Tests
{
	public class ModuleInitialiser
	{
		[ModuleInitializer]
		public static void Init()
		{
			VerifySourceGenerators.Enable();
		}
	}
}
