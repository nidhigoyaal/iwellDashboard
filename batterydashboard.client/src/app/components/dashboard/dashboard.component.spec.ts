import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';

import { DashboardComponent } from './dashboard.component';
import { BatteryService } from '../../services/battery.service';
import { AuthService } from '../../services/auth.service';
import { Availability, TelemetryResponse } from 'src/app/shared/battery.models';
import { minutesForPreset } from 'src/app/shared/util';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let batteryServiceSpy: jasmine.SpyObj<BatteryService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    batteryServiceSpy = jasmine.createSpyObj('BatteryService', ['getBatteryStatus', 'getTelemetry']);
    authServiceSpy = jasmine.createSpyObj('AuthService', ['getRole', 'getUserName']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      declarations: [DashboardComponent],
      providers: [
        { provide: BatteryService, useValue: batteryServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize role, username and call loadStatus/loadTelemetry on init', () => {
    authServiceSpy.getRole.and.returnValue('admin');
    authServiceSpy.getUserName.and.returnValue('testUser');
    batteryServiceSpy.getBatteryStatus.and.returnValue(of({
      batteryPowerKw: 1,
      chargeCapacityRemainingKwh: 2,
      dischargeCapacityRemainingKwh: 3,
      availableDischargePowerKw: 4
    }));
    batteryServiceSpy.getTelemetry.and.returnValue(of({ series: [] }));

    component.ngOnInit();

    expect(component.role).toBe('admin');
    expect(component.userName).toBe('testUser');
    expect(batteryServiceSpy.getBatteryStatus).toHaveBeenCalled();
    expect(batteryServiceSpy.getTelemetry).toHaveBeenCalled();
  });

  it('should update batteryStatus when loadStatus succeeds', () => {
    batteryServiceSpy.getBatteryStatus.and.returnValue(of({
      batteryPowerKw: 5,
      chargeCapacityRemainingKwh: 6,
      dischargeCapacityRemainingKwh: 7,
      availableDischargePowerKw: 8
    }));

    component.loadStatus();

    expect(component.batteryStatus.batteryPowerKw).toBe(5);
    expect(component.batteryStatus.status).toBe(Availability.NOT_AVAILABLE);
  });

  it('should handle error in loadStatus', () => {
    spyOn(console, 'error');
    batteryServiceSpy.getBatteryStatus.and.returnValue(throwError(() => new Error('fail')));

    component.loadStatus();

    expect(console.error).toHaveBeenCalled();
  });

  it('should update chart data when loadTelemetry succeeds', () => {
    const telemetryResponse: TelemetryResponse = {
      series: [
        { name: 'BatteryPowerW', data: [[123456, 2000] as [number, number]] }
      ]
    };
    batteryServiceSpy.getTelemetry.and.returnValue(of(telemetryResponse));

    component.loadTelemetry();

    expect(component.chartSeries.length).toBe(1);
    expect(component.isLoading).toBeFalse();
  });

  it('should set isLoading false when loadTelemetry fails', () => {
    batteryServiceSpy.getTelemetry.and.returnValue(throwError(() => new Error('fail')));

    component.loadTelemetry();

    expect(component.isLoading).toBeFalse();
  });

  it('should correctly map range to minutes', () => {
    expect(minutesForPreset('2h')).toBe(-120);
    expect(minutesForPreset('3d')).toBe(-4320);
    expect(minutesForPreset('15m')).toBe(-15);
    expect(minutesForPreset('')).toBe(-3600);
  });

  it('should update selectedRange and reload telemetry on range change', () => {
    spyOn(component, 'loadTelemetry');
    component.onRangeChange('2h');

    expect(component.selectedRange).toBe('2h');
    expect(component.loadTelemetry).toHaveBeenCalled();
  });

  it('should clear token and navigate to login on logout', () => {
    spyOn(localStorage, 'removeItem');
    component.logout();

    expect(localStorage.removeItem).toHaveBeenCalledWith('iw_token');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should unsubscribe refreshSub on destroy', () => {
    const subSpy = jasmine.createSpyObj('Subscription', ['unsubscribe']);
    component.refreshSub = subSpy;

    component.ngOnDestroy();

    expect(subSpy.unsubscribe).toHaveBeenCalled();
  });
});
