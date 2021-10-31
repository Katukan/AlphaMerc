using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleProject.ProjectSettings {
    public static class JsonSetting {
        public static async void JsonSettingsDeserializeAsync(BotSettings botSettings) {
            using(FileStream fileStream = new FileStream("botSettings.json", FileMode.OpenOrCreate)) {
                botSettings = await JsonSerializer.DeserializeAsync<BotSettings>(fileStream);
            }
        }
    }
}