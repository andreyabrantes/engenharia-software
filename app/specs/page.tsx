'use client';

import Link from 'next/link';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { FileText, Eye, Building2, Map, BookOpen, Lightbulb, Users, ShieldCheck } from 'lucide-react';

const specs = [
  {
    id: 'visao',
    title: 'Documento de Visão',
    description: 'Objetivo do produto, benefícios, público-alvo e valor entregue pelo BoraAli.',
    icon: Eye,
    href: '/specs/visao',
    badge: 'Concluído',
    badgeColor: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  },
  {
    id: 'arquitetura',
    title: 'Documento de Arquitetura',
    description: 'Stack tecnológica, arquitetura do produto, padrões, diagrama UML e modelo de dados.',
    icon: Building2,
    href: '/specs/arquitetura',
    badge: 'Concluído',
    badgeColor: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  },
  {
    id: 'roadmap',
    title: 'Roadmap',
    description: 'Todas as specs planejadas, executadas e apontamentos para visão e arquitetura.',
    icon: Map,
    href: '/specs/roadmap',
    badge: 'Concluído',
    badgeColor: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  },
  {
    id: 'casos-de-uso',
    title: 'Casos de Uso',
    description: '16 casos de uso detalhados com fluxos principal e alternativos, pré e pós-condições.',
    icon: Users,
    href: '/specs/casos-de-uso',
    badge: 'Novo',
    badgeColor: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
  },
  {
    id: 'requisitos-nao-funcionais',
    title: 'Requisitos Não-Funcionais',
    description: '45 requisitos de performance, segurança, usabilidade, disponibilidade e manutenibilidade.',
    icon: ShieldCheck,
    href: '/specs/requisitos-nao-funcionais',
    badge: 'Novo',
    badgeColor: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
  },
  {
    id: 'adr',
    title: 'ADR - Architecture Decision Records',
    description: 'Decisões arquiteturais importantes tomadas durante o desenvolvimento.',
    icon: BookOpen,
    href: '/specs/adr',
    badge: 'Concluído',
    badgeColor: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  },
  {
    id: 'licoes',
    title: 'Lições Aprendidas',
    description: 'Decisões tomadas, aprendizados e recomendações futuras.',
    icon: Lightbulb,
    href: '/specs/licoes',
    badge: 'Concluído',
    badgeColor: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  },
];

export default function SpecsPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />

      <main className="flex-1 bg-secondary/30 py-12">
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-10 text-center">
            <div className="mb-4 inline-flex items-center justify-center rounded-full bg-primary/10 p-3">
              <FileText className="h-8 w-8 text-primary" />
            </div>
            <h1 className="mb-3 text-3xl font-bold text-foreground sm:text-4xl">
              Especificações do Projeto
            </h1>
            <p className="mx-auto max-w-2xl text-muted-foreground">
              Documentos completos de especificação do BoraAli, incluindo visão do produto,
              arquitetura do sistema, roadmap, decisões arquiteturais e lições aprendidas.
            </p>
          </div>

          {/* Specs Grid */}
          <div className="grid gap-6 sm:grid-cols-2">
            {specs.map((spec) => {
              const Icon = spec.icon;
              return (
                <Link key={spec.id} href={spec.href}>
                  <Card className="h-full transition-all duration-300 hover:shadow-lg hover:border-primary/50 group cursor-pointer">
                    <CardHeader>
                      <div className="flex items-start justify-between">
                        <div className="flex items-center gap-3">
                          <div className="rounded-lg bg-primary/10 p-2.5 transition-colors group-hover:bg-primary/20">
                            <Icon className="h-6 w-6 text-primary" />
                          </div>
                          <div>
                            <CardTitle className="text-lg">{spec.title}</CardTitle>
                          </div>
                        </div>
                      </div>
                    </CardHeader>
                    <CardContent>
                      <CardDescription className="text-sm leading-relaxed">
                        {spec.description}
                      </CardDescription>
                      <div className="mt-4">
                        <Badge className={spec.badgeColor}>
                          {spec.badge}
                        </Badge>
                      </div>
                    </CardContent>
                  </Card>
                </Link>
              );
            })}
          </div>

          {/* Summary */}
          <div className="mt-10 rounded-lg border border-border bg-background p-6">
            <h2 className="mb-3 text-lg font-semibold text-foreground">Resumo da Documentação</h2>
            <div className="space-y-2 text-sm text-muted-foreground">
              <p>
                <strong className="text-foreground">Documento de Visão:</strong> Explica o que o produto faz,
                o problema que resolve e o valor que entrega para organizadores e participantes.
              </p>
              <p>
                <strong className="text-foreground">Documento de Arquitetura:</strong> Detalha a stack tecnológica
                (Next.js, .NET 8, SQLite, Dapper, etc.) e a arquitetura conceitual do produto com diagramas
                de camadas, diagrama de classes UML e fluxos.
              </p>
              <p>
                <strong className="text-foreground">Roadmap:</strong> Lista completa de todas as 21 specs do sistema
                (12 concluídas, 4 em planejamento, 5 futuras), cada uma apontando para a seção correspondente
                da Visão e da Arquitetura.
              </p>
              <p>
                <strong className="text-foreground">Casos de Uso:</strong> 16 casos de uso detalhados com diagrama
                ASCII, fluxos principal e alternativos, pré-condições, pós-condições e matriz de rastreabilidade.
              </p>
              <p>
                <strong className="text-foreground">Requisitos Não-Funcionais:</strong> 45 requisitos organizados
                em 8 categorias (performance, segurança, usabilidade, disponibilidade, manutenibilidade,
                portabilidade, escalabilidade, restrições técnicas) com métricas e rastreabilidade para ADRs.
              </p>
              <p>
                <strong className="text-foreground">ADR:</strong> 12 decisões arquiteturais documentadas com
                contexto, alternativas consideradas e consequências de cada escolha.
              </p>
              <p>
                <strong className="text-foreground">Lições Aprendidas:</strong> Reflexões sobre decisões técnicas,
                desafios enfrentados e recomendações para evolução futura do projeto.
              </p>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
