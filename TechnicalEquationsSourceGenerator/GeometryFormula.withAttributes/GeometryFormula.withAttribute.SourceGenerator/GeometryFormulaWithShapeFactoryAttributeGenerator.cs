using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GeometryFormula.withAttribute.SourceGenerator;

[Generator]
public class GeometryFormulaWithShapeFactoryAttributeGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		RegisterAttributesToSourceOutput(context, "FactoryMethodAttribute");

		var attributeImplementation = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (s, _) =>
			{
				if (s is AttributeSyntax attribute)
				{
					var testResult = s;
					return true;
				}
				return false;
			},
			transform: static (ctx, _) =>
			{
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

				if (!Regex.IsMatch(name, "^[a-zA-Z0-9]*$"))
				{
					if (name.Contains("="))
					{
						var splittedTexts = name.Split('=');
						var beforeEqualText = splittedTexts[0].Trim();
						var afterEqualText = splittedTexts[1].Trim();

						if (beforeEqualText == "name")
						{
							name = afterEqualText;
						}

					}
				}

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

	private static void RegisterAttributesToSourceOutput(IncrementalGeneratorInitializationContext context, string definitionName)
	{
		context.RegisterPostInitializationOutput(async ctx =>
		{

			var assembly = Assembly.GetExecutingAssembly();
			string resourcePath = assembly
				.GetManifestResourceNames()
				.Where(x => x.Contains($"{definitionName}.definition"))
				.FirstOrDefault();

			var fileContent = string.Empty;
			using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
			using (StreamReader reader = new StreamReader(stream))
			{
				fileContent = await reader.ReadToEndAsync();
			}

			ctx.AddSource("SharpAttribute.g.cs", SourceText.From(fileContent));
		});
	}
}