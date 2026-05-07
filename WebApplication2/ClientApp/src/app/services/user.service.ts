import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserInfo {
    name?: string;
    identityName?: string;
    isAuthenticated: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private readonly userInfoUrl = '/api/user/info';
    
    private readonly userInfoSignal = signal<UserInfo | null>(null);
    
    public readonly userInfo = computed(() => this.userInfoSignal());
    public readonly userName = computed(() => this.userInfoSignal()?.name || 'Anonymous');
    public readonly isAuthenticated = computed(() => this.userInfoSignal()?.isAuthenticated ?? false);

    constructor(private http: HttpClient) {
        this.loadUserInfo();
    }

    loadUserInfo(): void {
        this.getUserInfo().subscribe({
            next: (info) => this.userInfoSignal.set(info),
            error: (error) => {
                console.error('Failed to load user info:', error);
                this.userInfoSignal.set({ isAuthenticated: false });
            }
        });
    }

    getUserInfo(): Observable<UserInfo> {
        return this.http.get<UserInfo>(this.userInfoUrl);
    }

    refresh(): void {
        this.loadUserInfo();
    }
}