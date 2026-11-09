import { z } from "zod";
import { todayIso } from "../../shared/lib/date";

const today = todayIso();

export const studentFormSchema = z.object({
  fullName: z
    .string()
    .trim()
    .min(2, "Full name must be at least 2 characters.")
    .max(120, "Full name must be 120 characters or fewer.")
    .refine((value) => /[A-Za-z]/.test(value), "Full name must contain at least one letter."),
  phone: z
    .string()
    .trim()
    .min(7, "Phone must be at least 7 characters.")
    .max(20, "Phone must be 20 characters or fewer.")
    .regex(/^[0-9+\-() ]+$/, "Use digits, spaces, and + - ( ) only."),
  email: z
    .string()
    .trim()
    .max(200, "Email must be 200 characters or fewer.")
    .refine(
      (value) => value === "" || /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(value),
      "Enter a valid email address.",
    ),
  guardianName: z.string().trim().max(120, "Guardian name must be 120 characters or fewer."),
  guardianPhone: z
    .string()
    .trim()
    .max(20, "Guardian phone must be 20 characters or fewer.")
    .refine(
      (value) => value === "" || (/^[0-9+\-() ]+$/.test(value) && value.length >= 7),
      "Use 7–20 characters: digits, spaces, and + - ( ) only.",
    ),
  dateOfBirth: z
    .string()
    .refine((value) => value === "" || value <= today, "Date of birth cannot be in the future."),
  school: z.string().trim().max(120, "School must be 120 characters or fewer."),
  gradeLevel: z.string().trim().max(40, "Grade level must be 40 characters or fewer."),
  joinedOn: z
    .string()
    .min(1, "Joined date is required.")
    .refine((value) => value <= today, "Joined date cannot be in the future."),
  notes: z.string().max(1000, "Notes must be 1,000 characters or fewer."),
});

export type StudentFormValues = z.infer<typeof studentFormSchema>;

export function emptyStudentForm(): StudentFormValues {
  return {
    fullName: "",
    phone: "",
    email: "",
    guardianName: "",
    guardianPhone: "",
    dateOfBirth: "",
    school: "",
    gradeLevel: "",
    joinedOn: todayIso(),
    notes: "",
  };
}
