import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})
export class LoginComponent {
    private readonly auth = inject(AuthService);
    private readonly router = inject(Router);

    protected readonly username = signal('');
    protected readonly password = signal('');
    protected readonly errorMessage = signal<string | null>(null);
    protected readonly submitting = signal(false);

    protected onSubmit(event: Event): void {
        event.preventDefault();
        const username = this.username().trim();
        const password = this.password();

        if (!username || !password) {
            this.errorMessage.set('Username and password are required.');
            return;
        }

        this.errorMessage.set(null);
        this.submitting.set(true);

        this.auth.login(username, password).subscribe({
            next: () => {
                this.submitting.set(false);
                this.router.navigateByUrl('/');
            },
            error: (error: HttpErrorResponse) => {
                this.submitting.set(false);
                if (error.status === 401) {
                    this.errorMessage.set('Invalid username or password.');
                } else if (error.status === 0) {
                    this.errorMessage.set('Server is unreachable. Please try again.');
                } else {
                    this.errorMessage.set(error.error?.error || 'Login failed. Please try again.');
                }
            }
        });
    }
}
