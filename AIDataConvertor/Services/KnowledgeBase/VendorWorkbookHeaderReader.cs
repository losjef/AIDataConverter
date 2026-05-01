using ClosedXML.Excel;

namespace AIDataConvertor.Services.KnowledgeBase;

public sealed class VendorWorkbookHeaderReader : IVendorWorkbookHeaderReader
{
	private const string VendorRootDirectoryName = "VendorFiles";

	public Task<VendorWorkbookHeaders> LoadLatestHeadersAsync(
		string vendorName,
		string feedName,
		CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var vendorRootDirectory = ResolveRepoDirectory(VendorRootDirectoryName);
		if (vendorRootDirectory is null)
		{
			return Task.FromResult(new VendorWorkbookHeaders(
				vendorName,
				feedName,
				null,
				null,
				[],
				"Could not find the VendorFiles directory from the current app environment."));
		}

		var feedDirectory = Path.Combine(vendorRootDirectory, vendorName, feedName);
		if (!Directory.Exists(feedDirectory))
		{
			return Task.FromResult(new VendorWorkbookHeaders(
				vendorName,
				feedName,
				null,
				null,
				[],
				$"The vendor feed directory was not found: {feedDirectory}"));
		}

		var latestFile = new DirectoryInfo(feedDirectory)
			.EnumerateFiles("*.xlsx", SearchOption.TopDirectoryOnly)
			.OrderByDescending(file => file.LastWriteTimeUtc)
			.FirstOrDefault();

		if (latestFile is null)
		{
			return Task.FromResult(new VendorWorkbookHeaders(
				vendorName,
				feedName,
				null,
				null,
				[],
				"No vendor workbook was found for the requested feed."));
		}

		using var workbook = new XLWorkbook(latestFile.FullName);
		var worksheet = workbook.Worksheets.FirstOrDefault();
		var firstUsedRow = worksheet?.FirstRowUsed();

		if (firstUsedRow is null)
		{
			return Task.FromResult(new VendorWorkbookHeaders(
				vendorName,
				feedName,
				latestFile.FullName,
				latestFile.Name,
				[],
				"The vendor workbook does not contain a usable header row."));
		}

		var headers = firstUsedRow.CellsUsed()
			.Select(cell => cell.GetString().Trim())
			.Where(header => !string.IsNullOrWhiteSpace(header))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		return Task.FromResult(new VendorWorkbookHeaders(
			vendorName,
			feedName,
			latestFile.FullName,
			latestFile.Name,
			headers,
			headers.Count == 0 ? "The vendor workbook contains an empty header row." : null));
	}

	private static string? ResolveRepoDirectory(string targetDirectoryName)
	{
		var current = new DirectoryInfo(AppContext.BaseDirectory);
		while (current is not null)
		{
			var candidate = Path.Combine(current.FullName, targetDirectoryName);
			if (Directory.Exists(candidate))
			{
				return candidate;
			}

			current = current.Parent;
		}

		return null;
	}
}