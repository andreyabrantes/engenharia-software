'use client';

import Link from 'next/link';
import { ArrowLeft, ShieldCheck, Zap, Lock, Monitor, Activity, Wrench, Globe, TrendingUp, AlertTriangle } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

const categorias = [
  {
    id: 'performance',
    titulo: '1. Performance',
    icone: Zap,
    cor: 'text-blue-500',
    bgCor: 'bg-blue-50 dark:bg-blue-950',
    borderCor: 'border-blue-200 dark:border-blue-800',
    requisitos: [
      { id: 'RNF-01', descricao: 'Tempo de carregamento da página inicial', metrica: '< 2 segundos (3G simulado)', prioridade: 'Alta' },
      { id: 'RNF-02', descricao: 'Tempo de resposta da API (p95)', metrica: '< 500ms para 95% das requisições', prioridade: 'Alta' },
      { id: 'RNF-03', descricao: 'Tempo de resposta da API de login', metrica: '< 1 segundo (incluindo hash BCrypt)', prioridade: 'Alta' },
      { id: 'RNF-04', descricao: 'Tempo de carregamento de página de evento', metrica: '< 1.5 segundos', prioridade: 'Média' },
      { id: 'RNF-05', descricao: 'Tempo de processamento de pedido', metrica: '< 3 segundos (incluindo transação)', prioridade: 'Alta' },
      { id: 'RNF-06', descricao: 'Tamanho do bundle JavaScript inicial', metrica: '< 200KB (gzip)', prioridade: 'Média' },
      { id: 'RNF-07', descricao: 'Número de requisições simultâneas suportadas', metrica: '> 100 usuários concorrentes', prioridade: 'Média' },
      { id: 'RNF-08', descricao: 'Tempo de resposta para consultas com filtro', metrica: '< 800ms', prioridade: 'Média' },
    ],
    estrategias: [
      'Server Components (Next.js) para páginas estáticas',
      'Client Components apenas para páginas interativas',
      'Dapper (micro-ORM) para consultas SQL otimizadas',
      'Índices SQL em colunas de busca (City, CategoryId, Status, EventDate)',
      'Rate Limiting para prevenção de sobrecarga (100 req/min global)',
      'Lazy loading de imagens nos cards de eventos',
      'Paginação na listagem de eventos com offset/limit',
    ],
  },
  {
    id: 'seguranca',
    titulo: '2. Segurança',
    icone: Lock,
    cor: 'text-red-500',
    bgCor: 'bg-red-50 dark:bg-red-950',
    borderCor: 'border-red-200 dark:border-red-800',
    requisitos: [
      { id: 'RNF-09', descricao: 'Senhas armazenadas com hash', metrica: 'BCrypt com 11 rounds de salt', prioridade: 'Crítica' },
      { id: 'RNF-10', descricao: 'Autenticação via tokens', metrica: 'JWT com assinatura HMAC-SHA256', prioridade: 'Crítica' },
      { id: 'RNF-11', descricao: 'Expiração de token', metrica: '8 horas (configurável)', prioridade: 'Alta' },
      { id: 'RNF-12', descricao: 'Proteção contra força bruta', metrica: 'Rate limiting: 5 tentativas/min no login', prioridade: 'Alta' },
      { id: 'RNF-13', descricao: 'Controle de acesso por perfil', metrica: 'Role-based (Admin, Cliente, Organizador)', prioridade: 'Crítica' },
      { id: 'RNF-14', descricao: 'Validação de entrada', metrica: 'Frontend (Zod) + Backend (FluentValidation)', prioridade: 'Crítica' },
      { id: 'RNF-15', descricao: 'CORS restrito', metrica: 'Apenas origens configuradas', prioridade: 'Alta' },
      { id: 'RNF-16', descricao: 'Proteção contra XSS', metrica: 'React com escape automático de HTML', prioridade: 'Alta' },
      { id: 'RNF-17', descricao: 'Proteção contra SQL Injection', metrica: 'Dapper com parâmetros tipados', prioridade: 'Crítica' },
      { id: 'RNF-18', descricao: 'Headers de segurança', metrica: 'Content-Security-Policy, X-Content-Type-Options', prioridade: 'Média' },
    ],
    estrategias: [
      'JWT Bearer Token com expiração configurável',
      'BCrypt com salt de 11 rounds para hash de senhas',
      'Role-based authorization (Admin, Cliente, Organizador)',
      'CORS restrito a origens configuradas',
      'Rate Limiting: 100 req/min global, 5 req/min login',
      'FluentValidation em todas as requisições',
      'ExceptionMiddleware com respostas padronizadas',
    ],
  },
  {
    id: 'usabilidade',
    titulo: '3. Usabilidade',
    icone: Monitor,
    cor: 'text-green-500',
    bgCor: 'bg-green-50 dark:bg-green-950',
    borderCor: 'border-green-200 dark:border-green-800',
    requisitos: [
      { id: 'RNF-19', descricao: 'Design responsivo', metrica: 'Funcional em mobile (320px+) e desktop', prioridade: 'Alta' },
      { id: 'RNF-20', descricao: 'Suporte a tema claro/escuro', metrica: 'Alternância via next-themes', prioridade: 'Média' },
      { id: 'RNF-21', descricao: 'Feedback visual para ações', metrica: 'Toast notifications (Sonner) para sucesso/erro', prioridade: 'Alta' },
      { id: 'RNF-22', descricao: 'Formatação automática de campos', metrica: 'CPF, telefone, cartão de crédito, validade', prioridade: 'Média' },
      { id: 'RNF-23', descricao: 'Indicador de força de senha', metrica: 'Barra visual com níveis (fraca, média, forte)', prioridade: 'Média' },
      { id: 'RNF-24', descricao: 'Navegação por teclado', metrica: 'Componentes Radix UI com acessibilidade WAI-ARIA', prioridade: 'Média' },
      { id: 'RNF-25', descricao: 'Mensagens de erro em português', metrica: 'FluentValidation com mensagens customizadas', prioridade: 'Alta' },
      { id: 'RNF-26', descricao: 'Tempo máximo para criar evento', metrica: '< 5 minutos', prioridade: 'Média' },
    ],
    estrategias: [
      'shadcn/ui com Radix UI para acessibilidade WAI-ARIA',
      'Tailwind CSS 4 para design responsivo',
      'next-themes para alternância de tema claro/escuro',
      'Sonner para notificações toast',
      'Formatação automática de campos com JavaScript',
      'Indicador de força de senha com feedback visual',
    ],
  },
  {
    id: 'disponibilidade',
    titulo: '4. Disponibilidade e Confiabilidade',
    icone: Activity,
    cor: 'text-purple-500',
    bgCor: 'bg-purple-50 dark:bg-purple-950',
    borderCor: 'border-purple-200 dark:border-purple-800',
    requisitos: [
      { id: 'RNF-27', descricao: 'Disponibilidade da API', metrica: '99.5% (exceto manutenção programada)', prioridade: 'Alta' },
      { id: 'RNF-28', descricao: 'Tratamento de erros global', metrica: 'ExceptionMiddleware com respostas padronizadas', prioridade: 'Crítica' },
      { id: 'RNF-29', descricao: 'Fallback de dados mockados', metrica: 'Frontend funcional mesmo sem API', prioridade: 'Alta' },
      { id: 'RNF-30', descricao: 'Logging de erros', metrica: 'Serilog com rotação diária de arquivos', prioridade: 'Alta' },
      { id: 'RNF-31', descricao: 'Health check', metrica: 'Endpoint GET /health', prioridade: 'Média' },
      { id: 'RNF-32', descricao: 'Recuperação de falhas', metrica: 'Transações com rollback automático', prioridade: 'Crítica' },
    ],
    estrategias: [
      'ExceptionMiddleware para tratamento global de erros',
      'Dados mockados como fallback (lib/mock-data.ts)',
      'Serilog com rotação diária de arquivos',
      'Endpoint de health check (GET /health)',
      'UnitOfWork com transações e rollback automático',
    ],
  },
  {
    id: 'manutenibilidade',
    titulo: '5. Manutenibilidade',
    icone: Wrench,
    cor: 'text-amber-500',
    bgCor: 'bg-amber-50 dark:bg-amber-950',
    borderCor: 'border-amber-200 dark:border-amber-800',
    requisitos: [
      { id: 'RNF-33', descricao: 'Separação em camadas', metrica: '3 projetos .NET (Core, Infrastructure, Api)', prioridade: 'Alta' },
      { id: 'RNF-34', descricao: 'Código tipado', metrica: 'TypeScript no frontend, C# no backend', prioridade: 'Alta' },
      { id: 'RNF-35', descricao: 'Testes unitários', metrica: 'Cobertura mínima de 30% (serviços)', prioridade: 'Média' },
      { id: 'RNF-36', descricao: 'Documentação de API', metrica: 'Swagger/OpenAPI disponível em /swagger', prioridade: 'Alta' },
      { id: 'RNF-37', descricao: 'Logging estruturado', metrica: 'Serilog com formato JSON para análise', prioridade: 'Média' },
      { id: 'RNF-38', descricao: 'Configuração externalizada', metrica: 'appsettings.json para conexão, JWT, CORS', prioridade: 'Alta' },
    ],
    estrategias: [
      'Arquitetura em 3 camadas (Core, Infrastructure, Api)',
      'TypeScript no frontend, C# no backend',
      'Testes unitários com mocks via IDapperExecutor',
      'Swagger/OpenAPI para documentação interativa',
      'Serilog com logging estruturado em JSON',
      'Configuração externalizada no appsettings.json',
    ],
  },
  {
    id: 'portabilidade',
    titulo: '6. Portabilidade',
    icone: Globe,
    cor: 'text-cyan-500',
    bgCor: 'bg-cyan-50 dark:bg-cyan-950',
    borderCor: 'border-cyan-200 dark:border-cyan-800',
    requisitos: [
      { id: 'RNF-39', descricao: 'Backend cross-platform', metrica: '.NET 8 Runtime (Windows, Linux, macOS)', prioridade: 'Alta' },
      { id: 'RNF-40', descricao: 'Banco de dados portátil', metrica: 'SQLite (arquivo único .db)', prioridade: 'Alta' },
      { id: 'RNF-41', descricao: 'Frontend sem dependência de servidor', metrica: 'Next.js static export ou Vercel deploy', prioridade: 'Alta' },
      { id: 'RNF-42', descricao: 'Containerização', metrica: 'Dockerfile (planejado)', prioridade: 'Baixa' },
    ],
    estrategias: [
      '.NET 8 Runtime cross-platform',
      'SQLite como banco de dados embarcado',
      'Next.js com suporte a static export',
      'Docker (planejado para futuro)',
    ],
  },
  {
    id: 'escalabilidade',
    titulo: '7. Escalabilidade',
    icone: TrendingUp,
    cor: 'text-indigo-500',
    bgCor: 'bg-indigo-50 dark:bg-indigo-950',
    borderCor: 'border-indigo-200 dark:border-indigo-800',
    requisitos: [
      { id: 'RNF-43', descricao: 'Escalabilidade horizontal', metrica: 'API stateless (JWT) permite múltiplas instâncias', prioridade: 'Média' },
      { id: 'RNF-44', descricao: 'Cache de consultas frequentes', metrica: 'Redis ou MemoryCache (planejado)', prioridade: 'Baixa' },
      { id: 'RNF-45', descricao: 'Migração de banco', metrica: 'Troca de SQLite para PostgreSQL via Dapper', prioridade: 'Média' },
    ],
    estrategias: [
      'API stateless com JWT (escalabilidade horizontal)',
      'Cache com Redis ou MemoryCache (planejado)',
      'Abstração via Dapper facilita migração de banco',
    ],
  },
  {
    id: 'restricoes',
    titulo: '8. Restrições Técnicas',
    icone: AlertTriangle,
    cor: 'text-rose-500',
    bgCor: 'bg-rose-50 dark:bg-rose-950',
    borderCor: 'border-rose-200 dark:border-rose-800',
    requisitos: [
      { id: 'RT-01', descricao: 'Framework frontend', metrica: 'Next.js 16 (App Router)', prioridade: 'Obrigatório' },
      { id: 'RT-02', descricao: 'Framework backend', metrica: '.NET 8 (C# 12)', prioridade: 'Obrigatório' },
      { id: 'RT-03', descricao: 'Banco de dados', metrica: 'SQLite', prioridade: 'Definido' },
      { id: 'RT-04', descricao: 'ORM', metrica: 'Dapper (micro-ORM)', prioridade: 'Definido' },
      { id: 'RT-05', descricao: 'Design system', metrica: 'shadcn/ui + Tailwind CSS 4', prioridade: 'Definido' },
      { id: 'RT-06', descricao: 'Autenticação', metrica: 'JWT + BCrypt (sem provedor externo)', prioridade: 'Definido' },
      { id: 'RT-07', descricao: 'Hospedagem frontend', metrica: 'Vercel (otimizado para Next.js)', prioridade: 'Definido' },
      { id: 'RT-08', descricao: 'Sistema operacional', metrica: 'Windows 11 (dev), qualquer SO com .NET 8 (prod)', prioridade: 'Definido' },
    ],
    estrategias: [],
  },
];

