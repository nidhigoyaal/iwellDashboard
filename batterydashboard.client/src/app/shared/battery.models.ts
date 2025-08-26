export interface BatteryStatus {
  batteryPowerKw?: number;
  chargeCapacityRemainingKwh?: number;
  dischargeCapacityRemainingKwh?: number;
  status: Availability;
}

export interface TelemetrySeries {
  name: string;
  data: [number, number][];
}

export interface TelemetryResponse {
  series: TelemetrySeries[];
}

export enum Availability {
  OK = 'OK',
  PARTIAL = 'PARTIAL' ,
  NOT_AVAILABLE = 'NOT AVAILABLE'
}

export const NameMap: Record<string, string> = {
  'BatteryPowerW': 'Battery Power',
  'GridPowerW': 'Grid Power'
};

export function availabilityFromKw(availKw: number): Availability {
  if (availKw > 100) return  Availability.OK;
  if (availKw >= 50) return Availability.PARTIAL;
  return Availability.NOT_AVAILABLE;
}
