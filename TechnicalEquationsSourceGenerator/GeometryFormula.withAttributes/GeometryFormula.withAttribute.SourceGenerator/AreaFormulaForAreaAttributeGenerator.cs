using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace GeometryFormula.withAttribute.SourceGenerator;

[Generator]
public class AreaFormulaForAreaAttributeGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput( async ctx =>  {

			var assembly = Assembly.GetExecutingAssembly();
			string resourcePath = assembly
				.GetManifestResourceNames()
				.Where(x => x.Contains(".definition"))
				.FirstOrDefault();

			var fileContent = string.Empty;
			using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
			using (StreamReader reader = new StreamReader(stream))
			{
				fileContent = await reader.ReadToEndAsync();
			}

			ctx.AddSource("SharpAttribute.g.cs", SourceText.From(fileContent));
		});

		var attributeImplementation =  context.SyntaxProvider.CreateSyntaxProvider(
			predicate:static (s, _)=> {
				var testResult = s;
				return true;
			},
			transform: static (ctx, _)=> {
				var testResult = ctx.Node;
				return testResult;
			});

		var compilation = context.CompilationProvider.Combine(attributeImplementation.Collect());
		context.RegisterSourceOutput(compilation,async (ctx, source)=>
		{
			var matchedTokens = source.Right;
		});
	}
}