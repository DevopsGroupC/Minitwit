# Define a build-time argument for the environment
ARG ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore && \
    dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Use the build-time argument to set the ASPNETCORE_ENVIRONMENT variable
ARG ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}

COPY --from=build /app/out . 
COPY ./Databases/schema.sql ./schema.sql
COPY ./Services/latest_processed_sim_action_id.txt ./Services/latest_processed_sim_action_id.txt
EXPOSE 5000

ENTRYPOINT ["dotnet", "csharp-minitwit.dll"]