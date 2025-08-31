import React from "react";
import {
  Controller,
  FieldValues,
  Path,
  Control,
  FieldError,
} from "react-hook-form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";

interface FormFieldProps<TFieldValues extends FieldValues> {
  control: Control<TFieldValues>;
  name: Path<TFieldValues>;
  label?: string;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  className?: string;
  error?: FieldError;
}

interface FormInputProps<TFieldValues extends FieldValues>
  extends FormFieldProps<TFieldValues> {
  type?: "text" | "email" | "password" | "number" | "url";
}

interface FormTextareaProps<TFieldValues extends FieldValues>
  extends FormFieldProps<TFieldValues> {
  rows?: number;
}

interface FormSelectProps<TFieldValues extends FieldValues>
  extends FormFieldProps<TFieldValues> {
  options: Array<{ value: string; label: string }>;
}

// Form Input Component
export function FormInput<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  placeholder,
  required = false,
  disabled = false,
  type = "text",
  className,
  error,
}: FormInputProps<TFieldValues>) {
  return (
    <div className="space-y-2">
      {label && (
        <Label htmlFor={name} className="text-sm font-medium">
          {label} {required && <span className="text-red-500">*</span>}
        </Label>
      )}
      <Controller
        control={control}
        name={name}
        render={({ field }) => (
          <Input
            {...field}
            id={name}
            type={type}
            placeholder={placeholder}
            disabled={disabled}
            className={cn(
              error && "border-red-500 focus-visible:ring-red-500",
              className
            )}
          />
        )}
      />
      {error && <p className="text-sm text-red-500">{error.message}</p>}
    </div>
  );
}

// Form Textarea Component
export function FormTextarea<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  placeholder,
  required = false,
  disabled = false,
  rows = 3,
  className,
  error,
}: FormTextareaProps<TFieldValues>) {
  return (
    <div className="space-y-2">
      {label && (
        <Label htmlFor={name} className="text-sm font-medium">
          {label} {required && <span className="text-red-500">*</span>}
        </Label>
      )}
      <Controller
        control={control}
        name={name}
        render={({ field }) => (
          <Textarea
            {...field}
            id={name}
            placeholder={placeholder}
            disabled={disabled}
            rows={rows}
            className={cn(
              error && "border-red-500 focus-visible:ring-red-500",
              className
            )}
          />
        )}
      />
      {error && <p className="text-sm text-red-500">{error.message}</p>}
    </div>
  );
}

// Form Select Component
export function FormSelect<TFieldValues extends FieldValues>({
  control,
  name,
  label,
  placeholder,
  required = false,
  disabled = false,
  options,
  className,
  error,
}: FormSelectProps<TFieldValues>) {
  return (
    <div className="space-y-2">
      {label && (
        <Label htmlFor={name} className="text-sm font-medium">
          {label} {required && <span className="text-red-500">*</span>}
        </Label>
      )}
      <Controller
        control={control}
        name={name}
        render={({ field }) => (
          <Select
            value={field.value}
            onValueChange={field.onChange}
            disabled={disabled}
          >
            <SelectTrigger
              className={cn(
                error && "border-red-500 focus-visible:ring-red-500",
                className
              )}
            >
              <SelectValue placeholder={placeholder} />
            </SelectTrigger>
            <SelectContent>
              {options.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
      />
      {error && <p className="text-sm text-red-500">{error.message}</p>}
    </div>
  );
}

// Form Error Message Component
export function FormError({ error }: { error?: FieldError }) {
  if (!error) return null;

  return <p className="text-sm text-red-500 mt-1">{error.message}</p>;
}

// Generic Form Field Wrapper
export function FormField({
  label,
  required = false,
  error,
  children,
}: {
  label?: string;
  required?: boolean;
  error?: FieldError;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-2">
      {label && (
        <Label className="text-sm font-medium">
          {label} {required && <span className="text-red-500">*</span>}
        </Label>
      )}
      {children}
      {error && <p className="text-sm text-red-500">{error.message}</p>}
    </div>
  );
}
