FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /ofbot

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /ofbot
COPY --from=build-env /ofbot/out .
ENTRYPOINT ["dotnet", "ofbot.dll"]