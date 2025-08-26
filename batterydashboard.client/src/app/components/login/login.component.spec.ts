import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [ FormsModule ],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle password visibility', () => {
    expect(component.showPassword).toBeFalse();
    component.togglePassword();
    expect(component.showPassword).toBeTrue();
    component.togglePassword();
    expect(component.showPassword).toBeFalse();
  });

  it('should set error if email or password missing', () => {
    component.loginData = { UserEmail: '', UserName: '', Password: '' };
    component.onSubmit();
    expect(component.error).toBe('Enter username + password');
  });

  it('should call authService.login and navigate on success', () => {
    component.loginData = { UserEmail: 'test@test.com', UserName: 'Test', Password: '1234' };
    authServiceSpy.login.and.returnValue(of({ token: 'fake-token' }));

    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalledWith(component.loginData);
    expect(component.loading).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard']);
    expect(component.error).toBeNull();
  });

  it('should set error on login failure', () => {
    component.loginData = { UserEmail: 'test@test.com', UserName: 'Test', Password: '1234' };
    authServiceSpy.login.and.returnValue(throwError(() => ({ error: { message: 'Invalid credentials' } })));

    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalled();
    expect(component.loading).toBeFalse();
    expect(component.error).toBe('Invalid credentials');
  });
});
