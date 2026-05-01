using AIDataConvertor.Models.KnowledgeBase;

namespace AIDataConvertor.Services.KnowledgeBase;

public interface IManualLinksRepository
{
	Task<ManualLinksDocument> LoadAsync(CancellationToken cancellationToken = default);
	Task SaveAsync(ManualLinksDocument document, CancellationToken cancellationToken = default);
	string GetWorkingFilePath();
}