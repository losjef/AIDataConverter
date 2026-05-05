using Microsoft.Extensions.Logging;
using AIDataConvertor.Services.KnowledgeBase;

namespace AIDataConvertor;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<ISemanticDictionaryRepository, SemanticDictionaryRepository>();
		builder.Services.AddSingleton<IPropelloTemplateMemoryRepository, PropelloTemplateMemoryRepository>();
		builder.Services.AddSingleton<IBaselineSchemaRulesRepository, BaselineSchemaRulesRepository>();
		builder.Services.AddSingleton<IManualLinksRepository, ManualLinksRepository>();
		builder.Services.AddSingleton<ISemanticDictionaryQueryService, SemanticDictionaryQueryService>();
		builder.Services.AddSingleton<IBaselineSchemaAnalyzer, BaselineSchemaAnalyzer>();
		builder.Services.AddSingleton<ISuperiorHardwareBaselineHeaderReader, SuperiorHardwareBaselineHeaderReader>();
		builder.Services.AddSingleton<IPropelloTemplateCsvHeaderReader, PropelloTemplateCsvHeaderReader>();
		builder.Services.AddSingleton<IVendorWorkbookHeaderReader, VendorWorkbookHeaderReader>();
		builder.Services.AddSingleton<IMilwaukeeComparisonPreviewService, MilwaukeeComparisonPreviewService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
