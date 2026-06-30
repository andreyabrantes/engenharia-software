'use client';

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, Calendar, MapPin, CreditCard, Loader2, AlertCircle, Ticket, QrCode, Trash2, ShoppingBag, CheckCircle, QrCode as QrCodeIcon, FileText, ShieldCheck, Lock, AlertTriangle } from 'lucide-react';
import QRCode from 'qrcode';
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
import { useAuth } from '@/hooks/use-auth';
import { ApiResponse, resolveImageUrl } from '@/lib/api-types';
import { toast } from 'sonner';

interface OrderDto {
  id: number;
  orderCode: string;
  totalAmount: number;
  status: string;
  paymentMethod?: string;
  createdAt: string;
  confirmedAt?: string;
  eventId: number;
  eventTitle?: string;
  eventImageUrl?: string;
  eventDate?: string;
  eventLocation?: string;
}

const statusLabels: Record<string, string> = {
  Pending: 'Pendente',
  Confirmed: 'Confirmado',
  Cancelled: 'Cancelado',
  Refunded: 'Reembolsado',
};

const statusColors: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400',
  Confirmed: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  Cancelled: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
  Refunded: 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400',
};

const paymentLabels: Record<string, string> = {
  credit_card: 'Cartão de Crédito',
  pix: 'Pix',
  boleto: 'Boleto',
};

