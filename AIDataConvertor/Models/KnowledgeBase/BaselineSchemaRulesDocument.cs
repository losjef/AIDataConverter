using System.Text.Json.Serialization;

namespace AIDataConvertor.Models.KnowledgeBase;

public sealed class BaselineSchemaRulesDocument
{
	[JsonPropertyName("schema_version")]
	public string SchemaVersion { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("governance")]
	public BaselineSchemaRulesGovernance Governance { get; set; } = new();

	[JsonPropertyName("dynamic_time_series")]
	public DynamicTimeSeriesRules DynamicTimeSeries { get; set; } = new();

	[JsonPropertyName("workflow_profiles")]
	public Dictionary<string, BaselineWorkflowRuleDefinition> WorkflowProfiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class BaselineSchemaRulesGovernance
{
	[JsonPropertyName("notes")]
	public List<string> Notes { get; set; } = [];
}

public sealed class DynamicTimeSeriesRules
{
	[JsonPropertyName("supported_period_formats")]
	public List<string> SupportedPeriodFormats { get; set; } = [];

	[JsonPropertyName("quarter_pattern")]
	public string QuarterPattern { get; set; } = string.Empty;
}

public sealed class BaselineWorkflowRuleDefinition
{
	[JsonPropertyName("required_fields")]
	public List<string> RequiredFields { get; set; } = [];
}