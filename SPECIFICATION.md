# אפיון פתרון - Bank Balances Dashboard

## מטרת המסמך

מסמך זה מרכז את אפיון הפתרון למסך Dashboard להצגת יתרות בנק, כולל חיפוש, סינון והצגת נתונים בטבלה.  
המסמך ישמש כמקור ידע מתעדכן: בכל שינוי משמעותי בקוד, במבנה, בהתנהגות או בהחלטות הטכניות - יש לעדכן גם את סעיף "יומן שינויים".

## מטרת המערכת

בניית אפליקציית Web קטנה המדגימה:

- טעינת נתוני דמו מקובץ JSON.
- הצגת יתרות בנק בטבלת Dashboard נוחה לקריאה.
- חיפוש חופשי לפי שדות מרכזיים.
- סינון לפי מאפיינים עסקיים כגון בנק, מטבע, סוג יתרה, סטטוס וטווח סכומים.
- הפרדה ברורה בין Backend, Frontend ושכבות פנימיות.
- שימוש ב-Clean Architecture ובתבניות נפוצות כגון CQRS ו-MediatR.
- מחשבה על איכות קוד, חוויית משתמש וחוויית פיתוח.

## טכנולוגיות יעד

### Backend

- .NET 10
- ASP.NET Core Web API
- Clean Architecture
- MediatR עבור CQRS ו-decoupling בין API ל-Application layer
- Dependency Injection מובנה של ASP.NET Core
- טעינת Demo Data מקובץ JSON מקומי

### Frontend

- Angular 20
- TypeScript
- Standalone Components
- Signals / RxJS בהתאם לצורך המקומי
- CSS מודולרי או SCSS פשוט

## גבולות המימוש

אין ציפייה למערכת Production מלאה. המימוש יתמקד בפתרון נקי, קריא ומדגים יכולות:

- ללא הזדהות והרשאות.
- ללא בסיס נתונים אמיתי בשלב הראשון.
- ללא persistence של שינויים מצד המשתמש.
- כולל pagination בצד שרת כבר בשלב הדמו, כדי שה-UI וה-API יהיו מוכנים לכמויות נתונים גדולות.
- ולידציה בסיסית לקלטי סינון וחיפוש.

## נתוני דמו

מקור הנתונים יהיה קובץ JSON שיצורף לפרויקט. אם מבנה הקובץ בפועל שונה, האפיון יעודכן בהתאם.

מבנה רשומת יתרת בנק בקובץ `bank_balances_demo_5000.json`:

```json
{
  "id": 1,
  "date": "08/01/2025",
  "bankName": "דיסקונט",
  "accountNumber": "237167",
  "balanceType": "יתרת עו\"ש",
  "currency": "ILS",
  "amount": 245571.18,
  "status": "פעיל"
}
```

שדות להצגה בטבלה:

- תאריך
- שם בנק
- מספר חשבון
- סוג יתרה
- מטבע
- סכום
- סטטוס

## יכולות משתמש

### Dashboard

המסך הראשי יציג:

- כותרת ותיאור קצר.
- כרטיסי Summary:
  - מספר חשבונות.
  - סכום יתרות לפי מטבע.
  - מספר בנקים ייחודיים.
  - תאריך עדכון אחרון במערכת.
- אזור חיפוש וסינון.
- טבלת יתרות עם pagination.
- מצב ריק כאשר אין תוצאות.
- הודעת שגיאה ידידותית במקרה של כשל בטעינת נתונים.

### חיפוש

חיפוש חופשי יתבצע מול:

- שם בנק
- מספר חשבון
- סוג יתרה
- מטבע
- סטטוס

החיפוש יהיה case-insensitive בצד השרת כדי לשמור על מקור אמת יחיד להתנהגות.

### סינון

סינונים מתוכננים:

- בנק
- מטבע
- סוג יתרה
- סטטוס
- סכום מינימלי
- סכום מקסימלי

### מיון

בשלב ראשון:

- מיון ברירת מחדל לפי `date` בסדר יורד ולאחר מכן `id` בסדר יורד.

הרחבה אפשרית:

- מיון לפי כל עמודה מרכזית באמצעות פרמטרים ב-API.

### Pagination

