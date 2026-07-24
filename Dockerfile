# Build the React frontend.
FROM node:24-bookworm-slim AS frontend-build

WORKDIR /src/client

COPY client/package.json client/package-lock.json ./

RUN npm ci

COPY client/ ./

RUN npm run build


# Build and publish the ASP.NET Core API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build

WORKDIR /src

COPY server/HabitTracker.Api/HabitTracker.Api.csproj server/HabitTracker.Api/

RUN dotnet restore server/HabitTracker.Api/HabitTracker.Api.csproj

COPY server/HabitTracker.Api/ server/HabitTracker.Api/

RUN dotnet publish server/HabitTracker.Api/HabitTracker.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

COPY --from=frontend-build /src/client/dist/ /app/publish/wwwroot/


# Run the completed application.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=backend-build /app/publish/ ./

ENTRYPOINT ["dotnet", "HabitTracker.Api.dll"]