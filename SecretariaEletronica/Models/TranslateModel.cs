using Newtonsoft.Json;

namespace SecretariaEletronica.Models
{
    public struct TranslateModel
    {
        [JsonProperty("data")] public TranslateData Data;
    }

    public struct TranslateData
    {
        [JsonProperty("translations")] public TranslatedText[] Translations;
    }

    public struct TranslatedText
    {
        [JsonProperty("translatedText")] public string Content;
    }
}