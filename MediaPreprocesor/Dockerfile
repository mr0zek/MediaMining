#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mr0zek/net5-with-exiftool AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MediaPreprocessor.csproj", "."]
RUN dotnet restore "MediaPreprocessor.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "MediaPreprocessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MediaPreprocessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediaPreprocessor.dll"]
