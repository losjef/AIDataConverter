namespace AIDataConvertor.Services.KnowledgeBase;

public interface ISemanticDictionaryQueryService
{
	Task<IReadOnlyList<HeaderMatchCandidate>> FindHeaderMatchesAsync(
		string sourceHeader,
		string? vendorName = null,
		CancellationToken cancellationToken = default);
}