import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './services/auth.guard';

export const appRoutes: Routes = [
    {
        path: 'login',
        loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent),
        canActivate: [guestGuard]
    },
    {
        path: '',
        loadComponent: () => import('./components/chat/chat.component').then(m => m.ChatComponent),
        canActivate: [authGuard]
    },
    { path: '**', redirectTo: '' }
];
