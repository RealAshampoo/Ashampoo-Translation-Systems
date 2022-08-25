﻿using Ashampoo.Translation.Systems.Formats.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Ashampoo.Translation.Systems.Formats.TsProj.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddFormatFactory().AddFormatProvider(builder =>
        {
            return builder.SetId("tsproj")
                .SetSupportedFileExtensions(new[] { ".tsproj" })
                .SetFormatType<TsProjFormat>()
                .SetFormatBuilder<TsProjFormatBuilder>()
                .Create();
        });
    }
}