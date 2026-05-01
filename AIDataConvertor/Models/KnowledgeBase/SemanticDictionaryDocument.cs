using System.Text.Json.Serialization;

namespace AIDataConvertor.Models.KnowledgeBase;

public sealed class SemanticDictionaryDocument
{
	[JsonPropertyName("schema_version")]
	public string SchemaVersion { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("governance")]
	public GovernanceSettings Governance { get; set; } = new();

	[JsonPropertyName("cross_vendor_seed_behavior")]
	public CrossVendorSeedBehavior CrossVendorSeedBehavior { get; set; } = new();

	[JsonPropertyName("shared_concepts")]
	public Dictionary<string, SharedConcept> SharedConcepts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

	[JsonPropertyName("vendors")]
	public Dictionary<string, VendorDefinition> Vendors { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class GovernanceSettings
{
	[JsonPropertyName("user_review_precedes_ai_when_practical")]
	public bool UserReviewPrecedesAiWhenPractical { get; set; }

	[JsonPropertyName("cross_vendor_reuse_precedes_ai_for_new_vendors")]
	public bool CrossVendorReusePrecedesAiForNewVendors { get; set; }

	[JsonPropertyName("rule_statuses")]
	public List<string> RuleStatuses { get; set; } = [];

	[JsonPropertyName("confidence_scale")]
	public string ConfidenceScale { get; set; } = string.Empty;

	[JsonPropertyName("notes")]
	public List<string> Notes { get; set; } = [];
}

public sealed class CrossVendorSeedBehavior
{
	[JsonPropertyName("new_vendor_review_flow")]
	public List<string> NewVendorReviewFlow { get; set; } = [];

	[JsonPropertyName("seed_mode")]
	public string SeedMode { get; set; } = string.Empty;

	[JsonPropertyName("require_user_review_for_seeded_matches")]
	public bool RequireUserReviewForSeededMatches { get; set; }

	[JsonPropertyName("allow_seed_reuse_from_existing_vendors")]
	public bool AllowSeedReuseFromExistingVendors { get; set; }

	[JsonPropertyName("notes")]
	public List<string> Notes { get; set; } = [];
}

public sealed class SharedConcept
{
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("seed_headers")]
	public List<SeedHeaderRule> SeedHeaders { get; set; } = [];
}

public sealed class VendorDefinition
{
	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();

	[JsonPropertyName("seed_shared_concepts")]
	public List<string> SeedSharedConcepts { get; set; } = [];

	[JsonPropertyName("rule_sets")]
	public List<VendorRuleSet> RuleSets { get; set; } = [];
}

public sealed class VendorRuleSet
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("confidence")]
	public decimal Confidence { get; set; }

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();

	[JsonPropertyName("mapping_rules")]
	public List<HeaderMappingRule> MappingRules { get; set; } = [];

	[JsonPropertyName("default_rules")]
	public List<DefaultValueRule> DefaultRules { get; set; } = [];

	[JsonPropertyName("normalization_rules")]
	public List<NormalizationRule> NormalizationRules { get; set; } = [];
}

public sealed class SeedHeaderRule
{
	[JsonPropertyName("source_header")]
	public string SourceHeader { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("confidence")]
	public decimal Confidence { get; set; }

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();
}

public sealed class HeaderMappingRule
{
	[JsonPropertyName("source_header")]
	public string SourceHeader { get; set; } = string.Empty;

	[JsonPropertyName("target_field")]
	public string TargetField { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("confidence")]
	public decimal Confidence { get; set; }

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();
}

public sealed class DefaultValueRule
{
	[JsonPropertyName("target_field")]
	public string TargetField { get; set; } = string.Empty;

	[JsonPropertyName("value")]
	public object? Value { get; set; }

	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("confidence")]
	public decimal Confidence { get; set; }

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();
}

public sealed class NormalizationRule
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("applies_to")]
	public string AppliesTo { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("confidence")]
	public decimal Confidence { get; set; }

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();

	[JsonPropertyName("notes")]
	public List<string> Notes { get; set; } = [];
}

public sealed class RuleProvenance
{
	[JsonPropertyName("source")]
	public string Source { get; set; } = string.Empty;

	[JsonPropertyName("recorded_on")]
	public string? RecordedOn { get; set; }

	[JsonPropertyName("review_state")]
	public string ReviewState { get; set; } = string.Empty;
}