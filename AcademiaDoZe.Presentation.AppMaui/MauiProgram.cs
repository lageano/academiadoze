// gabriel geremias vieira
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Presentation.AppMaui.Configuration;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.AppMaui;

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
