import { z } from 'zod';

export const TokenResponseSchema = z.object({
  accessToken: z.string(),
  refreshToken: z.string(),
  expiresIn: z.number().int(), // seconds
  tokenType: z.literal('Bearer'),
});

export type TokenResponse = z.infer<typeof TokenResponseSchema>;

export const RefreshTokenRequestSchema = z.object({
  refreshToken: z.string().min(1),
});

export type RefreshTokenRequest = z.infer<typeof RefreshTokenRequestSchema>;

export const SwitchTenantRequestSchema = z.object({
  tenantId: z.string().uuid(),
});

export type SwitchTenantRequest = z.infer<typeof SwitchTenantRequestSchema>;
