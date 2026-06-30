'use client';

import Link from 'next/link';
import { ArrowLeft, FileText, CheckCircle2, XCircle, AlertTriangle } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

const adrs = [
  {
    id: 'ADR-001',
    titulo: 'Uso de SQLite como Banco de Dados',
    status: 'Aceito',
    contexto: 'Precisávamos de um banco de dados relacional para o MVP que fosse simples de configurar, não exigisse servidor externo e permitisse desenvolvimento rápido sem infraestrutura complexa.',
    decisao: 'Utilizar SQLite como banco de dados principal, com o pacote Microsoft.Data.Sqlite.',
    alternativas: [
      { nome: 'PostgreSQL', motivo: 'Exigiria servidor externo, configuração de Docker ou serviço cloud' },
      { nome: 'SQL Server', motivo: 'Mais pesado para desenvolvimento local' },
      { nome: 'MySQL', motivo: 'Configuração adicional' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Setup zero: apenas um arquivo .db é criado automaticamente' },
      { tipo: 'positiva', texto: 'Portabilidade: o banco viaja com o projeto' },
      { tipo: 'positiva', texto: 'Performance excelente para o volume esperado' },
      { tipo: 'negativa', texto: 'Limitação de concorrência (escrita simultânea) - aceitável para MVP' },
      { tipo: 'negativa', texto: 'Sem recursos avançados como stored procedures ou replicação' },
    ],
  },
  {
    id: 'ADR-002',
    titulo: 'Dapper como ORM (em vez de Entity Framework)',
    status: 'Aceito',
    contexto: 'Precisávamos de um ORM leve e performático para consultas SQL. Entity Framework Core é o padrão no ecossistema .NET, mas adiciona overhead significativo.',
    decisao: 'Utilizar Dapper (micro-ORM) para acesso a dados, combinado com o padrão Repository.',
    alternativas: [
      { nome: 'Entity Framework Core', motivo: 'Overhead de performance, mais complexidade de configuração' },
      { nome: 'ADO.NET puro', motivo: 'Muito trabalho manual para mapeamento objeto-relacional' },
      { nome: 'NHibernate', motivo: 'Complexidade excessiva para o escopo do projeto' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Performance superior (Dapper é um dos ORMs mais rápidos do .NET)' },
      { tipo: 'positiva', texto: 'Controle total sobre as queries SQL' },
      { tipo: 'positiva', texto: 'Facilidade para consultas complexas com JOINs' },
      { tipo: 'negativa', texto: 'Mais trabalho manual para escrever SQL' },
      { tipo: 'negativa', texto: 'Sem migrations automáticas (scripts SQL manuais)' },
    ],
  },
  {
    id: 'ADR-003',
    titulo: 'Arquitetura de 3 Camadas (Clean Architecture)',
    status: 'Aceito',
    contexto: 'Precisávamos de uma arquitetura que separasse responsabilidades, facilitasse testes e permitisse evolução independente das camadas.',
    decisao: 'Adotar arquitetura em 3 camadas com separação em projetos: BoraAli.Core (Domínio), BoraAli.Infrastructure (Implementações), BoraAli.Api (Apresentação).',
    alternativas: [
      { nome: 'Monólito em projeto único', motivo: 'Viola o princípio da separação de responsabilidades' },
      { nome: 'Microservices', motivo: 'Complexidade excessiva para o porte do projeto' },
      { nome: 'Vertical Slices', motivo: 'Boa alternativa, mas menos familiar para a equipe' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Separação clara de responsabilidades' },
      { tipo: 'positiva', texto: 'Facilidade para testes unitários (interfaces podem ser mockadas)' },
      { tipo: 'positiva', texto: 'Possibilidade de substituir implementações (ex: trocar SQLite por PostgreSQL)' },
      { tipo: 'negativa', texto: 'Maior número de projetos e arquivos para gerenciar' },
    ],
  },
  {
    id: 'ADR-004',
    titulo: 'Next.js com App Router (em vez de Pages Router)',
    status: 'Aceito',
    contexto: 'O frontend precisava de um framework React moderno com suporte a Server Components, roteamento baseado em arquivos e boa integração com o ecossistema React 19.',
    decisao: 'Utilizar Next.js 16 com App Router (diretório app/).',
    alternativas: [
      { nome: 'Next.js Pages Router', motivo: 'Legado, sem suporte a Server Components' },
      { nome: 'Vite + React', motivo: 'Sem SSR/SSG nativo, exigiria configuração adicional' },
      { nome: 'Remix', motivo: 'Curva de aprendizado maior, ecossistema menor' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Server Components para páginas estáticas (performance)' },
      { tipo: 'positiva', texto: 'Client Components para páginas interativas' },
      { tipo: 'positiva', texto: 'Roteamento baseado em arquivos (produtivo)' },
      { tipo: 'positiva', texto: 'Suporte nativo a React 19' },
      { tipo: 'negativa', texto: 'App Router ainda tem algumas limitações de maturidade' },
    ],
  },
  {
    id: 'ADR-005',
    titulo: 'shadcn/ui como Design System',
    status: 'Aceito',
    contexto: 'Precisávamos de componentes de UI acessíveis, customizáveis e com boa aparência sem reinventar a roda.',
    decisao: 'Utilizar shadcn/ui (baseado em Radix UI + Tailwind CSS) como design system.',
    alternativas: [
      { nome: 'Material UI', motivo: 'Pesado, difícil de customizar, estilo "Material" engessado' },
      { nome: 'Ant Design', motivo: 'Estilo muito característico, difícil de adaptar' },
      { nome: 'Chakra UI', motivo: 'Bom, mas menos integrado com Tailwind' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Componentes acessíveis (Radix UI cuida da acessibilidade)' },
      { tipo: 'positiva', texto: 'Código-fonte dos componentes no projeto (customização total)' },
      { tipo: 'positiva', texto: 'Estilização consistente com Tailwind CSS' },
      { tipo: 'positiva', texto: '40+ componentes prontos (button, card, dialog, dropdown, etc.)' },
      { tipo: 'negativa', texto: 'Dependência de múltiplos pacotes Radix UI' },
    ],
  },
  {
    id: 'ADR-006',
    titulo: 'Autenticação JWT com BCrypt',
    status: 'Aceito',
    contexto: 'Precisávamos de um sistema de autenticação seguro, stateless e compatível com APIs REST.',
    decisao: 'Implementar autenticação via JWT (JSON Web Tokens) com senhas hash usando BCrypt (11 rounds de salt).',
    alternativas: [
      { nome: 'ASP.NET Core Identity', motivo: 'Muito acoplado ao EF Core, pesado para o escopo' },
      { nome: 'Auth0 / Firebase Auth', motivo: 'Dependência externa, custo, lock-in' },
      { nome: 'Session-based auth', motivo: 'Stateful, não escala bem para APIs' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Stateless (escalável horizontalmente)' },
      { tipo: 'positiva', texto: 'Seguro (BCrypt + JWT assinado)' },
      { tipo: 'positiva', texto: 'Controle total sobre claims e roles' },
      { tipo: 'negativa', texto: 'Gerenciamento manual de refresh tokens (planejado para futura versão)' },
      { tipo: 'negativa', texto: 'Token armazenado no localStorage (vulnerável a XSS - mitigado por boas práticas)' },
    ],
  },
  {
    id: 'ADR-007',
    titulo: 'Rate Limiting para Proteção da API',
    status: 'Aceito',
    contexto: 'A API precisava de proteção contra abusos, ataques de força bruta e sobrecarga.',
    decisao: 'Implementar Rate Limiting com políticas de janela fixa usando o middleware nativo do .NET 8.',
    alternativas: [
      { nome: 'Cloudflare / API Gateway', motivo: 'Dependência externa, custo adicional' },
      { nome: 'Middleware customizado', motivo: 'Mais trabalho, menos confiável' },
      { nome: 'Sem rate limiting', motivo: 'Inaceitável para produção' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Proteção contra ataques de força bruta (5 tentativas/min no login)' },
      { tipo: 'positiva', texto: 'Prevenção de sobrecarga (100 req/min global)' },
      { tipo: 'positiva', texto: 'Configuração granular por endpoint' },
      { tipo: 'negativa', texto: 'Pode afetar usuários legítimos em casos extremos' },
    ],
  },
  {
    id: 'ADR-008',
    titulo: 'Serilog para Logging Estruturado',
    status: 'Aceito',
    contexto: 'Precisávamos de um sistema de logging que permitisse rastrear erros, monitorar a aplicação e facilitar debugging.',
    decisao: 'Utilizar Serilog com sinks de console e arquivo com rotação diária.',
    alternativas: [
      { nome: 'NLog', motivo: 'Similar, mas Serilog tem melhor integração com .NET 8' },
      { nome: 'log4net', motivo: 'Legado, menos flexível' },
      { nome: 'Console.WriteLine', motivo: 'Inaceitável para produção' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Logging estruturado (JSON) para análise' },
      { tipo: 'positiva', texto: 'Rotação diária de arquivos (sem acumular logs)' },
      { tipo: 'positiva', texto: 'Configuração declarativa no appsettings.json' },
      { tipo: 'positiva', texto: 'Múltiplos sinks (console + arquivo)' },
      { tipo: 'negativa', texto: 'Sem sink cloud (planejado: Application Insights ou Seq)' },
    ],
  },
  {
    id: 'ADR-009',
    titulo: 'FluentValidation para Validação',
    status: 'Aceito',
    contexto: 'Precisávamos de validação declarativa e reutilizável para as requisições da API.',
    decisao: 'Utilizar FluentValidation com validação automática via AddFluentValidationAutoValidation().',
    alternativas: [
      { nome: 'Data Annotations', motivo: 'Limitado, mistura validação com modelo' },
      { nome: 'Validação manual', motivo: 'Repetitivo, propenso a erros' },
      { nome: 'Sem validação', motivo: 'Inaceitável' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Validação declarativa e fluente' },
      { tipo: 'positiva', texto: 'Separação da lógica de validação dos modelos' },
      { tipo: 'positiva', texto: 'Validação automática no pipeline do ASP.NET Core' },
      { tipo: 'positiva', texto: 'Mensagens de erro customizadas em português' },
      { tipo: 'negativa', texto: 'Mais classes para gerenciar' },
    ],
  },
  {
    id: 'ADR-010',
    titulo: 'AutoMapper para Mapeamento DTO ↔ Entidade',
    status: 'Aceito',
    contexto: 'Precisávamos mapear entidades de domínio para DTOs de resposta da API sem código repetitivo.',
    decisao: 'Utilizar AutoMapper com perfil de mapeamento centralizado.',
    alternativas: [
      { nome: 'Mapeamento manual', motivo: 'Repetitivo, propenso a erros em propriedades esquecidas' },
      { nome: 'Mapster', motivo: 'Menos conhecido, ecossistema menor' },
      { nome: 'Implicit operators', motivo: 'Viola princípios de design' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Mapeamento centralizado e reutilizável' },
      { tipo: 'positiva', texto: 'Redução de código boilerplate' },
      { tipo: 'positiva', texto: 'Facilidade para evoluir os DTOs' },
      { tipo: 'negativa', texto: 'Performance overhead (aceitável para o volume)' },
      { tipo: 'negativa', texto: 'Complexidade adicional em mapeamentos não triviais' },
    ],
  },
  {
    id: 'ADR-011',
    titulo: 'Tailwind CSS 4 para Estilização',
    status: 'Aceito',
    contexto: 'Precisávamos de uma abordagem de estilização rápida, consistente e responsiva.',
    decisao: 'Utilizar Tailwind CSS 4 com PostCSS.',
    alternativas: [
      { nome: 'CSS Modules', motivo: 'Mais verboso, sem design system integrado' },
      { nome: 'Styled Components', motivo: 'Overhead de runtime, bundle maior' },
      { nome: 'Sass/SCSS', motivo: 'Sem utility-first, mais arquivos' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Desenvolvimento rápido com classes utilitárias' },
      { tipo: 'positiva', texto: 'Bundle pequeno (purge de CSS não utilizado)' },
      { tipo: 'positiva', texto: 'Design responsivo facilitado' },
      { tipo: 'positiva', texto: 'Integração nativa com shadcn/ui' },
      { tipo: 'negativa', texto: 'HTML pode ficar verboso com muitas classes' },
    ],
  },
  {
    id: 'ADR-012',
    titulo: 'Uso de Dados Mockados como Fallback',
    status: 'Aceito',
    contexto: 'Durante o desenvolvimento, a API pode não estar disponível. Precisávamos de uma forma de desenvolver e testar o frontend independentemente.',
    decisao: 'Manter dados mockados em lib/mock-data.ts como fallback, com transição gradual para API real via lib/api-types.ts.',
    alternativas: [
      { nome: 'MSW (Mock Service Worker)', motivo: 'Mais complexo, overhead de configuração' },
      { nome: 'JSON Server', motivo: 'Dependência externa, mais um serviço para rodar' },
      { nome: 'Sem mock, apenas API', motivo: 'Impede desenvolvimento frontend independente' },
    ],
    consequencias: [
      { tipo: 'positiva', texto: 'Desenvolvimento frontend independente do backend' },
      { tipo: 'positiva', texto: 'Facilidade para testes e demonstrações' },
      { tipo: 'positiva', texto: 'Transição gradual para API real' },
      { tipo: 'negativa', texto: 'Código legado de mock precisa ser mantido' },
      { tipo: 'negativa', texto: 'Possível divergência entre mock e API real' },
    ],
  },
];

const statusIcon = (status: string) => {
  if (status === 'Aceito') return <CheckCircle2 className="h-5 w-5 text-green-500" />;
  if (status === 'Rejeitado') return <XCircle className="h-5 w-5 text-red-500" />;
  return <AlertTriangle className="h-5 w-5 text-yellow-500" />;
};

export default function SpecAdrPage() {
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
              <FileText className="h-8 w-8 text-primary" />
              <h1 className="text-3xl font-bold tracking-tight">Architecture Decision Records (ADR)</h1>
            </div>
            <p className="mt-2 text-muted-foreground">
              Este documento registra as principais decisões arquiteturais tomadas durante o desenvolvimento do <strong>BoraAli</strong>.
              Cada ADR documenta o contexto, a decisão, as alternativas consideradas e as consequências.
            </p>
          </div>

          <div className="space-y-6">
            {adrs.map((adr) => (
              <Card key={adr.id} className="transition-shadow hover:shadow-md">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-3">
                      <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary/10 text-sm font-bold text-primary">
                        {adr.id.replace('ADR-', '')}
                      </span>
                      <div>
                        <CardTitle className="text-lg">{adr.titulo}</CardTitle>
                        <p className="text-xs text-muted-foreground">{adr.id}</p>
                      </div>
                    </div>
                    <Badge className="bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
                      {statusIcon(adr.status)} {adr.status}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Contexto */}
                  <div>
                    <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Contexto</h4>
                    <p className="text-sm">{adr.contexto}</p>
                  </div>

                  {/* Decisão */}
                  <div>
                    <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Decisão</h4>
                    <p className="text-sm font-medium">{adr.decisao}</p>
                  </div>

                  {/* Alternativas */}
                  <div>
                    <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Alternativas Consideradas</h4>
                    <div className="space-y-1">
                      {adr.alternativas.map((alt, i) => (
                        <div key={i} className="flex items-start gap-2 text-sm">
                          <XCircle className="mt-0.5 h-3.5 w-3.5 flex-shrink-0 text-red-400" />
                          <span><strong>{alt.nome}:</strong> {alt.motivo}</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  {/* Consequências */}
                  <div>
                    <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Consequências</h4>
                    <div className="space-y-1">
                      {adr.consequencias.map((conseq, i) => (
                        <div key={i} className="flex items-start gap-2 text-sm">
                          {conseq.tipo === 'positiva' ? (
                            <CheckCircle2 className="mt-0.5 h-3.5 w-3.5 flex-shrink-0 text-green-500" />
                          ) : (
                            <XCircle className="mt-0.5 h-3.5 w-3.5 flex-shrink-0 text-red-400" />
                          )}
                          <span>{conseq.texto}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

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
