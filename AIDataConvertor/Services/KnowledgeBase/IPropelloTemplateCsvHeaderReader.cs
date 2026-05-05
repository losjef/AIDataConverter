namespace AIDataConvertor.Services.KnowledgeBase;

public interface IPropelloTemplateCsvHeaderReader
{
	Task<PropelloTemplateCsvHeaders> LoadHeadersAsync(
		string templateName,
		CancellationToken cancellationToken = default);
}