כדי לתמוך בעתיד בכמויות נתונים גדולות, ה-API לא יחזיר את כל הנתונים כברירת מחדל אלא עמוד אחד בכל קריאה.

פרמטרים:

- `page` - מספר עמוד, מתחיל ב-`1`.
- `pageSize` - כמות רשומות בעמוד, ברירת מחדל `50`, מקסימום `500`.

ה-response יכלול:

- `items` - הרשומות בעמוד הנוכחי.
- `totalCount` - סך הרשומות אחרי סינון.
- `page`
- `pageSize`
- `totalPages`
- `hasPreviousPage`
- `hasNextPage`

במקור JSON קטן ה-pagination מתבצע לאחר סינון ומיון בזיכרון. כאשר מקור הנתונים יעבור ל-DB, אותו contract יישמר אבל היישום יעבור ל-query עם `Skip`/`Take` או cursor/keyset pagination בהתאם לצורך.

## ארכיטקטורת Backend

ה-Backend יבנה לפי Clean Architecture, עם תלות פנימה בלבד:

```text
BankDashboard.Api
  -> BankDashboard.Application
      -> BankDashboard.Domain
  -> BankDashboard.Infrastructure
      -> BankDashboard.Application
      -> BankDashboard.Domain
```

### Domain Layer

אחריות:

- מודלים עסקיים נקיים.
- Value Objects אם יהיה צורך.
- כללי Domain בסיסיים שאינם תלויים בתשתית.

דוגמאות:

- `BankAccountBalance`
- `CurrencyCode`
- `Money`

### Application Layer

אחריות:

- Use cases של המערכת.
- CQRS queries.
- DTOs.
- Interfaces לשירותי תשתית.
- לוגיקת סינון, חיפוש ומיון.

דוגמאות:

- `GetBankBalancesQuery`
- `GetBankBalancesQueryHandler`
- `BankBalanceDto`
- `IBankBalanceReadRepository`

### Infrastructure Layer

אחריות:

- קריאת קובץ JSON.
- מימוש repository עבור נתוני הדמו.
- caching בזיכרון אם ידרש.

דוגמאות:

- `JsonBankBalanceReadRepository`
- `DemoDataOptions`

### API Layer

אחריות:

- הגדרת endpoints.
- תרגום HTTP requests ל-MediatR requests.
- Swagger/OpenAPI.
- טיפול אחיד בשגיאות.

דוגמאות endpoints:

```http
GET /api/bank-balances
GET /api/bank-balances/filters
```

פרמטרים ל-`GET /api/bank-balances`:

- `search`
- `bankName`
- `currency`
- `balanceType`
- `status`
- `minAmount`
- `maxAmount`
- `page`
- `pageSize`

## MediatR ו-CQRS

המערכת תשתמש ב-MediatR כדי להפריד בין שכבת ה-API לבין תרחישי השימוש:

- Controller או Minimal API יקבל request.
- ה-request יתורגם ל-Query.
- Handler בשכבת Application יבצע את הלוגיקה.
- ה-Handler יחזיר DTO מוכן ל-API.

בשלב זה נדרש בעיקר Query side, מכיוון שאין פעולות כתיבה.

## Thread Safety

המערכת קוראת נתוני דמו, ולכן אין state משתנה עסקי משמעותי. עדיין נשמור על עקרונות thread-safe:

- רשומות Domain ו-DTOs יוגדרו כ-immutable records כאשר מתאים.
- repository שמחזיק cache בזיכרון ישתמש בטעינה חד-פעמית בטוחה, לדוגמה `Lazy<T>` או נעילה ממוקדת.
- לא תתבצע מוטציה על collections משותפים לאחר הטעינה.
- Handlers יהיו stateless ככל האפשר.
- שירותים עם lifetime של Singleton יכילו רק state immutable או thread-safe.

## מבנה תיקיות מוצע

