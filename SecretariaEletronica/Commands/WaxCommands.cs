using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

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
    }
}