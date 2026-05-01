using System.Globalization;
using System.Text.RegularExpressions;

namespace AIDataConvertor.Services.KnowledgeBase;

public sealed class BaselineSchemaAnalyzer : IBaselineSchemaAnalyzer
{
	private static readonly string[] SupportedPeriodFormats =
	[
		"MMM-yy",
		"MMM yy",
		"MMM-yyyy",
		"MMM yyyy",
		"MMMM-yy",
		"MMMM yyyy",
		"yyyy-MM",
		"MM-yyyy",
		"M-yyyy",
		"MM/yy",
		"M/yy",
		"MM/yyyy",
		"M/yyyy"
	];

	private static readonly Regex QuarterPattern = new(
		@"^Q[1-4][\s\-_/]?(\d{2}|\d{4})$",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private readonly ISemanticDictionaryQueryService semanticDictionaryQueryService;

	public BaselineSchemaAnalyzer(ISemanticDictionaryQueryService semanticDictionaryQueryService)
	{
		this.semanticDictionaryQueryService = semanticDictionaryQueryService;
	}

	public async Task<BaselineReadinessReport> AnalyzeAsync(
		IReadOnlyList<string> sourceHeaders,
		BaselineWorkflowProfile workflowProfile,
		CancellationToken cancellationToken = default)
	{
		var analyses = new List<BaselineColumnAnalysis>();
		var resolvedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var sourceHeader in sourceHeaders)
		{
			if (TryGetDynamicColumnGroup(sourceHeader, out var dynamicColumnGroup))
			{
				analyses.Add(new BaselineColumnAnalysis(
					sourceHeader,
					"DynamicTimeSeries",
					dynamicColumnGroup,
					false,
					false,
					true,
					null,
					"Detected as a rolling period or sales-history column that may appear, change, or disappear over time.",
					[]));
				continue;
			}

			var candidates = await semanticDictionaryQueryService.FindHeaderMatchesAsync(sourceHeader, cancellationToken: cancellationToken);
			var bestCandidate = candidates.FirstOrDefault();

			if (bestCandidate is not null)
			{
				resolvedFields.Add(bestCandidate.TargetField);
				analyses.Add(new BaselineColumnAnalysis(
					sourceHeader,
					"Resolved",
					null,
					workflowProfile.RequiredFields.Contains(bestCandidate.TargetField, StringComparer.OrdinalIgnoreCase),
					true,
					false,
					bestCandidate.TargetField,
					$"Resolved locally via {bestCandidate.MatchSource}.",
					candidates));
				continue;
			}

			analyses.Add(new BaselineColumnAnalysis(
				sourceHeader,
				"Unresolved",
				null,
				false,
				false,
				false,
				null,
				"No local semantic-memory match found for this baseline column.",
				[]));
		}

		var missingRequiredFields = workflowProfile.RequiredFields
			.Where(requiredField => !resolvedFields.Contains(requiredField))
			.OrderBy(requiredField => requiredField, StringComparer.OrdinalIgnoreCase)
			.ToList();

		var dynamicColumnCount = analyses.Count(column => column.IsDynamicTimeSeries);

		return new BaselineReadinessReport(
			workflowProfile.Name,
			workflowProfile.RequiredFields,
			analyses,
			missingRequiredFields,
			missingRequiredFields.Count == 0,
			dynamicColumnCount);
	}

	private static bool TryGetDynamicColumnGroup(string sourceHeader, out string dynamicColumnGroup)
	{
		var normalized = sourceHeader.Trim();
		if (string.IsNullOrWhiteSpace(normalized))
		{
			dynamicColumnGroup = string.Empty;
			return false;
		}

		if (DateTime.TryParseExact(
			normalized,
			SupportedPeriodFormats,
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out _))
		{
			dynamicColumnGroup = "RollingPeriod";
			return true;
		}

		if (QuarterPattern.IsMatch(normalized))
		{
			dynamicColumnGroup = "QuarterPeriod";
			return true;
		}

		dynamicColumnGroup = string.Empty;
		return false;
	}

	public static bool LooksLikeDynamicTimeSeriesHeader(string sourceHeader)
	{
		return TryGetDynamicColumnGroup(sourceHeader, out _);
		}
}