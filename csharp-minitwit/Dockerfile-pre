    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /app

    COPY . .

    RUN dotnet restore

    RUN dotnet publish -c Release -o out

    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    WORKDIR /app
    ENV ASPNETCORE_ENVIRONMENT=Development

    COPY --from=build /app/out .
    COPY ./Databases/schema.sql /app/Databases/schema.sql
    COPY ./Services/latest_processed_sim_action_id.txt /app/Services/latest_processed_sim_action_id.txt
    EXPOSE 5000

    ENTRYPOINT ["dotnet", "csharp-minitwit.dll"]
