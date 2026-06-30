'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import Image from 'next/image';
import { ArrowLeft, Pencil, Trash2, Plus, Calendar, MapPin, Loader2, TrendingUp, Ticket, DollarSign, BarChart3, Download, X } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { useAuth } from '@/hooks/use-auth';
import { ApiResponse, PagedResult, resolveImageUrl } from '@/lib/api-types';
import { toast } from 'sonner';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';

interface MyEvent {
  id: number;
  title: string;
  description: string;
  eventDate: string;
  time: string;
  location: string;
  city: string;
  imageUrl?: string;
  status: string;
  categoryName?: string;
  createdAt: string;
}

interface OrganizerStats {
  totalRevenue: number;
  totalTicketsSold: number;
  totalTicketsAvailable: number;
  totalEvents: number;
  totalOrders: number;
  revenueByTicketType: { name: string; sold: number; revenue: number }[];
  revenueByEvent: { eventId: number; title: string; ticketsSold: number; revenue: number }[];
}

interface SalesSummary {
  eventId: number;
  eventTitle: string;
  eventDate: string;
  totalRevenue: number;
  totalTicketsSold: number;
  totalTicketsAvailable: number;
  occupancyRate: number;
  totalOrders: number;
  confirmedOrders: number;
  pendingOrders: number;
  salesByTicketType: { ticketTypeId: number; ticketName: string; price: number; sold: number; available: number; revenue: number }[];
}

const API = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5188';

const statusLabels: Record<string, string> = {
  Draft: 'Rascunho',
  Published: 'Publicado',
  Cancelled: 'Cancelado',
  Finished: 'Finalizado',
};

const statusColors: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
  Published: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  Cancelled: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
  Finished: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
};

