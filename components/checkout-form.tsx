'use client';

import { useState, useRef, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { CreditCard, QrCode, FileText, Check, ShieldCheck, Lock, ArrowLeft, Ticket, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Separator } from '@/components/ui/separator';
import QRCode from 'qrcode';
import { Event } from '@/lib/mock-data';
import { toast } from 'sonner';

const API_BASE_URL = 'http://localhost:5188';

interface CheckoutFormProps {
  event: Event;
  selectedTickets: { [key: string]: number };
}

export function CheckoutForm({ event, selectedTickets }: CheckoutFormProps) {
  const router = useRouter();
  const [paymentMethod, setPaymentMethod] = useState('pix');
  const [isProcessing, setIsProcessing] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    cpf: '',
    cardNumber: '',
    cardExpiry: '',
    cardCvc: '',
    cardName: '',
  });

  // Coupon state
  const [couponCode, setCouponCode] = useState('');
  const [appliedCoupon, setAppliedCoupon] = useState<{
    code: string;
    discountPercent: number;
    description?: string;
  } | null>(null);
  const [isValidatingCoupon, setIsValidatingCoupon] = useState(false);

  // Pix simulation state
  const [createdOrder, setCreatedOrder] = useState<{
    id: number;
    orderCode: string;
    totalAmount: number;
  } | null>(null);
  const [isConfirmingPayment, setIsConfirmingPayment] = useState(false);

  // Calculate totals
  const ticketDetails = Object.entries(selectedTickets).map(([ticketId, qty]) => {
    const ticket = event.tickets.find((t) => t.id === ticketId);
    return {
      id: ticketId,
      name: ticket?.name || '',
      price: ticket?.price || 0,
      quantity: qty,
      subtotal: (ticket?.price || 0) * qty,
    };
  });

  const subtotal = ticketDetails.reduce((sum, t) => sum + t.subtotal, 0);
  const discount = appliedCoupon ? subtotal * (appliedCoupon.discountPercent / 100) : 0;
  const serviceFee = subtotal * 0.1;
  const total = subtotal + serviceFee - discount;

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const formatCPF = (value: string) => {
    return value.replace(/\D/g, '').replace(/(\d{3})(\d)/, '$1.$2').replace(/(\d{3})(\d)/, '$1.$2').replace(/(\d{3})(\d{1,2})/, '$1-$2').replace(/(-\d{2})\d+?$/, '$1');
  };

  const formatCardNumber = (value: string) => {
    return value.replace(/\D/g, '').replace(/(\d{4})(\d)/, '$1 $2').replace(/(\d{4})(\d)/, '$1 $2').replace(/(\d{4})(\d)/, '$1 $2').replace(/(\d{4})\d+?$/, '$1');
  };

  const formatExpiry = (value: string) => {
    return value.replace(/\D/g, '').replace(/(\d{2})(\d)/, '$1/$2').replace(/(\/\d{2})\d+?$/, '$1');
  };

  const handleValidateCoupon = async () => {
    if (!couponCode.trim()) return;
    setIsValidatingCoupon(true);

    try {
      const token = localStorage.getItem('@BoraAli:token');
      const res = await fetch(`${API_BASE_URL}/api/orders/validate-coupon`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({ code: couponCode.trim(), eventId: parseInt(event.id) }),
      });
      const data = await res.json();

      if (data.success) {
        setAppliedCoupon({
          code: data.data.code,
          discountPercent: data.data.discountPercent,
          description: data.data.description,
        });
        toast.success(data.message || `Cupom ${data.data.discountPercent}% aplicado!`);
      } else {
        toast.error(data.message || 'Cupom inválido');
        setAppliedCoupon(null);
      }
    } catch {
      toast.error('Erro ao validar cupom');
    } finally {
      setIsValidatingCoupon(false);
    }
  };

  const removeCoupon = () => {
    setAppliedCoupon(null);
    setCouponCode('');
    toast.info('Cupom removido');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsProcessing(true);

    try {
      const token = localStorage.getItem('@BoraAli:token');

      if (!token) {
        toast.error('Você precisa estar logado para realizar a compra');
        router.push('/login');
        return;
      }

      const items = Object.entries(selectedTickets).map(([ticketId, qty]) => ({
        ticketTypeId: parseInt(ticketId),
        quantity: qty,
      }));

      const payload = {
        eventId: parseInt(event.id),
        paymentMethod: paymentMethod === 'credit' ? 'CreditCard' : paymentMethod === 'pix' ? 'Pix' : 'Boleto',
        items,
      };

      const response = await fetch(`${API_BASE_URL}/api/orders`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });

      const data = await response.json();

      if (!data.success) {
        throw new Error(data.message || 'Erro ao processar pedido');
      }

      // Apply coupon usage if coupon was applied
      if (appliedCoupon) {
        // Note: coupon CurrentUses is incremented on the backend during validate
        // In production, this would happen atomically during order creation
      }

      // If payment is Pix or Boleto, show the fake payment screen
      if (paymentMethod === 'pix' || paymentMethod === 'boleto') {
        setCreatedOrder({
          id: data.data.id,
          orderCode: data.data.orderCode,
          totalAmount: total,
        });
        toast.success('Pedido criado! Simule o pagamento abaixo.');
      } else {
        // Credit card - auto confirm
        sessionStorage.removeItem('selectedTickets');
        sessionStorage.removeItem('eventId');
        toast.success('Compra realizada com sucesso!');
        router.push('/meus-pedidos');
      }
    } catch (error: any) {
      toast.error(error.message || 'Erro ao processar pagamento');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleSimulatePayment = async () => {
    if (!createdOrder) return;
    setIsConfirmingPayment(true);

    try {
      const token = localStorage.getItem('@BoraAli:token');
      const res = await fetch(`${API_BASE_URL}/api/orders/${createdOrder.id}/confirm-payment`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
      });
      const data = await res.json();

      if (data.success) {
        sessionStorage.removeItem('selectedTickets');
        sessionStorage.removeItem('eventId');
        toast.success('✅ Pagamento confirmado! Ingressos garantidos!');
        router.push('/meus-pedidos');
      } else {
        toast.error(data.message || 'Erro ao confirmar pagamento');
      }
    } catch {
      toast.error('Erro ao processar pagamento');
    } finally {
      setIsConfirmingPayment(false);
    }
  };

  // Fake Pix QR Code screen
  if (createdOrder && paymentMethod === 'pix') {
    return (
      <div className="min-h-screen bg-secondary/30 py-8">
        <div className="mx-auto max-w-lg px-4">
          <Card>
            <CardHeader className="text-center">
              <QrCode className="mx-auto mb-3 h-16 w-16 text-primary" />
              <CardTitle className="text-xl">Pagamento via Pix</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6 text-center">
                            {/* Real QR Code gerado com a lib qrcode */}
                            <div className="mx-auto flex h-56 w-56 items-center justify-center rounded-lg border-2 border-dashed border-primary/30 bg-white p-2">
                              <canvas
                                ref={(ref) => {
                                  if (ref && createdOrder) {
                                    QRCode.toCanvas(ref, createdOrder.orderCode, {
                                      width: 200,
                                      margin: 1,
                                      color: { dark: '#000000', light: '#ffffff' },
                                    });
                                  }
                                }}
                                className="rounded"
                              />
                            </div>

              <div className="rounded-lg bg-secondary/50 p-4 text-left text-sm">
                <p className="font-medium text-foreground">Pedido #{createdOrder.orderCode}</p>
                <p className="mt-1 text-muted-foreground">
                  Valor: R$ {createdOrder.totalAmount.toFixed(2).replace('.', ',')}
                </p>
                {appliedCoupon && (
                  <p className="mt-1 text-sm text-green-600">
                    Cupom {appliedCoupon.code} aplicado ({appliedCoupon.discountPercent}% de desconto)
                  </p>
                )}
                <p className="mt-2 text-xs text-muted-foreground">
                  Este é um ambiente de simulação. Nenhum pagamento real será processado.
                </p>
              </div>

              <Button
                onClick={handleSimulatePayment}
                className="w-full gap-2 bg-green-600 text-white hover:bg-green-700"
                size="lg"
                disabled={isConfirmingPayment}
              >
                {isConfirmingPayment ? (
                  <><Loader2 className="h-5 w-5 animate-spin" /> Processando...</>
                ) : (
                  <><Check className="h-5 w-5" /> Simular Pagamento Aprovado</>
                )}
              </Button>

              <Button
                variant="ghost"
                className="w-full text-muted-foreground"
                onClick={() => setCreatedOrder(null)}
              >
                Voltar
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  // Boleto fake screen
  if (createdOrder && paymentMethod === 'boleto') {
    return (
      <div className="min-h-screen bg-secondary/30 py-8">
        <div className="mx-auto max-w-lg px-4">
          <Card>
            <CardHeader className="text-center">
              <FileText className="mx-auto mb-3 h-16 w-16 text-primary" />
              <CardTitle className="text-xl">Boleto Gerado</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6 text-center">
              <div className="rounded-lg border-2 border-dashed border-primary/30 bg-white p-6">
                <div className="mx-auto h-8 w-full bg-gradient-to-r from-gray-300 via-gray-100 to-gray-300 rounded" />
                <div className="mt-3 space-y-2">
                  <div className="h-3 w-3/4 mx-auto bg-gray-200 rounded" />
                  <div className="h-3 w-1/2 mx-auto bg-gray-200 rounded" />
                </div>
                <p className="mt-3 text-xs text-muted-foreground">Código de barras — Simulação</p>
              </div>

              <div className="rounded-lg bg-secondary/50 p-4 text-left text-sm">
                <p className="font-medium">Pedido #{createdOrder.orderCode}</p>
                <p className="mt-1 text-muted-foreground">
                  Valor: R$ {createdOrder.totalAmount.toFixed(2).replace('.', ',')}
                </p>
                <p className="mt-2 text-xs text-muted-foreground">
                  Em ambiente real, o boleto seria enviado por e-mail.
                </p>
              </div>

              <Button
                onClick={handleSimulatePayment}
                className="w-full gap-2 bg-green-600 text-white hover:bg-green-700"
                size="lg"
                disabled={isConfirmingPayment}
              >
                {isConfirmingPayment ? (
                  <><Loader2 className="h-5 w-5 animate-spin" /> Processando...</>
                ) : (
                  <><Check className="h-5 w-5" /> Simular Pagamento Aprovado</>
                )}
              </Button>

              <Button variant="ghost" className="w-full" onClick={() => setCreatedOrder(null)}>
                Voltar
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-secondary/30 py-8">
      <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
        <Button variant="ghost" className="mb-6 gap-2" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4" /> Voltar
        </Button>

        <div className="grid gap-8 lg:grid-cols-3">
          <div className="lg:col-span-2">
            <form onSubmit={handleSubmit} id="checkout-form" className="space-y-6">
              {/* Personal Info */}
              <Card>
                <CardHeader><CardTitle className="text-lg">Informações Pessoais</CardTitle></CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid gap-4 sm:grid-cols-2">
                    <div className="sm:col-span-2">
                      <Label htmlFor="name">Nome completo</Label>
                      <Input id="name" name="name" placeholder="Digite seu nome completo" value={formData.name} onChange={handleInputChange} required />
                    </div>
                    <div>
                      <Label htmlFor="email">E-mail</Label>
                      <Input id="email" name="email" type="email" placeholder="seu@email.com" value={formData.email} onChange={handleInputChange} required />
                    </div>
                    <div>
                      <Label htmlFor="cpf">CPF</Label>
                      <Input id="cpf" name="cpf" placeholder="000.000.000-00" value={formData.cpf} onChange={(e) => { e.target.value = formatCPF(e.target.value); handleInputChange(e); }} maxLength={14} required />
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Coupon */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2 text-lg">
                    <Ticket className="h-5 w-5 text-primary" />
                    Cupom de Desconto
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {appliedCoupon ? (
                    <div className="flex items-center justify-between rounded-lg bg-green-50 p-3 dark:bg-green-950">
                      <div>
                        <p className="font-medium text-green-700 dark:text-green-400">
                          {appliedCoupon.code} — {appliedCoupon.discountPercent}% OFF
                        </p>
                        {appliedCoupon.description && (
                          <p className="text-sm text-green-600 dark:text-green-500">{appliedCoupon.description}</p>
                        )}
                      </div>
                      <Button variant="ghost" size="sm" className="text-red-500" onClick={removeCoupon}>
                        Remover
                      </Button>
                    </div>
                  ) : (
                    <div className="flex gap-2">
                      <Input
                        placeholder='Código do cupom (ex: POO100)'
                        value={couponCode}
                        onChange={(e) => setCouponCode(e.target.value.toUpperCase())}
                        className="font-mono"
                      />
                      <Button
                        type="button"
                        variant="outline"
                        onClick={handleValidateCoupon}
                        disabled={isValidatingCoupon || !couponCode.trim()}
                      >
                        {isValidatingCoupon ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Aplicar'}
                      </Button>
                    </div>
                  )}
                  <p className="mt-2 text-xs text-muted-foreground">
                    Cupons disponíveis: POO100 (100%), BORA50 (50%), ALUNO20 (20%)
                  </p>
                </CardContent>
              </Card>

              {/* Payment Method */}
              <Card>
                <CardHeader><CardTitle className="text-lg">Forma de Pagamento</CardTitle></CardHeader>
                <CardContent className="space-y-4">
                  <RadioGroup value={paymentMethod} onValueChange={setPaymentMethod} className="grid gap-3 sm:grid-cols-3">
                    {[
                      { value: 'credit', icon: CreditCard, label: 'Cartão' },
                      { value: 'pix', icon: QrCode, label: 'Pix' },
                      { value: 'boleto', icon: FileText, label: 'Boleto' },
                    ].map(({ value, icon: Icon, label }) => (
                      <div key={value}>
                        <RadioGroupItem value={value} id={value} className="peer sr-only" />
                        <Label htmlFor={value} className="flex cursor-pointer flex-col items-center gap-2 rounded-lg border-2 border-border p-4 transition-colors hover:bg-secondary peer-data-[state=checked]:border-primary peer-data-[state=checked]:bg-primary/5">
                          <Icon className="h-6 w-6 text-primary" />
                          <span className="text-sm font-medium">{label}</span>
                        </Label>
                      </div>
                    ))}
                  </RadioGroup>

                  {paymentMethod === 'credit' && (
                    <div className="mt-6 space-y-4">
                      <div><Label htmlFor="cardNumber">Número do cartão</Label>
                        <Input id="cardNumber" name="cardNumber" placeholder="0000 0000 0000 0000" value={formData.cardNumber} onChange={(e) => { e.target.value = formatCardNumber(e.target.value); handleInputChange(e); }} maxLength={19} required /></div>
                      <div><Label htmlFor="cardName">Nome no cartão</Label>
                        <Input id="cardName" name="cardName" placeholder="Nome como está no cartão" value={formData.cardName} onChange={handleInputChange} required /></div>
                      <div className="grid gap-4 sm:grid-cols-2">
                        <div><Label htmlFor="cardExpiry">Validade</Label>
                          <Input id="cardExpiry" name="cardExpiry" placeholder="MM/AA" value={formData.cardExpiry} onChange={(e) => { e.target.value = formatExpiry(e.target.value); handleInputChange(e); }} maxLength={5} required /></div>
                        <div><Label htmlFor="cardCvc">CVV</Label>
                          <Input id="cardCvc" name="cardCvc" placeholder="000" value={formData.cardCvc} onChange={handleInputChange} maxLength={4} required /></div>
                      </div>
                    </div>
                  )}

                  {paymentMethod === 'pix' && (
                    <div className="mt-6 rounded-lg bg-secondary/50 p-4">
                      <p className="text-sm text-muted-foreground">
                        Ao confirmar, você verá um QR Code para simulação de pagamento Pix.
                        Clique em "Simular Pagamento" para confirmar sua compra.
                      </p>
                    </div>
                  )}

                  {paymentMethod === 'boleto' && (
                    <div className="mt-6 rounded-lg bg-secondary/50 p-4">
                      <p className="text-sm text-muted-foreground">
                        Ao confirmar, um boleto simulado será gerado. Clique em "Simular Pagamento" para confirmar.
                      </p>
                    </div>
                  )}
                </CardContent>
              </Card>

              <div className="flex items-center gap-3 rounded-lg bg-secondary/50 p-4">
                <ShieldCheck className="h-6 w-6 shrink-0 text-primary" />
                <div className="text-sm">
                  <p className="font-medium text-foreground">Compra 100% segura</p>
                  <p className="text-muted-foreground">Seus dados estão protegidos.</p>
                </div>
              </div>

              <div className="lg:hidden">
                <Button type="submit" className="w-full gap-2 bg-primary text-primary-foreground hover:bg-primary/90" size="lg" disabled={isProcessing}>
                  {isProcessing ? <><Loader2 className="h-5 w-5 animate-spin" /> Processando...</> : <><Lock className="h-5 w-5" /> Confirmar Compra</>}
                </Button>
              </div>
            </form>
          </div>

          {/* Order Summary */}
          <div className="lg:col-span-1">
            <Card className="sticky top-24">
              <CardHeader><CardTitle className="text-lg">Resumo do Pedido</CardTitle></CardHeader>
              <CardContent className="space-y-4">
                <div className="rounded-lg bg-secondary/50 p-3">
                  <h4 className="font-medium text-foreground">{event.title}</h4>
                  <p className="text-sm text-muted-foreground">{event.date} às {event.time}</p>
                  <p className="text-sm text-muted-foreground">{event.location}</p>
                </div>
                <Separator />
                <div className="space-y-2">
                  {ticketDetails.map((ticket) => (
                    <div key={ticket.id} className="flex justify-between text-sm">
                      <span className="text-muted-foreground">{ticket.quantity}x {ticket.name}</span>
                      <span className="font-medium">R$ {ticket.subtotal.toFixed(2).replace('.', ',')}</span>
                    </div>
                  ))}
                </div>
                <Separator />
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Subtotal</span>
                    <span>R$ {subtotal.toFixed(2).replace('.', ',')}</span>
                  </div>
                  {appliedCoupon && (
                    <div className="flex justify-between text-sm text-green-600">
                      <span>Cupom {appliedCoupon.code} ({appliedCoupon.discountPercent}%)</span>
                      <span>- R$ {discount.toFixed(2).replace('.', ',')}</span>
                    </div>
                  )}
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Taxa de serviço</span>
                    <span>R$ {serviceFee.toFixed(2).replace('.', ',')}</span>
                  </div>
                  <Separator />
                  <div className="flex justify-between">
                    <span className="font-medium">Total</span>
                    <span className="text-xl font-bold text-primary">R$ {total.toFixed(2).replace('.', ',')}</span>
                  </div>
                </div>

                <Button type="submit" form="checkout-form" className="hidden w-full gap-2 bg-primary text-primary-foreground hover:bg-primary/90 lg:flex" size="lg" disabled={isProcessing}>
                  {isProcessing ? <><Loader2 className="h-5 w-5 animate-spin" /> Processando...</> : <><Lock className="h-5 w-5" /> Confirmar Compra</>}
                </Button>

                <div className="space-y-2 pt-4 text-sm">
                  {[
                    'Ingressos enviados por e-mail',
                    'Confirmação instantânea',
                    'Suporte 24h',
                  ].map((text) => (
                    <div key={text} className="flex items-center gap-2 text-muted-foreground">
                      <Check className="h-4 w-4 text-primary" /><span>{text}</span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}
