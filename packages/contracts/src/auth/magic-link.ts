import { z } from 'zod';

export const SendMagicLinkRequestSchema = z.object({
  email: z.string().email(),
});

export type SendMagicLinkRequest = z.infer<typeof SendMagicLinkRequestSchema>;

export const VerifyMagicLinkRequestSchema = z.object({
  token: z.string().min(1),
});

export type VerifyMagicLinkRequest = z.infer<typeof VerifyMagicLinkRequestSchema>;
