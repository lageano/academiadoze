// gabriel geremias vieira
using AcademiaDoZe.Application.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

// utilitário para seleção múltipla (CheckBoxes) com enum Flags
public class RestricaoSelectable : ObservableObject
{
    public AppMatriculaRestricoes Value { get; set; }

    public string Descricao => Value.GetDisplayName();

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
