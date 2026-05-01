using System.Text.Json.Serialization;

namespace AIDataConvertor.Models.KnowledgeBase;

public sealed class PropelloTemplateMemoryDocument
{
	[JsonPropertyName("schema_version")]
	public string SchemaVersion { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("governance")]
	public PropelloTemplateMemoryGovernance Governance { get; set; } = new();

	[JsonPropertyName("shared_concepts")]
	public Dictionary<string, PropelloSharedConcept> SharedConcepts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

	[JsonPropertyName("templates")]
	public Dictionary<string, PropelloTemplateDefinition> Templates { get; set; } = new(StringComparer.OrdinalIgnoreCase);

	[JsonPropertyName("relationships")]
	public List<PropelloTemplateRelationship> Relationships { get; set; } = [];
}

public sealed class PropelloTemplateMemoryGovernance
{
	[JsonPropertyName("single_template_execution_default")]
	public bool SingleTemplateExecutionDefault { get; set; }

	[JsonPropertyName("canonical_link_field")]
	public string CanonicalLinkField { get; set; } = string.Empty;

	[JsonPropertyName("notes")]
	public List<string> Notes { get; set; } = [];
}

public sealed class PropelloSharedConcept
{
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();
}

public sealed class PropelloTemplateDefinition
{
	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("primary_entity")]
	public string PrimaryEntity { get; set; } = string.Empty;

	[JsonPropertyName("execution_role")]
	public string ExecutionRole { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("shared_concepts")]
	public List<string> SharedConcepts { get; set; } = [];

	[JsonPropertyName("canonical_fields")]
	public List<PropelloTemplateField> CanonicalFields { get; set; } = [];
}

public sealed class PropelloTemplateField
{
	[JsonPropertyName("field_name")]
	public string FieldName { get; set; } = string.Empty;

	[JsonPropertyName("concept")]
	public string Concept { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	[JsonPropertyName("is_link_key")]
	public bool IsLinkKey { get; set; }

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();
}

public sealed class PropelloTemplateRelationship
{
	[JsonPropertyName("from_template")]
	public string FromTemplate { get; set; } = string.Empty;

	[JsonPropertyName("to_template")]
	public string ToTemplate { get; set; } = string.Empty;

	[JsonPropertyName("relationship_type")]
	public string RelationshipType { get; set; } = string.Empty;

	[JsonPropertyName("shared_concepts")]
	public List<string> SharedConcepts { get; set; } = [];

	[JsonPropertyName("notes")]
	public List<string> Notes { get; set; } = [];

	[JsonPropertyName("provenance")]
	public RuleProvenance Provenance { get; set; } = new();
}