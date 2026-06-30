'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, Upload, Calendar, Clock, MapPin, DollarSign, Plus, Trash2, Info, Loader2 } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Separator } from '@/components/ui/separator';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useAuth } from '@/hooks/use-auth';
import { toast } from 'sonner';
import { ApiCategory, ApiResponse } from '@/lib/api-types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5188';

interface TicketType {
  id: string;
  name: string;
  price: string;
  quantity: string;
  description: string;
}

export default function CreateEventPage() {
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading, getAuthHeaders } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [categories, setCategories] = useState<ApiCategory[]>([]);
  const [isLoadingCategories, setIsLoadingCategories] = useState(true);
  const [tickets, setTickets] = useState<TicketType[]>([
    { id: '1', name: '', price: '', quantity: '', description: '' },
  ]);

  // Form fields
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [fullDescription, setFullDescription] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [city, setCity] = useState('');
  const [location, setLocation] = useState('');
  const [address, setAddress] = useState('');
  const [addressNumber, setAddressNumber] = useState('');
  const [neighborhood, setNeighborhood] = useState('');
  const [state, setState] = useState('');
  const [date, setDate] = useState('');
  const [time, setTime] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [imageFile, setImageFile] = useState<File | null>(null);

  // CEP autocomplete
  const [cep, setCep] = useState('');
  const [isLoadingCep, setIsLoadingCep] = useState(false);
  const [cepError, setCepError] = useState('');

  // Honeypot: campo oculto para detectar bots
  const [honeypot, setHoneypot] = useState('');

  // Redireciona se não estiver autenticado
  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [authLoading, isAuthenticated, router]);

  // Carrega categorias da API
  useEffect(() => {
    async function fetchCategories() {
      try {
        const res = await fetch(`${API_BASE_URL}/api/events/categories`);
        const data: ApiResponse<ApiCategory[]> = await res.json();
        if (data.success) {
          setCategories(data.data);
        }
      } catch (err) {
        console.error('Erro ao carregar categorias:', err);
      } finally {
        setIsLoadingCategories(false);
      }
    }
    fetchCategories();
  }, []);

  const addTicket = () => {
    setTickets([
      ...tickets,
      { id: Date.now().toString(), name: '', price: '', quantity: '', description: '' },
    ]);
  };

  const removeTicket = (id: string) => {
    if (tickets.length > 1) {
      setTickets(tickets.filter((t) => t.id !== id));
    }
  };

  const updateTicket = (id: string, field: keyof TicketType, value: string) => {
    setTickets(
      tickets.map((t) => (t.id === id ? { ...t, [field]: value } : t))
    );
  };

  // Busca endereço pelo CEP na API ViaCEP
  const fetchCep = useCallback(async (rawCep: string) => {
    const digits = rawCep.replace(/\D/g, '');
    if (digits.length !== 8) return;

    setIsLoadingCep(true);
    setCepError('');

    try {
      const res = await fetch(`https://viacep.com.br/ws/${digits}/json/`);
      const data = await res.json();

      if (data.erro) {
        setCepError('CEP não encontrado.');
        return;
      }

      setAddress(data.logradouro || '');
      setNeighborhood(data.bairro || '');
      setCity(data.localidade || '');
      setState(data.uf || '');
    } catch {
      setCepError('Erro ao consultar o CEP. Tente novamente.');
    } finally {
      setIsLoadingCep(false);
    }
  }, []);

  const handleCepChange = useCallback((value: string) => {
    // Formata o CEP: 00000-000
    const raw = value.replace(/\D/g, '').substring(0, 8);
    let formatted = raw;
    if (raw.length > 5) {
      formatted = `${raw.substring(0, 5)}-${raw.substring(5)}`;
    }
    setCep(formatted);

    // Limpa erro ao digitar
    if (cepError) setCepError('');

    // Dispara busca automática ao completar 8 dígitos
    if (raw.length === 8) {
      fetchCep(raw);
    }
  }, [cepError, fetchCep]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);

    // Honeypot check: se o campo oculto foi preenchido, é um bot
    if (honeypot) {
      console.warn('[SECURITY] Honeypot triggered — possível bot detectado');
      toast.error('Erro ao processar. Tente novamente.');
      setIsLoading(false);
      return;
    }

    try {
      const headers = getAuthHeaders();

      // Upload da imagem primeiro, se houver ficheiro selecionado
      let uploadedImageUrl = imageUrl;

      if (imageFile) {
        const formData = new FormData();
        formData.append('file', imageFile);

        const uploadRes = await fetch(`${API_BASE_URL}/api/upload/image`, {
          method: 'POST',
          body: formData,
        });

        const uploadData = await uploadRes.json();

        if (uploadData.success) {
          uploadedImageUrl = uploadData.url;
        } else {
          throw new Error(uploadData.message || 'Erro ao fazer upload da imagem');
        }
      }

      // Monta o endereço completo combinando logradouro + número
      const fullAddress = addressNumber
        ? `${address}, ${addressNumber}`
        : address;

      const payload = {
        title,
        description,
        fullDescription: fullDescription || null,
        eventDate: new Date(date).toISOString(),
        time,
        location,
        address: fullAddress,
        cep: cep.replace(/\D/g, '') || null,
        street: address || null,
        neighborhood: neighborhood || null,
        state: state || null,
        addressNumber: addressNumber || null,
        city,
        imageUrl: uploadedImageUrl || null,
        categoryId: parseInt(categoryId),
        tickets: tickets.map((t) => ({
          name: t.name,
          price: parseFloat(t.price.replace(',', '.')),
          totalQuantity: parseInt(t.quantity),
          description: t.description || null,
        })),
      };

      const res = await fetch(`${API_BASE_URL}/api/events`, {
        method: 'POST',
        headers,
        body: JSON.stringify(payload),
      });

      const data = await res.json();

      if (!data.success) {
        throw new Error(data.message || 'Erro ao criar evento');
      }

      toast.success('Evento criado com sucesso!');
      router.push(`/evento/${data.data.id}`);
    } catch (error: any) {
      toast.error(error.message || 'Erro ao criar evento');
      console.error('Erro ao criar evento:', error);
    } finally {
      setIsLoading(false);
    }
  };

  // Mostra loading enquanto verifica autenticação
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

  // Não renderiza o formulário se não estiver autenticado
  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="flex min-h-screen flex-col">
      <Header />

      <main className="flex-1 bg-secondary/30 py-8">
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
          {/* Back Link */}
          <Link
            href="/"
            className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-primary"
          >
            <ArrowLeft className="h-4 w-4" />
            Voltar para eventos
          </Link>

          <div className="mb-8">
            <h1 className="text-2xl font-bold text-foreground sm:text-3xl">
              Criar novo evento
            </h1>
            <p className="mt-2 text-muted-foreground">
              Preencha as informações abaixo para criar seu evento e começar a vender ingressos.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
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
            {/* Basic Info */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Info className="h-5 w-5 text-primary" />
                  Informações Básicas
                </CardTitle>
                <CardDescription>
                  Dados principais do seu evento
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <Label htmlFor="title">Nome do evento *</Label>
                  <Input
                    id="title"
                    placeholder="Ex: Festival de Música 2024"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    required
                  />
                </div>

                <div>
                  <Label htmlFor="category">Categoria *</Label>
                  <Select
                    value={categoryId}
                    onValueChange={setCategoryId}
                    required
                    disabled={isLoadingCategories}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={
                        isLoadingCategories ? 'Carregando...' : 'Selecione uma categoria'
                      } />
                    </SelectTrigger>
                    <SelectContent>
                      {categories.map((category) => (
                        <SelectItem key={category.id} value={String(category.id)}>
                          {category.icon && `${category.icon} `}{category.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <Label htmlFor="description">Descrição curta *</Label>
                  <Textarea
                    id="description"
                    placeholder="Uma breve descrição do seu evento (máximo 200 caracteres)"
                    maxLength={200}
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    required
                  />
                </div>

                <div>
                  <Label htmlFor="fullDescription">Descrição completa</Label>
                  <Textarea
                    id="fullDescription"
                    placeholder="Descreva seu evento em detalhes: line-up, atrações, o que está incluso..."
                    className="min-h-[150px]"
                    value={fullDescription}
                    onChange={(e) => setFullDescription(e.target.value)}
                  />
                </div>
              </CardContent>
            </Card>

            {/* Image Upload */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Upload className="h-5 w-5 text-primary" />
                  Imagem do Evento
                </CardTitle>
                <CardDescription>
                  Faça upload da imagem de capa para seu evento
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-center rounded-lg border-2 border-dashed border-border bg-secondary/30 p-12 transition-colors hover:border-primary/50">
                  <div className="w-full text-center">
                    <Upload className="mx-auto mb-4 h-12 w-12 text-muted-foreground/50" />
                    <p className="mb-2 font-medium text-foreground">
                      Imagem do evento
                    </p>
                    <p className="mb-4 text-sm text-muted-foreground">
                      Recomendado: 1920x1080 (PNG, JPG)
                    </p>
                    <Input
                      type="file"
                      accept="image/*"
                      onChange={(e) => {
                        if (e.target.files && e.target.files[0]) {
                          setImageFile(e.target.files[0]);
                        }
                      }}
                    />
                    {imageFile && (
                      <p className="mt-2 text-sm text-green-600">
                        Ficheiro selecionado: {imageFile.name}
                      </p>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Date and Location */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Calendar className="h-5 w-5 text-primary" />
                  Data e Local
                </CardTitle>
                <CardDescription>
                  Quando e onde o evento acontecerá
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid gap-4 sm:grid-cols-2">
                  <div>
                    <Label htmlFor="date">Data *</Label>
                    <Input
                      type="date"
                      id="date"
                      value={date}
                      onChange={(e) => setDate(e.target.value)}
                      required
                    />
                  </div>
                  <div>
                    <Label htmlFor="time">Horário *</Label>
                    <Input
                      type="time"
                      id="time"
                      value={time}
                      onChange={(e) => setTime(e.target.value)}
                      required
                    />
                  </div>
                </div>

                <div>
                  <Label htmlFor="location">Nome do local *</Label>
                  <Input
                    id="location"
                    placeholder="Ex: Allianz Parque"
                    value={location}
                    onChange={(e) => setLocation(e.target.value)}
                    required
                  />
                </div>

                {/* CEP com autocompletar */}
                <div>
                  <Label htmlFor="cep">CEP *</Label>
                  <div className="relative">
                    <Input
                      id="cep"
                      placeholder="00000-000"
                      value={cep}
                      onChange={(e) => handleCepChange(e.target.value)}
                      maxLength={9}
                      className={cepError ? 'border-destructive pr-10' : ''}
                    />
                    {isLoadingCep && (
                      <Loader2 className="absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 animate-spin text-muted-foreground" />
                    )}
                  </div>
                  {cepError && (
                    <p className="mt-1 text-sm text-destructive">{cepError}</p>
                  )}
                </div>

                <div className="grid gap-4 sm:grid-cols-2">
                  <div>
                    <Label htmlFor="address">Logradouro *</Label>
                    <Input
                      id="address"
                      placeholder="Ex: Av. Francisco Matarazzo"
                      value={address}
                      onChange={(e) => setAddress(e.target.value)}
                      required
                    />
                  </div>
                  <div>
                    <Label htmlFor="addressNumber">Número</Label>
                    <Input
                      id="addressNumber"
                      placeholder="1705"
                      value={addressNumber}
                      onChange={(e) => setAddressNumber(e.target.value)}
                    />
                  </div>
                </div>

                <div>
                  <Label htmlFor="neighborhood">Bairro</Label>
                  <Input
                    id="neighborhood"
                    placeholder="Preenchido automaticamente pelo CEP"
                    value={neighborhood}
                    onChange={(e) => setNeighborhood(e.target.value)}
                    readOnly={!!cep}
                  />
                </div>

                <div className="grid gap-4 sm:grid-cols-2">
                  <div>
                    <Label htmlFor="city">Cidade *</Label>
                    <Input
                      id="city"
                      placeholder="Ex: São Paulo"
                      value={city}
                      onChange={(e) => setCity(e.target.value)}
                      required
                    />
                  </div>
                  <div>
                    <Label htmlFor="state">Estado</Label>
                    <Input
                      id="state"
                      placeholder="UF"
                      value={state}
                      onChange={(e) => setState(e.target.value)}
                      maxLength={2}
                      readOnly={!!cep}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Tickets */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <DollarSign className="h-5 w-5 text-primary" />
                  Ingressos
                </CardTitle>
                <CardDescription>
                  Configure os tipos de ingressos disponíveis
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {tickets.map((ticket, index) => (
                  <div
                    key={ticket.id}
                    className="rounded-lg border border-border p-4"
                  >
                    <div className="mb-4 flex items-center justify-between">
                      <h4 className="font-medium">Ingresso {index + 1}</h4>
                      {tickets.length > 1 && (
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          className="text-destructive hover:text-destructive"
                          onClick={() => removeTicket(ticket.id)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      )}
                    </div>

                    <div className="grid gap-4 sm:grid-cols-3">
                      <div>
                        <Label>Nome *</Label>
                        <Input
                          placeholder="Ex: 1º Lote"
                          value={ticket.name}
                          onChange={(e) =>
                            updateTicket(ticket.id, 'name', e.target.value)
                          }
                          required
                        />
                      </div>
                      <div>
                        <Label>Preço (R$) *</Label>
                        <Input
                          type="number"
                          min="0"
                          step="0.01"
                          placeholder="0,00"
                          value={ticket.price}
                          onChange={(e) =>
                            updateTicket(ticket.id, 'price', e.target.value)
                          }
                          required
                        />
                      </div>
                      <div>
                        <Label>Quantidade *</Label>
                        <Input
                          type="number"
                          min="1"
                          placeholder="100"
                          value={ticket.quantity}
                          onChange={(e) =>
                            updateTicket(ticket.id, 'quantity', e.target.value)
                          }
                          required
                        />
                      </div>
                    </div>

                    <div className="mt-4">
                      <Label>Descrição (opcional)</Label>
                      <Input
                        placeholder="Ex: Ingresso pista com acesso a todas as áreas"
                        value={ticket.description}
                        onChange={(e) =>
                          updateTicket(ticket.id, 'description', e.target.value)
                        }
                      />
                    </div>
                  </div>
                ))}

                <Button
                  type="button"
                  variant="outline"
                  className="w-full gap-2"
                  onClick={addTicket}
                >
                  <Plus className="h-4 w-4" />
                  Adicionar tipo de ingresso
                </Button>
              </CardContent>
            </Card>

            {/* Submit */}
            <div className="flex flex-col gap-4 sm:flex-row sm:justify-end">
              <Button type="button" variant="outline" className="sm:order-1">
                Salvar rascunho
              </Button>
              <Button
                type="submit"
                className="bg-primary text-primary-foreground hover:bg-primary/90 sm:order-2"
                disabled={isLoading}
              >
                {isLoading ? 'Publicando...' : 'Publicar evento'}
              </Button>
            </div>
          </form>
        </div>
      </main>

      <Footer />
    </div>
  );
}
