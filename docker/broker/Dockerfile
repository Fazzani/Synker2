FROM microsoft/dotnet:2.0-sdk AS build-env
RUN ls
WORKDIR ./hfa.Notification.Brokers/

RUN ls

RUN dotnet restore
RUN ls

RUN dotnet publish -c Release -o out
RUN ls

COPY ./hfa.Notification.Brokers/out /app
# Build runtime image
FROM microsoft/dotnet:2.0-runtime 
WORKDIR /app
RUN ls
COPY --from=build-env ./hfa.Notification.Brokers/out .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]