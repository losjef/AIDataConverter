namespace AIDataConvertor.Services.KnowledgeBase;

public sealed record PropelloTemplateCsvHeaders(
	string TemplateName,
	string? FilePath,
	string? FileName,
	IReadOnlyList<string> Headers,
	string? ErrorMessage)
{
	public bool FoundFile => !string.IsNullOrWhiteSpace(FilePath);
	public bool HasHeaders => Headers.Count > 0;
}