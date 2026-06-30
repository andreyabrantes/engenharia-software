'use client';

import Link from 'next/link';
import { ArrowLeft, Map, CheckCircle2, Clock, ListTodo, ExternalLink } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

const specs = [
  {
    fase: 'Fase 1: Fundação (MVP)',
    status: '✅ Concluído',
    cor: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    items: [
      {
        id: 'S1',
        nome: 'Configuração do Projeto',
        status: '✅ Concluído',
        descricao: 'Inicialização dos projetos frontend (Next.js) e backend (.NET 8) com todas as configurações base.',
        entregaveis: [
          'Projeto Next.js 16 com TypeScript e Tailwind CSS 4',
          'Solution .NET 8 com 3 projetos (Core, Infrastructure, Api)',
          'Configuração de pacotes NuGet e npm',
          'Estrutura de diretórios organizada',
        ],
        visao: 'V6 - Infraestrutura Tecnológica',
        arquitetura: 'A1 - Stack Tecnológica',
      },
      {
        id: 'S2',
        nome: 'Modelagem de Dados',
        status: '✅ Concluído',
        descricao: 'Criação do esquema de banco de dados SQLite com todas as tabelas e relacionamentos.',
        entregaveis: [
          'Script 01-create-tables.sql com 7 tabelas',
          'Índices para otimização de consultas',
          'Constraints de integridade referencial',
          'Script 02-seed-data.sql com dados iniciais',
        ],
        visao: 'V5 - Funcionalidades do Produto',
        arquitetura: 'A2.5 - Modelo de Dados ER',
      },
      {
        id: 'S3',
        nome: 'Entidades de Domínio',
        status: '✅ Concluído',
        descricao: 'Implementação das classes de entidade no projeto Core.',
        entregaveis: [
          'User, Category, Event, TicketType',
          'Order, OrderItem, Seat',
        ],
        visao: 'V5 - Funcionalidades do Produto',
        arquitetura: 'A2.2.1 - BoraAli.Core',
      },
      {
        id: 'S4',
        nome: 'Interfaces e Contratos',
        status: '✅ Concluído',
        descricao: 'Definição das interfaces para repositórios, Unit of Work e executor Dapper.',
        entregaveis: [
          'IGenericRepository<T> - CRUD genérico com paginação',
          'IEventRepository - Consultas específicas',
          'IUnitOfWork - Gerenciamento de transações',
          'IDapperExecutor - Abstração para queries Dapper',
        ],
        visao: 'V3.1 - Integração Completa',
        arquitetura: 'A2.4 - Padrões Arquiteturais',
      },
      {
        id: 'S5',
        nome: 'Camada de Infraestrutura',
        status: '✅ Concluído',
        descricao: 'Implementação dos repositórios, DbSession, UnitOfWork e middleware.',
        entregaveis: [
          'DbSession - Gerenciamento de conexão SQLite',
          'GenericRepository<T> - Implementação genérica com Dapper',
          'EventRepository - Consultas especializadas com JOINs',
          'UnitOfWork - Coordenação de transações',
          'ExceptionMiddleware - Tratamento global de erros',
        ],
        visao: 'V3.4 - Segurança',
        arquitetura: 'A2.2.2 - BoraAli.Infrastructure',
      },
      {
        id: 'S6',
        nome: 'API Controllers e Services',
        status: '✅ Concluído',
        descricao: 'Implementação dos controllers REST e serviços de aplicação.',
        entregaveis: [
          'EventsController - CRUD de eventos, busca, filtros',
          'AuthController - Registro e login com JWT',
          'OrdersController - Criação e consulta de pedidos',
          'EventService, AuthService, OrderService',
          'AutoMapperProfile e Validators FluentValidation',
        ],
        visao: 'V5.1 - Funcionalidades Implementadas',
        arquitetura: 'A2.8 - API Endpoints',
      },
      {
        id: 'S7',
        nome: 'Frontend: Layout e Navegação',
        status: '✅ Concluído',
        descricao: 'Estrutura base do frontend com layout responsivo, header, footer e navegação.',
        entregaveis: [
          'Layout raiz com fonte Poppins e metadata',
          'Header com logo, busca, categorias, menu do usuário',
          'Footer com links e informações',
          'Tema claro/escuro com next-themes',
          '40+ componentes shadcn/ui configurados',
        ],
        visao: 'V3.2 - Experiência Moderna',
        arquitetura: 'A2.3 - Estrutura do Frontend',
      },
      {
        id: 'S8',
        nome: 'Frontend: Home Page',
        status: '✅ Concluído',
        descricao: 'Página inicial com hero section (carrossel) e grid de eventos com filtros.',
        entregaveis: [
          'HeroSection - Carrossel de eventos em destaque',
          'EventsGrid - Grid responsivo com filtros',
          'EventCard - Card com imagem, data, local, preço',
          'Seção de newsletter',
        ],
        visao: 'F2 - Catálogo de Eventos',
        arquitetura: 'A2.6 - Fluxo de Dados',
      },
      {
        id: 'S9',
        nome: 'Frontend: Autenticação',
        status: '✅ Concluído',
        descricao: 'Página de login/cadastro com validação de CPF, força de senha e seleção de perfil.',
        entregaveis: [
          'Tela de login com e-mail e senha',
          'Tela de cadastro com CPF formatado e força de senha',
          'Seleção de perfil (Cliente/Organizador)',
          'useAuth hook - gerenciamento de token',
          'useRequireAuth hook - proteção de rotas',
        ],
        visao: 'F1 - Autenticação e Cadastro',
        arquitetura: 'A2.7 - Segurança',
      },
      {
        id: 'S10',
        nome: 'Frontend: Página do Evento',
        status: '✅ Concluído',
        descricao: 'Página de detalhes do evento com seletor de ingressos.',
        entregaveis: [
          'Banner do evento com gradiente e badge',
          'Informações rápidas (data, horário, local)',
          'Descrição completa com formatação',
          'Card do organizador',
          'TicketSelector - Seleção de tipos/quantidade',
        ],
        visao: 'F3/F4 - Evento + Ingressos',
        arquitetura: 'A2.6 - Fluxo de Dados',
      },
      {
        id: 'S11',
        nome: 'Frontend: Checkout',
        status: '✅ Concluído',
        descricao: 'Página de checkout com formulário de pagamento e resumo do pedido.',
        entregaveis: [
          'Formulário de informações pessoais',
          'Seleção de método de pagamento (cartão, Pix, boleto)',
          'Campos de cartão com formatação',
          'Resumo do pedido com itens e total',
          'Integração com API de pedidos',
        ],
        visao: 'F5 - Checkout',
        arquitetura: 'A2.6 - Fluxo de Dados',
      },
      {
        id: 'S12',
        nome: 'Frontend: Criação de Eventos',
        status: '✅ Concluído',
        descricao: 'Página para organizadores criarem eventos com formulário completo.',
        entregaveis: [
          'Formulário dividido em cards',
          'Upload de imagem com preview',
          'Seleção de categoria via API',
          'Adição/remoção dinâmica de tipos de ingresso',
          'Proteção de rota (apenas organizadores)',
        ],
        visao: 'F6 - Criação de Eventos',
        arquitetura: 'A2.8 - API Endpoints',
      },
    ],
  },
  {
    fase: 'Fase 2: Aprimoramentos',
    status: '🔄 Em Planejamento',
    cor: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200',
    items: [
      {
        id: 'S13',
        nome: 'Dashboard do Organizador',
        status: '📋 Planejado',
        descricao: 'Painel administrativo para organizadores acompanharem vendas e gerenciarem eventos.',
        entregaveis: [
          'Gráficos de vendas (Recharts)',
          'Tabela de ingressos vendidos por evento',
          'Faturamento total e por período',
          'Gerenciamento de eventos',
        ],
        visao: 'FP1 - Dashboard do Organizador',
        arquitetura: 'A1.1 - Recharts',
      },
      {
        id: 'S14',
        nome: 'Notificações por E-mail',
        status: '📋 Planejado',
        descricao: 'Envio de e-mails transacionais usando MailKit.',
        entregaveis: [
          'E-mail de confirmação de cadastro',
          'E-mail de confirmação de compra com QR Code',
          'E-mail de lembrete de evento',
          'Templates de e-mail responsivos',
        ],
        visao: 'FP2 - Notificações por E-mail',
        arquitetura: 'A1.2 - MailKit',
      },
      {
        id: 'S15',
        nome: 'Mapa de Assentos Interativo',
        status: '📋 Planejado',
        descricao: 'Interface visual para seleção de assentos no mapa do evento.',
        entregaveis: [
          'Componente visual de mapa de assentos',
          'Cores por status (disponível, reservado, vendido)',
          'Seleção de múltiplos assentos',
        ],
        visao: 'F7 - Mapa de Assentos',
        arquitetura: 'A2.5 - Seats',
      },
      {
        id: 'S16',
        nome: 'Upload de Imagens com Preview',
        status: '📋 Planejado',
        descricao: 'Sistema completo de upload com redimensionamento e cache.',
        entregaveis: [
          'Upload com drag & drop',
          'Redimensionamento automático',
          'Preview antes do envio',
        ],
        visao: 'FP3 - Upload de Imagens',
        arquitetura: 'A2.8 - Upload Endpoint',
      },
    ],
  },
  {
    fase: 'Fase 3: Expansão',
    status: '📋 Planejado',
    cor: 'bg-slate-100 text-slate-800 dark:bg-slate-800 dark:text-slate-200',
    items: [
      {
        id: 'S17',
        nome: 'Avaliações e Comentários',
        status: '📋 Planejado',
        descricao: 'Sistema de avaliação de eventos pós-realização.',
        entregaveis: [
          'Avaliação por estrelas (1-5)',
          'Comentários textuais',
          'Moderação de conteúdo',
        ],
        visao: 'FP4 - Avaliações e Comentários',
        arquitetura: '—',
      },
      {
        id: 'S18',
        nome: 'Wishlist e Favoritos',
        status: '📋 Planejado',
        descricao: 'Participantes salvam eventos para acompanhar.',
        entregaveis: [
          'Botão de favoritar na página do evento',
          'Página de favoritos do usuário',
          'Notificação de mudança de preço',
        ],
        visao: 'FP5 - Wishlist/Favoritos',
        arquitetura: '—',
      },
      {
        id: 'S19',
        nome: 'App Mobile',
        status: '📋 Planejado',
        descricao: 'Versão mobile nativa do BoraAli.',
        entregaveis: [
          'React Native ou Flutter',
          'Push notifications',
          'Offline mode',
        ],
        visao: 'FP6 - App Mobile',
        arquitetura: '—',
      },
      {
        id: 'S20',
        nome: 'Integração com Redes Sociais',
        status: '📋 Planejado',
        descricao: 'Login social e compartilhamento de eventos.',
        entregaveis: [
          'Login com Google, Facebook, Apple',
          'Compartilhamento de eventos',
          'Open Graph tags',
        ],
        visao: 'FP7 - Integração com Redes Sociais',
        arquitetura: '—',
      },
      {
        id: 'S21',
        nome: 'Relatórios Avançados',
        status: '📋 Planejado',
        descricao: 'Exportação de dados e relatórios gerenciais.',
        entregaveis: [
          'Exportação CSV/Excel',
          'Relatórios por período',
          'Dashboard administrativo global',
        ],
        visao: 'FP8 - Relatórios Avançados',
        arquitetura: '—',
      },
    ],
  },
];

