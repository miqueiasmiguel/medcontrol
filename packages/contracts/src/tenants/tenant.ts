import { z } from 'zod';

export const TenantDtoSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  slug: z.string(),
  isActive: z.boolean(),
  createdAt: z.string().datetime(),
  role: z.string(), // current user's role in this tenant
});

export type TenantDto = z.infer<typeof TenantDtoSchema>;

export const CreateTenantRequestSchema = z.object({
  name: z.string().min(2).max(200),
});

export type CreateTenantRequest = z.infer<typeof CreateTenantRequestSchema>;

export const UpdateTenantRequestSchema = z.object({
  name: z.string().min(2).max(200),
});

export type UpdateTenantRequest = z.infer<typeof UpdateTenantRequestSchema>;
