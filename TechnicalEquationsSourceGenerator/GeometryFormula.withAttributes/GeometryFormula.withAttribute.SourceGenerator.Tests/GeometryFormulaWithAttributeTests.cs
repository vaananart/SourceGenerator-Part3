
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
		var assembly = Assembly.GetExecutingAssembly();
		string resourcePath = assembly
			.GetManifestResourceNames()
		.Where(x => x.Contains(".input"))
		.FirstOrDefault()!;

		var fileContent = string.Empty;
		using (Stream stream = assembly.GetManifestResourceStream(resourcePath)!)
		using (StreamReader reader = new StreamReader(stream))
		{
			fileContent = await reader.ReadToEndAsync();
		}
		var directoryString = Path.GetDirectoryName(assembly.Location);
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText( SourceText.From(fileContent));

		CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName: typeof(GeometryFormulaWithAttributeTests).Name,
				syntaxTrees: new[] { syntaxTree }
			);

		var generator = new AreaFormulaForAreaAttributeGenerator();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

		//Action
		driver = driver.RunGenerators(compilation);

		//Assert
		return Verifier.Verify(driver);

	}
}