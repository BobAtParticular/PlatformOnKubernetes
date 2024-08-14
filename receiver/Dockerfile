FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

COPY receiver /code/receiver
COPY shared /code/shared

WORKDIR /code/receiver

RUN dotnet restore
RUN dotnet publish -o /code/out

FROM mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled

COPY --from=build-env /code/out .

USER $APP_UID
ENTRYPOINT ["dotnet", "Receiver.dll"]