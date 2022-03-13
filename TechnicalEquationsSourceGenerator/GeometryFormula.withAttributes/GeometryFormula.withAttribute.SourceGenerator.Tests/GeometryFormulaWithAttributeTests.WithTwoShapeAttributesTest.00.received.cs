//HintName: SharpAttribute.g.cs
namespace GeometryFormula.withAttribute.SourceGenerator.Formulas
{
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
	public class ShapeAttribute : System.Attribute
	{
		public ShapeAttribute(string name, string areaFormula)
		{
		}

		public ShapeAttribute(string name)
		{
		}

		public ShapeAttribute()
		{
		}

		public string areaFormula {get; set;}
		public string name {get; set;}
	}
}