const statusBadge = (status: string) => {
  if (status.includes('Concluído')) return <Badge className="bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 hover:bg-green-100">✅ Concluído</Badge>;
  if (status.includes('Planejado')) return <Badge variant="secondary">📋 Planejado</Badge>;
  return <Badge variant="outline">🔄 Em andamento</Badge>;
};

export default function SpecRoadmapPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1 bg-secondary/30 py-8">
        <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
          <Link
            href="/specs"
            className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-primary"
          >
            <ArrowLeft className="h-4 w-4" /> Voltar para Specs
          </Link>

          <div className="mb-8">
            <div className="flex items-center gap-3">
              <Map className="h-8 w-8 text-primary" />
              <h1 className="text-3xl font-bold tracking-tight">Roadmap</h1>
            </div>
            <p className="mt-2 text-muted-foreground">
              Roadmap completo do projeto BoraAli, listando todas as especificações (specs) planejadas, em desenvolvimento e já executadas.
              Cada spec está referenciada aos documentos de <strong>Visão</strong> e <strong>Arquitetura</strong>.
            </p>
          </div>

          {/* Resumo */}
          <Card className="mb-8">
            <CardContent className="p-6">
              <h2 className="mb-4 text-lg font-semibold">Resumo do Roadmap</h2>
              <div className="grid gap-4 sm:grid-cols-3">
                <div className="rounded-lg border border-green-200 bg-green-50 p-4 text-center dark:border-green-800 dark:bg-green-950">
                  <div className="text-2xl font-bold text-green-700 dark:text-green-300">12</div>
                  <div className="text-sm text-green-600 dark:text-green-400">Fase 1 - MVP</div>
                  <div className="text-xs text-green-500">✅ Concluídas</div>
                </div>
                <div className="rounded-lg border border-yellow-200 bg-yellow-50 p-4 text-center dark:border-yellow-800 dark:bg-yellow-950">
                  <div className="text-2xl font-bold text-yellow-700 dark:text-yellow-300">4</div>
                  <div className="text-sm text-yellow-600 dark:text-yellow-400">Fase 2 - Aprimoramentos</div>
                  <div className="text-xs text-yellow-500">📋 Planejadas</div>
                </div>
                <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 text-center dark:border-slate-700 dark:bg-slate-900">
                  <div className="text-2xl font-bold text-slate-700 dark:text-slate-300">5</div>
                  <div className="text-sm text-slate-600 dark:text-slate-400">Fase 3 - Expansão</div>
                  <div className="text-xs text-slate-500">📋 Planejadas</div>
                </div>
              </div>
              <p className="mt-4 text-center text-sm text-muted-foreground">
                <strong>Total: 21 specs</strong> &middot; 12 concluídas (57%)
              </p>
            </CardContent>
          </Card>

          {/* Specs por Fase */}
          {specs.map((fase) => (
            <div key={fase.fase} className="mb-10">
              <div className="mb-4 flex items-center gap-3">
                <h2 className="text-2xl font-bold">{fase.fase}</h2>
                <Badge className={fase.cor}>{fase.status}</Badge>
              </div>

              <div className="space-y-4">
                {fase.items.map((spec) => (
                  <Card key={spec.id} className="transition-shadow hover:shadow-md">
                    <CardHeader className="pb-3">
                      <div className="flex items-start justify-between">
                        <div className="flex items-center gap-2">
                          <span className="flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                            {spec.id.replace('S', '')}
                          </span>
                          <CardTitle className="text-lg">{spec.nome}</CardTitle>
                        </div>
                        {statusBadge(spec.status)}
                      </div>
                    </CardHeader>
                    <CardContent>
                      <p className="mb-3 text-sm text-muted-foreground">{spec.descricao}</p>

                      <div className="mb-3">
                        <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                          Entregáveis:
                        </h4>
                        <ul className="space-y-0.5">
                          {spec.entregaveis.map((item, i) => (
                            <li key={i} className="flex items-start gap-2 text-sm">
                              <span className="mt-1 block h-1.5 w-1.5 flex-shrink-0 rounded-full bg-primary/60" />
                              {item}
                            </li>
                          ))}
                        </ul>
                      </div>

                      <div className="flex flex-wrap gap-4 border-t border-border pt-3 text-xs text-muted-foreground">
                        {spec.visao !== '—' && (
                          <span className="flex items-center gap-1">
                            <ExternalLink className="h-3 w-3" />
                            <strong>Visão:</strong> {spec.visao}
                          </span>
                        )}
                        {spec.arquitetura !== '—' && (
                          <span className="flex items-center gap-1">
                            <ExternalLink className="h-3 w-3" />
                            <strong>Arquitetura:</strong> {spec.arquitetura}
                          </span>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </div>
          ))}

          <div className="mt-6 text-center">
            <Link href="/specs">
              <Button variant="outline" className="gap-2">
                <ArrowLeft className="h-4 w-4" /> Voltar para Specs
              </Button>
            </Link>
          </div>
        </div>
      </main>
      <Footer />
    </div>
  );
}
