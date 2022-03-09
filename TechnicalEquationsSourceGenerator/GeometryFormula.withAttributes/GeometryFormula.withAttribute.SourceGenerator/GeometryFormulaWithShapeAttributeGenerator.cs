using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GeometryFormula.withAttribute.SourceGenerator;

[Generator]
public class GeometryFormulaWithShapeAttributeGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		RegisterAttributesToSourceOutput(context, "ShapeAttribute");

		var attributeImplementation = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (s, _) =>
			{
			if (s is AttributeSyntax attribute && attribute.Name.GetText().ToString()=="Shape")
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
				string? argument1 = token.ArgumentList?
								.Arguments[0]?
								.ToString().Replace("\"", String.Empty);

				string? argument2 = token.ArgumentList?
								.Arguments[1]?
								.ToString().Replace("\"", String.Empty);

				string name = argument1;
				string areaformula = argument2;

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
						else
						{ 
							areaformula = afterEqualText;
						}
					}
				}

				if (!Regex.IsMatch(areaformula, "^[a-zA-Z0-9]*$"))
				{
					if (areaformula.Contains("="))
					{
						var splittedTexts = areaformula.Split('=');
						var beforeEqualText = splittedTexts[0].Trim();
						var afterEqualText = splittedTexts[1].Trim();

						if (beforeEqualText == "areaFormula")
						{
							areaformula = afterEqualText;
						}
						else
						{
							name = afterEqualText;
						}
					}
				}

				fileContent = fileContent.Replace("##SHAPEAREA##", name);

				string constructorContentString = string.Empty;
				string constructorParamString = string.Empty;
				string processingFormulaString = areaformula;
				string privateVariablesString = string.Empty;
				//fileContent = fileContent.Replace("##CONSTRUCTOR##", $"{name}({constructorParamString})\n\t\t{{\n\t\t\t{constructorContentString}\n\t\t}}");

				if (string.IsNullOrEmpty(areaformula))
				{
					fileContent = fileContent.Replace("##AREAFORMULA##", "throw new NotImplementedException();");
					fileContent = fileContent.Replace("##PRIVATEVARAIBLES##", String.Empty);
				}
				else
				{
					var variables = (from character in areaformula
									where character >= 'a' && character <= 'z'
									select character).Distinct();

					foreach (var variable in variables)
					{
						constructorParamString += $"double {variable},";
						processingFormulaString = processingFormulaString.Replace(variable.ToString(), $"_{variable}");
						privateVariablesString += $"private readonly double _{variable};\n\t\t";
						constructorContentString += $"_{variable} = {variable};\n\t\t\t";
					}
					
					constructorParamString = constructorParamString.TrimEnd(',');
					constructorParamString = constructorParamString.Replace(",", ", ");
					constructorContentString = constructorContentString.TrimEnd('\n', '\t', '\t');

					fileContent = fileContent.Replace("##CONSTRUCTOR##", $"{name}({constructorParamString})\n\t\t{{\n\t\t\t{constructorContentString}\n\t\t}}");
					fileContent = fileContent.Replace("##AREAFORMULA##", new StringBuilder().Append(processingFormulaString).Append(";").ToString());
					fileContent = fileContent.Replace("##PRIVATEVARAIBLES##", $"{privateVariablesString}");
				}

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

			ctx.AddSource("SharpAttribute.g.cs", SourceText.From(fileContent, Encoding.UTF8));

			var test = 2;
		});
	}
}