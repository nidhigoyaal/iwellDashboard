import { TestBed } from '@angular/core/testing';
import { BatteryService } from './battery.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { environment } from '../../environments/environment';
import { TelemetryResponse } from '../shared/battery.models';

describe('BatteryService', () => {
  let service: BatteryService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BatteryService]
    });

    service = TestBed.inject(BatteryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getBatteryStatus endpoint', () => {
    service.getBatteryStatus().subscribe();

    const req = httpMock.expectOne(`${environment.authUrl}/Battery/${environment.deviceId}/status`);
    expect(req.request.method).toBe('GET');
    req.flush({}); // mock empty response
  });

  it('should call getTelemetry endpoint with default offset', () => {
    service.getTelemetry().subscribe((res: TelemetryResponse) => {
      expect(res.series).toEqual([]);
    });

    const req = httpMock.expectOne(`${environment.authUrl}/Battery/${environment.deviceId}/telemetry/?offsetMinutes=-3600`);
    expect(req.request.method).toBe('GET');
    req.flush({ series: [] }); // mock TelemetryResponse
  });

  it('should call getTelemetry endpoint with provided offset', () => {
    const offset = -120;
    service.getTelemetry(offset).subscribe((res: TelemetryResponse) => {
      expect(res.series).toEqual([]);
    });

    const req = httpMock.expectOne(`${environment.authUrl}/Battery/${environment.deviceId}/telemetry/?offsetMinutes=${offset}`);
    expect(req.request.method).toBe('GET');
    req.flush({ series: [] });
  });
});