```text
src/
  backend/
    BankDashboard.sln
    BankDashboard.Api/
      Controllers/
      Extensions/
      Program.cs
      appsettings.json
    BankDashboard.Application/
      BankBalances/
        Queries/
        Dtos/
      Abstractions/
      DependencyInjection.cs
    BankDashboard.Domain/
      BankBalances/
      Common/
    BankDashboard.Infrastructure/
      Data/
      Repositories/
      Options/
      DependencyInjection.cs
  frontend/
    bank-dashboard/
      src/
        app/
          core/
          features/
            dashboard/
              components/
              models/
              services/
          shared/
        styles/
data/
  bank-balances.demo.json
docs/
  screenshots/
SPECIFICATION.md
README.md
```

## מבנה Frontend מוצע

```text
features/dashboard/
  dashboard-page.component.ts
  dashboard-page.component.html
  dashboard-page.component.scss
  components/
    balance-summary-cards/
    balance-filter-bar/
    balance-table/
  models/
    bank-balance.model.ts
    bank-balance-filter.model.ts
  services/
    bank-balances-api.service.ts
```

עקרונות:

- רכיבי UI קטנים וממוקדים.
- Service אחד לתקשורת מול ה-API.
- הפרדה בין models של Frontend לבין DTOs במקרה שיידרש mapping.
- שמירה על נגישות בסיסית: labels, focus states, טקסטים ברורים.

## חוויית משתמש

דגשים:

- Dashboard נקי עם מידע מרכזי מעל הטבלה.
- סינונים גלויים וקלים לשימוש.
- כפתור ניקוי סינונים.
- הצגת מספר תוצאות.
- עיצוב responsive בסיסי.
- תמיכה טובה בקריאת מספרים וכספים:
  - הפרדת אלפים.
  - סימון מטבע.
  - צבע עדין ליתרות שליליות אם קיימות.

## API Contract ראשוני

### Get balances

```http
GET /api/bank-balances?search=דיסקונט&currency=ILS&minAmount=0&page=1&pageSize=50
```

Response:

```json
{
  "items": [
    {
      "id": 1,
      "date": "2025-01-08",
      "bankName": "דיסקונט",
      "accountNumber": "237167",
      "balanceType": "יתרת עו\"ש",
      "currency": "ILS",
      "amount": 245571.18,
      "status": "פעיל"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 50,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false,
  "summary": {
    "totalCount": 1,
    "bankCount": 1,
    "latestDate": "2025-01-08",
    "totalAmountByCurrency": {
      "ILS": 245571.18
    }
  }
}
```

### Get filter values

```http
GET /api/bank-balances/filters
```

Response:

```json
{
  "banks": ["דיסקונט", "פועלים"],
  "currencies": ["ILS", "USD", "EUR"],
  "balanceTypes": ["יתרת עו\"ש", "מניות"],
  "statuses": ["פעיל", "לא פעיל", "חסום"]
}
```

## בדיקות

בדיקות מומלצות:

- Unit tests ל-Application query handler:
  - חיפוש.
  - סינון לפי מטבע.
  - סינון לפי סטטוס וטווח סכומים.
  - שילוב כמה סינונים.
- Unit tests לטעינת JSON תקין.
- בדיקות ידניות ל-Frontend:
  - טעינה ראשונית.
  - מצב ללא תוצאות.
  - שגיאת API.
  - responsive layout בסיסי.

## הוראות הרצה

### Backend

```bash
cd src/backend
dotnet restore
dotnet run --project BankDashboard.Api --urls http://localhost:5000
```

### Frontend

```bash
cd src/frontend/bank-dashboard
npm install
npm start
```

### כתובות צפויות

- API: `https://localhost:7001` או `http://localhost:5000`
- Frontend: `http://localhost:4200`
- OpenAPI: `/swagger` או endpoint OpenAPI מקביל בהתאם להגדרת .NET 10

### Docker Compose

```bash
docker compose up --build
```

- Frontend: `http://localhost:4200`
- Backend API: `http://localhost:5000/api/bank-balances`

ה-Frontend נבנה כקבצים סטטיים ומוגש על ידי Nginx. קריאות `/api` עוברות מ-Nginx לשירות ה-backend בתוך רשת ה-Compose.

## החלטות מימוש ויכולת הרחבה

### ריבוי קריאות במקביל

