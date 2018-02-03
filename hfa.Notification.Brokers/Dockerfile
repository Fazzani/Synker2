FROM microsoft/dotnet:2.0-sdk AS build-env

RUN dotnet restore
RUN dotnet publish -c Release -o ./out

# Build runtime image
FROM microsoft/dotnet:2.0-runtime 
WORKDIR /app
COPY --from=build-env ./out .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]