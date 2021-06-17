using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SecretariaEletronica.Models;

namespace SecretariaEletronica.Commands
{
    public class WaxCommands : BaseCommandModule
    {
        [Command("binary")]
        public async Task Binary(CommandContext ctx, [RemainingText] string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);

            string finalText = String.Empty;
            
            foreach (byte fraction in textBytes)
            {
                finalText += Convert.ToString(fraction, 2).PadLeft(8, '0') + " ";
            }

            if (finalText.Length <= 2000) await ctx.RespondAsync(finalText);
            else await ctx.RespondAsync("Very large text");
        }

        [Command("translate")]
        public async Task Translate(CommandContext ctx, string language, string target, [RemainingText] string text)
        {
            HttpClient client = new HttpClient();
            
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://google-translate1.p.rapidapi.com/language/translate/v2"),
                Headers =
                {
                    { "x-rapidapi-key", Startup.Configuration.RapidApiKey },
                    { "x-rapidapi-host", "google-translate1.p.rapidapi.com" },
                },
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "q", text },
                    { "target", target },
                    { "source", language },
                }),
            };
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string body = await response.Content.ReadAsStringAsync();

                TranslateModel translation = JsonConvert.DeserializeObject<TranslateModel>(body);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = "Translation",
                    Description = $"Source language: `{language}`, target language: `{target}`\n" +
                                  $"Text: `{text}`\n" +
                                  $"Translation: `{translation.Data.Translations[0].Content}`"
                };

                await ctx.RespondAsync(embed.Build());
            }
        }
    }
}