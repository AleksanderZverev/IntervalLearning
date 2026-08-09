#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
#EXPOSE 7249
#EXPOSE 5249

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["IntervalLearningApi/IntervalLearningApi.csproj", "IntervalLearningApi/"]
COPY ["DB/DB.csproj", "DB/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "IntervalLearningApi/IntervalLearningApi.csproj"
COPY . .
WORKDIR "/src/IntervalLearningApi"
RUN dotnet build "IntervalLearningApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IntervalLearningApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntervalLearningApi.dll"]