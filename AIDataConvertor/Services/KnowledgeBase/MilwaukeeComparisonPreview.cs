namespace AIDataConvertor.Services.KnowledgeBase;

public sealed record MilwaukeeComparisonPreview(
	string? CostsFileName,
	string? ImapFileName,
	int CostsRowCount,
	int ImapRowCount,
	int MatchedRowCount,
	int UnmatchedRowCount,
	int AmbiguousRowCount,
	IReadOnlyList<MilwaukeeComparisonPreviewRow> Rows,
	string? ErrorMessage)
{
	public bool HasRows => Rows.Count > 0;
}