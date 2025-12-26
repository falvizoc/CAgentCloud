'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { useRegister } from '@/lib/hooks/use-auth';

const registerSchema = z
  .object({
    email: z.string().email('Ingresa un correo electrónico válido'),
    password: z
      .string()
      .min(8, 'La contraseña debe tener al menos 8 caracteres'),
    confirmPassword: z.string(),
    nombre: z.string().min(2, 'El nombre debe tener al menos 2 caracteres'),
    organizacion: z.object({
      nombre: z
        .string()
        .min(2, 'El nombre de la organización debe tener al menos 2 caracteres'),
      rfc: z.string().optional(),
    }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Las contraseñas no coinciden',
    path: ['confirmPassword'],
  });

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const { mutate: registerUser, isPending } = useRegister();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      email: '',
      password: '',
      confirmPassword: '',
      nombre: '',
      organizacion: {
        nombre: '',
        rfc: '',
      },
    },
  });

  const onSubmit = (data: RegisterFormData) => {
    const { confirmPassword, ...registerData } = data;
    registerUser(registerData);
  };

  return (
    <Card>
      <CardHeader className="space-y-1">
        <CardTitle className="text-xl">Crear cuenta</CardTitle>
        <CardDescription>
          Completa el formulario para crear tu cuenta
        </CardDescription>
      </CardHeader>

      <form onSubmit={handleSubmit(onSubmit)}>
        <CardContent className="space-y-4">
          {/* Nombre */}
          <div className="space-y-2">
            <Label htmlFor="nombre">Tu nombre</Label>
            <Input
              id="nombre"
              type="text"
              placeholder="Juan Pérez"
              autoComplete="name"
              disabled={isPending}
              aria-invalid={errors.nombre ? 'true' : 'false'}
              aria-describedby={errors.nombre ? 'nombre-error' : undefined}
              {...register('nombre')}
            />
            {errors.nombre && (
              <p id="nombre-error" className="text-sm text-destructive">
                {errors.nombre.message}
              </p>
            )}
          </div>

          {/* Email */}
          <div className="space-y-2">
            <Label htmlFor="email">Correo electrónico</Label>
            <Input
              id="email"
              type="email"
              placeholder="correo@empresa.com"
              autoComplete="email"
              disabled={isPending}
              aria-invalid={errors.email ? 'true' : 'false'}
              aria-describedby={errors.email ? 'email-error' : undefined}
              {...register('email')}
            />
            {errors.email && (
              <p id="email-error" className="text-sm text-destructive">
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Organization Name */}
          <div className="space-y-2">
            <Label htmlFor="orgNombre">Nombre de la empresa</Label>
            <Input
              id="orgNombre"
              type="text"
              placeholder="Mi Empresa S.A. de C.V."
              autoComplete="organization"
              disabled={isPending}
              aria-invalid={errors.organizacion?.nombre ? 'true' : 'false'}
              aria-describedby={
                errors.organizacion?.nombre ? 'org-nombre-error' : undefined
              }
              {...register('organizacion.nombre')}
            />
            {errors.organizacion?.nombre && (
              <p id="org-nombre-error" className="text-sm text-destructive">
                {errors.organizacion.nombre.message}
              </p>
            )}
          </div>

          {/* RFC (Optional) */}
          <div className="space-y-2">
            <Label htmlFor="rfc">
              RFC <span className="text-muted-foreground">(opcional)</span>
            </Label>
            <Input
              id="rfc"
              type="text"
              placeholder="XAXX010101000"
              disabled={isPending}
              {...register('organizacion.rfc')}
            />
          </div>

          {/* Password */}
          <div className="space-y-2">
            <Label htmlFor="password">Contraseña</Label>
            <Input
              id="password"
              type="password"
              placeholder="••••••••"
              autoComplete="new-password"
              disabled={isPending}
              aria-invalid={errors.password ? 'true' : 'false'}
              aria-describedby={errors.password ? 'password-error' : undefined}
              {...register('password')}
            />
            {errors.password && (
              <p id="password-error" className="text-sm text-destructive">
                {errors.password.message}
              </p>
            )}
          </div>

          {/* Confirm Password */}
          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Confirmar contraseña</Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder="••••••••"
              autoComplete="new-password"
              disabled={isPending}
              aria-invalid={errors.confirmPassword ? 'true' : 'false'}
              aria-describedby={
                errors.confirmPassword ? 'confirm-password-error' : undefined
              }
              {...register('confirmPassword')}
            />
            {errors.confirmPassword && (
              <p id="confirm-password-error" className="text-sm text-destructive">
                {errors.confirmPassword.message}
              </p>
            )}
          </div>
        </CardContent>

        <CardFooter className="flex flex-col space-y-4">
          <Button type="submit" className="w-full" disabled={isPending}>
            {isPending ? 'Creando cuenta...' : 'Crear cuenta'}
          </Button>

          <p className="text-sm text-muted-foreground text-center">
            ¿Ya tienes una cuenta?{' '}
            <Link href="/login" className="text-primary hover:underline">
              Inicia sesión
            </Link>
          </p>
        </CardFooter>
      </form>
    </Card>
  );
}
