﻿using Ashampoo.Translations.Logging.Notifications;
using MediatR;
using MudBlazor;

namespace Ashampoo.Translations.Tools.ComponentLibrary.NotificationHandlers;

public class ImportedTranslationsNotificationHandler : INotificationHandler<ImportedTranslationsNotification>
{
    private readonly ISnackbar snackbar;

    public ImportedTranslationsNotificationHandler(ISnackbar snackbar)
    {
        this.snackbar = snackbar;
    }

    public Task Handle(ImportedTranslationsNotification notification, CancellationToken cancellationToken)
    {
        switch (notification.Count)
        {
            case 0:
                snackbar.Add("No new translations were imported.", Severity.Warning);
                break;
            case 1:
                snackbar.Add("1 new translations was imported.", Severity.Success);
                break;
            default:
                snackbar.Add($"{notification.Count} new translations were imported.", Severity.Success);
                break;
        }

        return Task.CompletedTask;
    }
}