FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CodeRunner.Worker/", "/src/CodeRunner.Worker"]
COPY ["CodeRunner.Common.Kafka/", "/src/CodeRunner.Common.Kafka"]
COPY ["CodeRunner.Common.Quartz/", "/src/CodeRunner.Common.Quartz"]

RUN dotnet restore "CodeRunner.Worker/CodeRunner.Worker.csproj"
RUN dotnet build "CodeRunner.Worker/CodeRunner.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeRunner.Worker/CodeRunner.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install prerequisites
RUN apt-get update && apt-get install -y golang-go
ENTRYPOINT ["dotnet", "CodeRunner.Worker.dll"]
