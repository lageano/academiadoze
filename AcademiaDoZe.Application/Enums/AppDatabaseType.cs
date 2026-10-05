// gabriel geremias vieira
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

// Gerenciadores de banco suportados, expostos para a camada de Apresentação
// sem que ela precise conhecer a Infraestrutura.
public enum AppDatabaseType
{
    [Display(Name = "SQL Server")]
    SqlServer = 0,

    [Display(Name = "MySQL")]
    MySql = 1,

    [Display(Name = "SQLite")]
    Sqlite = 2
}
