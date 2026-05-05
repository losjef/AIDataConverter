using AIDataConvertor.Services.KnowledgeBase;
using Xunit;

namespace AIDataConvertor.Tests.Services.KnowledgeBase;

public sealed class PropelloTemplateCsvHeaderReaderTests
{
	[Fact]
	public async Task LoadHeadersAsync_ProductImport_ReturnsExactOrderedHeaders()
	{
		var reader = new PropelloTemplateCsvHeaderReader();

		var result = await reader.LoadHeadersAsync("ProductImport");

		Assert.True(result.FoundFile);
		Assert.True(result.HasHeaders);
		Assert.Null(result.ErrorMessage);
		Assert.Equal(95, result.Headers.Count);
		Assert.Equal("Store Number", result.Headers[0]);
		Assert.Equal("Retail Price Control", result.Headers[^1]);
	}
}