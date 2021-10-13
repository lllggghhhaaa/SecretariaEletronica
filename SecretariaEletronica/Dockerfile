FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SecretariaEletronica.csproj", "SecretariaEletronica/"]
RUN dotnet restore "SecretariaEletronica/SecretariaEletronica.csproj"
COPY . "/src/SecretariaEletronica"
WORKDIR "/src/SecretariaEletronica"
RUN dotnet build "SecretariaEletronica.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecretariaEletronica.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get -y update
RUN apt-get -y upgrade
RUN apt-get -y install openjdk-17-jre
ENTRYPOINT ["dotnet", "SecretariaEletronica.dll"]