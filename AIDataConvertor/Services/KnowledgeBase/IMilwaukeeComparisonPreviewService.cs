namespace AIDataConvertor.Services.KnowledgeBase;

public interface IMilwaukeeComparisonPreviewService
{
	Task<MilwaukeeComparisonPreview> LoadPreviewAsync(CancellationToken cancellationToken = default);
}