FROM microsoft/aspnetcore-build:2.0 AS build-env
WORKDIR ./

# Copy csproj and restore as distinct layers
COPY ./hfa.Notification.Brokers/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR ./
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]