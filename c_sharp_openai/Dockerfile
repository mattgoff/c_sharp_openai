FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["c_sharp_openai/c_sharp_openai.csproj", "c_sharp_openai/"]
RUN dotnet restore "c_sharp_openai/c_sharp_openai.csproj"
COPY . .
WORKDIR "/src/c_sharp_openai"
RUN dotnet build "c_sharp_openai.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "c_sharp_openai.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "c_sharp_openai.dll"]
