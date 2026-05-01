namespace AIDataConvertor.Services.KnowledgeBase;

public sealed record BaselineWorkflowProfile(string Name)
{
	public static readonly BaselineWorkflowProfile UpcMatching = new("UPC Matching");

	public static readonly BaselineWorkflowProfile ProductImportEnrichment = new("ProductImport Enrichment (first pass)");
}