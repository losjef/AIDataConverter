namespace AIDataConvertor.Services.KnowledgeBase;

public sealed record VendorComparisonRow(
	string SourceHeader,
	string? ResolvedField,
	string ResolutionSource,
	decimal? Confidence,
	bool ExistsInBaselineReadiness,
	string ComparisonNote);