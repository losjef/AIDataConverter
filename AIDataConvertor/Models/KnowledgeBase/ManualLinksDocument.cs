using System.Text.Json.Serialization;

namespace AIDataConvertor.Models.KnowledgeBase;

public sealed class ManualLinksDocument
{
	[JsonPropertyName("schema_version")]
	public string SchemaVersion { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("governance")]
	public ManualLinksGovernance Governance { get; set; } = new();

	[JsonPropertyName("links")]
	public List<ManualLinkRecord> Links { get; set; } = [];
}

public sealed class ManualLinksGovernance
{
	[JsonPropertyName("new_links_require_user_review")]
	public bool NewLinksRequireUserReview { get; set; }

	[JsonPropertyName("statuses")]
	public List<string> Statuses { get; set; } = [];

	[JsonPropertyName("notes")]
	public List<string> Notes { get; set; } = [];
}

public sealed class ManualLinkRecord
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("source_system")]
	public string SourceSystem { get; set; } = string.Empty;

	[JsonPropertyName("source_vendor")]
	public string? SourceVendor { get; set; }

	[JsonPropertyName("source_item_number")]
	public string? SourceItemNumber { get; set; }

	[JsonPropertyName("source_upc")]
	public string? SourceUpc { get; set; }

	[JsonPropertyName("target_system")]
	public string TargetSystem { get; set; } = string.Empty;

	[JsonPropertyName("target_dataset")]
	public string? TargetDataset { get; set; }

	[JsonPropertyName("target_item_number")]
	public string? TargetItemNumber { get; set; }

	[JsonPropertyName("target_upc")]
	public string? TargetUpc { get; set; }

	[JsonPropertyName("link_basis")]
	public List<string> LinkBasis { get; set; } = [];

	[JsonPropertyName("notes")]
	public List<string> Notes { get; set; } = [];

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();
}