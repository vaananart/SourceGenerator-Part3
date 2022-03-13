using GeometryFormula.withAttribute.SourceGenerator.Formulas;

namespace GeometryFormula.withAttribute;

[Shape("Trapezoid", "((a + b) / 2.0) * h")]
[Shape("Parallelogram", "a * h")]
[Shape("Reactangle","a * b")]
[Shape("Triangle", "(b * h)/2.0")]
[Shape( name="Square", areaFormula = "a * a")]
public class ShapeFacade
{
	public double ForParallelogram(double a, double h) => new SourceGenerator.Formulas.Parallelogram(a, h).Area;

	public double ForReactangle(double a, double b) => new SourceGenerator.Formulas.Reactangle(a, b).Area;

	public double ForSquare(double a) => new SourceGenerator.Formulas.Square(a).Area;

	public double ForTrapezoid(double a, double b,double h) => new SourceGenerator.Formulas.Trapezoid( a, b, h).Area;

	public double ForTriangle(double a, double h) => new SourceGenerator.Formulas.Triangle(a, h).Area;
}

