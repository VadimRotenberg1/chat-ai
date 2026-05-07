import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';

@Component({
    selector: 'app-header',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.css']
})
export class HeaderComponent {
    protected readonly userService = inject(UserService);
    protected readonly userName = this.userService.userName;
    protected readonly isAuthenticated = this.userService.isAuthenticated;

    protected onRefresh(): void {
        this.userService.refresh();
    }

    protected getInitials(name: string): string {
        if (!name) return '?';
        const parts = name.split(/[\s.]+/).filter(Boolean);
        if (parts.length === 0) return '?';
        if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
        return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }
}
