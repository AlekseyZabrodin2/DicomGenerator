using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DicomGenerator.UI.Wpf.Configuration
{
    public class AppConfigService
    {
        private readonly string _configPath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public AppConfigService()
        {
            _configPath = Path.Combine(AppContext.BaseDirectory, "appconfig.json");
        }

        public async Task<AppConfig> LoadAsync()
        {
            if (!File.Exists(_configPath))
            {
                var config = new AppConfig();

                await SaveAsync(config);

                return config;
            }

            await using var stream = File.OpenRead(_configPath);

            return await JsonSerializer.DeserializeAsync<AppConfig>(stream, _jsonOptions) ?? new AppConfig();
        }

        public async Task SaveAsync(AppConfig config)
        {
            await using var stream = File.Create(_configPath);

            await JsonSerializer.SerializeAsync(stream, config, _jsonOptions);
        }
    }
}
