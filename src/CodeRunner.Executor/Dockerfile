FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CodeRunner.Executor/", "/src/CodeRunner.Executor"]
COPY ["CodeRunner.Common.Kafka/", "/src/CodeRunner.Common.Kafka"]
COPY ["CodeRunner.Common.Quartz/", "/src/CodeRunner.Common.Quartz"]

RUN dotnet restore "CodeRunner.Executor/CodeRunner.Executor.csproj"
RUN dotnet build "CodeRunner.Executor/CodeRunner.Executor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeRunner.Executor/CodeRunner.Executor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeRunner.Executor.dll"]
