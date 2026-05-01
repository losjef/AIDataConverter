namespace AIDataConvertor.Services.KnowledgeBase;

public interface IVendorWorkbookHeaderReader
{
	Task<VendorWorkbookHeaders> LoadLatestHeadersAsync(
		string vendorName,
		string feedName,
		CancellationToken cancellationToken = default);
}