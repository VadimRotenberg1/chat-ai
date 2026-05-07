import { Injectable, signal, computed, inject, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService, UserProfile } from './auth.service';

@Injectable({ providedIn: 'root' })
export class UserService {
    private readonly http = inject(HttpClient);
    private readonly auth = inject(AuthService);

    private readonly userInfoUrl = '/api/user/info';
    private readonly userInfoSignal = signal<UserProfile | null>(null);

    readonly userInfo = computed(() => this.userInfoSignal() ?? this.auth.user());
    readonly userName = computed(() => this.userInfo()?.displayName || this.userInfo()?.username || 'Anonymous');
    readonly isAuthenticated = computed(() => this.auth.isAuthenticated());

    constructor() {
        // Reload the profile from the server whenever the auth token changes.
        effect(() => {
            if (this.auth.isAuthenticated()) {
                this.loadUserInfo();
            } else {
                this.userInfoSignal.set(null);
            }
        });
    }

    loadUserInfo(): void {
        this.getUserInfo().subscribe({
            next: (info) => this.userInfoSignal.set(info),
            error: (error) => {
                console.error('Failed to load user info:', error);
                this.userInfoSignal.set(null);
            }
        });
    }

    getUserInfo(): Observable<UserProfile> {
        return this.http.get<UserProfile>(this.userInfoUrl);
    }

    refresh(): void {
        this.loadUserInfo();
    }
}
