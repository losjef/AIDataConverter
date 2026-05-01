using AIDataConvertor.Models.KnowledgeBase;

namespace AIDataConvertor.Services.KnowledgeBase;

public interface IPropelloTemplateMemoryRepository
{
	Task<PropelloTemplateMemoryDocument> LoadAsync(CancellationToken cancellationToken = default);
	Task SaveAsync(PropelloTemplateMemoryDocument document, CancellationToken cancellationToken = default);
	string GetWorkingFilePath();
}