# PHIMAS Deployment

## 1. Local Docker stack

From the repo root:

```bash
docker compose up --build
```

This starts:

- `phimas-db` on `localhost:3306`
- `phimas-web` on `http://localhost:5187`

The compose stack imports `phimas.sql` into MariaDB on first boot and points the ASP.NET app at that shared database.

## 2. Render deployment

The repo now includes a `render.yaml` Blueprint and a production `Dockerfile` for the ASP.NET app.

Create a Render Blueprint or Web Service from this repo and use:

- Root directory: `PHIMAS_PREDICTIVE_ANALYTICS/PHIMAS_PREDICTIVE_ANALYTICS`
- Dockerfile: `Dockerfile`
- Health check path: `/healthz`

Set these environment variables in Render:

- `ASPNETCORE_ENVIRONMENT=Production`
- `AppStartup__SeedDemoData=false`
- `ConnectionStrings__DefaultConnection=<your production MySQL connection string>`

The app also accepts:

- `DATABASE_URL`
- `MYSQLCONNSTR_DefaultConnection`
- `Database__Host`, `Database__Port`, `Database__Name`, `Database__User`, `Database__Password`, `Database__SslMode`

Use whichever format matches your database provider.

## 3. Shared mobile + web sync

After Render assigns your live backend URL, use the same URL everywhere:

- Web: open `https://<your-render-service>.onrender.com`
- Mobile login screen: set `Server URL` to the same URL
- Mobile Account tab: keep the same `Server URL`

If you want new APK installs to default to the live backend, set one of:

- `PHIMAS_BHW_MOBILE/gradle.properties` -> `bhwApiBaseUrl=https://<your-render-service>.onrender.com/`
- `PHIMAS_BHW_MOBILE/local.properties` -> `bhwApiBaseUrl=https://<your-render-service>.onrender.com/`
- shell env var -> `BHW_API_BASE_URL=https://<your-render-service>.onrender.com/`

Once both clients use the same backend URL, they read and write the same MySQL-backed data and sync through the same API.
