using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SecretariaEletronica.Events.Client
{
    public class MessageCreated
    {
        private readonly IMongoDatabase _databaseHelper;

        public MessageCreated(IMongoDatabase databaseHelper)
        {
            this._databaseHelper = databaseHelper;
        }
        
        public Task Client_MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if(e.Author.IsBot) return Task.CompletedTask;

            string time = DateTime.Now.ToUniversalTime().AddHours(3).ToString("dd/MM/yyyy");
            
            BsonDocument filter = new BsonDocument
            {
                { "member", $"{e.Author.Id}" }
            };

            IMongoCollection<BsonDocument> iCollection = _databaseHelper.GetCollection<BsonDocument>("users");

            if (iCollection.Find(filter).CountDocuments() > 0)
            {
                BsonDocument document = iCollection.Find(filter).FirstOrDefault();

                int value = 1;
                if (document.TryGetElement($"{e.Guild.Id}|{time}", out BsonElement bvalue))
                {
                    value = Convert.ToInt32(bvalue.Value);
                    value++;
                }

                var update = Builders<BsonDocument>.Update.Set($"{e.Guild.Id}|{time}", value.ToString());

                iCollection.UpdateOne(filter, update);
            }
            else
            {
                BsonDocument document = new BsonDocument
                {
                    { "member", $"{e.Author.Id}" },
                    { $"{e.Guild.Id}|{time}", "1" }
                };
                
                iCollection.InsertOne(document);
            }

            return Task.CompletedTask;
        }
    }
}