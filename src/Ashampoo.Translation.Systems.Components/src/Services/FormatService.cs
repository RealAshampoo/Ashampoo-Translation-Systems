using Ashampoo.Translation.Systems.Components.Dialogs;
using Ashampoo.Translation.Systems.Formats.Abstractions;
using MudBlazor;

namespace Ashampoo.Translation.Systems.Components.Services;

/// <inheritdoc />
public class FormatService : IFormatService
{
    private readonly IFormatFactory formatFactory;

    private readonly IDialogService dialogService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatService"/> class.
    /// </summary>
    /// <param name="dialogService">
    /// The dialog service.
    /// </param>
    /// <param name="formatFactory">
    /// The format factory.
    /// </param>
    public FormatService
    (
        IDialogService dialogService,
        IFormatFactory formatFactory
    )
    {
        this.dialogService = dialogService;
        this.formatFactory = formatFactory;
    }


    /// <inheritdoc />
    public IFormat ConvertTo(IFormat format, string convertFormatId,
        FormatOptionsCallback? formatOptionsCallback = null, AssignOptions? options = null)
    {
        return ConvertToAsync(format, convertFormatId, formatOptionsCallback, options).Result;
    }

    /// <inheritdoc />
    public async Task<IFormat?> ReadFromStreamAsync(Stream stream, string fileName,
        FormatOptionsCallback? formatOptionsCallback = null)
    {
        var format = formatFactory.TryCreateFormatByFileName(fileName) ??
                     throw new InvalidOperationException("File type not supported.");

        var targetLanguage = LanguageParser.TryParseLanguageId(fileName);
        FormatReadOptions formatReadOptions = new()
        {
            TargetLanguage = targetLanguage,
            FormatOptionsCallback = formatOptionsCallback
        };

        await format.ReadAsync(stream, formatReadOptions);

        return formatReadOptions.IsCancelled ? null : format;
    }

    /// <inheritdoc />
    public async Task<IFormat?> ReadFromStreamAsync(Stream stream, string fileName, FormatReadOptions readOptions)
    {
        var format = formatFactory.TryCreateFormatByFileName(fileName) ??
                     throw new InvalidOperationException("File type not supported.");
        
        await format.ReadAsync(stream, readOptions);
        
        return readOptions.IsCancelled ? null : format;
        
        
    }
    
    /// <inheritdoc />
    public async Task ConfigureFormatOptionsAsync(FormatOptions options, string title = "Configure format options")
    {
        var dialogOptions = new DialogOptions { CloseOnEscapeKey = true };


        var parameter = new DialogParameters { { "FormatOptions", options } };

        var dialogReference = dialogService.Show<ConfigureFormatOptionsDialog>(title, parameter, dialogOptions);
        var result = await dialogReference.Result;
        options.IsCanceled = result is null || result.Cancelled;
    }

    /// <inheritdoc />
    public async Task<IFormat> ConvertToAsync(IFormat format, string convertFormatId,
        FormatOptionsCallback? formatOptionsCallback = null, AssignOptions? options = null)
    {
        var convertFormatType = formatFactory.GetFormatProvider(convertFormatId).FormatType;

        options ??= new AssignOptions
        {
            FormatOptionsCallback = formatOptionsCallback
        };

        var convertedFormat = await format.ConvertToAsync(convertFormatType, formatFactory, options);

        return convertedFormat;
    }

    /// <inheritdoc />
    public async Task SwitchLanguageAsync(IFormat format, string oldLanguage, string? newLanguage = null)
    {
        if (!oldLanguage.Equals(format.Header.SourceLanguage) && !oldLanguage.Equals(format.Header.TargetLanguage))
            throw new ArgumentException($"Did not find {oldLanguage} in the given format.");

        if (newLanguage is null)
        {
            FormatStringOption language = new("New language", true);
            FormatOptions options = new()
            {
                Options = new FormatOption[]
                {
                    language
                }
            };

            await ConfigureFormatOptionsAsync(options, "Select new language");

            if (options.IsCanceled) return;

            newLanguage = language.Value;
        }


        var isTargetLanguage = oldLanguage.Equals(format.Header.TargetLanguage);

        foreach (var translationUnit in format)
        {
            var translation = translationUnit.TryGet(oldLanguage);
            if (translation is null) continue;

            translation.Language = newLanguage;
        }

        if (isTargetLanguage) format.Header.TargetLanguage = newLanguage;
        else
            format.Header.SourceLanguage = newLanguage;
    }
    
    /// <inheritdoc />
    public async Task<string?> GetFormatIdAsync()
    {
        var dialogReference = dialogService.Show<SelectFormatDialog>("Select a Format");
        var result = await dialogReference.Result;

        return result.Cancelled ? null : result.Data.ToString();
    }
}