import { z } from 'zod';

export const UserDtoSchema = z.object({
  id: z.string().uuid(),
  email: z.string().email(),
  displayName: z.string().nullable(),
  avatarUrl: z.string().url().nullable(),
  isEmailVerified: z.boolean(),
  globalRole: z.enum(['None', 'Support', 'Admin']),
  lastLoginAt: z.string().datetime().nullable(),
});

export type UserDto = z.infer<typeof UserDtoSchema>;

export const UpdateProfileRequestSchema = z.object({
  displayName: z.string().min(1).max(100).nullable(),
});

export type UpdateProfileRequest = z.infer<typeof UpdateProfileRequestSchema>;
