import { Injectable, inject } from '@angular/core';
import {
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpInterceptor,
    HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    private readonly auth = inject(AuthService);

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        let headers = req.headers
            .set('Cache-Control', 'no-cache, no-store, must-revalidate')
            .set('Pragma', 'no-cache')
            .set('Expires', '0');

        const token = this.auth.token();
        if (token && !req.url.endsWith('/api/auth/login')) {
            headers = headers.set('Authorization', `Bearer ${token}`);
        }

        const authReq = req.clone({ headers });

        return next.handle(authReq).pipe(
            catchError((error: HttpErrorResponse) => {
                if (error instanceof HttpErrorResponse && error.status === 401) {
                    this.auth.logout();
                }
                return throwError(() => error);
            })
        );
    }
}
