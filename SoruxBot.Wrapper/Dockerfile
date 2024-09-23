FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SoruxBot.Wrapper/SoruxBot.Wrapper.csproj", "SoruxBot.Wrapper/"]
COPY ["SoruxBot.Kernel/SoruxBot.Kernel.csproj", "SoruxBot.Kernel/"]
COPY ["SoruxBot.SDK/SoruxBot.SDK.csproj", "SoruxBot.SDK/"]
RUN dotnet restore "SoruxBot.Wrapper/SoruxBot.Wrapper.csproj"
COPY . .
WORKDIR "/src/SoruxBot.Wrapper"
RUN dotnet build "SoruxBot.Wrapper.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SoruxBot.Wrapper.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoruxBot.Wrapper.dll"]
