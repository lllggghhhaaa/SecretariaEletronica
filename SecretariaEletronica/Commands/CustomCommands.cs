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

using System.Reflection;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SecretariaEletronica.Commands;

public class CustomCommands : BaseCommandModule
{
    [Command("upload"), RequireOwner]
    public async Task Upload(CommandContext ctx)
    {
        if (ctx.Message.Attachments.Count is not 1)
        {
            await ctx.RespondAsync("No File");
            return;
        }

        if (!ctx.Message.Attachments[0].FileName.EndsWith(".dll"))
        {
            await ctx.RespondAsync("File are not an DLL");
            return;
        }
            
        using (HttpClient client = new HttpClient())
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(),
                "CustomCommands",
                ctx.Message.Attachments[0].FileName);

            if (File.Exists(path)) File.Delete(path);

            HttpResponseMessage response = await client.GetAsync(ctx.Message.Attachments[0].Url);
            FileStream fs = File.Create(path);

            await response.Content.CopyToAsync(fs);
                
            Assembly assembly = Assembly.LoadFile(path);
                    
            Type type = assembly.GetType("SecretariaEletronica.CustomCommands.Main");
            if (type?.BaseType == typeof(BaseCommandModule)) ctx.CommandsNext.RegisterCommands(type);

            MethodInfo methodInfo = type?.GetMethod("Load");
            object o = Activator.CreateInstance(type);

            methodInfo?.Invoke(o, new []{ Startup.Client });

            await ctx.RespondAsync("Loaded");
        }
    }
}