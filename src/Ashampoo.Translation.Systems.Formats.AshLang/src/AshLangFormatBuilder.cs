using Ashampoo.Translation.Systems.Formats.Abstractions;
using Ashampoo.Translation.Systems.Formats.Abstractions.Translation;
using Ashampoo.Translation.Systems.Formats.AshLang.Chunk;
using CommunityToolkit.Diagnostics;

namespace Ashampoo.Translation.Systems.Formats.AshLang;

/// <summary>
/// Builder for <see cref="AshLangFormat"/>
/// </summary>
public class AshLangFormatBuilder : IFormatBuilderWithSourceAndTarget
{
    private const string SourceLanguage = "en-US"; // AshLang source is always in English
    private string? targetLanguage;
    private readonly Dictionary<string, (string source, string target)> translations = new();

    /// <inheritdoc />
    public IFormat Build()
    {
        Guard.IsNotNullOrWhiteSpace(targetLanguage, nameof(targetLanguage));

        var ashLang = new AshLangFormat
        {
            Header =
            {
                TargetLanguage = targetLanguage
            }
        };
        var translationChunk = (TranslationChunk)ashLang.Chunks.Last();

        foreach (var (id, (source, target)) in translations)
        {
            var translation = new TranslationChunk.Translation(0, id, target, source, "");
            translationChunk.Translations.Add(translation);

            var translationUnit = new DefaultTranslationUnit(id)
            {
                new SourceTranslationString(SourceLanguage, translation),
                new TargetTranslationString(targetLanguage, translation)
            };
            ashLang.Add(translationUnit);
        }


        return ashLang;
    }

    /// <inheritdoc />
    public void Add(string id, string source, string target)
    {
        translations.Add(id, (source, target));
    }

    /// <inheritdoc />
    public void SetSourceLanguage(string language)
    {
        // AshLang source is always in English
    }

    /// <inheritdoc />
    public void SetTargetLanguage(string language)
    {
        targetLanguage = language;
    }
}