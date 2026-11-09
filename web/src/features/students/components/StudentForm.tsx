import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { ApiError } from "../../../shared/api/client";
import { Button, buttonStyles } from "../../../shared/components/Button";
import { errorInputClass, Field, inputClass } from "../../../shared/components/Field";
import { cn } from "../../../shared/lib/cn";
import { todayIso } from "../../../shared/lib/date";
import { useCreateStudent, useUpdateStudent } from "../queries";
import { emptyStudentForm, studentFormSchema, type StudentFormValues } from "../schema";
import type { SaveStudentPayload, StudentDetail } from "../types";

interface StudentFormProps {
  student?: StudentDetail;
}

export function StudentForm({ student }: StudentFormProps) {
  const navigate = useNavigate();
  const createStudent = useCreateStudent();
  const updateStudent = useUpdateStudent(student?.id ?? "");
  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<StudentFormValues>({
    resolver: zodResolver(studentFormSchema),
    defaultValues: toFormValues(student),
    mode: "onBlur",
  });

  useEffect(() => {
    reset(toFormValues(student));
  }, [reset, student]);

  async function submit(values: StudentFormValues, acknowledgeDuplicate = false) {
    setSubmitError(null);
    const payload = toPayload(values, acknowledgeDuplicate);

    try {
      const saved = student
        ? await updateStudent.mutateAsync(payload)
        : await createStudent.mutateAsync(payload);

      navigate(`/students/${saved.id}`);
    } catch (error) {
      if (
        error instanceof ApiError &&
        error.code === "student.duplicate_warning" &&
        !acknowledgeDuplicate
      ) {
        if (
          window.confirm(
            "Another student already has this name and phone number. Save this record anyway?",
          )
        ) {
          await submit(values, true);
        }
        return;
      }

      if (error instanceof ApiError && error.errors) {
        let mapped = false;
        for (const [field, messages] of Object.entries(error.errors)) {
          if (field in values && messages.length > 0) {
            setError(field as keyof StudentFormValues, {
              type: "server",
              message: messages[0],
            });
            mapped = true;
          }
        }

        if (mapped) {
          return;
        }
      }

      setSubmitError(error instanceof Error ? error.message : "Could not save the student.");
    }
  }

  return (
    <form
      className="space-y-6"
      onSubmit={handleSubmit((values) => submit(values))}
      noValidate
    >
      {submitError ? (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {submitError}
        </div>
      ) : null}

      <section className="rounded-xl border border-line bg-surface p-6 shadow-sm">
        <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">Student details</h2>

        <div className="mt-5 grid gap-5 md:grid-cols-2">
          <Field
            label="Full name"
            htmlFor="fullName"
            required
            error={errors.fullName?.message}
            className="md:col-span-2"
          >
            <input
              id="fullName"
              autoFocus
              className={cn(inputClass, errors.fullName && errorInputClass)}
              aria-invalid={Boolean(errors.fullName)}
              aria-describedby={errors.fullName ? "fullName-error" : undefined}
              {...register("fullName")}
            />
          </Field>

          <Field label="Phone" htmlFor="phone" required error={errors.phone?.message}>
            <input
              id="phone"
              type="tel"
              className={cn(inputClass, errors.phone && errorInputClass)}
              aria-invalid={Boolean(errors.phone)}
              aria-describedby={errors.phone ? "phone-error" : undefined}
              {...register("phone")}
            />
          </Field>

          <Field label="Email" htmlFor="email" error={errors.email?.message}>
            <input
              id="email"
              type="email"
              className={cn(inputClass, errors.email && errorInputClass)}
              aria-invalid={Boolean(errors.email)}
              aria-describedby={errors.email ? "email-error" : undefined}
              {...register("email")}
            />
          </Field>

          <Field label="Guardian name" htmlFor="guardianName" error={errors.guardianName?.message}>
            <input
              id="guardianName"
              className={cn(inputClass, errors.guardianName && errorInputClass)}
              {...register("guardianName")}
            />
          </Field>

          <Field
            label="Guardian phone"
            htmlFor="guardianPhone"
            error={errors.guardianPhone?.message}
          >
            <input
              id="guardianPhone"
              type="tel"
              className={cn(inputClass, errors.guardianPhone && errorInputClass)}
              {...register("guardianPhone")}
            />
          </Field>

          <Field label="Date of birth" htmlFor="dateOfBirth" error={errors.dateOfBirth?.message}>
            <input
              id="dateOfBirth"
              type="date"
              max={todayIso()}
              className={cn(inputClass, errors.dateOfBirth && errorInputClass)}
              {...register("dateOfBirth")}
            />
          </Field>

          <Field label="Joined on" htmlFor="joinedOn" required error={errors.joinedOn?.message}>
            <input
              id="joinedOn"
              type="date"
              max={todayIso()}
              className={cn(inputClass, errors.joinedOn && errorInputClass)}
              aria-invalid={Boolean(errors.joinedOn)}
              {...register("joinedOn")}
            />
          </Field>
        </div>
      </section>

      <section className="rounded-xl border border-line bg-surface p-6 shadow-sm">
        <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">School and notes</h2>

        <div className="mt-5 grid gap-5 md:grid-cols-2">
          <Field label="School" htmlFor="school" error={errors.school?.message}>
            <input
              id="school"
              className={cn(inputClass, errors.school && errorInputClass)}
              {...register("school")}
            />
          </Field>

          <Field
            label="Grade level"
            htmlFor="gradeLevel"
            hint="For example: Grade 10, Year 11, A/L."
            error={errors.gradeLevel?.message}
          >
            <input
              id="gradeLevel"
              className={cn(inputClass, errors.gradeLevel && errorInputClass)}
              {...register("gradeLevel")}
            />
          </Field>

          <Field label="Notes" htmlFor="notes" error={errors.notes?.message} className="md:col-span-2">
            <textarea
              id="notes"
              rows={4}
              className={cn(inputClass, "resize-y", errors.notes && errorInputClass)}
              {...register("notes")}
            />
          </Field>
        </div>
      </section>

      <div className="flex items-center justify-end gap-3">
        <Link to={student ? `/students/${student.id}` : "/students"} className={buttonStyles("secondary")}>
          Cancel
        </Link>
        <Button type="submit" disabled={isSubmitting || createStudent.isPending || updateStudent.isPending}>
          {isSubmitting || createStudent.isPending || updateStudent.isPending
            ? "Saving…"
            : student
              ? "Save changes"
              : "Add student"}
        </Button>
      </div>
    </form>
  );
}

function toFormValues(student?: StudentDetail): StudentFormValues {
  if (!student) {
    return emptyStudentForm();
  }

  return {
    fullName: student.fullName,
    phone: student.phone,
    email: student.email ?? "",
    guardianName: student.guardianName ?? "",
    guardianPhone: student.guardianPhone ?? "",
    dateOfBirth: student.dateOfBirth ?? "",
    school: student.school ?? "",
    gradeLevel: student.gradeLevel ?? "",
    joinedOn: student.joinedOn,
    notes: student.notes ?? "",
  };
}

function toPayload(values: StudentFormValues, acknowledgeDuplicate: boolean): SaveStudentPayload {
  const optional = (value: string) => value.trim() || null;

  return {
    fullName: values.fullName.trim(),
    phone: values.phone.trim(),
    email: optional(values.email),
    guardianName: optional(values.guardianName),
    guardianPhone: optional(values.guardianPhone),
    dateOfBirth: values.dateOfBirth || null,
    school: optional(values.school),
    gradeLevel: optional(values.gradeLevel),
    joinedOn: values.joinedOn,
    notes: optional(values.notes),
    acknowledgeDuplicate,
  };
}
