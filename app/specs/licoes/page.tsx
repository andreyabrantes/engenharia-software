'use client';

import Link from 'next/link';
import { ArrowLeft, BookOpen, Lightbulb, AlertTriangle, CheckCircle2, Star, ArrowRight } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

const secoes = [
  {
    titulo: '1. Arquitetura e Tecnologia',
    icone: 'arquitetura',
    itens: [
      {
        titulo: 'SQLite vs PostgreSQL',
        decisao: 'Optamos por SQLite em vez de PostgreSQL para o MVP.',
        licao: 'SQLite é excelente para prototipagem e desenvolvimento local, mas para produção com múltiplos acessos concorrentes, seria necessário migrar para PostgreSQL ou SQL Server. A abstração via Dapper facilita essa migração.',
      },
      {
        titulo: 'Dapper vs Entity Framework',
        decisao: 'Escolhemos Dapper (micro-ORM) em vez de Entity Framework Core.',
        licao: 'Dapper é significativamente mais rápido e dá controle total sobre as queries SQL. Porém, exige mais trabalho manual para escrever SQL e não possui migrations automáticas. Para um projeto com muitas entidades e relacionamentos complexos, EF Core poderia ser mais produtivo.',
      },
      {
        titulo: 'Arquitetura em Camadas',
        decisao: 'Adotamos Clean Architecture com 3 projetos (Core, Infrastructure, Api).',
        licao: 'A separação clara de responsabilidades facilitou a manutenção e os testes. No entanto, o número de arquivos e indireção pode ser excessivo para projetos pequenos. Para este porte, o custo-benefício foi positivo.',
      },
      {
        titulo: 'Next.js App Router',
        decisao: 'Utilizamos Next.js 16 com App Router.',
        licao: 'O App Router é moderno e performático, mas ainda tem algumas arestas. A distinção entre Server Components e Client Components exige planejamento cuidadoso. Componentes que usam hooks (useState, useEffect) precisam ser marcados com "use client", o que pode levar a uma proliferação de componentes client-side se não for bem planejado.',
      },
    ],
  },
  {
    titulo: '2. Desenvolvimento',
    icone: 'dev',
    itens: [
      {
        titulo: 'shadcn/ui e Componentes',
        decisao: 'Utilizamos shadcn/ui como design system.',
        licao: 'A abordagem de copiar os componentes para o projeto (em vez de importar de um pacote) dá controle total sobre a customização. Porém, atualizações dos componentes originais precisam ser aplicadas manualmente. Recomenda-se manter um registro das versões dos componentes.',
      },
      {
        titulo: 'Dados Mockados vs API Real',
        decisao: 'Mantivemos dados mockados como fallback durante o desenvolvimento.',
        licao: 'A transição de mock para API real foi suave, mas exigiu manter dois conjuntos de tipos (mock-data.ts e api-types.ts). Idealmente, deveríamos ter definido os tipos da API primeiro e feito o mock conformar a esses tipos.',
      },
      {
        titulo: 'Autenticação no Frontend',
        decisao: 'Token JWT armazenado no localStorage.',
        licao: 'localStorage é vulnerável a ataques XSS. Para produção, considerar cookies HttpOnly ou armazenamento em memória com refresh tokens. O uso de sessionStorage para dados temporários (selectedTickets) foi uma boa prática.',
      },
      {
        titulo: 'Validação Dupla (Frontend + Backend)',
        decisao: 'Validação no frontend (React Hook Form + Zod) e no backend (FluentValidation).',
        licao: 'A validação dupla é essencial para segurança e UX. O frontend valida para feedback imediato, o backend para segurança. FluentValidation se mostrou muito expressivo e fácil de integrar com ASP.NET Core.',
      },
    ],
  },
  {
    titulo: '3. Gerenciamento de Projeto',
    icone: 'projeto',
    itens: [
      {
        titulo: 'Versionamento Semântico',
        decisao: 'Não adotamos versionamento semântico formal.',
        licao: 'Para projetos maiores, é essencial definir uma estratégia de versionamento desde o início (SemVer). Isso facilita o rastreamento de mudanças e a comunicação com a equipe.',
      },
      {
        titulo: 'Documentação',
        decisao: 'Documentação gerada após o desenvolvimento.',
        licao: 'Documentar durante o desenvolvimento (ou antes, como ADRs) é mais eficiente e preciso. A documentação retrospectiva corre o risco de perder detalhes importantes das decisões tomadas.',
      },
      {
        titulo: 'Testes',
        decisao: 'Testes unitários apenas para os serviços (AuthServiceTests, EventServiceTests, OrderServiceTests).',
        licao: 'A cobertura de testes poderia ser maior, incluindo testes de integração para os repositórios e testes end-to-end para o fluxo completo de compra. A abstração via interfaces (IDapperExecutor) facilitou a criação de mocks para os testes existentes.',
      },
    ],
  },
  {
    titulo: '4. Decisões Técnicas Específicas',
    icone: 'tecnicas',
    itens: [
      {
        titulo: 'Rate Limiting',
        decisao: 'Implementamos rate limiting com 100 req/min global e 5 req/min para login.',
        licao: 'O rate limiting é essencial para proteção contra abusos. A política de 5 tentativas por minuto para login é um bom equilíbrio entre segurança e usabilidade. Para produção, considerar políticas baseadas em usuário autenticado vs IP.',
      },
      {
        titulo: 'Logging com Serilog',
        decisao: 'Serilog com console + arquivo com rotação diária.',
        licao: 'Serilog é extremamente flexível e a configuração declarativa no appsettings.json facilita a manutenção. A rotação diária de arquivos evita acúmulo de logs. Para produção, considerar adicionar um sink cloud (Seq, Application Insights, DataDog).',
      },
      {
        titulo: 'CORS Configurado',
        decisao: 'CORS restrito a origens configuradas.',
        licao: 'A configuração explícita de origens permitidas é uma boa prática de segurança. Para desenvolvimento, incluímos localhost:3000 (frontend) e localhost:5188 (backend). Em produção, deve-se configurar apenas o domínio do frontend.',
      },
      {
        titulo: 'Inicialização Automática do Banco',
        decisao: 'O banco de dados é inicializado automaticamente na primeira execução.',
        licao: 'A abordagem de verificar se as tabelas existem e criar/seed automaticamente é muito conveniente para desenvolvimento. Para produção, recomenda-se usar migrations controladas.',
      },
    ],
  },
];

