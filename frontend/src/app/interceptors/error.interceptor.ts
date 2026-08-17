import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export function extractErrorMessage(err: HttpErrorResponse): string {
  if (err.status === 0) {
    return 'Não foi possível conectar ao servidor. Verifique se os serviços estão em execução.';
  }

  if (err.error?.errors) {
    return Object.values(err.error.errors as Record<string, string[]>).flat().join(' ');
  }

  if (typeof err.error === 'string' && err.error.trim().length > 0) {
    return err.error;
  }

  return 'Ocorreu um erro. Tente novamente.';
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      snackBar.open(extractErrorMessage(err), 'Fechar', { duration: 5000 });
      return throwError(() => err);
    })
  );
};
