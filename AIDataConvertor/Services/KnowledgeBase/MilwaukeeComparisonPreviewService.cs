using ClosedXML.Excel;
using AIDataConvertor.Models.KnowledgeBase;

namespace AIDataConvertor.Services.KnowledgeBase;

public sealed class MilwaukeeComparisonPreviewService : IMilwaukeeComparisonPreviewService
{
	private static readonly HashSet<string> ItemNumberAliases = new(StringComparer.OrdinalIgnoreCase)
	{
		"ITEMNO",
		"ITEMNUMBER",
		"ITEMNUM"
	};

	private static readonly HashSet<string> UpcCodeAliases = new(StringComparer.OrdinalIgnoreCase)
	{
		"UPCCODE",
		"UPC"
	};

	private static readonly HashSet<string> DescriptionAliases = new(StringComparer.OrdinalIgnoreCase)
	{
		"DESCRIPTION",
		"ITEMDESCRIPTION",
		"PRODUCTDESCRIPTION"
	};

	private static readonly HashSet<string> MinimumAdvertisedPriceAliases = new(StringComparer.OrdinalIgnoreCase)
	{
		"MAP",
		"MAPPRICE",
		"MINIMUMADVERTISEDPRICE",
		"IMAP"
	};

	private readonly IVendorWorkbookHeaderReader vendorWorkbookHeaderReader;
	private readonly IManualLinksRepository manualLinksRepository;

	public MilwaukeeComparisonPreviewService(
		IVendorWorkbookHeaderReader vendorWorkbookHeaderReader,
		IManualLinksRepository manualLinksRepository)
	{
		this.vendorWorkbookHeaderReader = vendorWorkbookHeaderReader;
		this.manualLinksRepository = manualLinksRepository;
	}

