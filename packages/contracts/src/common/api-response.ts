import { z } from 'zod';

export const ApiErrorSchema = z.object({
  code: z.string(),
  message: z.string(),
  details: z.record(z.array(z.string())).optional(),
});

export type ApiError = z.infer<typeof ApiErrorSchema>;

export const ProblemDetailsSchema = z.object({
  type: z.string().optional(),
  title: z.string(),
  status: z.number(),
  detail: z.string().optional(),
  errors: z.record(z.array(z.string())).optional(),
});

export type ProblemDetails = z.infer<typeof ProblemDetailsSchema>;
