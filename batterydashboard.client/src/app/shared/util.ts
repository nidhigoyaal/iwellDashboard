export function minutesForPreset(preset: '1h'|'24h'|'7d'|'30d'): number {
  switch (preset) {
    case '1h': return 60;
    case '24h': return 24 * 60;
    case '7d': return 7 * 24 * 60;
    case '30d': return 30 * 24 * 60;
  }
}
