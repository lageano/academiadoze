// gabriel geremias vieira
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Message;

// Avisa a aplicação de que as credenciais do banco de dados foram alteradas.
public class BancoPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value);
