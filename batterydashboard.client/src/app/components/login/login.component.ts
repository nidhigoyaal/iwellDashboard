import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { Login } from 'src/app/Models/login.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  loginData: Login = { UserEmail: '', UserName: '' , Password: ''};
  loading: boolean = false;
  error: string | null = null;
  showPassword: boolean = false;

  constructor( private auth: AuthService, private router: Router) {
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  onSubmit() {
    this.error = null;
    if (!this.loginData.UserEmail || !this.loginData.Password) { this.error = 'Enter username + password'; return; }

    this.loading = true;
    this.auth.login(this.loginData).subscribe({
      next: res => {
        this.loading = false;
        // success: navigate to dashboard
        this.router.navigate(['/dashboard']);
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message || 'Login failed';
      }
    });
  }
}
