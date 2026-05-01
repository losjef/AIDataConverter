namespace AIDataConvertor.Services.KnowledgeBase;

public interface IBaselineSchemaAnalyzer
{
	Task<BaselineReadinessReport> AnalyzeAsync(
		IReadOnlyList<string> sourceHeaders,
		BaselineWorkflowProfile workflowProfile,
		CancellationToken cancellationToken = default);
}