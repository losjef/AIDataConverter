using System.Globalization;
using System.Text.RegularExpressions;
using AIDataConvertor.Models.KnowledgeBase;

namespace AIDataConvertor.Services.KnowledgeBase;

public sealed class BaselineSchemaAnalyzer : IBaselineSchemaAnalyzer
{
	private readonly ISemanticDictionaryQueryService semanticDictionaryQueryService;
	private readonly IBaselineSchemaRulesRepository baselineSchemaRulesRepository;

	public BaselineSchemaAnalyzer(
		ISemanticDictionaryQueryService semanticDictionaryQueryService,
		IBaselineSchemaRulesRepository baselineSchemaRulesRepository)
	{
		this.semanticDictionaryQueryService = semanticDictionaryQueryService;
		this.baselineSchemaRulesRepository = baselineSchemaRulesRepository;
	}

	public async Task<BaselineReadinessReport> AnalyzeAsync(
		IReadOnlyList<string> sourceHeaders,
		BaselineWorkflowProfile workflowProfile,
		CancellationToken cancellationToken = default)
	{
		var rulesDocument = await baselineSchemaRulesRepository.LoadAsync(cancellationToken);
		var workflowDefinition = GetWorkflowDefinition(rulesDocument, workflowProfile);
		var requiredFields = workflowDefinition.RequiredFields;
		var analyses = new List<BaselineColumnAnalysis>();
		var resolvedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var quarterPattern = new Regex(
			rulesDocument.DynamicTimeSeries.QuarterPattern,
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		foreach (var sourceHeader in sourceHeaders)
		{
			if (TryGetDynamicColumnGroup(sourceHeader, rulesDocument.DynamicTimeSeries, quarterPattern, out var dynamicColumnGroup))
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
					requiredFields.Contains(bestCandidate.TargetField, StringComparer.OrdinalIgnoreCase),
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

		var missingRequiredFields = requiredFields
			.Where(requiredField => !resolvedFields.Contains(requiredField))
			.OrderBy(requiredField => requiredField, StringComparer.OrdinalIgnoreCase)
			.ToList();

		var dynamicColumnCount = analyses.Count(column => column.IsDynamicTimeSeries);

		return new BaselineReadinessReport(
			workflowProfile.Name,
			requiredFields,
			analyses,
			missingRequiredFields,
			missingRequiredFields.Count == 0,
			dynamicColumnCount);
	}

	private static BaselineWorkflowRuleDefinition GetWorkflowDefinition(
		BaselineSchemaRulesDocument rulesDocument,
		BaselineWorkflowProfile workflowProfile)
	{
		if (rulesDocument.WorkflowProfiles.TryGetValue(workflowProfile.Name, out var workflowDefinition))
		{
			return workflowDefinition;
		}

		throw new InvalidOperationException(
			$"No baseline workflow rule definition was found for '{workflowProfile.Name}'.");
	}

	private static bool TryGetDynamicColumnGroup(
		string sourceHeader,
		DynamicTimeSeriesRules dynamicTimeSeriesRules,
		Regex quarterPattern,
		out string dynamicColumnGroup)
	{
		var normalized = sourceHeader.Trim();
		if (string.IsNullOrWhiteSpace(normalized))
		{
			dynamicColumnGroup = string.Empty;
			return false;
		}

		if (DateTime.TryParseExact(
			normalized,
			dynamicTimeSeriesRules.SupportedPeriodFormats.ToArray(),
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out _))
		{
			dynamicColumnGroup = "RollingPeriod";
			return true;
		}

		if (quarterPattern.IsMatch(normalized))
		{
			dynamicColumnGroup = "QuarterPeriod";
			return true;
		}

		dynamicColumnGroup = string.Empty;
		return false;
	}
}