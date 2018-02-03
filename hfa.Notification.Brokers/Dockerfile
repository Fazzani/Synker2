FROM microsoft/dotnet:2.0-sdk AS build-env
COPY ../ /app
WORKDIR /app/hfa.Notification.Brokers

RUN ls

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN ls
RUN dotnet restore
RUN ls

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out
RUN ls

# Build runtime image
FROM microsoft/dotnet:2.0-runtime 
WORKDIR /app/hfa.Notification.Brokers
COPY --from=build-env /app/hfa.Notification.Brokers/out .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]