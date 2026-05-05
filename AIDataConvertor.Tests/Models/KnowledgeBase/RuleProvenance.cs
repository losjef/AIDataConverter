using System.Text.Json.Serialization;

namespace AIDataConvertor.Models.KnowledgeBase;

public sealed class RuleProvenance
{
	[JsonPropertyName("source")]
	public string Source { get; set; } = string.Empty;

	[JsonPropertyName("recorded_on")]
	public string? RecordedOn { get; set; }

	[JsonPropertyName("review_state")]
	public string ReviewState { get; set; } = string.Empty;
}