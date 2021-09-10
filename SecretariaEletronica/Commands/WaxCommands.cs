//   Copyright 2022 lllggghhhaaa
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//       You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//       distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//       See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
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
                    { "x-rapidapi-host", "google-translate1.p.rapidapi.com" }
                },
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "q", text },
                    { "target", target },
                    { "source", language }
                })
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

        [Command("clipboard")]
        public async Task ClipBoard(CommandContext ctx, string arg = "0", [RemainingText] string text = "")
        {
            IMongoCollection<BsonDocument> collection = Startup.Database.GetCollection<BsonDocument>("clipboard");
            
            if (arg == "add")
            {
                if (text == String.Empty)
                {
                    await ctx.RespondAsync("No text provided");
                    return;
                }
                
                IReadOnlyList<DiscordUser> mentionedUsers = ctx.Message.MentionedUsers;
                IReadOnlyList<DiscordRole> mentionedRoles = ctx.Message.MentionedRoles;

                foreach (DiscordUser user in mentionedUsers)
                {
                    text = text.Replace($"<@!{user.Id}>", user.Username);
                    text = text.Replace($"<@{user.Id}>", user.Username);
                }

                foreach (DiscordRole role in mentionedRoles)
                {
                    text = text.Replace($"<@&{role.Id}>", role.Name);
                }

                BsonDocument document = new BsonDocument
                {
                    { "content", text },
                    { "author", ctx.User.Id.ToString() },
                    { "index", (await collection.CountDocumentsAsync(new BsonDocument())).ToString() }
                };

                await collection.InsertOneAsync(document);
                await ctx.RespondAsync("Copied!");
                return;
            }

            if (!Int32.TryParse(arg, out int id))
            {
                id = new Random().Next(0, (int) await collection.CountDocumentsAsync(new BsonDocument()));
            }

            if (collection.FindSync(new BsonDocument {{"index", id.ToString()}}).FirstOrDefault() is null)
            {
                await ctx.RespondAsync("Error, cannot find index");
                return;
            }

            await ctx.RespondAsync(collection.FindSync(new BsonDocument {{"index", id.ToString()}}).FirstOrDefault()
                .GetElement("content").Value.AsString);
        }
    }
}