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
- שימוש ב-Clean Architecture עם Application services ו-DTOs.
- מחשבה על איכות קוד, חוויית משתמש וחוויית פיתוח.

## טכנולוגיות יעד

### Backend

- .NET 10
- ASP.NET Core Web API
- Clean Architecture
- Application services עבור הפרדה בין API ל-Application layer
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

החיפוש החופשי מפצל את הטקסט למילים ומחייב שכל מילה תימצא באחד משדות החיפוש. כך למשל `לאומי אופציות` יתאים לרשומה שבה `bankName` הוא `לאומי` ו-`balanceType` הוא `אופציות`, גם אם הביטוי המלא לא מופיע בשדה אחד.

ב-Frontend החיפוש החופשי מתבצע אוטומטית עם debounce קצר לאחר עצירה בהקלדה. אין צורך ללחוץ על "החלת סינון" עבור שדה החיפוש, ויש חיווי במסך שהחיפוש מתבצע אוטומטית.

### סינון

סינונים מתוכננים:

- בנק
- מטבע
- סוג יתרה
- סטטוס
- סכום מינימלי
- סכום מקסימלי

### מיון

- מיון ברירת מחדל לפי `date` בסדר יורד ולאחר מכן `id` בסדר יורד.
- מיון בצד שרת לפי whitelist של שדות: `id`, `date`, `bankName`, `accountNumber`, `balanceType`, `currency`, `amount`, `status`.
- כיוון מיון נתמך: `asc` או `desc`.

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

במקור JSON קטן הסינון, המיון וה-pagination מתבצעים לאחר טעינה לזיכרון. כאשר מקור הנתונים יעבור ל-DB, אותו contract יישמר אבל היישום יעבור ל-query עם `Where`, `OrderBy`, `Skip`/`Take` או cursor/keyset pagination בהתאם לצורך.

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
- Query objects ו-Application services.
- DTOs.
- Interfaces לשירותי תשתית.
- לוגיקת סינון, חיפוש ומיון.

דוגמאות:

- `GetBankBalancesQuery`
- `BankBalancesQueryService`
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
- תרגום HTTP requests ל-query objects והעברתם ל-Application services.
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
- `sortBy`
- `sortDirection`

## Application Services

המערכת משתמשת ב-Application services כדי להפריד בין שכבת ה-API לבין תרחישי השימוש:

- Controller או Minimal API יקבל request.
- ה-request יתורגם ל-Query.
- Service בשכבת Application יבצע ולידציה, סינון, מיון ו-pagination.
- ה-Service יחזיר DTO מוכן ל-API.

MediatR ו-CQRS מלא הוסרו מהשלב הנוכחי מכיוון שהמערכת קטנה, read-only, וללא פעולות כתיבה. ההפרדה עדיין נשמרת דרך `IBankBalancesQueryService`, DTOs ו-abstractions. אם בעתיד יתווספו workflows רבים, commands, cross-cutting behaviors מורכבים או פעולות כתיבה, ניתן לשקול מחדש שימוש ב-MediatR או מנגנון pipeline אחר.

## Thread Safety

המערכת קוראת נתוני דמו, ולכן אין state משתנה עסקי משמעותי. עדיין נשמור על עקרונות thread-safe:

- רשומות Domain ו-DTOs יוגדרו כ-immutable records כאשר מתאים.
- repository שמחזיק cache בזיכרון ישתמש בטעינה חד-פעמית בטוחה, לדוגמה `Lazy<T>` או נעילה ממוקדת.
- לא תתבצע מוטציה על collections משותפים לאחר הטעינה.
- Application services יהיו stateless ככל האפשר.
- שירותים עם lifetime של Singleton יכילו רק state immutable או thread-safe.
- אין צורך בנעילות ידניות סביב הקריאה השוטפת כי הנתונים נטענים פעם אחת דרך `Lazy` ולאחר מכן הרשימה אינה משתנה.
- אם בעתיד יתווסף state משתנה, רענון cache, או עדכון נתונים בזמן ריצה, יהיה צורך לבחון מנגנון locking, reader/writer lock, immutable snapshot replacement או distributed cache מתאים.

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
  components/
    summary-cards/
    filter-panel/
    balance-table/
  models/
    bank-balance.model.ts
    bank-balance-filter.model.ts
    sort-option.model.ts
  services/
    bank-balances-api.service.ts
