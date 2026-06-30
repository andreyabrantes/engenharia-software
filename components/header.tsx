'use client';

import Link from 'next/link';
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Search, Menu, User, Plus, LogOut, ChevronDown, Briefcase, Ticket, ListOrdered, Pencil, Heart, Users, Home } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  Sheet,
  SheetContent,
  SheetTrigger,
} from '@/components/ui/sheet';
import { useAuth } from '@/hooks/use-auth';
import type { ApiCategory, ApiResponse } from '@/lib/api-types';

const API_BASE_URL = 'http://localhost:5188';

export function Header() {
  const { user, isAuthenticated, isLoading, logout } = useAuth();
  const router = useRouter();
  const [searchQuery, setSearchQuery] = useState('');
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [categories, setCategories] = useState<ApiCategory[]>([]);

  const isOrganizer = user?.role === 'Organizador' || user?.role === 'Admin';

  useEffect(() => {
    async function fetchCategories() {
      try {
        const res = await fetch(`${API_BASE_URL}/api/events/categories`);
        const data: ApiResponse<ApiCategory[]> = await res.json();
        if (data.success) {
          setCategories(data.data);
        }
      } catch {
        // fallback silencioso — categorias vazias não quebram o layout
      }
    }
    fetchCategories();
  }, []);

  const handleLogout = () => {
    logout();
    router.push('/');
  };

  const getUserInitials = () => {
    if (!user?.name) return '?';
    return user.name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        {/* Logo */}
        <Link href="/" className="flex items-center gap-2">
          <span className="text-2xl font-bold text-primary">Bora</span>
          <span className="text-2xl font-bold text-foreground">Ali</span>
        </Link>

        {/* Desktop Search Bar */}
        <div className="hidden flex-1 items-center justify-center px-8 md:flex">
          <div className="relative w-full max-w-xl">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              type="search"
              placeholder="Buscar eventos, shows, workshops..."
              className="w-full pl-10 pr-4"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>
        </div>

        {/* Desktop Navigation */}
        <nav className="hidden items-center gap-4 lg:flex">
          {isOrganizer && (
            <Link href="/criar-evento">
              <Button variant="outline" className="gap-2">
                <Plus className="h-4 w-4" />
                Criar Evento
              </Button>
            </Link>
          )}

          {isLoading ? (
            <div className="h-9 w-24 animate-pulse rounded-md bg-muted" />
          ) : isAuthenticated && user ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="gap-2 px-2">
                  <Avatar className="h-8 w-8">
                    <AvatarImage src={user.avatarUrl} alt={user.name} />
                    <AvatarFallback className="bg-primary text-primary-foreground text-xs">
                      {getUserInitials()}
                    </AvatarFallback>
                  </Avatar>
                  <span className="hidden text-sm font-medium lg:inline">
                    {user.name.split(' ')[0]}
                  </span>
                  <ChevronDown className="h-4 w-4 text-muted-foreground" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <div className="px-2 py-1.5">
                  <p className="text-sm font-medium">{user.name}</p>
                  <p className="text-xs text-muted-foreground">{user.email}</p>
                  <div className="mt-1 flex items-center gap-1">
                    {isOrganizer ? (
                      <span className="inline-flex items-center gap-1 rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
                        <Briefcase className="h-3 w-3" />
                        Organizador
                      </span>
                    ) : (
                      <span className="inline-flex items-center gap-1 rounded-full bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-700 dark:bg-blue-900/30 dark:text-blue-400">
                        <Ticket className="h-3 w-3" />
                        Cliente
                      </span>
                    )}
                  </div>
                </div>
                <DropdownMenuSeparator />
                {isOrganizer && (
                  <>
                    <DropdownMenuItem onClick={() => router.push('/criar-evento')}>
                      <Plus className="mr-2 h-4 w-4" />
                      Criar Evento
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => router.push('/meus-eventos')}>
                      <Pencil className="mr-2 h-4 w-4" />
                      Meus Eventos
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                  </>
                )}
                                <DropdownMenuItem onClick={() => router.push('/favoritos')}>
                                  <Heart className="mr-2 h-4 w-4" />
                                  Meus Favoritos
                                </DropdownMenuItem>
                                <DropdownMenuItem onClick={() => router.push('/seguindo')}>
                                  <Users className="mr-2 h-4 w-4" />
                                  Seguindo
                                </DropdownMenuItem>
                                <DropdownMenuItem onClick={() => router.push('/meus-pedidos')}>
                  <ListOrdered className="mr-2 h-4 w-4" />
                  Meus Pedidos
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={handleLogout} className="text-destructive focus:text-destructive">
                  <LogOut className="mr-2 h-4 w-4" />
                  Sair
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <Link href="/login">
              <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90">
                <User className="h-4 w-4" />
                Entrar
              </Button>
            </Link>
          )}
        </nav>

        {/* Mobile Actions */}
        <div className="flex items-center gap-2 lg:hidden">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setIsSearchOpen(!isSearchOpen)}
          >
            <Search className="h-5 w-5" />
          </Button>
          
          <Sheet>
            <SheetTrigger asChild>
              <Button variant="ghost" size="icon">
                <Menu className="h-5 w-5" />
              </Button>
            </SheetTrigger>
            <SheetContent side="right" className="w-[300px] sm:w-[400px]">
              <nav className="flex flex-col gap-4 pt-8">
                <Link href="/" className="text-lg font-medium">
                  Início
                </Link>

                {/* User info on mobile */}
                {!isLoading && isAuthenticated && user && (
                  <div className="flex items-center gap-3 rounded-lg border border-border p-3">
                    <Avatar className="h-10 w-10">
                      <AvatarImage src={user.avatarUrl} alt={user.name} />
                      <AvatarFallback className="bg-primary text-primary-foreground text-xs">
                        {getUserInitials()}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1">
                      <p className="text-sm font-medium">{user.name}</p>
                      <p className="text-xs text-muted-foreground">{user.email}</p>
                    </div>
                  </div>
                )}

                <div className="border-t border-border pt-4">
                  <p className="mb-2 text-sm font-medium text-muted-foreground">
                    Categorias
                  </p>
                                    {categories.map((category) => (
                                      <Link
                                        key={category.id}
                                        href={`/?categoria=${category.slug}`}
                                        replace
                                        scroll={false}
                                        className="block py-2 text-foreground hover:text-primary"
                                      >
                                        {category.name}
                                      </Link>
                                    ))}
                </div>
                <div className="border-t border-border pt-4">
                  {isOrganizer && (
                    <>
                      <Link href="/criar-evento" className="block py-2">
                        <Button variant="outline" className="w-full gap-2">
                          <Plus className="h-4 w-4" />
                          Criar Evento
                        </Button>
                      </Link>
                      <Link href="/meus-eventos" className="block py-2">
                        <Button variant="outline" className="w-full gap-2">
                          <Pencil className="h-4 w-4" />
                          Meus Eventos
                        </Button>
                      </Link>
                    </>
                  )}
                                    <Link href="/favoritos" className="block py-2">
                                      <Button variant="outline" className="w-full gap-2">
                                        <Heart className="h-4 w-4" />
                                        Meus Favoritos
                                      </Button>
                                    </Link>
                                    <Link href="/seguindo" className="block py-2">
                                      <Button variant="outline" className="w-full gap-2">
                                        <Users className="h-4 w-4" />
                                        Seguindo
                                      </Button>
                                    </Link>
                                    <Link href="/meus-pedidos" className="block py-2">
                    <Button variant="outline" className="w-full gap-2">
                      <ListOrdered className="h-4 w-4" />
                      Meus Pedidos
                    </Button>
                  </Link>
                  {isAuthenticated ? (
                    <Button
                      variant="ghost"
                      className="mt-2 w-full gap-2 text-destructive"
                      onClick={handleLogout}
                    >
                      <LogOut className="h-4 w-4" />
                      Sair
                    </Button>
                  ) : (
                    <Link href="/login" className="mt-2 block py-2">
                      <Button className="w-full gap-2 bg-primary text-primary-foreground">
                        <User className="h-4 w-4" />
                        Entrar
                      </Button>
                    </Link>
                  )}
                </div>
              </nav>
            </SheetContent>
          </Sheet>
        </div>
      </div>

      {/* Mobile Search */}
      {isSearchOpen && (
        <div className="border-t border-border px-4 py-3 md:hidden">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              type="search"
              placeholder="Buscar eventos..."
              className="w-full pl-10 pr-4"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              autoFocus
            />
          </div>
        </div>
      )}

            {/* Categories Bar - Desktop */}
      {categories.length > 0 && (
        <div className="hidden border-t border-border bg-secondary/50 md:block">
          <div className="mx-auto flex max-w-7xl items-center gap-6 overflow-x-auto px-4 py-2 sm:px-6 lg:px-8">
            <Link
              href="/"
              replace
              scroll={false}
              className="flex shrink-0 items-center gap-1.5 whitespace-nowrap text-sm font-medium text-foreground transition-colors hover:text-primary"
            >
              <Home className="h-4 w-4" />
              Início
            </Link>
            <span className="h-4 w-px shrink-0 bg-border" />
                        {categories.map((category) => {
                          const href = `/?categoria=${category.slug}`;
                          return (
                            <Link
                              key={category.id}
                              href={href}
                              replace
                              scroll={false}
                              className="flex shrink-0 items-center gap-2 whitespace-nowrap text-sm text-muted-foreground transition-colors hover:text-primary"
                            >
                              <span>{category.name}</span>
                            </Link>
                          );
                        })}
          </div>
        </div>
      )}
    </header>
  );
}
