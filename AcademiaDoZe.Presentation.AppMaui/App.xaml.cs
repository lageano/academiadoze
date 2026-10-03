// gabriel geremias vieira
namespace AcademiaDoZe.Presentation.AppMaui;

// Application qualificado por causa do namespace AcademiaDoZe.Application da camada de aplicação.
public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell())
        {
            Title = "Academia do Zé"
        };
    }
}
