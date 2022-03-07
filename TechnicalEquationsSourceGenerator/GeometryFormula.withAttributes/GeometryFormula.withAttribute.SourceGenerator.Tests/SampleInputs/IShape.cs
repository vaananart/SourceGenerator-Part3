namespace GeometryFormula.withAttribute.SourceGenerator.Tests.SampleInputs
{
	[Shape("Square")]
	[Area("Square", "a * a")]
	public interface IShape
	{
		double Area{get;}

	}
}
