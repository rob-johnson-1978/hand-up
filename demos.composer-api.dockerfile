# bring in SDK in order to publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publisher
WORKDIR /root

# copy files
COPY ./src src
COPY ./demo demo

# publish
WORKDIR demo/ComposerApi
RUN dotnet publish -c Release -o /app

# bring in the runtime in order to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runner
WORKDIR /app

# expose the port
EXPOSE 80

# copy
COPY --from=publisher /app .

# run
ENTRYPOINT ["dotnet", "ComposerApi.dll"]