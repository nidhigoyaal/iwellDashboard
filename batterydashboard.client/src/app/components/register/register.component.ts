import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { Register } from 'src/app/shared/account.models';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  registerData: Register = {
    UserEmail: '',
    UserName: '',
    Password: '',
    Role: 'User'
  };

  loading: boolean = false;
  error: string | null = null;
  success: string | null = null;
  showPassword: boolean = false;

  constructor(private auth: AuthService, private router: Router) {
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  onSubmit() {
    if (!this.registerData.UserEmail || !this.registerData.Password) { this.error = 'Provide email and password'; return; }
    console.log(this.registerData)
    this.loading = true;
    this.auth.register(this.registerData).subscribe({
      next: () => {
        this.loading = false;
        this.success = 'Registered. Please login.';
        setTimeout(() => this.router.navigate(['/login']), 900);
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.message || 'Registration failed';
      }
    });
  }
}
