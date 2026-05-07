import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface UserProfile {
    id: string;
    username: string;
    displayName: string;
    email: string;
    roles: string[];
}

interface LoginResponse {
    accessToken: string;
    tokenType: string;
    expiresIn: number;
    user: UserProfile;
}

const TOKEN_STORAGE_KEY = 'auth.token';
const USER_STORAGE_KEY = 'auth.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly router = inject(Router);

    private readonly tokenSignal = signal<string | null>(this.readToken());
    private readonly userSignal = signal<UserProfile | null>(this.readUser());

    readonly token = computed(() => this.tokenSignal());
    readonly user = computed(() => this.userSignal());
    readonly isAuthenticated = computed(() => !!this.tokenSignal());

    login(username: string, password: string): Observable<LoginResponse> {
        return this.http
            .post<LoginResponse>('/api/auth/login', { username, password })
            .pipe(tap(response => this.applyLogin(response)));
    }

    logout(redirect: boolean = true): void {
        this.tokenSignal.set(null);
        this.userSignal.set(null);
        try {
            localStorage.removeItem(TOKEN_STORAGE_KEY);
            localStorage.removeItem(USER_STORAGE_KEY);
        } catch {
            // ignore storage failures (e.g. private mode)
        }
        if (redirect) {
            this.router.navigate(['/login']);
        }
    }

    private applyLogin(response: LoginResponse): void {
        this.tokenSignal.set(response.accessToken);
        this.userSignal.set(response.user);
        try {
            localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken);
            localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(response.user));
        } catch {
            // ignore storage failures (e.g. private mode)
        }
    }

    private readToken(): string | null {
        try {
            return localStorage.getItem(TOKEN_STORAGE_KEY);
        } catch {
            return null;
        }
    }

    private readUser(): UserProfile | null {
        try {
            const raw = localStorage.getItem(USER_STORAGE_KEY);
            return raw ? (JSON.parse(raw) as UserProfile) : null;
        } catch {
            return null;
        }
    }
}
