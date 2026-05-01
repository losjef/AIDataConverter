using AIDataConvertor.Models.KnowledgeBase;

namespace AIDataConvertor.Services.KnowledgeBase;

public interface IBaselineSchemaRulesRepository
{
	Task<BaselineSchemaRulesDocument> LoadAsync(CancellationToken cancellationToken = default);
	Task SaveAsync(BaselineSchemaRulesDocument document, CancellationToken cancellationToken = default);
	string GetWorkingFilePath();
}