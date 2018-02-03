FROM microsoft/dotnet:2.0-sdk AS build-env
COPY . /app
WORKDIR /app

RUN ls

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN ls
RUN dotnet restore
RUN ls

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.0-runtime 
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]