export default function MyEventsPage() {
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading, getAuthHeaders, user } = useAuth();
  const [events, setEvents] = useState<MyEvent[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [eventToDelete, setEventToDelete] = useState<MyEvent | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [stats, setStats] = useState<OrganizerStats | null>(null);
  const [isLoadingStats, setIsLoadingStats] = useState(true);

  // Sales summary modal state
  const [salesEvent, setSalesEvent] = useState<MyEvent | null>(null);
  const [salesSummary, setSalesSummary] = useState<SalesSummary | null>(null);
  const [isLoadingSales, setIsLoadingSales] = useState(false);

  const isOrganizer = user?.role === 'Organizador' || user?.role === 'Admin';

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [authLoading, isAuthenticated, router]);

  useEffect(() => {
    if (!isAuthenticated) return;

    async function fetchMyEvents() {
      try {
        const headers = getAuthHeaders();
        const res = await fetch(`${API}/api/events/my-events?pageSize=50`, { headers });
        const data: ApiResponse<PagedResult<MyEvent>> = await res.json();
        if (data.success) {
          setEvents(data.data.items);
        } else {
          setError(data.message || 'Erro ao carregar eventos');
        }
      } catch (err) {
        console.error('Erro ao carregar eventos:', err);
        setError('Erro ao conectar com o servidor');
      } finally {
        setIsLoading(false);
      }
    }

    fetchMyEvents();

    async function fetchStats() {
      try {
        const headers = getAuthHeaders();
        const res = await fetch(`${API}/api/events/stats`, { headers });
        const data = await res.json();
        if (data.success) setStats(data.data);
      } catch (err) {
        console.error('Erro ao carregar estatisticas:', err);
      } finally {
        setIsLoadingStats(false);
      }
    }
    fetchStats();
  }, [isAuthenticated, getAuthHeaders]);

  const handleDelete = async () => {
    if (!eventToDelete) return;
    setIsDeleting(true);
    try {
      const headers = getAuthHeaders();
      const res = await fetch(`${API}/api/events/${eventToDelete.id}`, { method: 'DELETE', headers });
      const data = await res.json();
      if (data.success) {
        setEvents((prev) => prev.filter((e) => e.id !== eventToDelete.id));
        toast.success('Evento excluido com sucesso!');
      } else {
        toast.error(data.message || 'Erro ao excluir evento');
      }
    } catch (err) {
      toast.error('Erro ao conectar com o servidor');
    } finally {
      setIsDeleting(false);
      setEventToDelete(null);
    }
  };

  const handleViewSales = async (event: MyEvent) => {
    setSalesEvent(event);
    setIsLoadingSales(true);
    setSalesSummary(null);
    try {
      const headers = getAuthHeaders();
      const res = await fetch(`${API}/api/events/${event.id}/sales-summary`, { headers });
      const data: ApiResponse<SalesSummary> = await res.json();
      if (data.success) {
        setSalesSummary(data.data);
      } else {
        toast.error(data.message || 'Erro ao carregar resumo de vendas');
      }
    } catch (err) {
      toast.error('Erro ao conectar com o servidor');
    } finally {
      setIsLoadingSales(false);
    }
  };

  const handleExportCSV = () => {
    if (!stats) return;
    const rows: string[] = ['Tipo,Nome,Vendidos,Receita (R$)'];
    for (const item of stats.revenueByTicketType) {
      rows.push(`Ingresso,${item.name},${item.sold},${item.revenue.toFixed(2)}`);
    }
    for (const item of stats.revenueByEvent) {
      rows.push(`Evento,${item.title},${item.ticketsSold},${item.revenue.toFixed(2)}`);
    }
    rows.push('');
    rows.push(`Receita Total,,,${stats.totalRevenue.toFixed(2)}`);
    rows.push(`Ingressos Vendidos,,${stats.totalTicketsSold},`);
    rows.push(`Total Pedidos,,${stats.totalOrders},`);
    rows.push(`Total Eventos,,${stats.totalEvents},`);

    const csv = rows.join('\n');
    const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `vendas-boraali-${new Date().toISOString().slice(0, 10)}.csv`;
    link.click();
    URL.revokeObjectURL(url);
    toast.success('Relatorio exportado com sucesso!');
  };

  const formatDate = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' }).replace('.', '');
    } catch {
      return dateStr;
    }
  };

  if (authLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <div className="mb-4 h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto" />
            <p className="text-muted-foreground">Verificando autenticacao...</p>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  if (!isAuthenticated) return null;

  return (
    <div className="flex min-h-screen flex-col">
      <Header />

      <main className="flex-1 bg-secondary/30 py-8">
        <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8 flex flex-wrap items-center justify-between gap-4">
            <div>
              <Link
                href="/"
                className="mb-2 inline-flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-primary"
              >
                <ArrowLeft className="h-4 w-4" />
                Voltar para eventos
              </Link>
              <h1 className="text-2xl font-bold text-foreground sm:text-3xl">
                Meus Eventos
              </h1>
              <p className="mt-1 text-muted-foreground">
                Gerencie os eventos que voce criou
              </p>
            </div>

            {isOrganizer && (
              <div className="flex gap-2">
                {!isLoadingStats && stats && stats.totalOrders > 0 && (
                  <Button variant="outline" className="gap-2" onClick={handleExportCSV}>
                    <Download className="h-4 w-4" />
                    Exportar
                  </Button>
                )}
                <Link href="/criar-evento">
                  <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90">
                    <Plus className="h-4 w-4" />
                    Criar Evento
                  </Button>
                </Link>
              </div>
            )}
          </div>

          {/* Dashboard Analitico */}
          {!isLoadingStats && stats && stats.totalEvents > 0 && (
            <div className="mb-8 space-y-6">
              {/* KPI Cards */}
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <Card>
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-green-100 dark:bg-green-900/30">
                        <DollarSign className="h-5 w-5 text-green-600 dark:text-green-400" />
                      </div>
                      <div>
                        <p className="text-xs text-muted-foreground">Receita Total</p>
                        <p className="text-xl font-bold text-foreground">
                          R$ {stats.totalRevenue.toFixed(2).replace('.', ',')}
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-100 dark:bg-blue-900/30">
                        <Ticket className="h-5 w-5 text-blue-600 dark:text-blue-400" />
                      </div>
                      <div>
                        <p className="text-xs text-muted-foreground">Ingressos Vendidos</p>
                        <p className="text-xl font-bold text-foreground">
                          {stats.totalTicketsSold} / {stats.totalTicketsAvailable}
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-purple-100 dark:bg-purple-900/30">
                        <TrendingUp className="h-5 w-5 text-purple-600 dark:text-purple-400" />
                      </div>
                      <div>
                        <p className="text-xs text-muted-foreground">Total de Pedidos</p>
                        <p className="text-xl font-bold text-foreground">{stats.totalOrders}</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-amber-100 dark:bg-amber-900/30">
                        <Calendar className="h-5 w-5 text-amber-600 dark:text-amber-400" />
                      </div>
                      <div>
                        <p className="text-xs text-muted-foreground">Eventos</p>
                        <p className="text-xl font-bold text-foreground">{stats.totalEvents}</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Charts */}
              <div className="grid gap-6 lg:grid-cols-2">
                {/* Revenue by Event */}
                <Card>
                  <CardHeader>
                    <CardTitle className="text-base">Receita por Evento</CardTitle>
                  </CardHeader>
                  <CardContent>
                    {stats.revenueByEvent.length > 0 ? (
                      <ResponsiveContainer width="100%" height={250}>
                        <BarChart data={stats.revenueByEvent}>
                          <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                          <XAxis dataKey="title" tick={{ fontSize: 11 }} angle={-20} textAnchor="end" height={60} />
                          <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `R$${v}`} />
                          <Tooltip formatter={(v: number) => [`R$ ${v.toFixed(2)}`, 'Receita']} />
                          <Bar dataKey="revenue" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                        </BarChart>
                      </ResponsiveContainer>
                    ) : (
                      <p className="py-8 text-center text-sm text-muted-foreground">Nenhuma venda ainda</p>
                    )}
                  </CardContent>
                </Card>

                {/* Revenue by Ticket Type */}
                <Card>
                  <CardHeader>
                    <CardTitle className="text-base">Receita por Tipo de Ingresso</CardTitle>
                  </CardHeader>
                  <CardContent>
                    {stats.revenueByTicketType.length > 0 ? (
                      <ResponsiveContainer width="100%" height={250}>
                        <PieChart>
                          <Pie
                            data={stats.revenueByTicketType}
                            dataKey="revenue"
                            nameKey="name"
                            cx="50%"
                            cy="50%"
                            outerRadius={80}
                            label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                          >
                            {stats.revenueByTicketType.map((_, i) => (
                              <Cell
                                key={i}
                                fill={['hsl(var(--primary))', '#f59e0b', '#10b981', '#8b5cf6', '#ec4899'][i % 5]}
                              />
                            ))}
                          </Pie>
                          <Tooltip formatter={(v: number) => [`R$ ${v.toFixed(2)}`, 'Receita']} />
                        </PieChart>
                      </ResponsiveContainer>
                    ) : (
                      <p className="py-8 text-center text-sm text-muted-foreground">Nenhuma venda ainda</p>
                    )}
                  </CardContent>
                </Card>
              </div>
            </div>
          )}

          {/* Content */}
          {isLoading ? (
            <div className="flex items-center justify-center py-20">
              <Loader2 className="h-8 w-8 animate-spin text-primary" />
              <p className="ml-3 text-muted-foreground">Carregando seus eventos...</p>
            </div>
          ) : error ? (
            <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-center">
              <p className="text-destructive font-medium">{error}</p>
              <Button variant="outline" className="mt-4" onClick={() => window.location.reload()}>
                Tentar novamente
              </Button>
            </div>
          ) : events.length === 0 ? (
            <div className="rounded-lg border border-border bg-card p-12 text-center">
              <p className="mb-2 text-lg font-medium text-foreground">
                Nenhum evento criado
              </p>
              <p className="mb-6 text-muted-foreground">
                Voce ainda nao criou nenhum evento. Comece agora mesmo!
              </p>
              {isOrganizer && (
                <Link href="/criar-evento">
                  <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90">
                    <Plus className="h-4 w-4" />
                    Criar Primeiro Evento
                  </Button>
                </Link>
              )}
            </div>
          ) : (
            <div className="space-y-4">
              {events.map((event) => (
                <Card key={event.id} className="overflow-hidden transition-shadow hover:shadow-md">
                  <CardContent className="p-0">
                    <div className="flex flex-col sm:flex-row">
                      {/* Thumbnail */}
                      <div className="relative h-40 w-full shrink-0 sm:h-auto sm:w-48">
                        <Image
                          src={resolveImageUrl(event.imageUrl)}
                          alt={event.title}
                          fill
                          className="object-cover"
                        />
                      </div>

                      {/* Info */}
                      <div className="flex flex-1 flex-col justify-between p-4 sm:p-5">
                        <div>
                          <div className="mb-2 flex flex-wrap items-center gap-2">
                            <Badge className={statusColors[event.status] || statusColors.Draft}>
                              {statusLabels[event.status] || event.status}
                            </Badge>
                            {event.categoryName && (
                              <Badge variant="outline">{event.categoryName}</Badge>
                            )}
                          </div>
                          <h3 className="mb-1 text-lg font-semibold text-foreground">
                            {event.title}
                          </h3>
                          <p className="mb-2 line-clamp-2 text-sm text-muted-foreground">
                            {event.description}
                          </p>
                          <div className="flex flex-wrap gap-x-6 gap-y-1 text-sm text-muted-foreground">
                            <span className="flex items-center gap-1">
                              <Calendar className="h-3.5 w-3.5" />
                              {formatDate(event.eventDate)} as {event.time}
                            </span>
                            <span className="flex items-center gap-1">
                              <MapPin className="h-3.5 w-3.5" />
                              {event.location}, {event.city}
                            </span>
                          </div>
                        </div>

                        {/* Actions */}
                        <div className="mt-4 flex flex-wrap gap-2">
                          <Link href={`/evento/${event.id}`}>
                            <Button variant="outline" size="sm">
                              Ver evento
                            </Button>
                          </Link>
                          <Link href={`/editar-evento/${event.id}`}>
                            <Button variant="outline" size="sm" className="gap-1">
                              <Pencil className="h-3.5 w-3.5" />
                              Editar
                            </Button>
                          </Link>
                          <Button
                            variant="outline"
                            size="sm"
                            className="gap-1"
                            onClick={() => handleViewSales(event)}
                          >
                            <BarChart3 className="h-3.5 w-3.5" />
                            Vendas
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            className="gap-1 border-destructive/30 text-destructive hover:bg-destructive/10 hover:text-destructive"
                            onClick={() => setEventToDelete(event)}
                          >
                            <Trash2 className="h-3.5 w-3.5" />
                            Excluir
                          </Button>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      </main>

      <Footer />

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!eventToDelete} onOpenChange={() => setEventToDelete(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Excluir evento</AlertDialogTitle>
            <AlertDialogDescription>
              Tem certeza que deseja excluir <strong>{eventToDelete?.title}</strong>?
              Esta acao nao pode ser desfeita. Todos os dados do evento, incluindo
              tipos de ingresso e pedidos associados, serao perdidos.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isDeleting}>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              disabled={isDeleting}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {isDeleting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Excluindo...
                </>
              ) : (
                'Sim, excluir evento'
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Sales Summary Modal */}
      <Dialog open={!!salesEvent} onOpenChange={() => { setSalesEvent(null); setSalesSummary(null); }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <BarChart3 className="h-5 w-5 text-primary" />
              Vendas: {salesEvent?.title}
            </DialogTitle>
          </DialogHeader>

          {isLoadingSales ? (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
          ) : salesSummary ? (
            <div className="space-y-4">
              {/* Occupancy Rate */}
              <div className="rounded-lg bg-secondary/50 p-4">
                <div className="mb-2 flex items-center justify-between">
                  <span className="text-sm font-medium">Taxa de Ocupacao</span>
                  <span className="text-lg font-bold text-primary">{salesSummary.occupancyRate}%</span>
                </div>
                <div className="h-2 w-full rounded-full bg-muted">
                  <div
                    className="h-2 rounded-full bg-primary transition-all"
                    style={{ width: `${Math.min(salesSummary.occupancyRate, 100)}%` }}
                  />
                </div>
              </div>

              {/* Summary Cards */}
              <div className="grid grid-cols-2 gap-3 text-sm">
                <div className="rounded-lg border p-3">
                  <p className="text-muted-foreground">Receita Total</p>
                  <p className="text-lg font-bold text-green-600 dark:text-green-400">
                    R$ {salesSummary.totalRevenue.toFixed(2).replace('.', ',')}
                  </p>
                </div>
                <div className="rounded-lg border p-3">
                  <p className="text-muted-foreground">Ingressos Vendidos</p>
                  <p className="text-lg font-bold">
                    {salesSummary.totalTicketsSold} / {salesSummary.totalTicketsAvailable}
                  </p>
                </div>
                <div className="rounded-lg border p-3">
                  <p className="text-muted-foreground">Pedidos Confirmados</p>
                  <p className="text-lg font-bold text-blue-600 dark:text-blue-400">
                    {salesSummary.confirmedOrders}
                  </p>
                </div>
                <div className="rounded-lg border p-3">
                  <p className="text-muted-foreground">Pedidos Pendentes</p>
                  <p className="text-lg font-bold text-amber-600 dark:text-amber-400">
                    {salesSummary.pendingOrders}
                  </p>
                </div>
              </div>

              {/* Ticket Type Breakdown */}
              {salesSummary.salesByTicketType.length > 0 && (
                <div>
                  <p className="mb-2 text-sm font-medium">Vendas por Tipo de Ingresso</p>
                  <div className="space-y-2">
                    {salesSummary.salesByTicketType.map((t) => (
                      <div key={t.ticketTypeId} className="flex items-center justify-between rounded-lg border p-3 text-sm">
                        <div>
                          <p className="font-medium">{t.ticketName}</p>
                          <p className="text-xs text-muted-foreground">
                            R$ {t.price.toFixed(2).replace('.', ',')} cada
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="font-bold">{t.sold} vendidos</p>
                          <p className="text-xs text-muted-foreground">{t.available} disponiveis</p>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ) : (
            <p className="py-8 text-center text-sm text-muted-foreground">
              Nao foi possivel carregar o resumo de vendas.
            </p>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
