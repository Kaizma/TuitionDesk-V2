import { describe, expect, it } from "vitest";
import { emptyStudentForm, studentFormSchema } from "./schema";

function valid() {
  return {
    ...emptyStudentForm(),
    fullName: "Kasun Perera",
    phone: "+94 77 123 4567",
    joinedOn: "2026-01-10",
  };
}

describe("studentFormSchema", () => {
  it("accepts a minimal valid student", () => {
    const result = studentFormSchema.safeParse(valid());
    expect(result.success).toBe(true);
  });

  it("rejects an invalid phone", () => {
    const result = studentFormSchema.safeParse({ ...valid(), phone: "call me" });
    expect(result.success).toBe(false);
  });

  it("rejects a future joined date", () => {
    const result = studentFormSchema.safeParse({ ...valid(), joinedOn: "2999-01-01" });
    expect(result.success).toBe(false);
  });

  it("rejects a name without letters", () => {
    const result = studentFormSchema.safeParse({ ...valid(), fullName: "12345" });
    expect(result.success).toBe(false);
  });
});
