import { TestBed } from '@angular/core/testing';
import { AuthService, DecodedToken } from './auth.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { environment } from '../../environments/environment';
import { Login } from '../Models/login.model';
import { Register } from '../shared/account.models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let currentUserValue: DecodedToken | null = null;

  beforeEach(() => {
    localStorage.clear();

    const payload: DecodedToken = {
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'John',
      exp: Math.floor(Date.now() / 1000) + 3600
    };
    const fakeToken = 'header.' + btoa(JSON.stringify(payload)) + '.signature';
    localStorage.setItem('iw_token', fakeToken);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });

    service = TestBed.inject(AuthService); // Inject after localStorage is ready
    httpMock = TestBed.inject(HttpTestingController);

    service.currentUser$.subscribe(value => currentUserValue = value);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('login should store token and update currentUser$', () => {
    const fakeTokenPayload = {
      exp: Math.floor(Date.now() / 1000) + 3600,
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'John',
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'User'
    };
    const fakeToken = 'header.' + btoa(JSON.stringify(fakeTokenPayload)) + '.signature';
    const loginData: Login = { UserEmail: 'test@test.com', UserName: '', Password: '1234' };

    service.login(loginData).subscribe(res => {
      expect(localStorage.getItem('iw_token')).toBe(fakeToken);
      expect(service.getToken()).toBe(fakeToken);
      expect(currentUserValue).toEqual(jasmine.objectContaining({
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'John',
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'User'
      }));
      expect(service.isTokenExpired(fakeToken)).toBeFalse();
    });

    const req = httpMock.expectOne(`${environment.authUrl}/Account/login`);
    expect(req.request.method).toBe('POST');
    req.flush({ token: fakeToken });
  });

  it('register should call API endpoint', () => {
    const registerData: Register = { UserEmail: 'a@b.com', UserName: 'Test', Password: '1234', Role: 'User' };
    service.register(registerData).subscribe();

    const req = httpMock.expectOne(`${environment.authUrl}/Account/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(registerData);
    req.flush({});
  });

  it('logout should clear token and currentUser', () => {
    localStorage.setItem('iw_token', 'fake-token');

    service.logout();
    expect(localStorage.getItem('iw_token')).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();
    expect(currentUserValue).toBeNull();
  });

  it('getRole should extract role from token', () => {
    const fakeToken = 'header.' + btoa(JSON.stringify({
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'User',
      exp: Math.floor(Date.now() / 1000) + 3600
    })) + '.signature';
    localStorage.setItem('iw_token', fakeToken);
    const service = TestBed.inject(AuthService);
    expect(service.getUserName()).toBe('User');
  });


  it('getUserName should extract name from token', () => {
    const fakeToken = 'header.' + btoa(JSON.stringify({
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'John',
      exp: Math.floor(Date.now() / 1000) + 3600
    })) + '.signature';
    localStorage.setItem('iw_token', fakeToken);
    const service = TestBed.inject(AuthService);
    expect(service.getUserName()).toBe('John');
  });

  it('isTokenExpired should return true for expired token', () => {
    const payload: DecodedToken = { exp: Math.floor(Date.now() / 1000) - 10 };
    const fakeToken = 'header.' + btoa(JSON.stringify(payload)) + '.signature';
    localStorage.setItem('iw_token', fakeToken);

    expect(service.isTokenExpired()).toBeTrue();
  });

  it('isTokenExpired should return false for valid token', () => {
    const payload: DecodedToken = { exp: Math.floor(Date.now() / 1000) + 3600 };
    const fakeToken = 'header.' + btoa(JSON.stringify(payload)) + '.signature';
    localStorage.setItem('iw_token', fakeToken);

    expect(service.isTokenExpired()).toBeFalse();
  });
});
