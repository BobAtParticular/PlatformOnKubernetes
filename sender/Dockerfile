FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

COPY sender /code/sender
COPY shared /code/shared

WORKDIR /code/sender

RUN dotnet restore
RUN dotnet publish -o /code/out

FROM mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled

COPY --from=build-env /code/out .

USER $APP_UID
ENTRYPOINT ["dotnet", "Sender.dll"]