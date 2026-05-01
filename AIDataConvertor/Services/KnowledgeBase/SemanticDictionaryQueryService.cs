using AIDataConvertor.Models.KnowledgeBase;

namespace AIDataConvertor.Services.KnowledgeBase;

public sealed class SemanticDictionaryQueryService : ISemanticDictionaryQueryService
{
	private static readonly HashSet<string> ActiveStatuses = new(StringComparer.OrdinalIgnoreCase)
	{
		"active"
	};

	private readonly ISemanticDictionaryRepository repository;

	public SemanticDictionaryQueryService(ISemanticDictionaryRepository repository)
	{
		this.repository = repository;
	}

	public async Task<IReadOnlyList<HeaderMatchCandidate>> FindHeaderMatchesAsync(
		string sourceHeader,
		string? vendorName = null,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(sourceHeader))
		{
			return [];
		}

		var document = await repository.LoadAsync(cancellationToken);
		var normalizedHeader = NormalizeHeader(sourceHeader);
		var results = new List<HeaderMatchCandidate>();

		if (!string.IsNullOrWhiteSpace(vendorName) && document.Vendors.TryGetValue(vendorName, out var vendor))
		{
			results.AddRange(GetVendorRuleMatches(sourceHeader, normalizedHeader, vendorName, vendor));
		}

		results.AddRange(GetSharedConceptMatches(document, sourceHeader, normalizedHeader));

		if (document.CrossVendorSeedBehavior.AllowSeedReuseFromExistingVendors)
		{
			results.AddRange(GetCrossVendorRuleMatches(document, sourceHeader, normalizedHeader, vendorName));
		}

		return results
			.DistinctBy(candidate => $"{candidate.TargetField}|{candidate.MatchSource}|{candidate.SourceVendor}|{candidate.SourceRuleSetId}")
			.OrderByDescending(candidate => candidate.Confidence)
			.ThenBy(candidate => candidate.MatchSource, StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	private static IEnumerable<HeaderMatchCandidate> GetVendorRuleMatches(
		string sourceHeader,
		string normalizedHeader,
		string vendorName,
		VendorDefinition vendor)
	{
		if (!IsActive(vendor.Status))
		{
			yield break;
		}

		foreach (var ruleSet in vendor.RuleSets.Where(ruleSet => IsActive(ruleSet.Status)))
		{
			foreach (var rule in ruleSet.MappingRules.Where(rule => IsActive(rule.Status)))
			{
				if (NormalizeHeader(rule.SourceHeader) != normalizedHeader)
				{
					continue;
				}

				yield return new HeaderMatchCandidate(
					sourceHeader,
					rule.TargetField,
					"VendorRule",
					vendorName,
					ruleSet.Id,
					rule.Confidence,
					rule.Status);
			}
		}
	}

	private static IEnumerable<HeaderMatchCandidate> GetSharedConceptMatches(
		SemanticDictionaryDocument document,
		string sourceHeader,
		string normalizedHeader)
	{
		foreach (var concept in document.SharedConcepts)
		{
			foreach (var seedHeader in concept.Value.SeedHeaders.Where(rule => IsActive(rule.Status)))
			{
				if (NormalizeHeader(seedHeader.SourceHeader) != normalizedHeader)
				{
					continue;
				}

				yield return new HeaderMatchCandidate(
					sourceHeader,
					concept.Key,
					"SharedConcept",
					null,
					null,
					seedHeader.Confidence,
					seedHeader.Status);
			}
		}
	}

	private static IEnumerable<HeaderMatchCandidate> GetCrossVendorRuleMatches(
		SemanticDictionaryDocument document,
		string sourceHeader,
		string normalizedHeader,
		string? currentVendor)
	{
		foreach (var vendor in document.Vendors)
		{
			if (!string.IsNullOrWhiteSpace(currentVendor) && string.Equals(vendor.Key, currentVendor, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			foreach (var candidate in GetVendorRuleMatches(sourceHeader, normalizedHeader, vendor.Key, vendor.Value))
			{
				yield return candidate with { MatchSource = "CrossVendorSeed" };
			}
		}
	}

	private static bool IsActive(string? status)
	{
		return !string.IsNullOrWhiteSpace(status) && ActiveStatuses.Contains(status);
	}

	private static string NormalizeHeader(string header)
	{
		var trimmed = header.Trim();
		var characters = trimmed
			.Where(character => char.IsLetterOrDigit(character))
			.Select(char.ToUpperInvariant)
			.ToArray();

		return new string(characters);
	}
}