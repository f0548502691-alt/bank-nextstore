# אפיון תמציתי - Bank Balances Dashboard

## מטרת המערכת

Dashboard להצגת יתרות בנק מתוך קובץ JSON דמו, עם:

- טעינת נתונים מ-`bank_balances_demo_5000.json`.
- חיפוש חופשי.
- סינון לפי בנק, מטבע, סוג יתרה, סטטוס וטווח סכומים.
- מיון בצד שרת.
- pagination בצד שרת.
- הצגת סיכום, גרפים וטבלה ב-Frontend.
- תמיכה בהרצה עם Docker Compose.

## מקור הנתונים

מבנה רשומה בקובץ JSON:

```json
{
  "id": 1,
  "date": "08/01/2025",
  "bankName": "דיסקונט",
  "accountNumber": "237167",
  "balanceType": "יתרת עו\"ש",
  "currency": "USD",
  "amount": 245571.18,
  "status": "פעיל"
}
```

הקובץ נטען פעם אחת לכל instance של השרת ונשמר בזיכרון כנתוני demo. אם הקובץ פגום, חסרים שדות חובה, יש תאריך לא תקין או id כפול, השרת מחזיר `ProblemDetails` וה-Frontend מציג הודעת שגיאה ידידותית.

## מבנה הפרויקט

```text
src/
  backend/
    BankDashboard.Api/
    BankDashboard.Application/
    BankDashboard.Domain/
    BankDashboard.Infrastructure/
    BankDashboard.Application.Tests/
  frontend/
    bank-dashboard/
      src/app/
        core/
        features/dashboard/
          components/
          models/
          services/
README.md
SPECIFICATION.md
docker-compose.yml
bank_balances_demo_5000.json
```

## Backend

### שכבות

- `Domain` - מודל עסקי נקי (`BankBalance`).
- `Application` - DTOs, query objects, Application services, FluentValidation.
- `Infrastructure` - קריאת JSON וטעינת נתוני demo.
- `Api` - Controllers, DI, CORS, ProblemDetails, logging.

### החלטות מרכזיות

- הוסרה תלות ב-MediatR/CQRS כי המערכת קטנה ו-read-only בשלב הנוכחי.
- ה-Controller נשאר דק ותלוי ב-`IBankBalancesQueryService`.
- DTOs נפרדים מ-Domain כדי לשמור contract יציב מול Frontend.
- `DemoDataOptions` מרכז את נתיב קובץ ה-JSON דרך configuration.
- `Lazy<Task<IReadOnlyList<BankBalance>>>` משמש לטעינה חד-פעמית, async ו-thread-safe של הנתונים.
- אין צורך בנעילות ידניות כרגע כי הנתונים נטענים פעם אחת ואינם משתנים.
- `CancellationToken` מועבר לאורך הקריאות כדי לאפשר ביטול request ו-I/O אם הלקוח התנתק.

### ולידציה ושגיאות

- FluentValidation בודק:
  - `page >= 1`
  - `pageSize` בין 1 ל-500
  - `sortBy` מתוך whitelist
  - `sortDirection` הוא `asc` או `desc`
  - `minAmount <= maxAmount`
- JSON validation בודק שדות חובה, תאריך, id חיובי וכפילויות.
- שגיאות מוחזרות כ-`ProblemDetails` עם `traceId`.
- Serilog משמש ל-structured request logging.

## Frontend

### טכנולוגיות

- Angular 20
- Standalone Components
- Signals
- RxJS לפי צורך
- TypeScript interfaces עבור חוזי API

### מבנה UI

המסך מפוצל לרכיבים:

- `summary-cards`
- `charts-panel`
- `filter-panel`
- `balance-table`

קומפוננטת `App` מחזיקה orchestration ו-state מקומי בלבד.

### חיפוש וסינון

- חיפוש חופשי מתבצע בצד שרת.
- החיפוש מפצל מילים ומחייב שכל מילה תופיע באחד משדות החיפוש.
- דוגמה: `לאומי אופציות` יתאים כאשר `לאומי` נמצא בשם הבנק ו-`אופציות` בסוג היתרה.
- ב-Frontend החיפוש נשלח אוטומטית אחרי debounce קצר; אין צורך ללחוץ "החלת סינון".
- סינון עסקי אינו מתבצע על כלל הנתונים בצד לקוח.

### Cache בצד לקוח

קיים cache קצר לפי URL ו-query params למשך 3 דקות, כדי למנוע קריאות זהות כאשר המשתמש לוחץ שוב על "החלת סינון" בלי שינוי פרמטרים.

לא נשמרים מיליוני רשומות בצד לקוח.

## API

### `GET /api/bank-balances`

