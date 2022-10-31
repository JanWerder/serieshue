# SeriesHue

## Project setup

1. Install the .NET Core SDK.
2. Set up the .NET Core Project:
```
> dotnet dev-certs https --trust
> dotnet restore
```

3. Install Postgres 14 and setup the user according to the appsettings.json.
4. Run the Migrations.

## Migrations

Install the Migration Tool
```
> dotnet tool install --global dotnet-ef
```

Update database schema:
```
> dotnet ef migrations add InitialCreate --context SeriesHueContext
> dotnet ef database update --context SeriesHueContext
```