const prioridadeBadge = (prioridade: string) => {
  const map: Record<string, string> = {
    'Crítica': 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
    'Alta': 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400',
    'Média': 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
    'Baixa': 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-400',
    'Obrigatório': 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400',
    'Definido': 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-400',
  };
  return <Badge className={map[prioridade] || ''}>{prioridade}</Badge>;
};

export default function SpecRequisitosNaoFuncionaisPage() {
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
              <ShieldCheck className="h-8 w-8 text-primary" />
              <h1 className="text-3xl font-bold tracking-tight">Requisitos Não-Funcionais</h1>
            </div>
            <p className="mt-2 text-muted-foreground">
              Especificação dos requisitos não-funcionais do sistema <strong>BoraAli</strong>.
              Diferentemente dos requisitos funcionais (que descrevem <strong>o que</strong> o sistema faz),
              estes requisitos descrevem <strong>como</strong> o sistema se comporta em termos de
              performance, segurança, usabilidade, disponibilidade e manutenibilidade.
            </p>
          </div>

          {/* Resumo */}
          <div className="mb-8 grid gap-3 sm:grid-cols-4">
            <div className="rounded-lg border border-blue-200 bg-blue-50 p-3 text-center dark:border-blue-800 dark:bg-blue-950">
              <div className="text-2xl font-bold text-blue-700 dark:text-blue-300">8</div>
              <div className="text-xs text-blue-600 dark:text-blue-400">Performance</div>
            </div>
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-center dark:border-red-800 dark:bg-red-950">
              <div className="text-2xl font-bold text-red-700 dark:text-red-300">10</div>
              <div className="text-xs text-red-600 dark:text-red-400">Segurança</div>
            </div>
            <div className="rounded-lg border border-green-200 bg-green-50 p-3 text-center dark:border-green-800 dark:bg-green-950">
              <div className="text-2xl font-bold text-green-700 dark:text-green-300">8</div>
              <div className="text-xs text-green-600 dark:text-green-400">Usabilidade</div>
            </div>
            <div className="rounded-lg border border-purple-200 bg-purple-50 p-3 text-center dark:border-purple-800 dark:bg-purple-950">
              <div className="text-2xl font-bold text-purple-700 dark:text-purple-300">6</div>
              <div className="text-xs text-purple-600 dark:text-purple-400">Disponibilidade</div>
            </div>
            <div className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-center dark:border-amber-800 dark:bg-amber-950">
              <div className="text-2xl font-bold text-amber-700 dark:text-amber-300">6</div>
              <div className="text-xs text-amber-600 dark:text-amber-400">Manutenibilidade</div>
            </div>
            <div className="rounded-lg border border-cyan-200 bg-cyan-50 p-3 text-center dark:border-cyan-800 dark:bg-cyan-950">
              <div className="text-2xl font-bold text-cyan-700 dark:text-cyan-300">4</div>
              <div className="text-xs text-cyan-600 dark:text-cyan-400">Portabilidade</div>
            </div>
            <div className="rounded-lg border border-indigo-200 bg-indigo-50 p-3 text-center dark:border-indigo-800 dark:bg-indigo-950">
              <div className="text-2xl font-bold text-indigo-700 dark:text-indigo-300">3</div>
              <div className="text-xs text-indigo-600 dark:text-indigo-400">Escalabilidade</div>
            </div>
            <div className="rounded-lg border border-rose-200 bg-rose-50 p-3 text-center dark:border-rose-800 dark:bg-rose-950">
              <div className="text-2xl font-bold text-rose-700 dark:text-rose-300">8</div>
              <div className="text-xs text-rose-600 dark:text-rose-400">Restrições Técnicas</div>
            </div>
          </div>

          {/* Categorias */}
          {categorias.map((cat) => {
            const Icon = cat.icone;
            return (
              <div key={cat.id} className="mb-8">
                <div className="mb-3 flex items-center gap-2">
                  <Icon className={`h-6 w-6 ${cat.cor}`} />
                  <h2 className="text-2xl font-bold">{cat.titulo}</h2>
                  <Badge variant="outline" className="ml-auto">{cat.requisitos.length} requisitos</Badge>
                </div>

                <Card className={`border ${cat.borderCor}`}>
                  <CardContent className="p-0">
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className={`border-b ${cat.borderCor}`}>
                            <th className={`px-4 py-3 text-left font-medium ${cat.bgCor}`}>ID</th>
                            <th className={`px-4 py-3 text-left font-medium ${cat.bgCor}`}>Descrição</th>
                            <th className={`px-4 py-3 text-left font-medium ${cat.bgCor}`}>Métrica</th>
                            <th className={`px-4 py-3 text-left font-medium ${cat.bgCor}`}>Prioridade</th>
                          </tr>
                        </thead>
                        <tbody>
                          {cat.requisitos.map((req, i) => (
                            <tr key={req.id} className={`border-b border-border/50 ${i % 2 === 0 ? 'bg-background' : 'bg-secondary/20'}`}>
                              <td className="px-4 py-2.5 font-mono text-xs font-medium">{req.id}</td>
                              <td className="px-4 py-2.5">{req.descricao}</td>
                              <td className="px-4 py-2.5 text-muted-foreground">{req.metrica}</td>
                              <td className="px-4 py-2.5">{prioridadeBadge(req.prioridade)}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>

                    {cat.estrategias.length > 0 && (
                      <div className={`border-t ${cat.borderCor} px-4 py-3`}>
                        <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                          Estratégias de Implementação
                        </h4>
                        <div className="flex flex-wrap gap-2">
                          {cat.estrategias.map((estr, i) => (
                            <span key={i} className={`rounded-full px-3 py-1 text-xs ${cat.bgCor} ${cat.cor}`}>
                              {estr}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}
                  </CardContent>
                </Card>
              </div>
            );
          })}

          {/* Matriz RNF → ADR */}
          <Card className="mb-8">
            <CardHeader>
              <CardTitle className="text-lg">Matriz de Rastreabilidade RNF → ADR</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border">
                      <th className="px-3 py-2 text-left font-medium">RNF</th>
                      <th className="px-3 py-2 text-left font-medium">ADR</th>
                      <th className="px-3 py-2 text-left font-medium">Decisão</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr className="border-b border-border/50">
                      <td className="px-3 py-2 font-mono text-xs">RNF-01 a RNF-08</td>
                      <td className="px-3 py-2">ADR-002, ADR-004, ADR-011</td>
                      <td className="px-3 py-2">Dapper, Next.js App Router, Tailwind CSS</td>
                    </tr>
                    <tr className="border-b border-border/50 bg-secondary/20">
                      <td className="px-3 py-2 font-mono text-xs">RNF-09 a RNF-18</td>
                      <td className="px-3 py-2">ADR-006, ADR-007, ADR-009</td>
                      <td className="px-3 py-2">JWT+BCrypt, Rate Limiting, FluentValidation</td>
                    </tr>
                    <tr className="border-b border-border/50">
                      <td className="px-3 py-2 font-mono text-xs">RNF-19 a RNF-26</td>
                      <td className="px-3 py-2">ADR-005, ADR-011</td>
                      <td className="px-3 py-2">shadcn/ui, Tailwind CSS</td>
                    </tr>
                    <tr className="border-b border-border/50 bg-secondary/20">
                      <td className="px-3 py-2 font-mono text-xs">RNF-27 a RNF-32</td>
                      <td className="px-3 py-2">ADR-003, ADR-008</td>
                      <td className="px-3 py-2">3 camadas, Serilog</td>
                    </tr>
                    <tr className="border-b border-border/50">
                      <td className="px-3 py-2 font-mono text-xs">RNF-33 a RNF-38</td>
                      <td className="px-3 py-2">ADR-003, ADR-010</td>
                      <td className="px-3 py-2">3 camadas, AutoMapper</td>
                    </tr>
                    <tr className="border-b border-border/50 bg-secondary/20">
                      <td className="px-3 py-2 font-mono text-xs">RNF-39 a RNF-42</td>
                      <td className="px-3 py-2">ADR-001</td>
                      <td className="px-3 py-2">SQLite</td>
                    </tr>
                    <tr>
                      <td className="px-3 py-2 font-mono text-xs">RNF-43 a RNF-45</td>
                      <td className="px-3 py-2">ADR-001, ADR-002</td>
                      <td className="px-3 py-2">SQLite, Dapper</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>

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
