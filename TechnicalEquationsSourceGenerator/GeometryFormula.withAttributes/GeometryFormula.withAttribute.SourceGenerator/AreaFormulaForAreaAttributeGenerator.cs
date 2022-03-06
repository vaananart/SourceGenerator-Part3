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
		context.RegisterPostInitializationOutput(async ctx => {

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

		var attributeImplementation = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (s, _) => {
				if (s is AttributeSyntax attribute)
				{
					var testResult = s;
					return true;
				}
				return false;
			},
			transform: static (ctx, _) => {
				var testResult = ctx.Node as AttributeSyntax;
				return testResult;
			});

		var compilation = context.CompilationProvider.Combine(attributeImplementation.Collect());
		context.RegisterSourceOutput(compilation, async (ctx, source) =>
		{
			var matchedTokens = source.Right;
			var assembly = Assembly.GetExecutingAssembly();
			string resourcePath = assembly
				.GetManifestResourceNames()
				.Where(x => x.Contains("AreaFormula.template"))
				.FirstOrDefault();

			var fileContent = string.Empty;
			using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
			using (StreamReader reader = new StreamReader(stream))
			{
				fileContent = await reader.ReadToEndAsync();
			}

			var template = fileContent;


			foreach (var token in matchedTokens)
			{
				string? name = token.ArgumentList?
								.Arguments.FirstOrDefault()
								.ToString().Replace("\"", String.Empty);
				fileContent = fileContent.Replace("##SHAPEAREA##", name);

				string constructorContentString = string.Empty;
				string constructorParamString = string.Empty;
				fileContent = fileContent.Replace("##CONSTRUCTOR##", $"{name}({constructorParamString})\n\t\t{{\n\t\t\t{constructorContentString}\n\t\t}}");
				fileContent = fileContent.Replace("##AREAFORMULA##", "throw new NotImplementedException();");
				fileContent = fileContent.Replace("##PRIVATEVARAIBLES##", String.Empty);
				ctx.AddSource($"{name}.g.cs", SourceText.From(fileContent, Encoding.UTF8));
				fileContent = template;
			}
		});
	}
}