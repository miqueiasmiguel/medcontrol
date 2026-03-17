import { z } from 'zod';

export const GoogleLoginRequestSchema = z.object({
  code: z.string().min(1),
  redirectUri: z.string().url(),
});

export type GoogleLoginRequest = z.infer<typeof GoogleLoginRequestSchema>;
