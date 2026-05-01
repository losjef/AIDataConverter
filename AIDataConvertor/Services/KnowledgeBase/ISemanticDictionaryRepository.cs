using AIDataConvertor.Models.KnowledgeBase;

namespace AIDataConvertor.Services.KnowledgeBase;

public interface ISemanticDictionaryRepository
{
	Task<SemanticDictionaryDocument> LoadAsync(CancellationToken cancellationToken = default);
	Task SaveAsync(SemanticDictionaryDocument document, CancellationToken cancellationToken = default);
	string GetWorkingFilePath();
}