# bank-nextstore

Dashboard להצגת יתרות בנק מתוך קובץ JSON דמו (`bank_balances_demo_5000.json`).

## מבנה

- `src/backend` - ASP.NET Core Web API ב-Clean Architecture.
- `src/frontend/bank-dashboard` - Angular 20 standalone app.
- `SPECIFICATION.md` - אפיון תמציתי, החלטות ארכיטקטורה והמלצות להמשך.

להמלצות המשך וגבולות הרחבה, ראו את סעיף **"המלצות להמשך"** בקובץ `SPECIFICATION.md`.

## תהליך העבודה

### שלב א' - תכנון ואפיון

בתחילת העבודה בוצע תכנון פתרון ונכתב קובץ אפיון מרכזי בשם `SPECIFICATION.md`.  
ניתנה הנחיה לעדכן את קובץ האפיון באופן דינמי לאורך כל המשימה, כדי לשמור מקור אמת מעודכן להחלטות, לשינויים ולשיקולי הארכיטקטורה.

### שלב ב' - מימוש בסיס ושאלות ארכיטקטורה

לאחר כתיבת האפיון מומש בסיס ראשוני של המערכת: Backend, Frontend, טעינת JSON, חיפוש, סינון וטבלה.  
לאחר מכן נשאלו שאלות ארכיטקטורה ודיזיין כדי לדייק את הפתרון: מה יקרה אם נרצה להרחיב לנתוני עתק, לתמוך במספר משתמשים גדול, לשנות מקור נתונים, להוסיף cache, לשנות מחלקות או להחליף תלויות.

### שלב ג' - ירידה לקוד ודיוק החלטות

לאחר שהכיוון הארכיטקטוני היה ברור יותר, בוצעה קריאה בפועל של הקבצים והקוד.  
בשלב זה נשאלו שאלות נקודתיות על חלקים שלא היו ברורים, ובוצעו התאמות בקוד: הסרת MediatR כאשר התברר שאינו נדרש לשלב read-only, פיצול קומפוננטות Frontend, הוספת ולידציות, טיפול שגיאות, cache קצר בצד לקוח ושיפור החיפוש החופשי.

### שלב ד' - הרצה ובדיקות בקליינט

המערכת הורצה ונבדקה בצד הקליינט, כולל build, unit tests, התנהגות חיפוש חופשי, debounce, pagination, וסינון.  
במהלך ההרצה נשאלו שאלות נוספות על חוויית משתמש, Virtual Scroll, cache, שגיאות, JSON פגום ותרחישי עומס.

### שלב ה' - אימות מול הדרישות

בסיום בוצעה חזרה על ההנחיות והדרישות כדי לוודא שהן בוצעו במלואן ובמדויק.  
קובץ `SPECIFICATION.md` עודכן לאורך הדרך כדי לשקף את ההחלטות הסופיות, וה-README נשאר נקודת כניסה קצרה להרצה, בדיקות ותיאור התהליך.

## הרצה עם Docker Compose

זוהי דרך ההרצה המומלצת. נדרש Docker עם Compose plugin.

```bash
docker compose up --build
```

כתובות:

- Frontend: `http://localhost:4200`
- Backend API: `http://localhost:5000/api/bank-balances`

במצב Docker, Nginx משרת את קבצי ה-Angular ומעביר קריאות `/api` לשירות ה-backend בתוך רשת ה-Compose.

## הרצה ידנית לפיתוח

### Backend

נדרש .NET 10 SDK.

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

שרת הפיתוח של Angular משתמש ב-`proxy.conf.json` ומעביר קריאות `/api` אל `http://localhost:5000`.

## API

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
- `sortBy` - אחד מהשדות `id`, `date`, `bankName`, `accountNumber`, `balanceType`, `currency`, `amount`, `status`
- `sortDirection` - `asc` או `desc`

ה-API מחזיר עמוד אחד בכל קריאה יחד עם metadata: מספר עמוד, גודל עמוד, סך תוצאות, סך עמודים, והאם יש עמוד קודם/הבא.
גם הסינון, המיון וה-pagination מתבצעים בצד שרת; בשלב הדמו מקור הנתונים נשאר JSON ונטען פעם אחת לזיכרון.
ולידציית הפרמטרים מתבצעת בשכבת Application באמצעות FluentValidation.
החיפוש החופשי תומך בכמה מילים על פני כמה שדות, למשל `לאומי אופציות`, וב-Frontend הוא נשלח אוטומטית אחרי השהיית הקלדה קצרה.

### CancellationToken

ה-Backend מעביר `CancellationToken` לאורך הקריאות האסינכרוניות.  
הוא חשוב כדי לאפשר ביטול עבודה כאשר הלקוח ניתק, request התבטל, או השרת מפסיק לעבד את הקריאה. זה שימושי במיוחד סביב I/O, טעינת קבצים, קריאות repository ופעולות שעלולות להיות יקרות בעתיד.

## בדיקות

```bash
cd src/backend
dotnet test

cd ../frontend/bank-dashboard
npm test -- --watch=false
```
