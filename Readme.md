# Secretaria Eletronica

### Um bot para a plataforma [Discord](https://discord.com/) na linguagem [C#](https://docs.microsoft.com/pt-br/dotnet/csharp/) com a API [DSharpPlus](https://dsharpplus.github.io/)

## Como Configurar

## Bot
### Instalando
#### Baixe os [arquivos ja compilados do bot](https://github.com/lllggghhhaaa/SecretariaEletronica/releases/tag/v1.0) ou baixe o código para compilar, para compilar você precisa do [MSBuild](https://docs.microsoft.com/pt-br/visualstudio/msbuild/msbuild?view=vs-2019), ou uma IDE como o [Visual Studio](https://visualstudio.microsoft.com/pt-br/)
`Nota: você precisa do dotnet core 6.0, runtime ou sdk` [Download](https://dotnet.microsoft.com/download/dotnet/6.0)
#### Crie um arquivo chamado `config.json` no caminho do bot com o seguinte conteúdo
```json
{
  "token": "TOKEN_DO_BOT", 
  "prefix": "!",
  "lavalink-ip": "127.0.0.1",
  "lavalink-port": 2333,
  "lavalink-pass": "lavalinkpassword"
}
```
## Lavalink
`Nota: essa etapa pode ser pulada, porem um erro irá aparecer e tambem alguns comandos irão parar de funcionar, caso você queira remover esse problema, e necessário alterar o código fonte`
### Requisitos
#### E necessário ter o **Java** instalado na versão **13** ou superior
[Donwload Java](https://www.oracle.com/java/technologies/javase-downloads.html)
### Instalando o lavalink
#### baixe-o em qualquer lugar do seu computador ou no próprio caminho do bot
[Download Lavalink.jar](https://ci.fredboat.com/viewLog.html?buildId=lastSuccessful&buildTypeId=Lavalink_Build&tab=artifacts&guest=1)

#### Crie um arquivo chamado `start.bat` para iniciar o lavalink com o seguinte conteúdo
```bat
color b
java -jar Lavalink.jar
PAUSE
```
#### Crie um arquivo yaml chamado `application.yml` que contem parâmetros para o lavalink com o seguinte conteúdo
```yaml
server: # REST and WS server
  port: 2333
  address: 127.0.0.1
spring:
  main:
    banner-mode: log
lavalink:
  server:
    password: "lavalinkpassword"
    sources:
      youtube: true
      bandcamp: true
      soundcloud: true
      twitch: true
      vimeo: true
      mixer: true
      http: true
      local: false
    bufferDurationMs: 400
    youtubePlaylistLoadLimit: 6 # Number of pages at 100 each
    youtubeSearchEnabled: true
    soundcloudSearchEnabled: true
    gc-warnings: true

metrics:
  prometheus:
    enabled: false
    endpoint: /metrics

sentry:
  dsn: ""
#  tags:
#    some_key: some_value
#    another_key: another_value

logging:
  file:
    max-history: 30
    max-size: 1GB
  path: ./logs/

  level:
    root: INFO
    lavalink: INFO
```

## Comandos Costumizados
#### No repositório, existe um outro projeto chamado [CommandTemplate](https://github.com/lllggghhhaaa/SecretariaEletronica/tree/master/CommandTemplate), você pode notar que existe apenas uma classe chamada [Main.cs](https://github.com/lllggghhhaaa/SecretariaEletronica/blob/master/CommandTemplate/Main.cs), essa é a base para um módulo de comando
```c#
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
```
#### [Introduçãodo CommandsNext](https://dsharpplus.github.io/articles/commands/intro.html)

#### Após terminar o seu módulo, voce deve compilar ele para **Biblioteca de Classes (.dll)**, e jogá-la dentro de uma pasta chamada `CustomCommands` no diretório do bot, depois ele sera carregado na próxima vez que iniciar o bot
`Nota: se exister mais de 1 comando repetido, o módulo nao sera carregado`


## Ajuda
`Minha tag no Discord: lllggghhhaaa#2195`
### Documentação
#### https://dsharpplus.github.io/