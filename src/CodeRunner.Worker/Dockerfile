FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/CodeRunner.Worker/CodeRunner.Worker.csproj", "CodeRunner.Worker/"]
RUN dotnet restore "src/CodeRunner.Worker/CodeRunner.Worker.csproj"
COPY . .
WORKDIR "/src/CodeRunner.Worker"
RUN dotnet build "CodeRunner.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeRunner.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeRunner.Worker.dll"]
