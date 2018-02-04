FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR ./hfa.Notification.Brokers/

RUN ls

RUN dotnet restore
RUN ls

RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.0-runtime 
WORKDIR ./hfa.Notification.Brokers/
COPY --from=build-env ./hfa.Notification.Brokers/out .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]