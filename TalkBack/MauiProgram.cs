using Plugin.Maui.Audio;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using TalkBack.Services;
using TalkBack.ViewModels;
using TalkBack.Views;

namespace TalkBack;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureSyncfusionToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton(AudioManager.Current);

		builder.Services.AddSingleton<AudioService>();



		// add services 
		builder.Services.AddSingleton(sp =>
	   {
		   // Copy model from RAW to app data dir
		   var modelPath = Path.Combine(FileSystem.AppDataDirectory, "ggml-tiny.bin");

		   if (!File.Exists(modelPath))
		   {
			   using var modelStream = FileSystem.OpenAppPackageFileAsync("ggml-tiny.bin").Result;
			   using var outStream = File.OpenWrite(modelPath);
			   modelStream.CopyTo(outStream);
		   }

		   return new WhisperService(modelPath);
	   });


		builder.Services.AddSingleton<MainPageViewModel>();
		builder.Services.AddSingleton<MainPage>();

		return builder.Build();
	}
}
