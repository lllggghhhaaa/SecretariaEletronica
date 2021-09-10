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

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

// ReSharper disable once CheckNamespace
namespace SecretariaEletronica.CustomCommands
{ 
    public class Main : BaseCommandModule
    {
        // Read command Attributes https://dsharpplus.github.io/articles/commands/command_attributes.html
        
        [Command("hello")]
        public async Task Hello(CommandContext ctx)
        {
            await ctx.RespondAsync("hi");
        }
        
        public void Load(DiscordShardedClient client)
        {
            // Add client events and etc.
        }
    }
}