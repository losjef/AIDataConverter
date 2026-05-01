using ClosedXML.Excel;

namespace AIDataConvertor.Services.KnowledgeBase;

public sealed class SuperiorHardwareBaselineHeaderReader : ISuperiorHardwareBaselineHeaderReader
{
	private const string BaselineDirectoryName = "SuperiorHardware";

	public Task<BaselineWorkbookHeaders> LoadLatestHeadersAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var baselineDirectory = ResolveBaselineDirectory();
		if (baselineDirectory is null)
		{
			return Task.FromResult(new BaselineWorkbookHeaders(
				null,
				null,
				[],
				"Could not find the SuperiorHardware directory from the current app environment."));
		}

		var latestFile = new DirectoryInfo(baselineDirectory)
			.EnumerateFiles("*.xlsx", SearchOption.TopDirectoryOnly)
			.OrderByDescending(file => file.LastWriteTimeUtc)
			.FirstOrDefault();

		if (latestFile is null)
		{
			return Task.FromResult(new BaselineWorkbookHeaders(
				null,
				null,
				[],
				"No SuperiorHardware .xlsx export was found."));
		}

		using var workbook = new XLWorkbook(latestFile.FullName);
		var worksheet = workbook.Worksheets.FirstOrDefault();
		var firstUsedRow = worksheet?.FirstRowUsed();

		if (firstUsedRow is null)
		{
			return Task.FromResult(new BaselineWorkbookHeaders(
				latestFile.FullName,
				latestFile.Name,
				[],
				"The latest SuperiorHardware export does not contain a usable header row."));
		}

		var headers = firstUsedRow.CellsUsed()
			.Select(cell => cell.GetString().Trim())
			.Where(header => !string.IsNullOrWhiteSpace(header))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		return Task.FromResult(new BaselineWorkbookHeaders(
			latestFile.FullName,
			latestFile.Name,
			headers,
			headers.Count == 0 ? "The latest SuperiorHardware export contains an empty header row." : null));
	}

	private static string? ResolveBaselineDirectory()
	{
		var current = new DirectoryInfo(AppContext.BaseDirectory);
		while (current is not null)
		{
			var candidate = Path.Combine(current.FullName, BaselineDirectoryName);
			if (Directory.Exists(candidate))
			{
				return candidate;
			}

			current = current.Parent;
		}

		return null;
	}
}