# SeriesHue

This project uses asp.net core with entity framework core and postgresql. For more functionality on the frontend it additionally uses Alpine.js.
Below are the steps to get the project running.

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

## Lessons learned to Production

### Install pg_trgm

sudo -u postgres psql --dbname=serieshue

CREATE EXTENSION pg_trgm;