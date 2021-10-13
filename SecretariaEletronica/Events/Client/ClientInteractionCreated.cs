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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using SecretariaEletronica.Commands;

namespace SecretariaEletronica.Events.Client;

public class ClientInteractionCreated
{
	public static async Task Client_InteractionCreated(DiscordClient client, InteractionCreateEventArgs args)
	{
		switch (args.Interaction.Data.CustomId)
		{
			case "sdd":
				LavalinkExtension lava = client.GetLavalink();
				LavalinkNodeConnection node = lava.ConnectedNodes.Values.FirstOrDefault();
				LavalinkGuildConnection conn = node?.GetGuildConnection(client.GetGuildAsync(args.Interaction.Guild.Id).Result);
				
				if (node is null)
				{
					await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate,
						new DiscordInteractionResponseBuilder(new DiscordMessageBuilder
						{
							Content = "Lavalink is not connected."
						}));
					return;
				}
				
				DiscordMember member = args.Interaction.Guild.GetMemberAsync(args.Interaction.User.Id).Result;
				
				if (member.VoiceState is null || member.VoiceState.Channel is null)
				{
					await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate,
						new DiscordInteractionResponseBuilder(new DiscordMessageBuilder
						{
							Content = "You are not in a voice channel."
						}));
					return;
				}
				
				if (conn is null)
				{
					await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate,
						new DiscordInteractionResponseBuilder(new DiscordMessageBuilder
						{
							Content = "Lavalink is not connected."
						}));
					return;
				}

				if (node.Rest is not null)
				{
					List<LavalinkTrack> tracks = new List<LavalinkTrack>();

					foreach (string value in args.Interaction.Data.Values)
					{
						LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(value.Substring(4));

						tracks.Add(loadResult.Tracks.First());
					}

					await LavaLinkCommands.AddTracks(tracks, client, args.Interaction.Guild.Id);
					
					await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
						new DiscordInteractionResponseBuilder(new DiscordMessageBuilder
						{
							Content = "Added to queue."
						}));
					return;
				}
				break;
		}
	}
}