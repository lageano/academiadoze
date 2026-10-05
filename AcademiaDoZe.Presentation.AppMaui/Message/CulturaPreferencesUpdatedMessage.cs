// gabriel geremias vieira
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Message;

// Avisa a aplicação de que o idioma preferido mudou ("pt-BR", "en-US" ou "es-ES").
public class CulturaPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value);
