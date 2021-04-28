using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

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
    }
}