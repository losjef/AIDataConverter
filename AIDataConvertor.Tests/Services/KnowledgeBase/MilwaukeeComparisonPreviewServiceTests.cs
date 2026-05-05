using ClosedXML.Excel;
using AIDataConvertor.Models.KnowledgeBase;
using AIDataConvertor.Services.KnowledgeBase;
using Xunit;

namespace AIDataConvertor.Tests.Services.KnowledgeBase;

public sealed class MilwaukeeComparisonPreviewServiceTests : IDisposable
{
	private readonly string workingDirectory;

	public MilwaukeeComparisonPreviewServiceTests()
	{
		workingDirectory = Path.Combine(Path.GetTempPath(), "AIDataConvertorTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(workingDirectory);
	}

	[Fact]
	public async Task LoadPreviewAsync_PrefersDeterministicMatchOverManualLinkFallback()
	{
		var costsPath = CreateWorkbook(
			"milwaukee-costs.xlsx",
			["Item No", "UPC Code", "Description"],
			[["A-100", "111", "Deterministic row"]]);
		var imapPath = CreateWorkbook(
			"milwaukee-imap.xlsx",
			["Item No", "UPC Code", "MAP"],
			[
				["A100", "111", "10.00"],
				["MANUALTARGET", "999", "20.00"]
			]);

		var service = CreateService(
			costsPath,
			imapPath,
			new ManualLinksDocument
			{
				Links =
				[
					new ManualLinkRecord
					{
						Id = "ml-1",
						Status = "active",
						SourceVendor = "Milwaukee",
						SourceItemNumber = "A-100",
						TargetDataset = "IMAP",
						TargetItemNumber = "MANUALTARGET",
						TargetUpc = "999"
					}
				]
			});

		var preview = await service.LoadPreviewAsync();

		var row = Assert.Single(preview.Rows);
		Assert.Equal("Matched", row.MatchStatus);
		Assert.Equal("Item No + UPC Code", row.MatchBasis);
		Assert.Equal("A100", row.ImapItemNumber);
		Assert.DoesNotContain("manual link", row.Note ?? string.Empty, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task LoadPreviewAsync_UsesManualLinkFallbackWhenDeterministicMatchIsMissing()
	{
		var costsPath = CreateWorkbook(
			"milwaukee-costs.xlsx",
			["Item No", "UPC Code", "Description"],
			[["A-100", "111", "Manual link row"]]);
		var imapPath = CreateWorkbook(
			"milwaukee-imap.xlsx",
			["Item No", "UPC Code", "MAP"],
			[["MANUALTARGET", "999", "20.00"]]);

		var service = CreateService(
			costsPath,
			imapPath,
			new ManualLinksDocument
			{
				Links =
				[
					new ManualLinkRecord
					{
						Id = "ml-2",
						Status = "active",
						SourceVendor = "Milwaukee",
						SourceItemNumber = "A-100",
						SourceUpc = "111",
						TargetDataset = "IMAP",
						TargetItemNumber = "MANUALTARGET",
						TargetUpc = "999"
					}
				]
			});

		var preview = await service.LoadPreviewAsync();

		var row = Assert.Single(preview.Rows);
		Assert.Equal("Matched", row.MatchStatus);
		Assert.Equal("Manual Link (Item No + UPC Code)", row.MatchBasis);
		Assert.Equal("MANUALTARGET", row.ImapItemNumber);
		Assert.Contains("ml-2", row.Note ?? string.Empty, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task LoadPreviewAsync_ReturnsAmbiguousWhenMultipleManualLinksApply()
	{
		var costsPath = CreateWorkbook(
			"milwaukee-costs.xlsx",
			["Item No", "UPC Code", "Description"],
			[["A-100", "111", "Conflict row"]]);
		var imapPath = CreateWorkbook(
			"milwaukee-imap.xlsx",
			["Item No", "UPC Code", "MAP"],
			[
				["MANUALTARGET1", "999", "20.00"],
				["MANUALTARGET2", "888", "30.00"]
			]);

		var service = CreateService(
			costsPath,
			imapPath,
			new ManualLinksDocument
			{
				Links =
				[
					new ManualLinkRecord
					{
						Id = "ml-3",
						Status = "active",
						SourceVendor = "Milwaukee",
						SourceItemNumber = "A-100",
						TargetDataset = "IMAP",
						TargetItemNumber = "MANUALTARGET1",
						TargetUpc = "999"
					},
					new ManualLinkRecord
					{
						Id = "ml-4",
						Status = "active",
						SourceVendor = "Milwaukee",
						SourceItemNumber = "A-100",
						TargetDataset = "IMAP",
						TargetItemNumber = "MANUALTARGET2",
						TargetUpc = "888"
					}
				]
			});

		var preview = await service.LoadPreviewAsync();

		var row = Assert.Single(preview.Rows);
		Assert.Equal("Ambiguous", row.MatchStatus);
		Assert.Equal("Manual Link Conflict", row.MatchBasis);
		Assert.Contains("ml-3", row.Note ?? string.Empty, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("ml-4", row.Note ?? string.Empty, StringComparison.OrdinalIgnoreCase);
	}

	public void Dispose()
	{
		if (Directory.Exists(workingDirectory))
		{
			Directory.Delete(workingDirectory, recursive: true);
		}
	}

	private MilwaukeeComparisonPreviewService CreateService(
		string costsPath,
		string imapPath,
		ManualLinksDocument manualLinksDocument)
	{
		return new MilwaukeeComparisonPreviewService(
			new StubVendorWorkbookHeaderReader(costsPath, imapPath),
			new StubManualLinksRepository(manualLinksDocument));
	}

	private string CreateWorkbook(string fileName, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
	{
		var filePath = Path.Combine(workingDirectory, fileName);

		using var workbook = new XLWorkbook();
		var worksheet = workbook.AddWorksheet("Sheet1");

		for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
		{
			worksheet.Cell(1, columnIndex + 1).Value = headers[columnIndex];
		}

		for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
		{
			for (var columnIndex = 0; columnIndex < rows[rowIndex].Count; columnIndex++)
			{
				worksheet.Cell(rowIndex + 2, columnIndex + 1).Value = rows[rowIndex][columnIndex];
			}
		}

		workbook.SaveAs(filePath);
		return filePath;
	}

	private sealed class StubVendorWorkbookHeaderReader : IVendorWorkbookHeaderReader
	{
		private readonly string costsPath;
		private readonly string imapPath;

		public StubVendorWorkbookHeaderReader(string costsPath, string imapPath)
		{
			this.costsPath = costsPath;
			this.imapPath = imapPath;
		}

		public Task<VendorWorkbookHeaders> LoadLatestHeadersAsync(string vendorName, string feedName, CancellationToken cancellationToken = default)
		{
			var filePath = string.Equals(feedName, "Costs", StringComparison.OrdinalIgnoreCase) ? costsPath : imapPath;
			return Task.FromResult(new VendorWorkbookHeaders(vendorName, feedName, filePath, Path.GetFileName(filePath), [], null));
		}
	}

	private sealed class StubManualLinksRepository : IManualLinksRepository
	{
		private readonly ManualLinksDocument document;

		public StubManualLinksRepository(ManualLinksDocument document)
		{
			this.document = document;
		}

		public Task<ManualLinksDocument> LoadAsync(CancellationToken cancellationToken = default)
		{
			return Task.FromResult(document);
		}

		public Task SaveAsync(ManualLinksDocument document, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException();
		}

		public string GetWorkingFilePath()
		{
			return string.Empty;
		}
	}
}