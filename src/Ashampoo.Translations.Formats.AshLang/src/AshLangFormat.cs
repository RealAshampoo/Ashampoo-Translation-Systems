﻿using Ashampoo.Translations.Formats.Abstractions;
using Ashampoo.Translations.Formats.Abstractions.Translation;
using Ashampoo.Translations.Formats.AshLang.Chunk;
using IFormatProvider = Ashampoo.Translations.Formats.Abstractions.IFormatProvider;

namespace Ashampoo.Translations.Formats.AshLang;

public class AshLangFormat : AbstractTranslationUnits, IFormat
{
    public IChunk[] Chunks { get; private set; }

    public FormatLanguageCount LanguageCount => FormatLanguageCount.SourceAndTarget;


    public Func<FormatProviderBuilder, IFormatProvider> BuildFormatProvider()
    {
        return builder => builder.SetId("ashlang")
            .SetSupportedFileExtensions(new[] { ".ashlang" })
            .SetFormatType<AshLangFormat>()
            .SetFormatBuilder<AshLangFormatBuilder>()
            .Create();
    }

    public IFormatHeader Header { get; private set; }


    public AshLangFormat()
    {
        var ashLangFormatHeader = new AshLangFormatHeader();
        Header = ashLangFormatHeader;

        // some default Chunks on the way we need.
        var appIdChunk = new AppIdChunk();
        var versionChunk = new VersionChunk();
        var translationChunk = new TranslationChunk();
        Chunks = new IChunk[]
        {
            ashLangFormatHeader.LanguageChunk,
            appIdChunk,
            ashLangFormatHeader.XDataChunk,
            versionChunk,
            translationChunk
        };
    }

    public void Write(Stream stream)
    {
        var chunkWriter = new ChunkWriter(stream, Chunks);
        chunkWriter.Write();
    }

    public Task ReadAsync(Stream stream, FormatReadOptions? options = null)
    {
        try
        {
            var reader = new ChunkReader(stream);

            var language = reader.TryGetOrDefault<LanguageChunk>(LanguageChunk.Id);
            var xdata = reader.TryGetOrDefault<XDataChunk>(XDataChunk.Id);

            Header = new AshLangFormatHeader(language, xdata);

            var translations = reader.TryGetOrDefault<TranslationChunk>(TranslationChunk.Id);

            var sourceLanguage = Header.SourceLanguage ??
                                 throw new NullReferenceException("SourceLanguage can not be null.");
            foreach (var translation in translations.Translations)
            {
                var translationUnit = new DefaultTranslationUnit(translation.Id)
                {
                    new SourceTranslationString(sourceLanguage, translation),
                    new TargetTranslationString(Header.TargetLanguage, translation)
                };
                Add(translationUnit);
            }

            Chunks = reader.Chunks;
        }
        catch (FormatException ex)
        {
            throw new UnsupportedFormatException(this, ex.Message, ex);
        }

        return Task.CompletedTask;
    }
}