'use client';

import Link from 'next/link';
import { ArrowLeft, Eye } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

export default function SpecVisaoPage() {
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
                <Eye className="h-6 w-6 text-primary" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-foreground sm:text-3xl">
                  Documento de Visão
                </h1>
                <p className="text-muted-foreground">BoraAli - Plataforma de Eventos e Ingressos</p>
              </div>
            </div>
          </div>

          <Card>
            <CardContent className="prose prose-slate max-w-none p-6 dark:prose-invert">
              <h2>1. Introdução</h2>
              <h3>1.1 Propósito do Documento</h3>
              <p>
                Este documento apresenta a visão geral do produto <strong>BoraAli</strong>, uma plataforma
                digital para descoberta, criação e venda de ingressos para eventos. Ele descreve o problema
                que o produto resolve, o público-alvo, as funcionalidades principais e o valor entregue aos usuários.
              </p>

              <h3>1.2 Escopo</h3>
              <p>O BoraAli é uma plataforma web completa que conecta organizadores de eventos a participantes:</p>
              <ul>
                <li><strong>Criação e gerenciamento de eventos</strong> por organizadores</li>
                <li><strong>Descoberta de eventos</strong> por categorias, cidades e busca textual</li>
                <li><strong>Compra de ingressos</strong> com seleção de tipos e assentos</li>
                <li><strong>Processamento de pagamentos</strong> via cartão de crédito, Pix e boleto</li>
                <li><strong>Autenticação e controle de acesso</strong> com perfis de usuário (Cliente, Organizador, Admin)</li>
              </ul>

              <h2>2. Problema e Oportunidade</h2>
              <h3>2.1 Problema</h3>
              <p>
                Atualmente, organizar eventos e vender ingressos envolve múltiplas ferramentas desconectadas:
                divulgação em redes sociais, venda de ingressos em plataformas separadas, controle manual de
                capacidade e pagamentos fragmentados. Para o público, encontrar eventos relevantes próximos a
                si exige consultar várias fontes diferentes.
              </p>

              <h3>2.2 Oportunidade</h3>
              <p>
                Existe uma demanda crescente por plataformas integradas que simplifiquem tanto a vida do
                organizador quanto do participante. O mercado brasileiro de eventos movimenta bilhões de reais
                anualmente, e uma solução moderna, acessível e intuitiva pode capturar uma fatia significativa
                desse mercado.
              </p>

              <h2>3. Descrição do Produto</h2>
              <h3>3.1 O que é o BoraAli?</h3>
              <p>
                O <strong>BoraAli</strong> é uma plataforma web de ponta a ponta para o ecossistema de eventos.
                Ela permite que organizadores criem, publiquem e gerenciem eventos com venda de ingressos,
                enquanto participantes descobrem eventos filtrando por categoria, cidade ou termo de busca,
                selecionam seus ingressos e realizam a compra de forma segura.
              </p>

              <h3>3.2 Valor Entregue</h3>
              <table>
                <thead>
                  <tr>
                    <th>Para</th>
                    <th>Valor</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td><strong>Organizadores</strong></td>
                    <td>Ferramenta completa para criar eventos, configurar tipos de ingressos, gerenciar mapa de assentos, publicar e acompanhar vendas</td>
                  </tr>
                  <tr>
                    <td><strong>Participantes</strong></td>
                    <td>Descoberta facilitada com busca e filtros, seleção intuitiva de ingressos, checkout seguro com múltiplas formas de pagamento</td>
                  </tr>
                  <tr>
                    <td><strong>Administradores</strong></td>
                    <td>Visão geral da plataforma, gerenciamento de usuários e moderação de conteúdo</td>
                  </tr>
                </tbody>
              </table>

              <h3>3.3 Benefícios Principais</h3>
              <ol>
                <li><strong>Integração Completa</strong> - Uma única plataforma para criar, divulgar e vender ingressos</li>
                <li><strong>Experiência Moderna</strong> - Interface responsiva e intuitiva com Next.js e Tailwind CSS</li>
                <li><strong>Segurança</strong> - Autenticação JWT, senhas hash com BCrypt, validação com FluentValidation</li>
                <li><strong>Mapa de Assentos</strong> - Seleção visual de assentos para eventos com lugares marcados</li>
                <li><strong>Múltiplos Pagamentos</strong> - Suporte a cartão de crédito, Pix e boleto bancário</li>
                <li><strong>Performance</strong> - Arquitetura otimizada com Dapper para consultas rápidas ao banco SQLite</li>
              </ol>

              <h2>4. Público-Alvo</h2>
              <h3>4.1 Personas</h3>
              <table>
                <thead>
                  <tr>
                    <th>Persona</th>
                    <th>Perfil</th>
                    <th>Necessidades</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td><strong>Carlos (Organizador)</strong></td>
                    <td>Produtor de eventos, 35-50 anos</td>
                    <td>Criar eventos rapidamente, configurar lotes, gerenciar vendas</td>
                  </tr>
                  <tr>
                    <td><strong>Ana (Cliente)</strong></td>
                    <td>Profissional 25-40 anos</td>
                    <td>Descobrir eventos próximos, comparar preços, comprar com segurança</td>
                  </tr>
                  <tr>
                    <td><strong>João (Admin)</strong></td>
                    <td>Administrador da plataforma</td>
                    <td>Moderar conteúdo, gerenciar usuários, acompanhar métricas</td>
                  </tr>
                </tbody>
              </table>

              <h2>5. Funcionalidades do Produto</h2>
              <h3>5.1 Funcionalidades Implementadas</h3>
              <table>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Funcionalidade</th>
                    <th>Descrição</th>
                  </tr>
                </thead>
                <tbody>
                  <tr><td>F1</td><td>Autenticação e Cadastro</td><td>Registro e login com validação de CPF, força de senha, seleção de perfil</td></tr>
                  <tr><td>F2</td><td>Catálogo de Eventos</td><td>Listagem com grid responsivo, filtros por categoria e cidade, busca textual</td></tr>
                  <tr><td>F3</td><td>Página do Evento</td><td>Detalhes completos: data, local, descrição, organizador, mapa</td></tr>
                  <tr><td>F4</td><td>Seletor de Ingressos</td><td>Escolha de tipos/lotes com controle de quantidade e cálculo de total</td></tr>
                  <tr><td>F5</td><td>Checkout</td><td>Formulário de pagamento com cartão, Pix e boleto, resumo do pedido</td></tr>
                  <tr><td>F6</td><td>Criação de Eventos</td><td>Formulário completo para organizadores</td></tr>
                  <tr><td>F7</td><td>Mapa de Assentos</td><td>Assentos individuais por setor/fileira com status</td></tr>
                  <tr><td>F8</td><td>Gerenciamento de Pedidos</td><td>Criação de pedidos com itens, cálculo de totais, status</td></tr>
                  <tr><td>F9</td><td>Categorias</td><td>Organização de eventos por categorias</td></tr>
                  <tr><td>F10</td><td>Destaques</td><td>Seção de eventos em destaque com carrossel automático</td></tr>
                </tbody>
              </table>

              <h2>6. Tecnologias Utilizadas</h2>
              <table>
                <thead>
                  <tr>
                    <th>Camada</th>
                    <th>Tecnologia</th>
                    <th>Finalidade</th>
                  </tr>
                </thead>
                <tbody>
                  <tr><td><strong>Frontend</strong></td><td>Next.js 16 + React 19</td><td>Framework web moderno com SSR/CSR</td></tr>
                  <tr><td><strong>Estilização</strong></td><td>Tailwind CSS 4 + shadcn/ui</td><td>Design system responsivo e acessível</td></tr>
                  <tr><td><strong>Backend</strong></td><td>.NET 8 (C#)</td><td>API RESTful robusta e performática</td></tr>
                  <tr><td><strong>ORM/Query</strong></td><td>Dapper 2.1</td><td>Micro-ORM para consultas SQL rápidas</td></tr>
                  <tr><td><strong>Banco</strong></td><td>SQLite</td><td>Banco de dados relacional embarcado</td></tr>
                  <tr><td><strong>Autenticação</strong></td><td>JWT + BCrypt</td><td>Tokens seguros e hash de senhas</td></tr>
                  <tr><td><strong>Validação</strong></td><td>FluentValidation</td><td>Validação declarativa de dados</td></tr>
                  <tr><td><strong>Logging</strong></td><td>Serilog</td><td>Logging estruturado em arquivo e console</td></tr>
                  <tr><td><strong>Documentação</strong></td><td>Swagger/OpenAPI</td><td>Documentação interativa da API</td></tr>
                </tbody>
              </table>

              <h2>7. Conclusão</h2>
              <p>
                O <strong>BoraAli</strong> é uma plataforma completa e moderna para o mercado de eventos brasileiro.
                Com uma arquitetura robusta (Next.js + .NET 8 + SQLite), interface intuitiva e funcionalidades que
                cobrem todo o ciclo de vida de um evento — da criação à venda de ingressos — o produto entrega
                valor real tanto para organizadores quanto para participantes.
              </p>
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