	public async Task<MilwaukeeComparisonPreview> LoadPreviewAsync(CancellationToken cancellationToken = default)
	{
		var costsWorkbook = await vendorWorkbookHeaderReader.LoadLatestHeadersAsync("Milwaukee", "Costs", cancellationToken);
		var imapWorkbook = await vendorWorkbookHeaderReader.LoadLatestHeadersAsync("Milwaukee", "IMAP", cancellationToken);

		if (!costsWorkbook.FoundFile || !imapWorkbook.FoundFile)
		{
			return new MilwaukeeComparisonPreview(
				costsWorkbook.FileName,
				imapWorkbook.FileName,
				0,
				0,
				0,
				0,
				0,
				[],
				CombineErrors(costsWorkbook.ErrorMessage, imapWorkbook.ErrorMessage));
		}

		var costsSheet = LoadWorksheet(costsWorkbook.FilePath!);
		var imapSheet = LoadWorksheet(imapWorkbook.FilePath!);

		var costsItemNumber = ResolveRequiredColumn(costsSheet.Columns, ItemNumberAliases, "Milwaukee Costs", "Item No");
		if (costsItemNumber.ErrorMessage is not null)
		{
			return BuildErrorPreview(costsWorkbook, imapWorkbook, costsSheet, imapSheet, costsItemNumber.ErrorMessage);
		}

		var costsUpcCode = ResolveRequiredColumn(costsSheet.Columns, UpcCodeAliases, "Milwaukee Costs", "UPC Code");
		if (costsUpcCode.ErrorMessage is not null)
		{
			return BuildErrorPreview(costsWorkbook, imapWorkbook, costsSheet, imapSheet, costsUpcCode.ErrorMessage);
		}

		var imapItemNumber = ResolveRequiredColumn(imapSheet.Columns, ItemNumberAliases, "Milwaukee IMAP", "Item No");
		if (imapItemNumber.ErrorMessage is not null)
		{
			return BuildErrorPreview(costsWorkbook, imapWorkbook, costsSheet, imapSheet, imapItemNumber.ErrorMessage);
		}

		var imapUpcCode = ResolveRequiredColumn(imapSheet.Columns, UpcCodeAliases, "Milwaukee IMAP", "UPC Code");
		if (imapUpcCode.ErrorMessage is not null)
		{
			return BuildErrorPreview(costsWorkbook, imapWorkbook, costsSheet, imapSheet, imapUpcCode.ErrorMessage);
		}

		var costsDescription = ResolveOptionalColumn(costsSheet.Columns, DescriptionAliases);
		var imapMinimumAdvertisedPrice = ResolveOptionalColumn(imapSheet.Columns, MinimumAdvertisedPriceAliases);
		var costsItemNumberColumn = costsItemNumber.Column!;
		var costsUpcCodeColumn = costsUpcCode.Column!;
		var imapItemNumberColumn = imapItemNumber.Column!;
		var imapUpcCodeColumn = imapUpcCode.Column!;

		var imapRowsByItemNumber = BuildIndex(imapSheet.Rows, imapItemNumberColumn);
		var imapRowsByUpcCode = BuildIndex(imapSheet.Rows, imapUpcCodeColumn);
		var manualLinksDocument = await manualLinksRepository.LoadAsync(cancellationToken);

		var previewRows = new List<MilwaukeeComparisonPreviewRow>(costsSheet.Rows.Count);
		var matchedRowCount = 0;
		var unmatchedRowCount = 0;
		var ambiguousRowCount = 0;

		foreach (var costsRow in costsSheet.Rows)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var normalizedCostsItemNumber = NormalizeValue(costsRow.GetValue(costsItemNumber.Column!.Key));
			var normalizedCostsUpcCode = NormalizeValue(costsRow.GetValue(costsUpcCode.Column!.Key));
			var deterministicResolution = ResolveImapMatch(normalizedCostsItemNumber, normalizedCostsUpcCode, imapRowsByItemNumber, imapRowsByUpcCode);
			var resolution = deterministicResolution.MatchStatus == "Matched"
				? deterministicResolution
				: ResolveManualLinkMatch(
					manualLinksDocument.Links,
					normalizedCostsItemNumber,
					normalizedCostsUpcCode,
					imapRowsByItemNumber,
					imapRowsByUpcCode,
					deterministicResolution);

			switch (resolution.MatchStatus)
			{
				case "Matched":
					matchedRowCount++;
					break;
				case "Ambiguous":
					ambiguousRowCount++;
					break;
				default:
					unmatchedRowCount++;
					break;
			}

			previewRows.Add(new MilwaukeeComparisonPreviewRow(
				costsRow.RowNumber,
				NullIfEmpty(costsRow.GetValue(costsItemNumberColumn.Key)),
				NullIfEmpty(costsRow.GetValue(costsUpcCodeColumn.Key)),
				costsDescription.Column is null ? null : NullIfEmpty(costsRow.GetValue(costsDescription.Column.Key)),
				resolution.MatchStatus,
				resolution.MatchBasis,
				resolution.MatchedRow is null ? null : NullIfEmpty(resolution.MatchedRow.GetValue(imapItemNumberColumn.Key)),
				resolution.MatchedRow is null ? null : NullIfEmpty(resolution.MatchedRow.GetValue(imapUpcCodeColumn.Key)),
				resolution.MatchedRow is null || imapMinimumAdvertisedPrice.Column is null
					? null
					: NullIfEmpty(resolution.MatchedRow.GetValue(imapMinimumAdvertisedPrice.Column.Key)),
				resolution.Note));
		}

