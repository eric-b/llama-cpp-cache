#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9105

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Directory.Build.props", "src/"]
COPY ["src/Llama.Cpp.Cache/Llama.Cpp.Cache.csproj", "src/Llama.Cpp.Cache/"]
RUN dotnet restore "src/Llama.Cpp.Cache/Llama.Cpp.Cache.csproj"
COPY src/ src
WORKDIR "/src"
RUN dotnet build "src/Llama.Cpp.Cache/Llama.Cpp.Cache.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Llama.Cpp.Cache/Llama.Cpp.Cache.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Llama.Cpp.Cache.dll"]