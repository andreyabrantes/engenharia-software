'use client';

import Link from 'next/link';
import { ArrowLeft, Building2 } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

export default function SpecArquiteturaPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1 bg-secondary/30 py-8">
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
          <Link
            href="/specs"
            className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-primary"
          >
            <ArrowLeft className="h-4 w-4" />
            Voltar para Specs
          </Link>

          <div className="mb-8">
            <div className="mb-4 inline-flex items-center gap-3">
              <div className="rounded-lg bg-primary/10 p-2.5">
                <Building2 className="h-6 w-6 text-primary" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-foreground sm:text-3xl">
                  Documento de Arquitetura
                </h1>
                <p className="text-muted-foreground">Stack tecnológica e arquitetura do BoraAli</p>
              </div>
            </div>
          </div>

          <Card>
            <CardContent className="prose prose-slate max-w-none p-6 dark:prose-invert">
              <h2>1. Aspecto 1: Stack Tecnológica</h2>

              <h3>1.1 Frontend</h3>
              <table>
                <thead>
                  <tr>
                    <th>Tecnologia</th>
                    <th>Versão</th>
                    <th>Finalidade</th>
                  </tr>
                </thead>
                <tbody>
                  <tr><td>Next.js</td><td>16.2.6</td><td>Framework React com SSR/CSR e App Router</td></tr>
                  <tr><td>React</td><td>19</td><td>Biblioteca para interfaces de usuário</td></tr>
                  <tr><td>TypeScript</td><td>5.7.3</td><td>Superset JavaScript com tipagem estática</td></tr>
                  <tr><td>Tailwind CSS</td><td>4.2</td><td>Framework CSS utility-first</td></tr>
                  <tr><td>shadcn/ui</td><td>-</td><td>Componentes React sobre Radix UI</td></tr>
                  <tr><td>Radix UI</td><td>-</td><td>Primitivas de UI acessíveis</td></tr>
                  <tr><td>Lucide React</td><td>0.564</td><td>Biblioteca de ícones SVG</td></tr>
                  <tr><td>React Hook Form</td><td>7.54</td><td>Gerenciamento de formulários</td></tr>
                  <tr><td>Zod</td><td>3.24</td><td>Validação de esquemas TypeScript</td></tr>
                  <tr><td>Sonner</td><td>1.7</td><td>Notificações toast</td></tr>
                  <tr><td>date-fns</td><td>4.1</td><td>Manipulação de datas</td></tr>
                  <tr><td>Recharts</td><td>2.15</td><td>Gráficos (planejado)</td></tr>
                </tbody>
              </table>

              <h3>1.2 Backend</h3>
              <table>
                <thead>
                  <tr>
                    <th>Tecnologia</th>
                    <th>Versão</th>
                    <th>Finalidade</th>
                  </tr>
                </thead>
                <tbody>
                  <tr><td>.NET</td><td>8.0</td><td>Framework de desenvolvimento cross-platform</td></tr>
                  <tr><td>C#</td><td>12</td><td>Linguagem de programação principal</td></tr>
                  <tr><td>Dapper</td><td>2.1.28</td><td>Micro-ORM para queries SQL rápidas</td></tr>
                  <tr><td>SQLite</td><td>-</td><td>Banco de dados relacional embarcado</td></tr>
                  <tr><td>BCrypt.Net-Next</td><td>4.0.3</td><td>Hash seguro de senhas</td></tr>
                  <tr><td>JWT Bearer</td><td>8.0</td><td>Autenticação stateless</td></tr>
                  <tr><td>FluentValidation</td><td>11.3</td><td>Validação declarativa</td></tr>
                  <tr><td>AutoMapper</td><td>12.0</td><td>Mapeamento objeto-objeto</td></tr>
                  <tr><td>Serilog</td><td>8.0</td><td>Logging estruturado</td></tr>
                  <tr><td>Swashbuckle</td><td>6.5</td><td>Swagger/OpenAPI</td></tr>
                  <tr><td>MailKit</td><td>4.16</td><td>Envio de e-mails (planejado)</td></tr>
                  <tr><td>QRCoder</td><td>1.8</td><td>Geração de QR Codes (planejado)</td></tr>
                </tbody>
              </table>

              <h2>2. Aspecto 2: Arquitetura do Produto</h2>

              <h3>2.1 Arquitetura em Camadas</h3>
              <p>
                O BoraAli adota uma <strong>arquitetura de camadas (layered architecture)</strong> com separação
                clara entre frontend e backend, seguindo princípios da Clean Architecture.
              </p>

              <pre><code>{`┌─────────────────────────────────────────────────────────┐
│                    FRONTEND (Next.js)                     │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │
│  │  Páginas  │  │Componentes│  │   Hooks   │  │  Utilit. │ │
│  │ (App Dir) │  │ (shadcn)  │  │ (useAuth) │  │ (api-typ)│ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │
│                        │ HTTP/JSON                        │
└────────────────────────┬──────────────────────────────────┘
                         │
┌────────────────────────▼──────────────────────────────────┐
│                    BACKEND (.NET 8)                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              API Layer (Controllers)                  │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐           │  │
│  │  │ Events   │  │   Auth   │  │  Orders  │           │  │
│  │  │Controller│  │Controller│  │Controller│           │  │
│  │  └──────────┘  └──────────┘  └──────────┘           │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Service Layer                            │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐           │  │
│  │  │EventSvc  │  │ AuthSvc  │  │OrderSvc  │           │  │
│  │  └──────────┘  └──────────┘  └──────────┘           │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           Repository Layer (Dapper)                   │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐           │  │
│  │  │EventRepo │  │GenericRepo│  │UnitOfWork│           │  │
│  │  └──────────┘  └──────────┘  └──────────┘           │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Database (SQLite)                        │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘`}</code></pre>

              <h3>2.2 Estrutura de Projetos (Backend)</h3>
              <ul>
                <li><strong>BoraAli.Core</strong> - Entidades, interfaces, exceções (camada de domínio)</li>
                <li><strong>BoraAli.Infrastructure</strong> - Repositórios Dapper, DbSession, middleware</li>
                <li><strong>BoraAli.Api</strong> - Controllers, services, configuração (Program.cs)</li>
              </ul>

              <h3>2.3 Padrões Arquiteturais</h3>
              <ul>
                <li><strong>Repository Pattern</strong> - Abstração do acesso a dados via interfaces genéricas</li>
                <li><strong>Unit of Work</strong> - Transações coordenadas entre múltiplos repositórios</li>
                <li><strong>Dapper</strong> - Consultas SQL otimizadas com mapeamento automático</li>
                <li><strong>JWT Authentication</strong> - Tokens com claims de Role e políticas de autorização</li>
                <li><strong>Rate Limiting</strong> - Proteção contra abusos (100 req/min global)</li>
              </ul>

              <h3>2.4 Modelo de Dados</h3>
              <p>7 tabelas relacionadas: Users, Categories, Events, TicketTypes, Orders, OrderItems, Seats</p>

              <h3>2.5 API Endpoints</h3>
              <table>
                <thead>
                  <tr>
                    <th>Método</th>
                    <th>Rota</th>
                    <th>Descrição</th>
                    <th>Auth</th>
                  </tr>
                </thead>
                <tbody>
                  <tr><td>GET</td><td>/api/events</td><td>Listar eventos (paginado)</td><td>-</td></tr>
                  <tr><td>GET</td><td>/api/events/featured</td><td>Eventos em destaque</td><td>-</td></tr>
                  <tr><td>GET</td><td>/api/events/categories</td><td>Listar categorias</td><td>-</td></tr>
                  <tr><td>GET</td><td>/api/events/{'{'}id{'}'}</td><td>Detalhes do evento</td><td>-</td></tr>
                  <tr><td>POST</td><td>/api/events</td><td>Criar evento</td><td>✅</td></tr>
                  <tr><td>PUT</td><td>/api/events/{'{'}id{'}'}</td><td>Atualizar evento</td><td>✅</td></tr>
                  <tr><td>DELETE</td><td>/api/events/{'{'}id{'}'}</td><td>Remover evento</td><td>✅</td></tr>
                  <tr><td>POST</td><td>/api/auth/register</td><td>Registrar usuário</td><td>-</td></tr>
                  <tr><td>POST</td><td>/api/auth/login</td><td>Login</td><td>-</td></tr>
                  <tr><td>POST</td><td>/api/orders</td><td>Criar pedido</td><td>✅</td></tr>
                  <tr><td>GET</td><td>/api/orders/my</td><td>Meus pedidos</td><td>✅</td></tr>
                  <tr><td>POST</td><td>/api/upload/image</td><td>Upload de imagem</td><td>✅</td></tr>
                  <tr><td>GET</td><td>/health</td><td>Health check</td><td>-</td></tr>
                </tbody>
              </table>
            </CardContent>
          </Card>

          <div className="mt-6 text-center">
            <Link href="/specs">
              <Button variant="outline" className="gap-2">
                <ArrowLeft className="h-4 w-4" />
                Voltar para Specs
              </Button>
            </Link>
          </div>
        </div>
      </main>
      <Footer />
    </div>
  );
}
