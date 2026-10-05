// gabriel geremias vieira
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using AcademiaDoZe.Presentation.AppMaui.Configuration;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Aplica a cultura salva nas preferências antes de criar a aplicação e os controles.
        // Sem preferência salva, usa a do sistema operacional e cai para pt-BR.
        LocalizationManager.Instance.SetCulture(Preferences.Get("Cultura", LocalizationManager.ObterCulturaPadrao()));

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configuração de acesso a dados consumida pelas fábricas de repositório.
        // É a mesma instância durante toda a execução: a tela de Configurações altera
        // as propriedades dela e os repositórios criados a seguir já usam os novos valores.
        builder.Services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = AppSettings.ConnectionString,
            DatabaseType = AppSettings.BancoSelecionado.ToInfrastructure()
        });

        // Serviços da camada de aplicação e fábricas de repositório
        builder.Services.AddApplicationServices();

        // Gerenciador de idioma disponível para injeção
        builder.Services.AddSingleton(LocalizationManager.Instance);

#if WINDOWS
        // Garante que o seletor de data do WinUI use a cultura atual nos nomes de dias e meses
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("CulturaLanguage", (handler, view) =>
        {
            handler.PlatformView.Language = System.Globalization.CultureInfo.CurrentCulture.Name;
        });
#endif

        RegistrarViewsEViewModels(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegistrarViewsEViewModels(IServiceCollection services)
    {
        services.AddSingleton<AppShell>();

        services.AddTransient<DashboardPage>();
        services.AddTransient<DashboardViewModel>();

        services.AddTransient<LogradouroListPage>();
        services.AddTransient<LogradouroListViewModel>();
        services.AddTransient<LogradouroPage>();
        services.AddTransient<LogradouroViewModel>();

        services.AddTransient<AlunoListPage>();
        services.AddTransient<AlunoListViewModel>();
        services.AddTransient<AlunoPage>();
        services.AddTransient<AlunoViewModel>();

        services.AddTransient<ColaboradorListPage>();
        services.AddTransient<ColaboradorListViewModel>();
        services.AddTransient<ColaboradorPage>();
        services.AddTransient<ColaboradorViewModel>();

        services.AddTransient<MatriculaListPage>();
        services.AddTransient<MatriculaListViewModel>();
        services.AddTransient<MatriculaPage>();
        services.AddTransient<MatriculaViewModel>();

        services.AddTransient<AcessoPage>();
        services.AddTransient<AcessoViewModel>();

        services.AddTransient<ConfigPage>();
    }
}