פרמטרים:

- `search`
- `bankName`
- `currency`
- `balanceType`
- `status`
- `minAmount`
- `maxAmount`
- `page`
- `pageSize`
- `sortBy`
- `sortDirection`

התגובה כוללת:

- `items`
- `totalCount`
- `page`
- `pageSize`
- `totalPages`
- `hasPreviousPage`
- `hasNextPage`
- `summary`

### `GET /api/bank-balances/filters`

מחזיר ערכי סינון ייחודיים:

- `banks`
- `currencies`
- `balanceTypes`
- `statuses`

## Thread safety ו-concurrency

- השרת stateless מבחינת HTTP/session.
- יש cache פנימי בזיכרון עבור נתוני demo בלבד.
- אם כמה requests מגיעים יחד לפני סיום הטעינה הראשונה, כולם ממתינים לאותה טעינת `Lazy<Task<...>>`.
- לאחר הטעינה, הנתונים נקראים כרשימה שאינה משתנה.
- בהרבה קריאות, צוואר הבקבוק יהיה CPU בגלל filtering/sorting בזיכרון, לא קריאה חוזרת מהדיסק.

## Docker

קיימים Dockerfiles multi-stage:

- Backend: build עם .NET SDK ואז runtime עם ASP.NET image.
- Frontend: build עם Node ואז runtime עם Nginx.

`docker-compose.yml` מריץ:

- `backend` על פורט `5000`.
- `frontend` על פורט `4200`, עם proxy של `/api` ל-backend.

## בדיקות

בדיקות קיימות:

- Application service:
  - חיפוש.
  - חיפוש מרובה מילים.
  - סינון.
  - מיון.
  - pagination.
- FluentValidation.
- טעינת JSON תקין.
- דחיית JSON עם id כפול.
- Angular component tests.

פקודות:

```bash
cd src/backend
dotnet test

cd ../frontend/bank-dashboard
npm test -- --watch=false
```

## המלצות להמשך

### נתוני עתק

ה-contract הנוכחי כבר מתאים לנתונים גדולים: server-side filtering, sorting ו-pagination. עם זאת, אם הנתונים יגדלו למאות אלפים או מיליוני רשומות, JSON בזיכרון כבר לא יהיה אידיאלי.

אפשרויות:

1. להישאר עם JSON בזיכרון - מתאים לנתונים קטנים/בינוניים.
2. JSON + indexes בזיכרון - מתאים לנתונים read-only עם שדות חיפוש מוגדרים.
3. JSON Lines + streaming + cursor pagination - מתאים לקריאה קדימה, פחות למיון דינמי.
4. SQLite embedded - פתרון file-based עם indexes ו-SQL.
5. DB או search index - מומלץ לנתוני עתק והרבה משתמשים.

### חוויית משתמש

- לשקול URL state עבור פילטרים, מיון ועמוד.
- לשקול loading skeleton.
- לשקול Virtual Scroll רק אם רוצים גלילה רציפה; הוא אינו מחליף server-side pagination.
- להוסיף ביטול requests קודמים בזמן הקלדה אם החיפוש יהפוך כבד יותר.

### ביצועים ותפעול

- להוסיף OpenTelemetry ו-correlation id.
- למדוד latency, error rate ו-slow queries.
- לשקול Redis רק אם יש צורך מוכח ב-distributed cache או caching של שאילתות יקרות.
- לשקול rate limiting אם יהיו הרבה משתמשים.
- לשקול precomputed summaries אם חישובי summary יהיו יקרים.

### ארכיטקטורה

- אם יתווספו פעולות כתיבה, workflows רבים או cross-cutting behaviors מורכבים, אפשר לשקול מחדש CQRS/MediatR או מנגנון pipeline אחר.
- אם state ה-Frontend יגדל או ישותף בין מסכים, אפשר לשקול SignalStore.
- אם מקור הנתונים יהפוך דינמי או רב-מקורי, אפשר לשקול Factory או Strategy לבחירת repository.

## יומן שינויים

| תאריך | שינוי |
| --- | --- |
| 2026-05-28 | יצירת אפיון ראשוני |
| 2026-05-28 | מימוש Backend/Frontend לפי JSON |
| 2026-05-28 | הוספת Docker Compose |
| 2026-05-28 | הוספת pagination, sorting, validation ו-error handling |
| 2026-05-28 | שיפור חיפוש חופשי עם debounce |
| 2026-05-28 | הסרת MediatR ופיצול קומפוננטות Frontend |
| 2026-05-28 | קיצור האפיון וריכוז המלצות להמשך |
| 2026-05-28 | הוספת גרפים ל-Dashboard ועדכון תיעוד שימוש ב-AI |
