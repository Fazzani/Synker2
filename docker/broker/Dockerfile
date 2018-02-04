FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /app

RUN ls

# Copy csproj and restore as distinct layers
COPY ./hfa.Notification.Brokers/*.csproj ./
RUN ls
RUN dotnet restore 
RUN ls

# Copy everything else and build
COPY . .
RUN ls
RUN dotnet publish ./hfa.Notification.Brokers/hfa.Notification.Brokers.csproj -c Release -o out
RUN ls

# Build runtime image
FROM microsoft/dotnet:2.0-runtime 
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]