		return new MilwaukeeComparisonPreview(
			costsWorkbook.FileName,
			imapWorkbook.FileName,
			costsSheet.Rows.Count,
			imapSheet.Rows.Count,
			matchedRowCount,
			unmatchedRowCount,
			ambiguousRowCount,
			previewRows,
			null);
	}

	private static MilwaukeeComparisonPreview BuildErrorPreview(
		VendorWorkbookHeaders costsWorkbook,
		VendorWorkbookHeaders imapWorkbook,
		WorkbookDataSet costsSheet,
		WorkbookDataSet imapSheet,
		string errorMessage)
	{
		return new MilwaukeeComparisonPreview(
			costsWorkbook.FileName,
			imapWorkbook.FileName,
			costsSheet.Rows.Count,
			imapSheet.Rows.Count,
			0,
			0,
			0,
			[],
			errorMessage);
	}

	private static string? CombineErrors(params string?[] errors)
	{
		var messages = errors
			.Where(error => !string.IsNullOrWhiteSpace(error))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		return messages.Count == 0 ? null : string.Join(" ", messages);
	}

	private static WorkbookDataSet LoadWorksheet(string filePath)
	{
		using var workbook = new XLWorkbook(filePath);
		var worksheet = workbook.Worksheets.FirstOrDefault();
		var firstUsedRow = worksheet?.FirstRowUsed();

		if (worksheet is null || firstUsedRow is null)
		{
			return new WorkbookDataSet([], []);
		}

		var headerRowNumber = firstUsedRow.RowNumber();
		var lastColumnNumber = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
		var columns = new List<WorkbookColumn>();
		var duplicateHeaderCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		for (var columnNumber = 1; columnNumber <= lastColumnNumber; columnNumber++)
		{
			var sourceHeader = worksheet.Cell(headerRowNumber, columnNumber).GetString().Trim();
			if (string.IsNullOrWhiteSpace(sourceHeader))
			{
				continue;
			}

			duplicateHeaderCounts.TryGetValue(sourceHeader, out var duplicateCount);
			duplicateHeaderCounts[sourceHeader] = duplicateCount + 1;
			var key = duplicateCount == 0 ? sourceHeader : $"{sourceHeader} ({duplicateCount + 1})";

			columns.Add(new WorkbookColumn(columnNumber, key, sourceHeader, NormalizeValue(sourceHeader)));
		}

		var lastRowNumber = worksheet.LastRowUsed()?.RowNumber() ?? headerRowNumber;
		var rows = new List<WorkbookDataRow>();

		for (var rowNumber = headerRowNumber + 1; rowNumber <= lastRowNumber; rowNumber++)
		{
			var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var hasAnyValue = false;

			foreach (var column in columns)
			{
				var value = worksheet.Cell(rowNumber, column.ColumnNumber).GetString().Trim();
				if (!string.IsNullOrWhiteSpace(value))
				{
					hasAnyValue = true;
				}

				values[column.Key] = value;
			}

			if (hasAnyValue)
			{
				rows.Add(new WorkbookDataRow(rowNumber, values));
			}
		}

		return new WorkbookDataSet(columns, rows);
	}

	private static ColumnResolution ResolveRequiredColumn(
		IReadOnlyList<WorkbookColumn> columns,
		HashSet<string> aliases,
		string workbookName,
		string fieldName)
	{
		var matches = columns
			.Where(column => aliases.Contains(column.NormalizedHeader))
			.ToList();

		if (matches.Count == 1)
		{
			return new ColumnResolution(matches[0], null);
		}

		if (matches.Count == 0)
		{
			return new ColumnResolution(
				null,
				$"Could not resolve the {fieldName} column in {workbookName}. Expected one of: {string.Join(", ", aliases.OrderBy(alias => alias, StringComparer.OrdinalIgnoreCase))}.");
		}

		return new ColumnResolution(
			null,
			$"Could not resolve the {fieldName} column in {workbookName} because multiple headers match: {string.Join(", ", matches.Select(match => match.SourceHeader))}.");
	}

	private static ColumnResolution ResolveOptionalColumn(IReadOnlyList<WorkbookColumn> columns, HashSet<string> aliases)
	{
		var matches = columns
			.Where(column => aliases.Contains(column.NormalizedHeader))
			.ToList();

		return matches.Count == 1
			? new ColumnResolution(matches[0], null)
			: new ColumnResolution(null, null);
	}

	private static Dictionary<string, List<WorkbookDataRow>> BuildIndex(
		IReadOnlyList<WorkbookDataRow> rows,
		WorkbookColumn column)
	{
		var index = new Dictionary<string, List<WorkbookDataRow>>(StringComparer.OrdinalIgnoreCase);

		foreach (var row in rows)
		{
			var normalizedValue = NormalizeValue(row.GetValue(column.Key));
			if (string.IsNullOrWhiteSpace(normalizedValue))
			{
				continue;
			}

			if (!index.TryGetValue(normalizedValue, out var bucket))
			{
				bucket = [];
				index[normalizedValue] = bucket;
			}

			bucket.Add(row);
		}

		return index;
	}

	private static MatchResolution ResolveImapMatch(
		string normalizedCostsItemNumber,
		string normalizedCostsUpcCode,
		IReadOnlyDictionary<string, List<WorkbookDataRow>> imapRowsByItemNumber,
		IReadOnlyDictionary<string, List<WorkbookDataRow>> imapRowsByUpcCode)
	{
		var itemMatches = GetMatches(imapRowsByItemNumber, normalizedCostsItemNumber);
		var upcMatches = GetMatches(imapRowsByUpcCode, normalizedCostsUpcCode);

		if (itemMatches.Count == 0 && upcMatches.Count == 0)
		{
			return string.IsNullOrWhiteSpace(normalizedCostsItemNumber) && string.IsNullOrWhiteSpace(normalizedCostsUpcCode)
				? new MatchResolution(null, "Unmatched", "-", "Costs row has no Item No or UPC Code value.")
				: new MatchResolution(null, "Unmatched", "-", "No deterministic Item No or UPC Code match was found in Milwaukee IMAP.");
		}

		var sharedMatches = itemMatches.Count > 0 && upcMatches.Count > 0
			? itemMatches.IntersectBy(upcMatches.Select(match => match.RowNumber), match => match.RowNumber).ToList()
			: [];

		if (sharedMatches.Count == 1)
		{
			return new MatchResolution(sharedMatches[0], "Matched", "Item No + UPC Code", "Item No and UPC Code resolve to the same Milwaukee IMAP row.");
		}

		if (itemMatches.Count == 1 && upcMatches.Count == 0)
		{
			return new MatchResolution(itemMatches[0], "Matched", "Item No", "Matched Milwaukee IMAP by normalized Item No.");
		}

		if (itemMatches.Count == 0 && upcMatches.Count == 1)
		{
			return new MatchResolution(upcMatches[0], "Matched", "UPC Code", "Matched Milwaukee IMAP by normalized UPC Code.");
		}

		if (itemMatches.Count == 1 && upcMatches.Count == 1)
		{
			return new MatchResolution(null, "Ambiguous", "Conflict", "Item No and UPC Code point to different Milwaukee IMAP rows.");
		}

		if (sharedMatches.Count > 1)
		{
			return new MatchResolution(null, "Ambiguous", "Conflict", "Item No and UPC Code still resolve to multiple shared Milwaukee IMAP rows.");
		}

		if (itemMatches.Count > 1 && upcMatches.Count == 0)
		{
			return new MatchResolution(null, "Ambiguous", "Item No", $"Normalized Item No matches {itemMatches.Count} Milwaukee IMAP rows.");
		}

		if (upcMatches.Count > 1 && itemMatches.Count == 0)
		{
			return new MatchResolution(null, "Ambiguous", "UPC Code", $"Normalized UPC Code matches {upcMatches.Count} Milwaukee IMAP rows.");
		}

		return new MatchResolution(null, "Ambiguous", "Conflict", "Item No and UPC Code do not collapse to one deterministic Milwaukee IMAP row.");
	}

	private static MatchResolution ResolveManualLinkMatch(
		IReadOnlyList<ManualLinkRecord> manualLinks,
		string normalizedCostsItemNumber,
		string normalizedCostsUpcCode,
		IReadOnlyDictionary<string, List<WorkbookDataRow>> imapRowsByItemNumber,
		IReadOnlyDictionary<string, List<WorkbookDataRow>> imapRowsByUpcCode,
		MatchResolution fallbackResolution)
	{
		var applicableLinks = manualLinks
			.Where(link => IsApplicableManualLink(link, normalizedCostsItemNumber, normalizedCostsUpcCode))
			.ToList();

		if (applicableLinks.Count == 0)
		{
			return fallbackResolution;
		}

		if (applicableLinks.Count > 1)
		{
			return new MatchResolution(
				null,
				"Ambiguous",
				"Manual Link Conflict",
				$"Multiple active Milwaukee manual links match this Costs row: {string.Join(", ", applicableLinks.Select(link => link.Id))}.");
		}

		var manualLink = applicableLinks[0];
		var manualResolution = ResolveImapMatch(
			NormalizeValue(manualLink.TargetItemNumber),
			NormalizeValue(manualLink.TargetUpc),
			imapRowsByItemNumber,
			imapRowsByUpcCode);

		return manualResolution.MatchStatus == "Matched"
			? new MatchResolution(
				manualResolution.MatchedRow,
				"Matched",
				$"Manual Link ({manualResolution.MatchBasis})",
				$"Applied active Milwaukee manual link {manualLink.Id} after deterministic matching remained {fallbackResolution.MatchStatus.ToLowerInvariant()}.")
			: fallbackResolution;
	}

	private static bool IsApplicableManualLink(
		ManualLinkRecord link,
		string normalizedCostsItemNumber,
		string normalizedCostsUpcCode)
	{
		if (!string.Equals(link.Status, "active", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (!string.Equals(link.SourceVendor, "Milwaukee", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (!string.Equals(link.TargetDataset, "IMAP", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		var normalizedSourceItemNumber = NormalizeValue(link.SourceItemNumber);
		var normalizedSourceUpc = NormalizeValue(link.SourceUpc);
		if (string.IsNullOrWhiteSpace(normalizedSourceItemNumber) && string.IsNullOrWhiteSpace(normalizedSourceUpc))
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(NormalizeValue(link.TargetItemNumber)) && string.IsNullOrWhiteSpace(NormalizeValue(link.TargetUpc)))
		{
			return false;
		}

		return (string.IsNullOrWhiteSpace(normalizedSourceItemNumber) || string.Equals(normalizedSourceItemNumber, normalizedCostsItemNumber, StringComparison.OrdinalIgnoreCase))
			&& (string.IsNullOrWhiteSpace(normalizedSourceUpc) || string.Equals(normalizedSourceUpc, normalizedCostsUpcCode, StringComparison.OrdinalIgnoreCase));
	}

	private static IReadOnlyList<WorkbookDataRow> GetMatches(
		IReadOnlyDictionary<string, List<WorkbookDataRow>> index,
		string normalizedValue)
	{
		return string.IsNullOrWhiteSpace(normalizedValue) || !index.TryGetValue(normalizedValue, out var matches)
			? []
			: matches;
	}

	private static string NormalizeValue(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}

		var characters = value
			.Where(char.IsLetterOrDigit)
			.Select(char.ToUpperInvariant)
			.ToArray();

		return new string(characters);
	}

	private static string? NullIfEmpty(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value;
	}

	private sealed record WorkbookDataSet(IReadOnlyList<WorkbookColumn> Columns, IReadOnlyList<WorkbookDataRow> Rows);

	private sealed record WorkbookColumn(int ColumnNumber, string Key, string SourceHeader, string NormalizedHeader);

	private sealed record WorkbookDataRow(int RowNumber, IReadOnlyDictionary<string, string> Values)
	{
		public string GetValue(string key)
		{
			return Values.TryGetValue(key, out var value) ? value : string.Empty;
		}
	}

	private sealed record ColumnResolution(WorkbookColumn? Column, string? ErrorMessage);

	private sealed record MatchResolution(WorkbookDataRow? MatchedRow, string MatchStatus, string MatchBasis, string Note);
}