export default function MyOrdersPage() {
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading, getAuthHeaders } = useAuth();
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
        const [orderToRefund, setOrderToRefund] = useState<OrderDto | null>(null);
  const [isRefunding, setIsRefunding] = useState(false);

    // Payment modal state
    const [orderToPay, setOrderToPay] = useState<OrderDto | null>(null);
    const [isPaying, setIsPaying] = useState(false);
    const [selectedPaymentMethod, setSelectedPaymentMethod] = useState<string>('pix');
    const [paymentStep, setPaymentStep] = useState<'choose' | 'simulate'>('choose');
    // Card form fields
    const [cardForm, setCardForm] = useState({
      cardNumber: '',
      cardName: '',
      cardExpiry: '',
      cardCvc: '',
      cpf: '',
    });
    const [cardErrors, setCardErrors] = useState<Record<string, string>>({});

    const hasRefundedOrders = orders.some((o) => o.status === 'Refunded');
  const hasPendingOrders = orders.some((o) => o.status === 'Pending');

  const API_BASE_URL = 'http://localhost:5188';

  const clearRefundedOrders = async () => {
    const headers = getAuthHeaders();
    const refundedOrders = orders.filter((o) => o.status === 'Refunded');

    let deletedCount = 0;
    for (const order of refundedOrders) {
      try {
        const res = await fetch(`${API_BASE_URL}/api/orders/${order.id}`, {
          method: 'DELETE',
          headers,
        });
        const data = await res.json();
        if (data.success) deletedCount++;
      } catch {
        // continua mesmo se um falhar
      }
    }

    if (deletedCount > 0) {
      setOrders((prev) => prev.filter((o) => o.status !== 'Refunded'));
      toast.success(`${deletedCount} pedido(s) reembolsado(s) excluído(s) permanentemente`);
    } else {
      toast.error('Erro ao excluir pedidos');
    }
  };

    const removeOrder = async (orderId: number) => {
    const headers = getAuthHeaders();
    try {
      const res = await fetch(`${API_BASE_URL}/api/orders/${orderId}`, {
        method: 'DELETE',
        headers,
      });
      const data = await res.json();
      if (data.success) {
        setOrders((prev) => prev.filter((o) => o.id !== orderId));
        toast.success('Pedido excluído permanentemente');
      } else {
        toast.error(data.message || 'Erro ao excluir pedido');
      }
    } catch {
      toast.error('Erro ao conectar com o servidor');
    }
  };

    const handlePayOrder = async () => {
      if (!orderToPay) return;
      setIsPaying(true);

    try {
      const headers = getAuthHeaders();
      const res = await fetch(`${API_BASE_URL}/api/orders/${orderToPay.id}/confirm-payment`, {
        method: 'POST',
        headers,
      });
      const data = await res.json();

      if (data.success) {
        setOrders((prev) =>
          prev.map((o) => (o.id === orderToPay.id ? { ...o, status: 'Confirmed' } : o))
        );
        toast.success('✅ Pagamento confirmado! Seus ingressos estão garantidos.');
        setOrderToPay(null);
        setPaymentStep('choose');
      } else {
        toast.error(data.message || 'Erro ao confirmar pagamento');
      }
    } catch {
      toast.error('Erro ao conectar com o servidor');
    } finally {
      setIsPaying(false);
    }
  };

    const resetPaymentState = () => {
    setOrderToPay(null);
    setPaymentStep('choose');
    setSelectedPaymentMethod('pix');
    setCardForm({ cardNumber: '', cardName: '', cardExpiry: '', cardCvc: '', cpf: '' });
    setCardErrors({});
  };

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [authLoading, isAuthenticated, router]);

  useEffect(() => {
    if (!isAuthenticated) return;

    async function fetchOrders() {
      try {
        const headers = getAuthHeaders();
        const res = await fetch(`${API_BASE_URL}/api/orders`, { headers });
        const data: ApiResponse<OrderDto[]> = await res.json();

        if (data.success) {
          setOrders(data.data);
        } else {
          setError(data.message || 'Erro ao carregar pedidos');
        }
      } catch (err) {
        console.error('Erro ao carregar pedidos:', err);
        setError('Erro ao conectar com o servidor');
      } finally {
        setIsLoading(false);
      }
    }

    fetchOrders();
  }, [isAuthenticated, getAuthHeaders]);

  const handleRefund = async () => {
    if (!orderToRefund) return;
    setIsRefunding(true);

    try {
      const headers = getAuthHeaders();
      const res = await fetch(`${API_BASE_URL}/api/orders/${orderToRefund.id}/refund`, {
        method: 'POST',
        headers,
      });
      const data = await res.json();

      if (data.success) {
        setOrders((prev) =>
          prev.map((o) => (o.id === orderToRefund.id ? { ...o, status: 'Refunded' } : o))
        );
        toast.success('Reembolso solicitado com sucesso!');
      } else {
        toast.error(data.message || 'Erro ao solicitar reembolso');
      }
    } catch (err) {
      toast.error('Erro ao conectar com o servidor');
    } finally {
      setIsRefunding(false);
      setOrderToRefund(null);
    }
  };

    /**
   * Verifica se o reembolso está disponível para um pedido.
   * Regra de negócio: apenas pedidos Confirmed com evento futuro.
   */
  const canRefund = (order: OrderDto): boolean => {
    if (order.status !== 'Confirmed') return false;
    if (!order.eventDate) return true;
    const eventDate = new Date(order.eventDate);
    return eventDate > new Date();
  };

  const formatDate = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
      }).replace('.', '');
    } catch {
      return dateStr;
    }
  };

  const formatDateTime = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      }).replace('.', '');
    } catch {
      return dateStr;
    }
  };

    const formatCurrency = (value: number) => {
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  };

  const formatCardNumber = (value: string) => {
    return value.replace(/\D/g, '').replace(/(\d{4})(\d)/, '$1 $2').replace(/(\d{4})(\d)/, '$1 $2').replace(/(\d{4})(\d)/, '$1 $2').replace(/(\d{4})\d+?$/, '$1');
  };

  const formatExpiry = (value: string) => {
    return value.replace(/\D/g, '').replace(/(\d{2})(\d)/, '$1/$2').replace(/(\/\d{2})\d+?$/, '$1');
  };

  const formatCPF = (value: string) => {
    return value.replace(/\D/g, '').replace(/(\d{3})(\d)/, '$1.$2').replace(/(\d{3})(\d)/, '$1.$2').replace(/(\d{3})(\d{1,2})/, '$1-$2').replace(/(-\d{2})\d+?$/, '$1');
  };

  const updateCardField = (field: string, value: string) => {
    let formatted = value;
    if (field === 'cardNumber') formatted = formatCardNumber(value);
    else if (field === 'cardExpiry') formatted = formatExpiry(value);
    else if (field === 'cpf') formatted = formatCPF(value);
    setCardForm(prev => ({ ...prev, [field]: formatted }));
    setCardErrors(prev => ({ ...prev, [field]: '' }));
  };

  if (authLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <div className="mb-4 h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto" />
            <p className="text-muted-foreground">Verificando autenticação...</p>
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
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <Link
              href="/"
              className="mb-2 inline-flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-primary"
            >
              <ArrowLeft className="h-4 w-4" />
              Voltar para eventos
            </Link>
                        <h1 className="text-2xl font-bold text-foreground sm:text-3xl">
              Meus Pedidos
            </h1>
            <p className="mt-1 text-muted-foreground">
              Histórico de compras e ingressos
            </p>
          </div>

                    {hasRefundedOrders && (
            <div className="mb-6 flex items-center justify-between rounded-lg border border-purple-200 bg-purple-50 p-4 dark:border-purple-900/50 dark:bg-purple-950/30">
              <p className="text-sm text-purple-700 dark:text-purple-400">
                Você possui pedidos reembolsados na lista.
              </p>
              <Button
                variant="outline"
                size="sm"
                onClick={clearRefundedOrders}
                className="border-purple-300 text-purple-700 hover:bg-purple-100 dark:border-purple-800 dark:text-purple-400 dark:hover:bg-purple-900/50"
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Limpar reembolsados
              </Button>
            </div>
          )}

          {hasPendingOrders && (
            <div className="mb-6 rounded-lg border border-yellow-200 bg-yellow-50 p-4 dark:border-yellow-900/50 dark:bg-yellow-950/30">
              <div className="flex items-center gap-3">
                <ShoppingBag className="h-5 w-5 text-yellow-600 dark:text-yellow-400" />
                <div>
                  <p className="text-sm font-medium text-yellow-800 dark:text-yellow-300">
                    Você possui pedidos pendentes de pagamento
                  </p>
                  <p className="text-xs text-yellow-600 dark:text-yellow-400">
                    Finalize o pagamento para garantir seus ingressos
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Content */}
          {isLoading ? (
            <div className="flex items-center justify-center py-20">
              <Loader2 className="h-8 w-8 animate-spin text-primary" />
              <p className="ml-3 text-muted-foreground">Carregando seus pedidos...</p>
            </div>
          ) : error ? (
            <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-center">
              <AlertCircle className="mx-auto mb-3 h-8 w-8 text-destructive" />
              <p className="font-medium text-destructive">{error}</p>
              <Button
                variant="outline"
                className="mt-4"
                onClick={() => window.location.reload()}
              >
                Tentar novamente
              </Button>
            </div>
          ) : orders.length === 0 ? (
            <div className="rounded-lg border border-border bg-card p-12 text-center">
              <Ticket className="mx-auto mb-4 h-12 w-12 text-muted-foreground/50" />
              <p className="mb-2 text-lg font-medium text-foreground">
                Nenhum pedido encontrado
              </p>
              <p className="mb-6 text-muted-foreground">
                Você ainda não realizou nenhuma compra. Explore os eventos disponíveis!
              </p>
              <Link href="/">
                <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90">
                  Ver eventos
                </Button>
              </Link>
            </div>
          ) : (
            <div className="space-y-4">
              {orders.map((order) => {
                const refundAvailable = canRefund(order);

                return (
                  <Card key={order.id} className="overflow-hidden transition-shadow hover:shadow-md">
                    <CardContent className="p-5">
                      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                        {/* Order Info */}
                        <div className="flex-1 space-y-3">
                          <div className="flex flex-wrap items-center gap-2">
                            <Badge className={statusColors[order.status] || statusColors.Pending}>
                              {statusLabels[order.status] || order.status}
                            </Badge>
                            <span className="text-sm font-mono text-muted-foreground">
                              #{order.orderCode}
                            </span>
                          </div>

                          {order.eventTitle && (
                            <div>
                              <Link
                                href={`/evento/${order.eventId}`}
                                className="text-lg font-semibold text-foreground hover:text-primary transition-colors"
                              >
                                {order.eventTitle}
                              </Link>
                              <div className="mt-1 flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
                                {order.eventDate && (
                                  <span className="flex items-center gap-1">
                                    <Calendar className="h-3.5 w-3.5" />
                                    {formatDate(order.eventDate)}
                                  </span>
                                )}
                                {order.eventLocation && (
                                  <span className="flex items-center gap-1">
                                    <MapPin className="h-3.5 w-3.5" />
                                    {order.eventLocation}
                                  </span>
                                )}
                              </div>
                            </div>
                          )}

                          <div className="flex flex-wrap items-center gap-x-6 gap-y-1 text-sm text-muted-foreground">
                            <span>
                              Comprado em {formatDateTime(order.createdAt)}
                            </span>
                            {order.paymentMethod && (
                              <span className="flex items-center gap-1">
                                <CreditCard className="h-3.5 w-3.5" />
                                {paymentLabels[order.paymentMethod] || order.paymentMethod}
                              </span>
                            )}
                          </div>
                        </div>

                        {/* Price & Actions */}
                        <div className="flex flex-col items-start gap-3 sm:items-end">
                          <div className="text-right">
                            <p className="text-xs text-muted-foreground">Total</p>
                            <p className="text-xl font-bold text-primary">
                              {formatCurrency(order.totalAmount)}
                            </p>
                          </div>

                          {/* QR Code for Confirmed orders */}
                          {order.status === 'Confirmed' && (
                            <QrCodeDisplay orderCode={order.orderCode} />
                          )}

                                                    {order.status === 'Pending' && (
                                                      <Button
                                                        variant="default"
                                                        size="sm"
                                                        className="w-full gap-2 bg-yellow-500 text-white hover:bg-yellow-600 dark:bg-yellow-600 dark:hover:bg-yellow-700"
                                                        onClick={() => setOrderToPay(order)}
                                                      >
                                                        <ShoppingBag className="h-4 w-4" />
                                                        Pagar Agora
                                                      </Button>
                                                    )}

                                                    {refundAvailable && (
                                                      <Button
                                                        variant="outline"
                                                        size="sm"
                                                        className="border-purple-300 text-purple-700 hover:bg-purple-50 dark:border-purple-800 dark:text-purple-400 dark:hover:bg-purple-950"
                                                        onClick={() => setOrderToRefund(order)}
                                                      >
                                                        Solicitar Reembolso
                                                      </Button>
                                                    )}

                          {order.status === 'Refunded' && (
                            <Button
                              variant="ghost"
                              size="sm"
                              className="text-muted-foreground hover:text-destructive hover:bg-destructive/10"
                              onClick={() => removeOrder(order.id)}
                            >
                              <Trash2 className="mr-1 h-4 w-4" />
                              Remover
                            </Button>
                          )}

                          <Link href={`/evento/${order.eventId}`}>
                            <Button variant="ghost" size="sm">
                              Ver evento
                            </Button>
                          </Link>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </div>
      </main>

      <Footer />

            {/* Payment Modal - Escolha forma de pagamento e simulação */}
      <AlertDialog open={!!orderToPay} onOpenChange={() => { if (!isPaying) resetPaymentState(); }}>
        <AlertDialogContent className="max-w-md">
          <AlertDialogHeader>
            <AlertDialogTitle className="flex items-center gap-2 text-xl">
              {paymentStep === 'choose' ? (
                <><ShoppingBag className="h-6 w-6 text-yellow-500" /> Finalizar Pagamento</>
              ) : (
                <><QrCodeIcon className="h-6 w-6 text-primary" /> Pagamento via {paymentLabels[selectedPaymentMethod] || selectedPaymentMethod}</>
              )}
            </AlertDialogTitle>
            {paymentStep === 'simulate' && (
              <AlertDialogDescription className="text-sm">
                Pedido #{orderToPay?.orderCode} — {orderToPay?.eventTitle}
              </AlertDialogDescription>
            )}
          </AlertDialogHeader>

          {paymentStep === 'choose' ? (
            /* Etapa 1: Escolher forma de pagamento */
            <>
              <div className="space-y-4 py-4">
                <div className="rounded-lg bg-secondary/50 p-4 text-left text-sm space-y-2">
                  <p className="font-medium text-foreground">
                    Pedido #{orderToPay?.orderCode}
                  </p>
                  <p className="text-muted-foreground">
                    Valor: <span className="font-bold text-primary">{orderToPay ? formatCurrency(orderToPay.totalAmount) : ''}</span>
                  </p>
                  {orderToPay?.eventTitle && (
                    <p className="text-muted-foreground">
                      Evento: <span className="font-medium">{orderToPay.eventTitle}</span>
                    </p>
                  )}
                </div>

                <div>
                  <p className="mb-3 text-sm font-medium text-foreground">Selecione a forma de pagamento:</p>
                  <div className="grid gap-3 sm:grid-cols-3">
                    {[
                      { value: 'pix', icon: QrCodeIcon, label: 'Pix' },
                      { value: 'credit_card', icon: CreditCard, label: 'Cartão' },
                      { value: 'boleto', icon: FileText, label: 'Boleto' },
                    ].map(({ value, icon: Icon, label }) => (
                      <button
                        key={value}
                        type="button"
                        onClick={() => setSelectedPaymentMethod(value)}
                        className={`flex cursor-pointer flex-col items-center gap-2 rounded-lg border-2 p-4 transition-all ${
                          selectedPaymentMethod === value
                            ? 'border-primary bg-primary/5'
                            : 'border-border hover:bg-secondary'
                        }`}
                      >
                        <Icon className={`h-6 w-6 ${selectedPaymentMethod === value ? 'text-primary' : 'text-muted-foreground'}`} />
                        <span className={`text-sm font-medium ${selectedPaymentMethod === value ? 'text-foreground' : 'text-muted-foreground'}`}>
                          {label}
                        </span>
                      </button>
                    ))}
                  </div>
                </div>

                <div className="flex items-center gap-3 rounded-lg bg-secondary/50 p-3">
                  <ShieldCheck className="h-5 w-5 shrink-0 text-primary" />
                  <p className="text-xs text-muted-foreground">
                    Compra 100% segura. Seus dados estão protegidos.
                  </p>
                </div>
              </div>

              <AlertDialogFooter className="flex-col gap-2 sm:flex-col">
                <Button
                  onClick={() => setPaymentStep('simulate')}
                  className="w-full gap-2 bg-yellow-500 text-white hover:bg-yellow-600 dark:bg-yellow-600 dark:hover:bg-yellow-700"
                >
                  <Lock className="h-5 w-5" />
                  Continuar para pagamento
                </Button>
                <AlertDialogCancel className="w-full">
                  Cancelar
                </AlertDialogCancel>
              </AlertDialogFooter>
            </>
          ) : (
            /* Etapa 2: Simular pagamento */
            <>
              <div className="space-y-6 py-4">
                                {/* Real QR Code gerado com a lib qrcode */}
                                {(selectedPaymentMethod === 'pix') && (
                                  <div className="mx-auto flex h-48 w-48 items-center justify-center rounded-lg border-2 border-dashed border-primary/30 bg-white p-2">
                                    <canvas
                                      ref={(ref) => {
                                        if (ref && orderToPay) {
                                          QRCode.toCanvas(ref, orderToPay.orderCode, {
                                            width: 170,
                                            margin: 1,
                                            color: { dark: '#000000', light: '#ffffff' },
                                          });
                                        }
                                      }}
                                      className="rounded"
                                    />
                                  </div>
                                )}

                {(selectedPaymentMethod === 'boleto') && (
                  <div className="mx-auto max-w-xs rounded-lg border-2 border-dashed border-primary/30 bg-white p-6">
                    <div className="mx-auto h-8 w-full rounded bg-gradient-to-r from-gray-300 via-gray-100 to-gray-300" />
                    <div className="mt-3 space-y-2">
                      <div className="mx-auto h-3 w-3/4 rounded bg-gray-200" />
                      <div className="mx-auto h-3 w-1/2 rounded bg-gray-200" />
                    </div>
                    <p className="mt-3 text-center text-xs text-muted-foreground">Código de barras — Simulação</p>
                  </div>
                )}

                                {(selectedPaymentMethod === 'credit_card') && (
                  <div className="space-y-4">
                    <div className="rounded-lg border border-border bg-white p-4">
                      <p className="mb-4 text-center text-sm font-medium text-foreground">💳 Preencha os dados do cartão</p>
                      <div className="space-y-3">
                        <div>
                          <label className="block text-xs font-medium text-muted-foreground mb-1">Número do cartão</label>
                          <input
                            type="text"
                            placeholder="0000 0000 0000 0000"
                            maxLength={19}
                            value={cardForm.cardNumber}
                            onChange={(e) => updateCardField('cardNumber', e.target.value)}
                            className={`w-full rounded-lg border px-3 py-2 text-sm outline-none transition-colors focus:ring-2 focus:ring-primary/50 ${
                              cardErrors.cardNumber ? 'border-red-400 bg-red-50' : 'border-border'
                            }`}
                          />
                          {cardErrors.cardNumber && <p className="mt-1 text-xs text-red-500">{cardErrors.cardNumber}</p>}
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-muted-foreground mb-1">Nome no cartão</label>
                          <input
                            type="text"
                            placeholder="Nome como está no cartão"
                            value={cardForm.cardName}
                            onChange={(e) => updateCardField('cardName', e.target.value)}
                            className={`w-full rounded-lg border px-3 py-2 text-sm outline-none transition-colors focus:ring-2 focus:ring-primary/50 ${
                              cardErrors.cardName ? 'border-red-400 bg-red-50' : 'border-border'
                            }`}
                          />
                          {cardErrors.cardName && <p className="mt-1 text-xs text-red-500">{cardErrors.cardName}</p>}
                        </div>
                        <div className="grid grid-cols-2 gap-3">
                          <div>
                            <label className="block text-xs font-medium text-muted-foreground mb-1">Validade</label>
                            <input
                              type="text"
                              placeholder="MM/AA"
                              maxLength={5}
                              value={cardForm.cardExpiry}
                              onChange={(e) => updateCardField('cardExpiry', e.target.value)}
                              className={`w-full rounded-lg border px-3 py-2 text-sm outline-none transition-colors focus:ring-2 focus:ring-primary/50 ${
                                cardErrors.cardExpiry ? 'border-red-400 bg-red-50' : 'border-border'
                              }`}
                            />
                            {cardErrors.cardExpiry && <p className="mt-1 text-xs text-red-500">{cardErrors.cardExpiry}</p>}
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-muted-foreground mb-1">CVV</label>
                            <input
                              type="text"
                              placeholder="000"
                              maxLength={4}
                              value={cardForm.cardCvc}
                              onChange={(e) => updateCardField('cardCvc', e.target.value.replace(/\D/g, ''))}
                              className={`w-full rounded-lg border px-3 py-2 text-sm outline-none transition-colors focus:ring-2 focus:ring-primary/50 ${
                                cardErrors.cardCvc ? 'border-red-400 bg-red-50' : 'border-border'
                              }`}
                            />
                            {cardErrors.cardCvc && <p className="mt-1 text-xs text-red-500">{cardErrors.cardCvc}</p>}
                          </div>
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-muted-foreground mb-1">CPF do titular</label>
                          <input
                            type="text"
                            placeholder="000.000.000-00"
                            maxLength={14}
                            value={cardForm.cpf}
                            onChange={(e) => updateCardField('cpf', e.target.value)}
                            className={`w-full rounded-lg border px-3 py-2 text-sm outline-none transition-colors focus:ring-2 focus:ring-primary/50 ${
                              cardErrors.cpf ? 'border-red-400 bg-red-50' : 'border-border'
                            }`}
                          />
                          {cardErrors.cpf && <p className="mt-1 text-xs text-red-500">{cardErrors.cpf}</p>}
                        </div>
                      </div>
                    </div>
                  </div>
                )}

                <div className="rounded-lg bg-secondary/50 p-4 text-left text-sm space-y-1">
                  <p className="font-medium text-foreground">
                    Pedido #{orderToPay?.orderCode}
                  </p>
                  <p className="text-muted-foreground">
                    Forma de pagamento: <span className="font-medium">{paymentLabels[selectedPaymentMethod] || selectedPaymentMethod}</span>
                  </p>
                  <p className="text-muted-foreground">
                    Valor: <span className="font-bold text-primary">{orderToPay ? formatCurrency(orderToPay.totalAmount) : ''}</span>
                  </p>
                  <p className="mt-2 text-xs text-muted-foreground">
                    Este é um ambiente de simulação. Nenhum pagamento real será processado.
                  </p>
                </div>
              </div>

                            <AlertDialogFooter className="flex-col gap-2 sm:flex-col">
                              <AlertDialogAction
                                onClick={(e) => {
                                  // Validar cartão antes de prosseguir
                                  if (selectedPaymentMethod === 'credit_card') {
                                    const errors: Record<string, string> = {};
                                    if (!cardForm.cardNumber.replace(/\s/g, '') || cardForm.cardNumber.replace(/\s/g, '').length < 13) errors.cardNumber = 'Número do cartão inválido';
                                    if (!cardForm.cardName.trim()) errors.cardName = 'Nome no cartão é obrigatório';
                                    if (!cardForm.cardExpiry || cardForm.cardExpiry.length < 5) errors.cardExpiry = 'Data de validade inválida';
                                    if (!cardForm.cardCvc || cardForm.cardCvc.length < 3) errors.cardCvc = 'CVV inválido';
                                    if (!cardForm.cpf.replace(/\D/g, '') || cardForm.cpf.replace(/\D/g, '').length !== 11) errors.cpf = 'CPF inválido';

                                    if (Object.keys(errors).length > 0) {
                                      e.preventDefault();
                                      setCardErrors(errors);
                                      toast.error('Preencha todos os dados do cartão corretamente antes de confirmar');
                                      return;
                                    }
                                    setCardErrors({});
                                  }
                                  // Prosseguir com o pagamento
                                  handlePayOrder();
                                }}
                                disabled={isPaying}
                                className="w-full gap-2 bg-green-600 text-white hover:bg-green-700 dark:bg-green-700 dark:hover:bg-green-600"
                              >
                  {isPaying ? (
                    <><Loader2 className="h-5 w-5 animate-spin" /> Processando...</>
                  ) : (
                    <><CheckCircle className="h-5 w-5" /> Confirmar Pagamento</>
                  )}
                </AlertDialogAction>
                <Button
                  variant="ghost"
                  className="w-full text-muted-foreground"
                  onClick={() => setPaymentStep('choose')}
                  disabled={isPaying}
                >
                  Voltar e escolher outra forma
                </Button>
                <AlertDialogCancel disabled={isPaying} className="w-full">
                  Cancelar
                </AlertDialogCancel>
              </AlertDialogFooter>
            </>
          )}
        </AlertDialogContent>
      </AlertDialog>

      {/* Refund Confirmation Dialog */}
      <AlertDialog open={!!orderToRefund} onOpenChange={() => setOrderToRefund(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Solicitar reembolso</AlertDialogTitle>
            <AlertDialogDescription>
              {orderToRefund?.eventDate && new Date(orderToRefund.eventDate) > new Date() ? (
                <>
                  Deseja solicitar o reembolso do pedido{' '}
                  <strong>#{orderToRefund?.orderCode}</strong> para o evento{' '}
                  <strong>{orderToRefund?.eventTitle}</strong>?
                  <br /><br />
                  O valor de {orderToRefund ? formatCurrency(orderToRefund.totalAmount) : ''} será
                  estornado e os ingressos voltarão a ficar disponíveis para venda.
                </>
              ) : (
                <>
                  Deseja solicitar o reembolso do pedido{' '}
                  <strong>#{orderToRefund?.orderCode}</strong>?
                  <br /><br />
                  O valor de {orderToRefund ? formatCurrency(orderToRefund.totalAmount) : ''} será
                  estornado e os ingressos voltarão a ficar disponíveis para venda.
                </>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isRefunding}>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleRefund}
              disabled={isRefunding}
              className="bg-purple-600 text-white hover:bg-purple-700 dark:bg-purple-700 dark:hover:bg-purple-600"
            >
              {isRefunding ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Processando...
                </>
              ) : (
                'Sim, solicitar reembolso'
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

/**
 * Componente que renderiza um QR Code em um canvas a partir do código do pedido.
 * Usado para check-in na entrada do evento.
 */
function QrCodeDisplay({ orderCode }: { orderCode: string }) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    if (canvasRef.current) {
      QRCode.toCanvas(canvasRef.current, orderCode, {
        width: 100,
        margin: 1,
        color: { dark: '#000000', light: '#ffffff' },
      });
    }
  }, [orderCode]);

  return (
    <div className="flex flex-col items-center gap-1">
      <canvas ref={canvasRef} className="rounded border border-border" />
      <span className="text-[10px] text-muted-foreground flex items-center gap-1">
        <QrCode className="h-3 w-3" />
        QR Code de Entrada
      </span>
    </div>
  );
}
