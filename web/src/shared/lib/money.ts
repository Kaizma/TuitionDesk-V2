export function formatMoney(minorUnits: number, currency: string): string {
  const amount = minorUnits / 100;

  try {
    return new Intl.NumberFormat("en-GB", {
      style: "currency",
      currency,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount);
  } catch {
    return `${currency} ${amount.toFixed(2)}`;
  }
}

export function parseMoneyToMinor(value: string): number {
  const cleaned = value.replace(/,/g, "").trim();
  const [whole = "0", fraction = ""] = cleaned.split(".");
  const major = Number(whole);
  const minor = Number((fraction + "00").slice(0, 2));
  return Math.round((Number.isFinite(major) ? major : 0) * 100 + (Number.isFinite(minor) ? minor : 0));
}
