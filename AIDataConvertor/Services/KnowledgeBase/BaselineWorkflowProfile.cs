namespace AIDataConvertor.Services.KnowledgeBase;

public sealed record BaselineWorkflowProfile(string Name, IReadOnlyList<string> RequiredFields)
{
	public static readonly BaselineWorkflowProfile UpcMatching = new(
		"UPC Matching",
		["Item", "UPC", "Description"]);

	public static readonly BaselineWorkflowProfile ProductImportEnrichment = new(
		"ProductImport Enrichment (first pass)",
		["Item", "Description", "UPC", "Default Sell Price", "Purchase Cost"]);
}