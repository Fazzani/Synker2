FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY Synker.sln ./
COPY Brokers/hfa.Notification.Brokers/hfa.Notification.Brokers.csproj Brokers/hfa.Notification.Brokers/
COPY Brokers/hfa.Notification.DTo/hfa.Brokers.Messages.csproj Brokers/hfa.Notification.DTo/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/Brokers/hfa.Notification.Brokers
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "hfa.Notification.Brokers.dll"]
