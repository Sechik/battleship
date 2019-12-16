FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY EP.SeaBattleAI.Web/EP.SeaBattleAI.Web.csproj EP.SeaBattleAI.Web/
COPY EP.SeaBattleAI.Logic/EP.SeaBattleAI.Logic.csproj EP.SeaBattleAI.Logic/
COPY EP.SeaBattleAI.Common/EP.SeaBattleAI.Common.csproj EP.SeaBattleAI.Common/
COPY EP.SeaBattleAI.Data/EP.SeaBattleAI.Data.csproj EP.SeaBattleAI.Data/
RUN dotnet restore EP.SeaBattleAI.Web/EP.SeaBattleAI.Web.csproj
COPY . .
WORKDIR /src/EP.SeaBattleAI.Web
RUN dotnet build EP.SeaBattleAI.Web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish EP.SeaBattleAI.Web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "EP.SeaBattleAI.Web.dll"]
