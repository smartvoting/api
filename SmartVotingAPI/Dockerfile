FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SmartVotingAPI/SmartVotingAPI.csproj", "SmartVotingAPI/"]
RUN dotnet restore "SmartVotingAPI/SmartVotingAPI.csproj"
COPY . .
WORKDIR "/src/SmartVotingAPI"
RUN dotnet build "SmartVotingAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_14.x | bash \
    && apt-get install nodejs -yq
RUN dotnet publish "SmartVotingAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartVotingAPI.dll"]
