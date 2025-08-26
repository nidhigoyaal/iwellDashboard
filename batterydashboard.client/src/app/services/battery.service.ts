import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { TelemetryResponse } from '../shared/battery.models';

@Injectable({
  providedIn: 'root'
})
export class BatteryService {
  constructor(private http: HttpClient) {}

  getBatteryStatus() {
    return this.http.get(`${environment.iwellApiUrl}/Battery/${environment.deviceId}/status`);
  }

  getTelemetry(offsetMinutes: number = -3600) {
    return this.http.get<TelemetryResponse>(`${environment.iwellApiUrl}/Battery/${environment.deviceId}/telemetry/?offsetMinutes=${offsetMinutes}`);
  }
}