- ה-API stateless ברמת request.
- `JsonBankBalanceReadRepository` רשום כ-Singleton וטוען את קובץ ה-JSON דרך `Lazy<Task<IReadOnlyList<BankBalance>>>`.
- אם כמה קריאות מגיעות יחד לפני סיום הטעינה הראשונה, כולן ממתינות לאותה משימת טעינה במקום לבצע קריאות דיסק כפולות.
- לאחר הטעינה, הקריאות עובדות מול רשימה immutable בפועל, והסינון והמיון מתבצעים בזיכרון לכל request.
- ה-response מחזיר רק עמוד אחד של נתונים כדי למנוע עומס מיותר על הרשת ועל רינדור הטבלה.

### ביצועי קריאת הקובץ

קובץ של 5,000 רשומות הוא קטן יחסית. הקריאה האיטית ביותר היא הטעינה הראשונה מהדיסק וה-deserialization, ולאחר מכן אין קריאה חוזרת לקובץ. כבר קיים pagination בצד שרת, אך עבור כמויות עצומות מקור הנתונים יצטרך לעבור ל-DB או search/index store, כך שהסינון, המיון וה-pagination יתבצעו בשאילתה עצמה ולא בזיכרון של שרת האפליקציה.

### Factory

בשלב הנוכחי אין צורך ב-Factory: קיימת תשתית אחת ברורה לקריאת נתונים, וה-DI מחבר abstraction ל-implementation. Factory יהיה מוצדק אם יתווספו כמה מקורות נתונים, בחירה דינמית לפי tenant, קבצים שונים לפי סביבה, או אסטרטגיות parsing שונות.

### שימוש בפיצ'רים חדשים

- Angular: standalone component, `inject`, Signals, `computed`, control flow חדש עם `@if` / `@for`, ו-`provideHttpClient`.
- .NET: target ל-`.NET 10`, minimal hosting model, records immutable, primary constructors, `DateOnly`, async APIs ו-DI מובנה.

### הפרדה בין שכבות וצימוד

- `Domain` מכיל מודל עסקי נקי ללא תלות בתשתיות.
- `Application` מכיל DTOs, queries, handlers ו-abstractions.
- `Infrastructure` מממש את הקריאה מה-JSON דרך abstraction של Application.
- `Api` מכיר את Application ו-Infrastructure לצורך composition root בלבד.
- Frontend מבודד את חוזה ה-API ב-service וב-models.

הצימוד המרכזי שנותר הוא חוזה ה-DTO בין API ל-Frontend ושמות שדות ה-JSON בתוך ה-repository. זה צימוד סביר לשלב הדמו, וניתן להרחבה באמצעות mapping/versioning אם החוזה יהפוך ציבורי או יציב לאורך זמן.

## שימוש ב-AI במהלך הפיתוח

כלי AI בשימוש:

- GPT-5.5 בתוך Cursor Cloud.

אופן השימוש:

- פירוק דרישות לאפיון טכני.
- תכנון מבנה Clean Architecture.
- בחירת תבניות מתאימות כגון CQRS ו-MediatR.
- ניסוח שיקולי thread-safety.
- בהמשך: סיוע בכתיבת קוד, בדיקות, README והוראות הרצה.

## החלטות פתוחות

- האם להוסיף sorting דינמי לפי עמודות.
- האם להעביר את כתובת ה-API ב-Frontend לקובץ environment/config במקום proxy יחסי.

## יומן שינויים

| תאריך | שינוי | סיבה |
| --- | --- | --- |
| 2026-05-28 | יצירת אפיון ראשוני | תכנון Dashboard ליתרות בנק לפי דרישות המטלה, כולל Clean Architecture, MediatR, Angular 20 ו-.NET 10 |
| 2026-05-28 | תחילת מימוש Backend ו-Frontend לפי קובץ JSON בפועל | התאמת האפיון לשדות `date`, `balanceType`, `amount`, `status`; הוספת API, repository, queries, בדיקות יחידה ראשוניות ומסך Dashboard |
| 2026-05-28 | הוספת Docker ותיעוד החלטות הרחבה | תמיכה בהרצה מלאה עם Docker Compose והבהרת concurrency, factory, שימוש בפיצ'רים חדשים והפרדה בין שכבות |
| 2026-05-28 | הוספת pagination בצד שרת ובמסך | הכנת חוזה ה-API וה-UI לכמויות נתונים גדולות והפחתת עומס רינדור/רשת |