const iconeMap: Record<string, React.ReactNode> = {
  arquitetura: <Lightbulb className="h-6 w-6 text-amber-500" />,
  dev: <CheckCircle2 className="h-6 w-6 text-green-500" />,
  projeto: <Star className="h-6 w-6 text-blue-500" />,
  tecnicas: <AlertTriangle className="h-6 w-6 text-purple-500" />,
};

const recomendacoes = [
  'Migrar para PostgreSQL em produção para melhor concorrência.',
  'Implementar refresh tokens para autenticação mais segura.',
  'Adicionar testes de integração para o fluxo completo de compra.',
  'Implementar CI/CD com GitHub Actions.',
  'Adicionar monitoramento e alertas (Application Insights ou similar).',
  'Containerizar a aplicação com Docker para facilitar deploy.',
  'Implementar cache (Redis ou MemoryCache) para consultas frequentes.',
  'Adicionar health checks mais robustos.',
  'Implementar versionamento de API (v1, v2).',
  'Criar documentação de API interativa com exemplos de uso.',
];

export default function SpecLicoesPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1 bg-secondary/30 py-8">
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
          <Link
            href="/specs"
            className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-primary"
          >
            <ArrowLeft className="h-4 w-4" /> Voltar para Specs
          </Link>

          <div className="mb-8">
            <div className="flex items-center gap-3">
              <BookOpen className="h-8 w-8 text-primary" />
              <h1 className="text-3xl font-bold tracking-tight">Lições Aprendidas</h1>
            </div>
            <p className="mt-2 text-muted-foreground">
              Documento de lições aprendidas durante o desenvolvimento do <strong>BoraAli</strong>,
              registrando decisões, aprendizados e recomendações futuras.
            </p>
            <p className="text-sm text-muted-foreground">
              Projeto: BoraAli - Plataforma de Eventos e Ingressos &middot; Data: Junho/2026
            </p>
          </div>

          {secoes.map((secao) => (
            <div key={secao.titulo} className="mb-10">
              <div className="mb-4 flex items-center gap-3">
                {iconeMap[secao.icone]}
                <h2 className="text-2xl font-bold">{secao.titulo}</h2>
              </div>

              <div className="space-y-4">
                {secao.itens.map((item) => (
                  <Card key={item.titulo} className="transition-shadow hover:shadow-md">
                    <CardHeader className="pb-2">
                      <CardTitle className="text-base">{item.titulo}</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-3">
                      <div>
                        <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                          Decisão
                        </h4>
                        <p className="text-sm">{item.decisao}</p>
                      </div>
                      <div>
                        <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                          Lição
                        </h4>
                        <p className="text-sm text-muted-foreground">{item.licao}</p>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </div>
          ))}

          {/* Recomendações Futuras */}
          <Card className="border-primary/20 bg-primary/5">
            <CardHeader>
              <div className="flex items-center gap-2">
                <ArrowRight className="h-5 w-5 text-primary" />
                <CardTitle>Recomendações Futuras</CardTitle>
              </div>
            </CardHeader>
            <CardContent>
              <ol className="space-y-2">
                {recomendacoes.map((rec, i) => (
                  <li key={i} className="flex items-start gap-3 text-sm">
                    <span className="flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                      {i + 1}
                    </span>
                    <span className="pt-0.5">{rec}</span>
                  </li>
                ))}
              </ol>
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
