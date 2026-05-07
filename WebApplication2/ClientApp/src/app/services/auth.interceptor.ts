import { Injectable } from '@angular/core';
import {
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpInterceptor,
    HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Add cache-control headers to prevent caching of authenticated requests
        const headers = req.headers
            .append('Cache-Control', 'no-cache, no-store, must-revalidate')
            .append('Pragma', 'no-cache')
            .append('Expires', '0');

        const authReq = req.clone({ headers });

        return next.handle(authReq).pipe(
            catchError((error: HttpErrorResponse) => {
                if (error instanceof HttpErrorResponse) {
                    switch (error.status) {
                        case 401:
                            console.error('Unauthorized: Please log in with your Windows credentials.');
                            break;
                        case 403:
                            console.error('Forbidden: You do not have permission to access this resource.');
                            break;
                        case 404:
                            console.error('Not Found: The requested resource was not found.');
                            break;
                        case 500:
                            console.error('Internal Server Error: Please try again later.');
                            break;
                        default:
                            console.error(`HTTP Error ${error.status}: ${error.message}`);
                            break;
                    }
                }
                return throwError(() => error);
            })
        );
    }
}