FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CodeRunner.SubmissionsService/CodeRunner.SubmissionsService.csproj", "CodeRunner.SubmissionsService/"]
RUN dotnet restore "CodeRunner.SubmissionsService/CodeRunner.SubmissionsService.csproj"
COPY . .
WORKDIR "/src/CodeRunner.SubmissionsService"
RUN dotnet build "CodeRunner.SubmissionsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeRunner.SubmissionsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeRunner.SubmissionsService.dll"]
