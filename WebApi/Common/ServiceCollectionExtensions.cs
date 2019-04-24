namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Linq;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureAndValidate<T>(this IServiceCollection @this,
            IConfiguration config) where T : class
            => @this
                .Configure<T>(config.GetSection(typeof(T).Name))
                .PostConfigure<T>(settings =>
                {
                    var configErrors = settings.ValidationErrors().ToArray();
                    if (configErrors.Any())
                    {
                        var aggrErrors = string.Join(",", configErrors);
                        var count = configErrors.Length;
                        var configType = typeof(T).Name;
                        throw new ApplicationException(
                            $"Found {count} configuration error(s) in {configType}: {aggrErrors}");
                    }
                });
    }
}
