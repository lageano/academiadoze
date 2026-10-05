// gabriel geremias vieira
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Message;

// Avisa a aplicação de que o tema preferido mudou ("light", "dark" ou "system").
public class TemaPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value);
