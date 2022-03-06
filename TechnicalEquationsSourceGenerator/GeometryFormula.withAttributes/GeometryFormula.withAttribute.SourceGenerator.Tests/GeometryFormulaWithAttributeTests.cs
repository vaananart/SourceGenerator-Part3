
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using VerifyXunit;

using Xunit;

namespace GeometryFormula.withAttribute.SourceGenerator.Tests;

[UsesVerify]
public class GeometryFormulaWithAttributeTests
{

    [Fact]
    public async Task<Task> SimpleAttributeTest()
	{
		//Arrange
		SyntaxTree syntaxTree = await CreateSyntaxTree("IShape.input");

		CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName: typeof(GeometryFormulaWithAttributeTests).Name,
				syntaxTrees: new[] { syntaxTree }
			);

		var generator = new GeometryFormulaWithShapeAttributeGenerator();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

		//Action
		driver = driver.RunGenerators(compilation);

		//Assert
		return Verifier.Verify(driver);

	}

	[Fact]
	public async Task<Task> SimpleAttributeWithAssignedParameterNameTest()
	{
		//Arrange
		SyntaxTree syntaxTree = await CreateSyntaxTree("IShapeWithAssignedParameter.input");

		CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName: typeof(GeometryFormulaWithAttributeTests).Name,
				syntaxTrees: new[] { syntaxTree }
			);

		var generator = new GeometryFormulaWithShapeAttributeGenerator();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

		//Action
		driver = driver.RunGenerators(compilation);

		//Assert
		return Verifier.Verify(driver);

	}

	[Fact]
	public async Task<Task> WithTwoShapeAttributesTest()
	{
		//Arrange
		SyntaxTree syntaxTree = await CreateSyntaxTree("IShapeWithTwoShapes.input");

		CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName: typeof(GeometryFormulaWithAttributeTests).Name,
				syntaxTrees: new[] { syntaxTree }
			);

		var generator = new GeometryFormulaWithShapeAttributeGenerator();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

		//Action
		driver = driver.RunGenerators(compilation);

		//Assert
		return Verifier.Verify(driver);

	}

	//[Fact]
	//public async Task<Task> WithAreaest()
	//{
	//	//Arrange
	//	SyntaxTree syntaxTree = await CreateSyntaxTree("IShapeWithArea.input");

	//	CSharpCompilation compilation = CSharpCompilation.Create(
	//			assemblyName: typeof(GeometryFormulaWithAttributeTests).Name,
	//			syntaxTrees: new[] { syntaxTree }
	//		);

	//	var generator = new GeometryFormulaWithShapeAttributeGenerator();
	//	GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

	//	//Action
	//	driver = driver.RunGenerators(compilation);

	//	//Assert
	//	return Verifier.Verify(driver);

	//}

	private static async Task<SyntaxTree> CreateSyntaxTree(string inputFileName)
	{
		var assembly = Assembly.GetExecutingAssembly();
		string resourcePath = assembly
			.GetManifestResourceNames()
		.Where(x => x.Contains(inputFileName))
		.FirstOrDefault()!;

		var fileContent = string.Empty;
		using (Stream stream = assembly.GetManifestResourceStream(resourcePath)!)
		using (StreamReader reader = new StreamReader(stream))
		{
			fileContent = await reader.ReadToEndAsync();
		}
		
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(fileContent));
		return syntaxTree;
	}

}