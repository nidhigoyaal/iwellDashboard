import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';


describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;
  const toastrSpy = jasmine.createSpyObj('ToastrService', ['error']);

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['register']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      declarations: [RegisterComponent],
      imports: [FormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ToastrService, useValue: toastrSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
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
    component.registerData = { UserEmail: '', UserName: '', Password: '', Role: 'User' };
    component.onSubmit();
    expect(component.error).toBe('Provide email and password');
    expect(component.loading).toBeFalse();
  });

  it('should call authService.register and set success on success', fakeAsync(() => {
    component.registerData = { UserEmail: 'test@test.com', UserName: 'Test', Password: '1234', Role: 'User' };
    authServiceSpy.register.and.returnValue(of({}));
    component.onSubmit();

    tick();
    expect(authServiceSpy.register).toHaveBeenCalledWith(component.registerData);
    expect(component.loading).toBeFalse();
    expect(component.success).toBe('Registered. Please login.');

    tick(900);
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  }));


  it('should set error on registration failure', () => {
    component.registerData = { UserEmail: 'test@test.com', UserName: 'Test', Password: '1234', Role: 'User' };
    authServiceSpy.register.and.returnValue(throwError(() => ({ error: { message: 'Email exists' } })));

    component.onSubmit();

    expect(authServiceSpy.register).toHaveBeenCalled();
    expect(component.loading).toBeFalse();
    expect(component.error).toBe('Email exists');
  });

  it('should set default error if registration fails without message', () => {
    component.registerData = { UserEmail: 'a@b.com', UserName: 'Test', Password: '1234', Role: 'User' };
    authServiceSpy.register.and.returnValue(throwError(() => ({})));

    component.onSubmit();

    expect(component.loading).toBeFalse();
    expect(component.error).toBe('Registration failed');
  });
});
