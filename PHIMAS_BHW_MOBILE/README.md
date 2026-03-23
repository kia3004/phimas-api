# BHW Mobile App

This Android Studio project connects only to the ASP.NET Core API used by the shared PHIMAS web system.

## Shared backend setup

1. Deploy the ASP.NET Core backend to a public URL such as `https://<your-render-service>.onrender.com`.
2. Open `PHIMAS_BHW_MOBILE` in Android Studio.
3. The default configuration in this workspace is `bhwApiBaseUrl=https://your-render-service.onrender.com/`. Replace it with your actual Render URL in `gradle.properties`, `local.properties`, or the `BHW_API_BASE_URL` environment variable if you want new installs to default to the live backend.
4. The mobile login screen and Account tab also let you edit the server URL at runtime, so you do not need to rebuild the APK when the backend URL changes.
5. For local-only development, you can still point the app to a LAN or emulator URL:
   - Android emulator: `http://10.0.2.2:5187/`
   - Physical device: `http://<your-pc-lan-ip>:5187/`
6. Sync Gradle and run the `app` module.

This shell session did not have a Gradle executable or wrapper available, so Android compilation must be completed from Android Studio on your machine.

## Mobile behavior

- Login, dashboard, tasks, health records, consultation logs, profile updates, password changes, availability changes, and profile image uploads all call the API.
- No direct database connection is used in the Android app.
- After every mutation, the app reloads data from the API.
- The app also refreshes on resume and on a timed sync loop so web changes appear on mobile without reinstalling or reseeding data.
- Relative file URLs from the API, such as uploaded profile pictures, resolve against the currently saved server URL.

## Backend endpoint used

Base path: `api/BHWApi`

- `POST /login`
- `GET /profile`
- `GET /dashboard`
- `GET /tasks`
- `POST /updateTaskStatus`
- `GET /households`
- `GET /healthrecords`
- `POST /healthrecords`
- `GET /reports/recent`
- `GET /consultationlogs`
- `POST /submitConsultation`
- `POST /updateProfile`
- `POST /changePassword`
- `POST /availability`
- `GET /insights`
- `POST /uploadProfilePic`