```

עקרונות:

- רכיבי UI קטנים וממוקדים.
- Service אחד לתקשורת מול ה-API.
- קומפוננטת root מחזיקה orchestration ו-state מקומי, אך תצוגת summary, filter panel וטבלה מפוצלות לקומפוננטות ייעודיות.
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
GET /api/bank-balances?search=דיסקונט&currency=ILS&minAmount=0&page=1&pageSize=50&sortBy=date&sortDirection=desc
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

### סיכום החלטות ארכיטקטורה שעלו במהלך הסקירה

סעיף זה מרכז את השאלות המרכזיות שעלו במהלך האפיון והמימוש, ואת ההחלטות הנוכחיות:

- כן משתמשים ב-DTOs כדי להפריד בין מודל Domain פנימי לבין חוזה API חיצוני.
- לא משתמשים כרגע ב-MediatR/CQRS; למערכת read-only קטנה Application service ישיר מספיק וברור יותר.
- כן השרת נשאר stateless מבחינת HTTP/session, למרות שיש cache קריאה בזיכרון עבור נתוני ה-JSON.
- כן קיימת ולידציה בצד שרת באמצעות FluentValidation עבור query parameters, ובנפרד קיימת ולידציית תקינות לנתוני JSON.
- כן הסינון, המיון וה-pagination מתבצעים בצד שרת. בצד לקוח לא מבצעים סינון עסקי על כלל הנתונים.
- כן קיים cache קצר בצד לקוח לקריאות API זהות, כדי שלחיצה חוזרת על "החלת סינון" בלי שינוי פרמטרים לא תשלח request נוסף במשך כמה דקות.
- כן יש ניהול שגיאות אחיד: ProblemDetails בצד שרת ו-HTTP interceptor בצד Angular.
- כן יש קובצי environment בצד Angular עבור `apiBaseUrl`.
- כן Dockerfiles הם multi-stage.
- כן יש logging אחיד בצד שרת באמצעות Serilog.
- לא משתמשים כרגע ב-SignalStore; Signals מקומיים מספיקים למסך יחיד.
- לא נדרש Redis בשלב הנוכחי; הוא רלוונטי רק אם מדידה תראה צורך ב-distributed cache או caching של שאילתות יקרות.
- לא קוראים את קובץ ה-JSON בצורה מקבילית; הקובץ נטען פעם אחת בצורה thread-safe.
- לא חייבים DB כדי לבצע pagination על JSON, אבל עבור מיליוני רשומות עם sorting/filtering/concurrency גבוה DB או index ייעודי יהיו עדיפים.

### ריבוי קריאות במקביל

- ה-API stateless ברמת request.
- `JsonBankBalanceReadRepository` רשום כ-Singleton וטוען את קובץ ה-JSON דרך `Lazy<Task<IReadOnlyList<BankBalance>>>`.
- אם כמה קריאות מגיעות יחד לפני סיום הטעינה הראשונה, כולן ממתינות לאותה משימת טעינה במקום לבצע קריאות דיסק כפולות.
- לאחר הטעינה, הקריאות עובדות מול רשימה immutable בפועל, והסינון והמיון מתבצעים בזיכרון לכל request.
- ה-response מחזיר רק עמוד אחד של נתונים כדי למנוע עומס מיותר על הרשת ועל רינדור הטבלה.

המשמעות היא שהשרת stateless מבחינת התנהגות API: אין session, אין שמירת מצב משתמש, אין תלות בסדר קריאות, ואין persistence של בחירות UI. ה-cache בזיכרון הוא optimization פנימי של נתוני demo בלבד. אם מריצים כמה instances, כל instance יטען לעצמו cache נפרד; אם בעתיד צריך cache משותף בין instances, אפשר לשקול Redis או distributed cache.

במצב של הרבה קריאות במקביל:

- אין קריאת דיסק חוזרת לכל request.
- הטעינה הראשונה מתבצעת פעם אחת בלבד.
- כל request עדיין מבצע filtering/sorting/pagination בזיכרון.
- צוואר הבקבוק האפשרי הוא CPU וזיכרון, לא I/O של הקובץ.
- עבור עומסים גבוהים או מיליוני רשומות, יש להעביר את פעולת השאילתה ל-DB/search index או לבנות indexes ייעודיים סביב JSON.

### ביצועי קריאת הקובץ

קובץ של 5,000 רשומות הוא קטן יחסית. הקריאה האיטית ביותר היא הטעינה הראשונה מהדיסק וה-deserialization, ולאחר מכן אין קריאה חוזרת לקובץ. כבר קיים pagination בצד שרת, אך עבור כמויות עצומות מקור הנתונים יצטרך לעבור ל-DB או search/index store, כך שהסינון, המיון וה-pagination יתבצעו בשאילתה עצמה ולא בזיכרון של שרת האפליקציה.

לא מומלץ לקרוא את אותו קובץ JSON בצורה מקבילית עבור אותו dataset:

- JSON array רגיל אינו מתאים טוב לחלוקה פשוטה למקטעים מקביליים כי צריך להבין גבולות אובייקטים.
- קריאה מקבילית מאותו קובץ עלולה להגדיל מורכבות בלי לשפר משמעותית ביצועים.
- עבור 5,000 רשומות העלות זניחה.
- עבור קבצים גדולים יותר עדיף לשקול JSON Lines, indexing, SQLite/DB, או טעינה batch עם preprocessing.

אם חייבים להישאר עם JSON בכמויות גדולות, קיימות כמה רמות פתרון:

1. JSON נטען לזיכרון - מתאים לנתונים קטנים/בינוניים.
2. JSON עם indexes בזיכרון - מתאים לנתונים read-only עם שדות סינון/מיון מוגדרים מראש.
3. JSON Lines + streaming + cursor pagination - מתאים לקריאה קדימה ולסדר טבעי של קובץ, פחות מתאים למיון דינמי.
4. SQLite כ-embedded database - עדיין file-based, אבל מאפשר indexes, SQL ו-pagination יעיל יותר.

לכן pagination על JSON אפשרי, אך השילוב של filtering, sorting, total count, summary והרבה משתמשים על מיליוני רשומות מצדיק בדרך כלל DB או index store.

### `Lazy<Task<IReadOnlyList<BankBalance>>>`

ה-repository משתמש ב:

```csharp
private readonly Lazy<Task<IReadOnlyList<BankBalance>>> _balances;
```

המטרה:

- טעינה רק כאשר מגיעה הקריאה הראשונה שדורשת נתונים.
- טעינה פעם אחת בלבד לכל instance של השרת.
- thread-safety מובנה דרך `LazyThreadSafetyMode.ExecutionAndPublication`.
- תמיכה בטעינה async של קובץ ו-deserialization.
- מניעת מצב שבו כמה requests ראשונים קוראים את אותו קובץ במקביל.

חלופה אפשרית היא טעינה בזמן startup או hosted service. בשלב הדמו, Lazy מתאים כי הוא פשוט, יעיל, ולא מאריך startup ללא צורך.

### Cache בצד שרת ובצד לקוח

#### Cache בצד שרת

בשלב הנוכחי כן נשמר cache בצד שרת:

- קובץ JSON נטען פעם אחת לזיכרון.
- כל המשתמשים נהנים מאותה טעינה.
- לא מעבירים את כל 5,000 הרשומות לכל לקוח.

עבור מיליוני רשומות, cache מלא בזיכרון עלול להיות בעייתי מבחינת RAM ו-CPU. במצב כזה עדיף:

- DB עם indexes.
- search/index store.
- cache חלקי לפי שאילתות נפוצות עם TTL.
- precomputed summaries.
- Redis רק אם יש צורך מוכח ב-distributed cache או עומס שאילתות חוזר.

#### Cache בצד לקוח

בצד לקוח לא נשמור את כלל dataset. כן נשמר cache קצר של response לפי URL ו-query params למשך 3 דקות, כדי למנוע קריאות כפולות כאשר המשתמש לוחץ שוב על "החלת סינון" בלי שינוי פרמטרים.

בנוסף אפשר לשמור:

- filter options כגון בנקים, מטבעות, סוגי יתרה וסטטוסים.
- העמוד האחרון שנטען.
- מצב UI כמו פילטרים, עמוד, גודל עמוד ומיון.
- state ב-URL כדי לאפשר שיתוף וחזרה לאותו מצב.

לא מומלץ לשמור בצד לקוח מיליוני רשומות או לבצע client-side filtering על כלל הנתונים.

### TypeScript interfaces בצד לקוח

מודלי ה-Frontend מוגדרים כ-`interface` משום שהם מייצגים contract של JSON שמגיע מה-API ואינם צריכים runtime behavior.

יתרונות:

- compile-time type safety.
- אין יצירת class runtime מיותרת.
- התאמה טבעית ל-DTOs שמגיעים מ-HTTP.
- קל להרחיב ולבצע type checking ב-TypeScript.

אם בעתיד יידרש behavior, validation runtime או methods על המודל, ניתן לעבור ל-class או להוסיף mapping layer.

### Factory

בשלב הנוכחי אין צורך ב-Factory: קיימת תשתית אחת ברורה לקריאת נתונים, וה-DI מחבר abstraction ל-implementation. Factory יהיה מוצדק אם יתווספו כמה מקורות נתונים, בחירה דינמית לפי tenant, קבצים שונים לפי סביבה, או אסטרטגיות parsing שונות.

### `DemoDataOptions`

`DemoDataOptions` הוא מימוש של Options pattern ב-ASP.NET Core. הוא מרכז את הגדרת מקור נתוני הדמו:

```json
"DemoData": {
  "FilePath": "data/bank_balances_demo_5000.json"
}
```

הסיבה לשימוש בו:

- לא לקודד path לקובץ ישירות בתוך repository.
- לאפשר הגדרה שונה בין development, Docker, tests או סביבות אחרות.
- לשמור על repository תלוי ב-configuration abstraction ולא בערכים hard-coded.

### ולידציות ושגיאות

- ולידציית query parameters מתבצעת בשכבת Application באמצעות FluentValidation: טווח סכומים, `page`, `pageSize`, `sortBy`, `sortDirection`.
- ה-API Controller נשאר דק ומתרגם HTTP request ל-query בלבד.
- ולידציית מבנה JSON מתבצעת ב-`JsonBankBalanceReadRepository`: שדות חובה, פורמט תאריך, id חיובי וזיהוי כפילויות id.
- שגיאות שרת מוחזרות כ-ProblemDetails עם `traceId`.
- בצד הלקוח קיים HTTP interceptor שמרכז טיפול בשגיאות API ומייצר הודעה ידידותית למשתמש.
- אם קובץ ה-JSON פגום, חסרים בו שדות חובה, יש תאריך לא תקין או id כפול, השרת מחזיר ProblemDetails עם הכותרת `Demo data is invalid` והלקוח מציג הודעת שגיאה ידידותית.

חלוקת אחריות:

- ולידציית request שייכת ל-Application layer כי זו לוגיקה של use case.
- ולידציית JSON שייכת ל-Infrastructure כי היא קשורה לאיכות מקור הנתונים.
- ולידציית UI בסיסית יכולה לשפר חוויית משתמש, אך אינה מחליפה ולידציה בשרת.

### DTO מול Domain

קיום `Domain` וגם `DTO` אינו כפילות אלא הפרדה מכוונת:

- `Domain` מייצג מודל עסקי פנימי, בלתי תלוי ב-HTTP, JSON או Angular.
- `DTO` מייצג contract חיצוני של API מול ה-Frontend.

בשלב הדמו השדות דומים, אבל ההפרדה מאפשרת:

- שינוי מודל פנימי בלי לשבור לקוחות.
- הסתרת שדות פנימיים.
- הוספת calculated fields.
- versioning עתידי של API.
- שמירה על גבולות Clean Architecture.

### MediatR ו-CQRS

MediatR אינו נדרש בשלב הנוכחי והוסר מהקוד. הסיבות:

- אין פעולות כתיבה.
- יש use case מרכזי אחד של query.
- FluentValidation ניתן להרצה ישירות מתוך Application service.
- הסרה מצמצמת תלות חיצונית ורעש רישוי.
- Controller עדיין נשאר דק כי הוא תלוי ב-`IBankBalancesQueryService`.

אם בעתיד יתווספו commands, workflows רבים, pipeline behaviors מורכבים או צורך ב-dispatching אחיד בין use cases רבים, ניתן לשקול מחדש MediatR או חלופה דומה. בשלב הנוכחי הוא היה מיועד בעיקר להמשך ולכן הוסר.

### לוגים

- השרת משתמש ב-Serilog ל-structured logging.
- כל request נרשם דרך `UseSerilogRequestLogging`.
- טעינת קובץ ה-JSON נרשמת עם נתיב וכמות רשומות.

הרחבה מומלצת ל-production:

- correlation id אחיד בין Frontend, Nginx ו-Backend.
- OpenTelemetry ל-tracing.
- מדדים כגון latency, error rate, request count ו-slow queries.
- לוגים מובנים גם בצד Frontend לשגיאות משמעותיות, בלי לחשוף מידע רגיש.

### קונפיגורציה

- ה-Frontend משתמש בקובצי `environment.ts` / `environment.development.ts` עבור `apiBaseUrl`.
- ב-Docker ובפיתוח מקומי `apiBaseUrl` נשאר יחסי (`/api`) כדי לאפשר proxy דרך Angular dev server או Nginx.

### שימוש בפיצ'רים חדשים

- Angular: standalone component, `inject`, Signals, `computed`, control flow חדש עם `@if` / `@for`, ו-`provideHttpClient`.
- .NET: target ל-`.NET 10`, minimal hosting model, records immutable, primary constructors, `DateOnly`, async APIs ו-DI מובנה.

### Signals, SignalStore ו-state בצד לקוח

בשלב הנוכחי נעשה שימוש ב-Angular Signals מקומיים בתוך component:

- רשימת יתרות.
- summary.
- loading/error.
- pagination state.
- computed עבור סכומי מטבעות.

לא נעשה שימוש ב-SignalStore כרגע כי state המסך עדיין מקומי ופשוט. SignalStore יהיה רלוונטי אם:

- כמה מסכים ישתפו את אותם נתונים.
- יתווסף client-side cache מורכב.
- תהיה שמירת state ב-URL עם flows מורכבים.
- יתווספו selection, bulk actions, permissions או workflows נוספים.

### Virtual Scroll וחוויית משתמש לנתוני עתק

Virtual Scroll אינו תחליף ל-server-side pagination/filtering/sorting. הוא פותר בעיקר את בעיית רינדור DOM בדפדפן.

בשלב הנוכחי pagination מספיק כי ה-UI מציג עמוד מוגבל של רשומות. Virtual Scroll יהיה מתאים אם רוצים חוויית גלילה רציפה במקום pagination קלאסי, בתנאי שהנתונים נטענים מהשרת במנות קטנות.

המלצות UX לנתוני עתק:

- server-side filtering/sorting/pagination.
- debounce לחיפוש.
- ביטול requests קודמים בזמן הקלדה.
- loading skeleton או מצב טעינה ברור.
- שמירת state ב-URL.
- הצגת פילטרים פעילים.
- export אסינכרוני במקום הורדת מיליוני רשומות דרך endpoint הטבלה.
- לשקול cursor/keyset pagination עבור datasets גדולים ומשתנים.

### ניהול תלויות

- התלויות נוספו דרך package managers (`dotnet add package`, Angular CLI/npm) כדי לקבל גרסאות עדכניות.
- MediatR הוסר כדי לצמצם תלות ורישוי שאינם נדרשים בשלב read-only קטן.

ספריות מרכזיות:

- Backend: ASP.NET Core, FluentValidation, Serilog.
- Frontend: Angular, RxJS, TypeScript.

יש להמשיך לבדוק עדכניות, רישוי ופגיעויות אבטחה לפני production.

### Redis

Redis לא נדרש בשלב הנוכחי:

- הנתונים נטענים מ-JSON פעם אחת לכל instance.
- dataset הדמו קטן.
- אין session server-side.
- אין multi-instance shared cache.
- אין מדידה שמראה bottleneck שמצריך distributed cache.

Redis יהיה רלוונטי אם:

- יש כמה instances וצריך cache משותף.
- יש שאילתות חוזרות ויקרות עם אותם פילטרים.
- summary/aggregations יקרים.
- יש צורך ב-rate limiting מבוזר.
- יש צורך ב-pub/sub או invalidation בין instances.

### Docker

קיימים Dockerfiles multi-stage:

- Backend: build עם .NET SDK ואז runtime עם ASP.NET image.
- Frontend: build עם Node ואז runtime עם Nginx.

`docker-compose.yml` מריץ שני שירותים:

- `backend` על פורט `5000`.
- `frontend` על פורט `4200`, כאשר Nginx מעביר `/api` לשירות backend.

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
- בחירת תבניות מתאימות כגון Application services, DTOs ו-FluentValidation.
- ניסוח שיקולי thread-safety.
- בהמשך: סיוע בכתיבת קוד, בדיקות, README והוראות הרצה.

## החלטות פתוחות

- האם לעבור בעתיד ל-cursor/keyset pagination במקום page/pageSize.
- האם להוסיף OpenTelemetry להפצת trace בין Frontend, Nginx ו-Backend.
- האם להוסיף SignalStore כאשר state ה-Frontend יתרחב מעבר למסך יחיד.
- האם להוסיף Redis לאחר מדידה של עומסי שאילתות או צורך ב-distributed cache.
- האם להישאר עם JSON + indexes, לעבור ל-SQLite embedded, או לעבור ל-DB/search index כאשר הנתונים יגדלו למיליוני רשומות.

## יומן שינויים

| תאריך | שינוי | סיבה |
| --- | --- | --- |
| 2026-05-28 | יצירת אפיון ראשוני | תכנון Dashboard ליתרות בנק לפי דרישות המטלה, כולל Clean Architecture, MediatR, Angular 20 ו-.NET 10 |
| 2026-05-28 | תחילת מימוש Backend ו-Frontend לפי קובץ JSON בפועל | התאמת האפיון לשדות `date`, `balanceType`, `amount`, `status`; הוספת API, repository, queries, בדיקות יחידה ראשוניות ומסך Dashboard |
| 2026-05-28 | הוספת Docker ותיעוד החלטות הרחבה | תמיכה בהרצה מלאה עם Docker Compose והבהרת concurrency, factory, שימוש בפיצ'רים חדשים והפרדה בין שכבות |
| 2026-05-28 | הוספת pagination בצד שרת ובמסך | הכנת חוזה ה-API וה-UI לכמויות נתונים גדולות והפחתת עומס רינדור/רשת |
| 2026-05-28 | הוספת sorting, ולידציות JSON, interceptor ולוגים אחידים | חיזוק חוויית תפעול ופיתוח תוך שמירה על מקור נתונים JSON בשלב הדמו |
| 2026-05-28 | מעבר ל-FluentValidation עבור query validation | ריכוז חוקי ולידציה בשכבת Application ושמירה על Controller דק |
| 2026-05-28 | ריכוז החלטות ארכיטקטורה מהשיחה | תיעוד MediatR, stateless/cache, Domain מול DTO, Lazy loading, Redis, SignalStore, Virtual Scroll, JSON במיליוני רשומות ו-Docker |
| 2026-05-28 | שיפור חיפוש חופשי | תמיכה בחיפוש מרובה מילים על פני כמה שדות והוספת חיפוש אוטומטי עם debounce בצד הלקוח |
| 2026-05-28 | פישוט ארכיטקטורה ופיצול Frontend | הסרת MediatR/CQRS שאינם נדרשים בשלב read-only, מעבר ל-Application service, פיצול קומפוננטות UI, הוספת cache קצר בצד לקוח ותיעוד JSON פגום |
