using Microsoft.VisualBasic.FileIO;

namespace AIDataConvertor.Services.KnowledgeBase;

public sealed class PropelloTemplateCsvHeaderReader : IPropelloTemplateCsvHeaderReader
{
	private const string TemplateDirectoryName = "PropelloTemplates";

	public Task<PropelloTemplateCsvHeaders> LoadHeadersAsync(
		string templateName,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
		cancellationToken.ThrowIfCancellationRequested();

		var templateDirectory = ResolveRepoDirectory(TemplateDirectoryName);
		if (templateDirectory is null)
		{
			return Task.FromResult(new PropelloTemplateCsvHeaders(
				templateName,
				null,
				null,
				[],
				"Could not find the PropelloTemplates directory from the current app environment."));
		}

		var filePath = Path.Combine(templateDirectory, $"{templateName}.csv");
		if (!File.Exists(filePath))
		{
			return Task.FromResult(new PropelloTemplateCsvHeaders(
				templateName,
				null,
				$"{templateName}.csv",
				[],
				$"The Propello template CSV was not found: {filePath}"));
		}

		using var parser = new TextFieldParser(filePath);
		parser.TextFieldType = FieldType.Delimited;
		parser.SetDelimiters(",");
		parser.HasFieldsEnclosedInQuotes = true;

		var headerRow = parser.ReadFields();
		var headers = headerRow?
			.Select(header => header.Trim())
			.Where(header => !string.IsNullOrWhiteSpace(header))
			.ToList() ?? [];

		return Task.FromResult(new PropelloTemplateCsvHeaders(
			templateName,
			filePath,
			Path.GetFileName(filePath),
			headers,
			headers.Count == 0 ? "The Propello template CSV contains an empty header row." : null));
	}

	private static string? ResolveRepoDirectory(string targetDirectoryName)
	{
		var current = new DirectoryInfo(AppContext.BaseDirectory);
		while (current is not null)
		{
			var candidate = Path.Combine(current.FullName, targetDirectoryName);
			if (Directory.Exists(candidate))
			{
				return candidate;
			}

			current = current.Parent;
		}

		return null;
	}
}