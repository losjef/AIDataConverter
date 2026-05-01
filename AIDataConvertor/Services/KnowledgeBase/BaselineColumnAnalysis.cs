namespace AIDataConvertor.Services.KnowledgeBase;

public sealed record BaselineColumnAnalysis(
	string SourceHeader,
	string Classification,
	string? DynamicColumnGroup,
	bool IsRequired,
	bool IsResolved,
	bool IsDynamicTimeSeries,
	string? ResolvedField,
	string Reason,
	IReadOnlyList<HeaderMatchCandidate> Candidates);

public sealed record BaselineReadinessReport(
	string WorkflowName,
	IReadOnlyList<string> RequiredFields,
	IReadOnlyList<BaselineColumnAnalysis> Columns,
	IReadOnlyList<string> MissingRequiredFields,
	bool IsReady,
	int DynamicColumnCount);