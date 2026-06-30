'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { Eye, EyeOff, ArrowLeft, User, Briefcase } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { toast } from 'sonner';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5188';

export default function LoginPage() {
  const router = useRouter();
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSignUp, setIsSignUp] = useState(false);

  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [cpf, setCpf] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [role, setRole] = useState<'Cliente' | 'Organizador'>('Cliente');

  const [passwordStrength, setPasswordStrength] = useState({ label: '', color: 'bg-muted', percentage: 0 });
  const [authError, setAuthError] = useState('');

  // Honeypot: campo oculto para detectar bots
  const [honeypot, setHoneypot] = useState('');

  // Formata o CPF em tempo real (000.000.000-00)
  const handleCpfChange = useCallback((value: string) => {
    const raw = value.replace(/\D/g, '').substring(0, 11);
    let formatted = raw;
    if (raw.length > 9) {
      formatted = `${raw.substring(0, 3)}.${raw.substring(3, 6)}.${raw.substring(6, 9)}-${raw.substring(9)}`;
    } else if (raw.length > 6) {
      formatted = `${raw.substring(0, 3)}.${raw.substring(3, 6)}.${raw.substring(6)}`;
    } else if (raw.length > 3) {
      formatted = `${raw.substring(0, 3)}.${raw.substring(3)}`;
    }
    setCpf(formatted);
  }, []);

  // Avaliador de força de senha
  useEffect(() => {
    if (!password) {
      setPasswordStrength({ label: '', color: 'bg-muted', percentage: 0 });
      return;
    }
    let points = 0;
    if (password.length >= 6) points += 1;
    if (password.length >= 10) points += 1;
    if (/[A-Z]/.test(password)) points += 1;
    if (/[0-9]/.test(password)) points += 1;
    if (/[^A-Za-z0-9]/.test(password)) points += 1;

    if (points <= 2) {
      setPasswordStrength({ label: 'Fraca', color: 'bg-destructive', percentage: 33 });
    } else if (points <= 4) {
      setPasswordStrength({ label: 'Média', color: 'bg-orange-500', percentage: 66 });
    } else {
      setPasswordStrength({ label: 'Forte', color: 'bg-green-600', percentage: 100 });
    }
  }, [password]);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    e.stopPropagation();

    console.log('[LOGIN] Form submitted', { isSignUp, email });

    setIsLoading(true);
    setAuthError('');

    // Honeypot check: se o campo oculto foi preenchido, é um bot
    if (honeypot) {
      console.warn('[SECURITY] Honeypot triggered — possível bot detectado');
      setAuthError('Erro ao processar. Tente novamente.');
      setIsLoading(false);
      return;
    }

    if (isSignUp) {
      if (name.trim().split(' ').length < 2) {
        toast.error('Informe seu nome completo (Nome e Sobrenome).');
        setIsLoading(false);
        return;
      }
      if (cpf.replace(/\D/g, '').length !== 11) {
        toast.error('Informe um CPF válido com 11 dígitos.');
        setIsLoading(false);
        return;
      }
      if (password !== confirmPassword) {
        toast.error('As senhas não coincidem');
        setIsLoading(false);
        return;
      }
    }

    try {
      if (isSignUp) {
        console.log('[LOGIN] Enviando registro para API...');
        const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            name,
            email,
            cpf: cpf.replace(/\D/g, ''),
            password,
            role
          }),
        });

        const data = await response.json();
        console.log('[LOGIN] Resposta do registro:', data);

        if (!data.success) {
          const errorMessage = data.errors && data.errors.length > 0
            ? `${data.message}: ${data.errors.join(', ')}`
            : (data.message || 'Erro ao criar conta');
          throw new Error(errorMessage);
        }

        localStorage.setItem('@BoraAli:token', data.data.token);
        localStorage.setItem('@BoraAli:user', JSON.stringify(data.data.user));

        toast.success('Conta criada com sucesso!');
        router.push('/');
        router.refresh();
      } else {
        console.log('[LOGIN] Enviando login para API...');
        const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ email, password }),
        });

        const data = await response.json();
        console.log('[LOGIN] Resposta do login:', data);

        if (!data.success) {
          const loginError = data.errors && data.errors.length > 0
            ? `${data.message}: ${data.errors.join(', ')}`
            : (data.message || 'E-mail ou senha incorretos');
          throw new Error(loginError);
        }

        localStorage.setItem('@BoraAli:token', data.data.token);
        localStorage.setItem('@BoraAli:user', JSON.stringify(data.data.user));

        toast.success('Login efetuado com sucesso!');
        router.push('/');
        router.refresh();
      }
    } catch (error: any) {
      const errorMessage = error.message || 'Erro ao conectar com o servidor';
      setAuthError(errorMessage);
      toast.error(errorMessage);
      console.error('[LOGIN] Erro na autenticação:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const toggleMode = useCallback(() => {
    setIsSignUp(prev => !prev);
    setName('');
    setCpf('');
    setConfirmPassword('');
    setRole('Cliente');
    setAuthError('');
  }, []);

  return (
    <div className="flex min-h-screen flex-col">
      <Header />

      <main className="flex flex-1 items-center justify-center bg-secondary/30 px-4 py-12">
        <div className="w-full max-w-md">
          <Link
            href="/"
            className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-primary"
          >
            <ArrowLeft className="h-4 w-4" />
            Voltar para eventos
          </Link>

          <Card className="shadow-lg">
            <CardHeader className="text-center">
              <Link href="/" className="mb-4 inline-flex items-center justify-center gap-1">
                <span className="text-2xl font-bold text-primary">Bora</span>
                <span className="text-2xl font-bold text-foreground">Ali</span>
              </Link>
              <CardTitle className="text-xl">
                {isSignUp ? 'Criar sua conta' : 'Entrar na sua conta'}
              </CardTitle>
              <CardDescription>
                {isSignUp
                  ? 'Crie sua conta para comprar ingressos e muito mais'
                  : 'Acesse sua conta para gerenciar seus ingressos'}
              </CardDescription>
            </CardHeader>

            <CardContent>
              <form onSubmit={handleLogin} className="space-y-4" noValidate>
                {/* Honeypot: campo oculto para detectar bots — humanos não veem, bots preenchem */}
                <input
                  type="text"
                  name="phone_alt"
                  value={honeypot}
                  onChange={(e) => setHoneypot(e.target.value)}
                  tabIndex={-1}
                  autoComplete="off"
                  className="opacity-0 absolute -z-10 h-0 w-0"
                />
                {isSignUp && (
                  <>
                    <div className="space-y-2">
                      <Label htmlFor="name">Nome completo</Label>
                      <Input
                        id="name"
                        placeholder="Nome e Sobrenome"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        disabled={isLoading}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="cpf">CPF</Label>
                      <Input
                        id="cpf"
                        placeholder="000.000.000-00"
                        value={cpf}
                        onChange={(e) => handleCpfChange(e.target.value)}
                        disabled={isLoading}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label>Você é um *</Label>
                      <div className="grid grid-cols-2 gap-3">
                        <button
                          type="button"
                          onClick={() => setRole('Cliente')}
                          disabled={isLoading}
                          className={`flex flex-col items-center gap-2 rounded-lg border-2 p-4 transition-all ${
                            role === 'Cliente'
                              ? 'border-primary bg-primary/5'
                              : 'border-border hover:border-primary/50 hover:bg-secondary/50'
                          }`}
                        >
                          <User className={`h-6 w-6 ${role === 'Cliente' ? 'text-primary' : 'text-muted-foreground'}`} />
                          <span className={`text-sm font-medium ${role === 'Cliente' ? 'text-primary' : 'text-foreground'}`}>
                            Cliente
                          </span>
                          <span className="text-xs text-muted-foreground">Quero comprar ingressos</span>
                        </button>
                        <button
                          type="button"
                          onClick={() => setRole('Organizador')}
                          disabled={isLoading}
                          className={`flex flex-col items-center gap-2 rounded-lg border-2 p-4 transition-all ${
                            role === 'Organizador'
                              ? 'border-primary bg-primary/5'
                              : 'border-border hover:border-primary/50 hover:bg-secondary/50'
                          }`}
                        >
                          <Briefcase className={`h-6 w-6 ${role === 'Organizador' ? 'text-primary' : 'text-muted-foreground'}`} />
                          <span className={`text-sm font-medium ${role === 'Organizador' ? 'text-primary' : 'text-foreground'}`}>
                            Organizador
                          </span>
                          <span className="text-xs text-muted-foreground">Quero criar eventos</span>
                        </button>
                      </div>
                    </div>
                  </>
                )}

                <div className="space-y-2">
                  <Label htmlFor="email">E-mail</Label>
                  <Input
                    id="email"
                    type="email"
                    placeholder="seu@email.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    disabled={isLoading}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="password">Senha</Label>
                  <div className="relative">
                    <Input
                      id="password"
                      type={showPassword ? 'text' : 'password'}
                      placeholder="••••••••"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      disabled={isLoading}
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                      tabIndex={-1}
                    >
                      {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                    </button>
                  </div>
                  {isSignUp && password && (
                    <div className="mt-2 space-y-1">
                      <div className="h-1.5 w-full bg-muted rounded-full overflow-hidden">
                        <div
                          className={`h-full transition-all duration-300 ${passwordStrength.color}`}
                          style={{ width: `${passwordStrength.percentage}%` }}
                        />
                      </div>
                      <span className="text-xs text-muted-foreground">
                        Força da senha: <strong>{passwordStrength.label}</strong>
                      </span>
                    </div>
                  )}
                </div>

                {isSignUp && (
                  <div className="space-y-2">
                    <Label htmlFor="confirmPassword">Confirmar senha</Label>
                    <Input
                      id="confirmPassword"
                      type="password"
                      placeholder="••••••••"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      disabled={isLoading}
                    />
                  </div>
                )}

                {authError && (
                  <div className="rounded-md bg-destructive/10 border border-destructive/30 px-4 py-3">
                    <p className="text-sm text-destructive font-medium">{authError}</p>
                  </div>
                )}

                <Button
                  type="submit"
                  className="w-full bg-primary text-primary-foreground hover:bg-primary/90"
                  disabled={isLoading}
                >
                  {isLoading ? 'Carregando...' : isSignUp ? 'Criar conta' : 'Entrar'}
                </Button>
              </form>

              <Separator className="my-6" />

              <p className="text-center text-sm text-muted-foreground">
                {isSignUp ? 'Já tem uma conta?' : 'Não tem uma conta?'}{' '}
                <button
                  type="button"
                  onClick={toggleMode}
                  className="font-medium text-primary hover:underline"
                  disabled={isLoading}
                >
                  {isSignUp ? 'Entrar' : 'Criar conta'}
                </button>
              </p>
            </CardContent>
          </Card>
        </div>
      </main>

      <Footer />
    </div>
  );
}
