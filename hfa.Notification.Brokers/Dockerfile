FROM microsoft/dotnet:2.0-sdk AS build-env

WORKDIR /app

RUN pwd

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN pwd
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.0-runtime 
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]