import { Component, OnInit, OnDestroy } from '@angular/core';
import { BatteryService } from '../../services/battery.service';
import { AuthService } from '../../services/auth.service';
import { interval, Subscription } from 'rxjs';
import { Availability, availabilityFromKw, BatteryStatus, NameMap } from 'src/app/shared/battery.models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  Availability = Availability;
  batteryStatus: BatteryStatus = {
    batteryPowerKw: 0,
    status: Availability.OK,
    chargeCapacityRemainingKwh: 0,
    dischargeCapacityRemainingKwh: 0
  }
  chartSeries: any[] = [];
  chartOptions: any = {};
  selectedRange = '60h';
  autoRefresh = false;
  refreshSub?: Subscription;
  role: string | null = null;
  userName: string | null = null;
  isLoading = false;

  constructor(
    private battery: BatteryService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.role = this.auth.getRole();
    this.userName = this.auth.getUserName();
    this.loadStatus();
    this.loadTelemetry();

    this.refreshSub = interval(60000).subscribe(() => this.loadTelemetry(false));
  }

  loadStatus() {
    this.battery.getBatteryStatus().subscribe({
      next: (res: any) => {
        this.batteryStatus = {
          batteryPowerKw: res.batteryPowerKw,
          chargeCapacityRemainingKwh: res.chargeCapacityRemainingKwh,
          dischargeCapacityRemainingKwh: res.dischargeCapacityRemainingKwh,
          status: availabilityFromKw(res.availableDischargePowerKw)
        }
      },
      error: (err) => console.error(err)
    });
  }

  loadTelemetry(showSpinner = true) {
    if(showSpinner) this.isLoading = true;

    const offset = this.mapRangeToMinutes(this.selectedRange);
    this.battery.getTelemetry(offset).subscribe({
      next: (res: any) => {
        this.chartSeries = res.series.map((s: any) => ({
          name: NameMap[s.name] || s.name,
          data: s.data.map(([t, v]: [number, number]) =>
                [t, Math.round(((v/1000) + Number.EPSILON) * 100) / 100])
        }));

        this.chartOptions = {
          chart: { type: 'line', height: 360, animations: { enabled: true }, toolbar:{ export: { csv: {filename: 'power data'}, png: {filename: 'power data'}, svg: {filename: 'power data'}}}},
          xaxis: { type: 'datetime', labels: { datetimeUTC: false } },
          stroke: { curve: 'smooth' },
          legend: { show: true, position: 'top', horizontalAlign: 'center',floating: false },
          tooltip: { shared: true, x: { format: 'ddd, dd MMM yyyy HH:mm'}, y: { formatter: (val: number) => `${val.toFixed(2)} kW` }},
          yaxis: { labels: {formatter: (val: number) => `${val.toFixed(2)} kW` }},
        };

        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  mapRangeToMinutes(range: string): number {
    const num = parseInt(range);
    if (range.endsWith('h')) return -num * 60;
    if (range.endsWith('d')) return -num * 24 * 60;
    if (range.endsWith('m')) return -num;
    return -3600;
  }

  onRangeChange(range: string) {
    this.selectedRange = range;
    this.loadTelemetry();
  }

  logout() {
  localStorage.removeItem('iw_token');

  this.router.navigate(['/login']);
}

  ngOnDestroy(): void {
    this.refreshSub?.unsubscribe();
  }
}
