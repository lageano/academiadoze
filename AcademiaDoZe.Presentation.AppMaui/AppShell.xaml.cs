// gabriel geremias vieira
using AcademiaDoZe.Presentation.AppMaui.Views;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegistrarRotas();
    }

    // Rotas de detalhe/edição, navegadas por GoToAsync a partir das listagens
    private static void RegistrarRotas()
    {
        Routing.RegisterRoute("logradouro", typeof(LogradouroPage));
        Routing.RegisterRoute("aluno", typeof(AlunoPage));
        Routing.RegisterRoute("colaborador", typeof(ColaboradorPage));
        Routing.RegisterRoute("matricula", typeof(MatriculaPage));
    }
}
