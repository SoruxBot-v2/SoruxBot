# 使用包含 ASP.NET Core 运行时的基础镜像，并确保为 arm64 架构
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 设置构建阶段，使用 SDK 镜像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SoruxBot.Provider.QQ/SoruxBot.Provider.QQ.csproj", "SoruxBot.Provider.QQ/"]
COPY ["SoruxBot.SDK.QQ/SoruxBot.SDK.QQ.csproj", "SoruxBot.SDK.QQ/"]
COPY ["SoruxBot.SDK/SoruxBot.SDK.csproj", "SoruxBot.SDK/"]

# 恢复依赖
RUN dotnet restore "SoruxBot.Provider.QQ/SoruxBot.Provider.QQ.csproj"

# 复制源码并编译项目
COPY . .
WORKDIR "/src/SoruxBot.Provider.QQ"
RUN dotnet build "SoruxBot.Provider.QQ.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 发布应用
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SoruxBot.Provider.QQ.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 使用包含 ASP.NET Core 运行时的镜像作为最终运行环境
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoruxBot.Provider.QQ.dll"]
