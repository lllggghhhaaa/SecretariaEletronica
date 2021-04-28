# Secretaria Eletronica

### Um bot para a plataforma [Discord](https://discord.com/) na linguagem [C#](https://docs.microsoft.com/pt-br/dotnet/csharp/) com a API [DSharpPlus](https://dsharpplus.github.io/)

## Como Configurar

## Bot
### Instalando
#### Baixe os [arquivos ja compilados do bot](https://github.com/lllggghhhaaa/SecretariaEletronica/releases/tag/v1.0) ou baixe o codigo para compilar, para compilar voce precisa do MSBuild, ou uma IDE como o Visual Studio
`nota: voce precisa do dotnet core 6.0, runtime ou sdk` [Download](https://dotnet.microsoft.com/download/dotnet/6.0)
#### Crie um arquivo chamado `config.json` no caminho do bot com o seguinte conteudo
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
#### `nota: essa etapa pode ser pulada, porem um erro ira aparecer e tambem alguns comandos irao parar de funcionar, caso voce queira remover esse modulo, e necessario alterar o codigo fonte`
### Requisitos
#### E necessario ter o `Java` instalado na versao 13 ou superior
[Donwload Java](https://www.oracle.com/java/technologies/javase-downloads.html)
### Instalando o lavalink
#### baixe-o em qualquer lugar do seu computador ou no proprio caminho do bot
[Download Lavalink.jar](https://ci.fredboat.com/viewLog.html?buildId=lastSuccessful&buildTypeId=Lavalink_Build&tab=artifacts&guest=1)

#### Crie um arquivo chamado `start.bat` para iniciar o lavalink com o seguinte conteudo
```bat
color b
java -jar Lavalink.jar
PAUSE
```
#### Crie um arquivo yaml chamado `application.yml` que contem parametros para o lavalink com o seguinte conteudo
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
## Ajuda
`Minha tag no Discord: lllggghhhaaa#2195`
### Documentacao
#### https://dsharpplus.github.io/
### Servidor

![DSharpPlus Discord Server](https://discordapp.com/api/guilds/379378609942560770/embed.png?style=banner3) 
![Discord API Server](https://discordapp.com/api/guilds/81384788765712384/embed.png?style=banner3)