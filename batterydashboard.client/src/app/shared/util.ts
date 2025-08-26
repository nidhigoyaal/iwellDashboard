export function minutesForPreset(range: string): number {
  const num = parseInt(range);
  if (range.endsWith('h')) return -num * 60;
    if (range.endsWith('d')) return -num * 24 * 60;
    if (range.endsWith('m')) return -num;
    return -3600;
}
