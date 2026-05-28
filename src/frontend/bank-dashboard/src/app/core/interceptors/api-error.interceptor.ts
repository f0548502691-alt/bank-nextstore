import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: unknown) => {
      const message = toUserMessage(error);
      console.error('API request failed', { url: request.urlWithParams, error });

      return throwError(() => new Error(message));
    })
  );

function toUserMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'אירעה שגיאה לא צפויה. נסו שוב מאוחר יותר.';
  }

  if (error.status === 0) {
    return 'לא ניתן להתחבר לשרת. בדקו שה-API פעיל ונסו שוב.';
  }

  if (error.status === 400) {
    return extractProblemDetailsMessage(error) ?? 'אחד מערכי החיפוש או הסינון אינו תקין.';
  }

  return extractProblemDetailsMessage(error) ?? 'לא ניתן לטעון את הנתונים כרגע. נסו שוב מאוחר יותר.';
}

function extractProblemDetailsMessage(error: HttpErrorResponse): string | null {
  const body = error.error as { detail?: string; title?: string } | null;
  return body?.detail ?? body?.title ?? null;
}
