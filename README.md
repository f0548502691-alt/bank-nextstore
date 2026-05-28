# bank-nextstore

Dashboard להצגת יתרות בנק מתוך קובץ JSON דמו (`bank_balances_demo_5000.json`).

## מבנה

- `src/backend` - ASP.NET Core Web API ב-Clean Architecture עם MediatR.
- `src/frontend/bank-dashboard` - Angular 20 standalone app.
- `SPECIFICATION.md` - אפיון הפתרון ויומן שינויים.

## הרצת Backend

נדרש .NET 10 SDK.

```bash
cd src/backend
dotnet restore
dotnet run --project BankDashboard.Api --urls http://localhost:5000
```

ה-Frontend מוגדר להעביר קריאות API אל `http://localhost:5000`.

Endpoints:

- `GET /api/bank-balances`
- `GET /api/bank-balances/filters`

פרמטרי סינון נתמכים:

- `search`
- `bankName`
- `currency`
- `balanceType`
- `status`
- `minAmount`
- `maxAmount`
- `page` - ברירת מחדל `1`
- `pageSize` - ברירת מחדל `50`, מקסימום `500`

ה-API מחזיר עמוד אחד בכל קריאה יחד עם metadata: מספר עמוד, גודל עמוד, סך תוצאות, סך עמודים, והאם יש עמוד קודם/הבא.

## הרצת Frontend

```bash
cd src/frontend/bank-dashboard
npm install
npm start
```

שרת הפיתוח של Angular משתמש ב-`proxy.conf.json` ומעביר קריאות `/api` אל `http://localhost:5000`.

## הרצה עם Docker Compose

נדרש Docker עם Compose plugin.

```bash
docker compose up --build
```

כתובות:

- Frontend: `http://localhost:4200`
- Backend API: `http://localhost:5000/api/bank-balances`

במצב Docker, Nginx משרת את קבצי ה-Angular ומעביר קריאות `/api` לשירות ה-backend בתוך רשת ה-Compose.

## בדיקות

```bash
cd src/backend
dotnet test

cd ../frontend/bank-dashboard
npm test -- --watch=false
```
