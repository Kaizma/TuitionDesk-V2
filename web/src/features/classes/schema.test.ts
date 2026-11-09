import { describe, expect, it } from "vitest";
import { formatMoney, parseMoneyToMinor } from "../../shared/lib/money";
import { classFormSchema, emptyClassForm } from "./schema";

function valid() {
  return {
    ...emptyClassForm(),
    name: "Grade 10 Mathematics",
    subject: "Mathematics",
    startDate: "2026-01-10",
    monthlyFee: "4000.00",
  };
}

describe("classFormSchema", () => {
  it("accepts a minimal valid class", () => {
    const result = classFormSchema.safeParse(valid());
    expect(result.success).toBe(true);
  });

  it("rejects an end date before the start date", () => {
    const result = classFormSchema.safeParse({
      ...valid(),
      startDate: "2026-06-01",
      endDate: "2026-05-01",
    });
    expect(result.success).toBe(false);
  });

  it("rejects zero capacity", () => {
    const result = classFormSchema.safeParse({ ...valid(), capacity: "0" });
    expect(result.success).toBe(false);
  });

  it("rejects an invalid fee", () => {
    const result = classFormSchema.safeParse({ ...valid(), monthlyFee: "4,000" });
    expect(result.success).toBe(false);
  });
});

describe("money helpers", () => {
  it("parses decimal amounts into minor units", () => {
    expect(parseMoneyToMinor("4000.00")).toBe(400_000);
    expect(parseMoneyToMinor("1,250.50")).toBe(125_050);
    expect(parseMoneyToMinor("75")).toBe(7_500);
  });

  it("formats minor units with the class currency", () => {
    const formatted = formatMoney(400_000, "LKR");
    expect(formatted).toContain("4,000.00");
  });
});
