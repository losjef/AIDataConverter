using System.Text.Json;
using AIDataConvertor.Models.KnowledgeBase;

namespace AIDataConvertor.Services.KnowledgeBase;

public sealed class ManualLinksRepository : IManualLinksRepository
{
	private const string PackagedAssetName = "manual_links.json";
	private static readonly string[] PackagedAssetCandidates =
	[
		PackagedAssetName,
		$"Resources/Raw/{PackagedAssetName}",
		$"Resources\\Raw\\{PackagedAssetName}"
	];
	private static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		WriteIndented = true
	};

	private readonly SemaphoreSlim gate = new(1, 1);

	public async Task<ManualLinksDocument> LoadAsync(CancellationToken cancellationToken = default)
	{
		await gate.WaitAsync(cancellationToken);
		try
		{
			await EnsureSeededAsync(cancellationToken);

			await using var stream = File.OpenRead(GetWorkingFilePath());
			var document = await JsonSerializer.DeserializeAsync<ManualLinksDocument>(stream, SerializerOptions, cancellationToken);
			return document ?? new ManualLinksDocument();
		}
		finally
		{
			gate.Release();
		}
	}

	public async Task SaveAsync(ManualLinksDocument document, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(document);

		await gate.WaitAsync(cancellationToken);
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(GetWorkingFilePath())!);
			await using var stream = File.Create(GetWorkingFilePath());
			await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken);
		}
		finally
		{
			gate.Release();
		}
	}

	public string GetWorkingFilePath()
	{
		return Path.Combine(FileSystem.Current.AppDataDirectory, "KnowledgeBase", PackagedAssetName);
	}

	private async Task EnsureSeededAsync(CancellationToken cancellationToken)
	{
		if (File.Exists(GetWorkingFilePath()))
		{
			return;
		}

		Directory.CreateDirectory(Path.GetDirectoryName(GetWorkingFilePath())!);

		await using var packagedStream = await OpenPackagedAssetAsync(cancellationToken);
		await using var outputStream = File.Create(GetWorkingFilePath());
		await packagedStream.CopyToAsync(outputStream, cancellationToken);
	}

	private static async Task<Stream> OpenPackagedAssetAsync(CancellationToken cancellationToken)
	{
		foreach (var candidate in PackagedAssetCandidates)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				return await FileSystem.Current.OpenAppPackageFileAsync(candidate);
			}
			catch (FileNotFoundException)
			{
			}
		}

		throw new FileNotFoundException(
			$"Unable to locate the packaged manual links asset. Tried: {string.Join(", ", PackagedAssetCandidates)}.");
	}
}