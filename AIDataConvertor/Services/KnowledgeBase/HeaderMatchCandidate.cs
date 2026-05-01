namespace AIDataConvertor.Services.KnowledgeBase;

public sealed record HeaderMatchCandidate(
	string SourceHeader,
	string TargetField,
	string MatchSource,
	string? SourceVendor,
	string? SourceRuleSetId,
	decimal Confidence,
	string Status);