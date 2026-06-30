'use client';

import { useState } from 'react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { CheckCircle, XCircle, QrCode, Camera, Loader2, ArrowLeft } from 'lucide-react';
import Link from 'next/link';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5188';

export default function CheckInPage() {
  const [orderCode, setOrderCode] = useState('');
  const [isScanning, setIsScanning] = useState(false);
  const [result, setResult] = useState<{
    success: boolean;
    message: string;
    order?: { orderCode: string; eventTitle?: string; eventLocation?: string };
  } | null>(null);

  const handleCheckIn = async (code?: string) => {
    const codeToCheck = code || orderCode.trim();
    if (!codeToCheck) return;

    setIsScanning(true);
    setResult(null);

    try {
      const res = await fetch(`${API_BASE_URL}/api/orders/public-checkin`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderCode: codeToCheck }),
      });
      const data = await res.json();

      setResult({
        success: data.success,
        message: data.message || (data.success ? 'Entrada liberada!' : 'Erro na validação'),
        order: data.success ? data.data : undefined,
      });

      if (data.success) {
        setOrderCode('');
      }
    } catch {
      setResult({ success: false, message: 'Erro ao conectar com o servidor' });
    } finally {
      setIsScanning(false);
    }
  };

  return (
    <div className="flex min-h-screen flex-col">
      <Header />

      <main className="flex-1 bg-secondary/30 py-8">
        <div className="mx-auto max-w-lg px-4">
          <Link
            href="/"
            className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-primary"
          >
            <ArrowLeft className="h-4 w-4" />
            Voltar
          </Link>

          <Card>
            <CardHeader className="text-center">
              <div className="mx-auto mb-3 flex h-16 w-16 items-center justify-center rounded-full bg-primary/10">
                <Camera className="h-8 w-8 text-primary" />
              </div>
              <CardTitle className="text-2xl">Check-in de Ingressos</CardTitle>
              <p className="text-sm text-muted-foreground">
                Valide ingressos escaneando o QR Code ou digitando o código do pedido
              </p>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Manual code input */}
              <div className="space-y-2">
                <Label htmlFor="orderCode">Código do Pedido</Label>
                <div className="flex gap-2">
                  <Input
                    id="orderCode"
                    placeholder="Ex: BA-20260615-A1B2C3D4"
                    value={orderCode}
                    onChange={(e) => setOrderCode(e.target.value.toUpperCase())}
                    onKeyDown={(e) => e.key === 'Enter' && handleCheckIn()}
                    className="font-mono"
                    autoFocus
                  />
                  <Button
                    onClick={() => handleCheckIn()}
                    disabled={isScanning || !orderCode.trim()}
                    className="gap-2"
                  >
                    {isScanning ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      <QrCode className="h-4 w-4" />
                    )}
                    Validar
                  </Button>
                </div>
              </div>

              {/* Result */}
              {result && (
                <div
                  className={`rounded-lg border-2 p-6 text-center transition-all ${
                    result.success
                      ? 'border-green-500 bg-green-50 dark:bg-green-950'
                      : 'border-red-500 bg-red-50 dark:bg-red-950'
                  }`}
                >
                  {result.success ? (
                    <>
                      <CheckCircle className="mx-auto mb-3 h-16 w-16 text-green-500" />
                      <h2 className="mb-2 text-3xl font-bold text-green-700 dark:text-green-400">
                        ✅ Entrada Liberada!
                      </h2>
                      <p className="text-green-600 dark:text-green-500">
                        {result.message}
                      </p>
                      {result.order && (
                        <div className="mt-4 space-y-1 rounded bg-white/50 p-3 text-sm dark:bg-black/20">
                          <p><strong>Pedido:</strong> #{result.order.orderCode}</p>
                          {result.order.eventTitle && (
                            <p><strong>Evento:</strong> {result.order.eventTitle}</p>
                          )}
                          {result.order.eventLocation && (
                            <p><strong>Local:</strong> {result.order.eventLocation}</p>
                          )}
                        </div>
                      )}
                    </>
                  ) : (
                    <>
                      <XCircle className="mx-auto mb-3 h-16 w-16 text-red-500" />
                      <h2 className="mb-2 text-2xl font-bold text-red-700 dark:text-red-400">
                        ❌ Acesso Negado
                      </h2>
                      <p className="text-red-600 dark:text-red-500">
                        {result.message}
                      </p>
                    </>
                  )}
                </div>
              )}

              {/* Instructions */}
              <div className="rounded-lg bg-secondary/50 p-4 text-sm text-muted-foreground">
                <p className="mb-2 font-medium text-foreground">Como usar:</p>
                <ol className="list-inside list-decimal space-y-1">
                  <li>Peça ao participante para mostrar o QR Code na tela "Meus Pedidos"</li>
                  <li>Aponte a câmera do celular para o QR Code (ou digite o código manualmente)</li>
                  <li>O sistema validará o ingresso e exibirá "Entrada Liberada"</li>
                  <li>O ingresso será marcado como "Utilizado" no sistema</li>
                </ol>
              </div>
            </CardContent>
          </Card>
        </div>
      </main>

      <Footer />
    </div>
  );
}
