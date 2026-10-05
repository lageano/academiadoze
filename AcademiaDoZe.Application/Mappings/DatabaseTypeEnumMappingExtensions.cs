// gabriel geremias vieira
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Application.Mappings;

// Converte o enum da aplicação para o da infraestrutura e vice-versa.
public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType appDatabaseType)
    {
        return (DatabaseType)appDatabaseType;
    }

    public static AppDatabaseType ToApplication(this DatabaseType databaseType)
    {
        return (AppDatabaseType)databaseType;
    }
}
