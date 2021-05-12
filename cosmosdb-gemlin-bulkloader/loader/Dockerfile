FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY loader.csproj .
RUN  dotnet restore
RUN  dotnet list package

# copy and publish app and libraries
COPY . .
RUN  dotnet publish -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "loader.dll"]


# docker build -t cjoakim/cosmosdb-gremlin-bulkloader -f Dockerfile . 
# docker images | grep cjoakim/cosmosdb-gremlin-bulkloader
# docker login -u <user> -p <password>
# docker push cjoakim/cosmosdb-gremlin-bulkloader:latest
