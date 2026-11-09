import { z } from "zod";
import { todayIso } from "../../shared/lib/date";

export const classFormSchema = z
  .object({
    name: z
      .string()
      .trim()
      .min(2, "Class name must be at least 2 characters.")
      .max(120, "Class name must be 120 characters or fewer.")
      .refine((value) => /[A-Za-z]/.test(value), "Class name must contain at least one letter."),
    subject: z
      .string()
      .trim()
      .min(2, "Subject must be at least 2 characters.")
      .max(60, "Subject must be 60 characters or fewer."),
    gradeLevel: z.string().trim().max(40, "Grade level must be 40 characters or fewer."),
    scheduleText: z.string().trim().max(200, "Schedule must be 200 characters or fewer."),
    startDate: z.string().min(1, "Start date is required."),
    endDate: z.string(),
    monthlyFee: z
      .string()
      .trim()
      .min(1, "Monthly fee is required.")
      .regex(/^\d+(\.\d{1,2})?$/, "Use a number with up to two decimals."),
    currency: z
      .string()
      .trim()
      .length(3, "Use a three-letter currency code.")
      .regex(/^[A-Za-z]{3}$/, "Use a three-letter currency code."),
    capacity: z
      .string()
      .trim()
      .refine(
        (value) => value === "" || (/^\d+$/.test(value) && Number(value) > 0),
        "Capacity must be a whole number of at least 1, or blank for unlimited.",
      ),
    notes: z.string().max(1000, "Notes must be 1,000 characters or fewer."),
  })
  .refine((value) => !value.endDate || value.endDate >= value.startDate, {
    path: ["endDate"],
    message: "End date cannot be before the start date.",
  });

export type ClassFormValues = z.infer<typeof classFormSchema>;

export function emptyClassForm(): ClassFormValues {
  return {
    name: "",
    subject: "",
    gradeLevel: "",
    scheduleText: "",
    startDate: todayIso(),
    endDate: "",
    monthlyFee: "",
    currency: "LKR",
    capacity: "",
    notes: "",
  };
}
