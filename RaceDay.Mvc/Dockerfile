FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RaceDay.Mvc/RaceDay.Mvc.csproj", "RaceDay.Mvc/"]
RUN dotnet restore "RaceDay.Mvc/RaceDay.Mvc.csproj"

COPY . .
WORKDIR "/src/RaceDay.Mvc"

RUN dotnet build "RaceDay.Mvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RaceDay.Mvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]