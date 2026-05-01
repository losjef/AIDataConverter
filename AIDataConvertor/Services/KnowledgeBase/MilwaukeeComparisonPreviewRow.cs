namespace AIDataConvertor.Services.KnowledgeBase;

public sealed record MilwaukeeComparisonPreviewRow(
	int CostsRowNumber,
	string? CostsItemNumber,
	string? CostsUpcCode,
	string? CostsDescription,
	string MatchStatus,
	string MatchBasis,
	string? ImapItemNumber,
	string? ImapUpcCode,
	string? ImapMinimumAdvertisedPrice,
	string Note);