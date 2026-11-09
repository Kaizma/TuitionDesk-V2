import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { ApiError } from "../../../shared/api/client";
import { Button, buttonStyles } from "../../../shared/components/Button";
import { errorInputClass, Field, inputClass } from "../../../shared/components/Field";
import { cn } from "../../../shared/lib/cn";
import { parseMoneyToMinor } from "../../../shared/lib/money";
import { useCreateClass, useLookupsQuery, useUpdateClass } from "../queries";
import { classFormSchema, emptyClassForm, type ClassFormValues } from "../schema";
import type { ClassDetail, SaveClassPayload } from "../types";

interface ClassFormProps {
  classEntity?: ClassDetail;
}

const currencies = ["LKR", "USD", "GBP", "EUR", "AUD", "INR", "SGD", "AED"];

export function ClassForm({ classEntity }: ClassFormProps) {
  const navigate = useNavigate();
  const createClass = useCreateClass();
  const updateClass = useUpdateClass(classEntity?.id ?? "");
  const lookups = useLookupsQuery();
  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ClassFormValues>({
    resolver: zodResolver(classFormSchema),
    defaultValues: toFormValues(classEntity),
    mode: "onBlur",
  });

  useEffect(() => {
    reset(toFormValues(classEntity));
  }, [classEntity, reset]);

  async function submit(values: ClassFormValues, acknowledgeDuplicate = false) {
    setSubmitError(null);
    const payload = toPayload(values, acknowledgeDuplicate);

    try {
      const saved = classEntity
        ? await updateClass.mutateAsync(payload)
        : await createClass.mutateAsync(payload);

      navigate(`/classes/${saved.id}`);
    } catch (error) {
      if (
        error instanceof ApiError &&
        error.code === "class.duplicate_warning" &&
        !acknowledgeDuplicate
      ) {
        if (
          window.confirm(
            "An active class with the same name, subject, and schedule already exists. Save anyway?",
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
            setError(field as keyof ClassFormValues, {
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

      setSubmitError(error instanceof Error ? error.message : "Could not save the class.");
    }
  }

  const currencyOptions = currencies.includes(classEntity?.currency ?? "LKR")
    ? currencies
    : [classEntity?.currency ?? "LKR", ...currencies];

  return (
    <form className="space-y-6" onSubmit={handleSubmit((values) => submit(values))} noValidate>
      {submitError ? (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {submitError}
        </div>
      ) : null}

      <section className="rounded-xl border border-line bg-surface p-6 shadow-sm">
        <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">Class details</h2>

        <div className="mt-5 grid gap-5 md:grid-cols-2">
          <Field
            label="Class name"
            htmlFor="name"
            required
            error={errors.name?.message}
            className="md:col-span-2"
          >
            <input
              id="name"
              autoFocus
              placeholder="Grade 10 Mathematics"
              className={cn(inputClass, errors.name && errorInputClass)}
              aria-invalid={Boolean(errors.name)}
              {...register("name")}
            />
          </Field>

          <Field label="Subject" htmlFor="subject" required error={errors.subject?.message}>
            <input
              id="subject"
              list="subject-options"
              placeholder="Mathematics"
              className={cn(inputClass, errors.subject && errorInputClass)}
              {...register("subject")}
            />
            <datalist id="subject-options">
              {(lookups.data?.subjects ?? []).map((subject) => (
                <option key={subject} value={subject} />
              ))}
            </datalist>
          </Field>

          <Field label="Grade level" htmlFor="gradeLevel" error={errors.gradeLevel?.message}>
            <input
              id="gradeLevel"
              list="grade-options"
              placeholder="Grade 10"
              className={cn(inputClass, errors.gradeLevel && errorInputClass)}
              {...register("gradeLevel")}
            />
            <datalist id="grade-options">
              {(lookups.data?.gradeLevels ?? []).map((grade) => (
                <option key={grade} value={grade} />
              ))}
            </datalist>
          </Field>

          <Field
            label="Schedule"
            htmlFor="scheduleText"
            hint="Free text, for example: Wed 4–6pm, Sat 9–11am."
            error={errors.scheduleText?.message}
            className="md:col-span-2"
          >
            <input
              id="scheduleText"
              placeholder="Wed 4–6pm"
              className={cn(inputClass, errors.scheduleText && errorInputClass)}
              {...register("scheduleText")}
            />
          </Field>

          <Field label="Start date" htmlFor="startDate" required error={errors.startDate?.message}>
            <input
              id="startDate"
              type="date"
              className={cn(inputClass, errors.startDate && errorInputClass)}
              {...register("startDate")}
            />
          </Field>

          <Field label="End date" htmlFor="endDate" error={errors.endDate?.message}>
            <input
              id="endDate"
              type="date"
              className={cn(inputClass, errors.endDate && errorInputClass)}
              {...register("endDate")}
            />
          </Field>
        </div>
      </section>

      <section className="rounded-xl border border-line bg-surface p-6 shadow-sm">
        <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">Fee and capacity</h2>

        <div className="mt-5 grid gap-5 md:grid-cols-3">
          <Field label="Monthly fee" htmlFor="monthlyFee" required error={errors.monthlyFee?.message}>
            <input
              id="monthlyFee"
              inputMode="decimal"
              placeholder="4000.00"
              className={cn(inputClass, errors.monthlyFee && errorInputClass)}
              aria-invalid={Boolean(errors.monthlyFee)}
              {...register("monthlyFee")}
            />
          </Field>

          <Field label="Currency" htmlFor="currency" required error={errors.currency?.message}>
            <select
              id="currency"
              className={cn(inputClass, errors.currency && errorInputClass)}
              {...register("currency")}
            >
              {currencyOptions.map((currency) => (
                <option key={currency} value={currency}>
                  {currency}
                </option>
              ))}
            </select>
          </Field>

          <Field
            label="Capacity"
            htmlFor="capacity"
            hint="Leave blank for unlimited."
            error={errors.capacity?.message}
          >
            <input
              id="capacity"
              type="number"
              min={1}
              inputMode="numeric"
              placeholder="15"
              className={cn(inputClass, errors.capacity && errorInputClass)}
              {...register("capacity")}
            />
          </Field>

          <Field label="Notes" htmlFor="notes" error={errors.notes?.message} className="md:col-span-3">
            <textarea
              id="notes"
              rows={3}
              className={cn(inputClass, "resize-y", errors.notes && errorInputClass)}
              {...register("notes")}
            />
          </Field>
        </div>
      </section>

      <div className="flex items-center justify-end gap-3">
        <Link
          to={classEntity ? `/classes/${classEntity.id}` : "/classes"}
          className={buttonStyles("secondary")}
        >
          Cancel
        </Link>
        <Button
          type="submit"
          disabled={isSubmitting || createClass.isPending || updateClass.isPending}
        >
          {isSubmitting || createClass.isPending || updateClass.isPending
            ? "Saving…"
            : classEntity
              ? "Save changes"
              : "Add class"}
        </Button>
      </div>
    </form>
  );
}

function toFormValues(classEntity?: ClassDetail): ClassFormValues {
  if (!classEntity) {
    return emptyClassForm();
  }

  return {
    name: classEntity.name,
    subject: classEntity.subject,
    gradeLevel: classEntity.gradeLevel ?? "",
    scheduleText: classEntity.scheduleText ?? "",
    startDate: classEntity.startDate,
    endDate: classEntity.endDate ?? "",
    monthlyFee: (classEntity.monthlyFeeMinor / 100).toFixed(2),
    currency: classEntity.currency,
    capacity: classEntity.capacity?.toString() ?? "",
    notes: classEntity.notes ?? "",
  };
}

function toPayload(values: ClassFormValues, acknowledgeDuplicate: boolean): SaveClassPayload {
  const optional = (value: string) => value.trim() || null;

  return {
    name: values.name.trim(),
    subject: values.subject.trim(),
    gradeLevel: optional(values.gradeLevel),
    scheduleText: optional(values.scheduleText),
    startDate: values.startDate,
    endDate: values.endDate || null,
    monthlyFeeMinor: parseMoneyToMinor(values.monthlyFee),
    currency: values.currency.trim().toUpperCase(),
    capacity: values.capacity ? Number(values.capacity) : null,
    notes: optional(values.notes),
    acknowledgeDuplicate,
  };
}
