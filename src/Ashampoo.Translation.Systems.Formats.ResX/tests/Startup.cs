﻿using Ashampoo.Translation.Systems.Formats.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Ashampoo.Translation.Systems.Formats.ResX.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddFormatFactory().AddFormatProvider(builder =>
        {
            return builder.SetId("resx")
                .SetSupportedFileExtensions(new[] { ".resx" })
                .SetFormatType<ResXFormat>()
                .SetFormatBuilder<ResXFormatBuilder>()
                .Create();
        });
    }
}