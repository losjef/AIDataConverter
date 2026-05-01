namespace AIDataConvertor.Services.KnowledgeBase;

public interface ISuperiorHardwareBaselineHeaderReader
{
	Task<BaselineWorkbookHeaders> LoadLatestHeadersAsync(CancellationToken cancellationToken = default);
}