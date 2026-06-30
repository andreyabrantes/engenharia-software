'use client';

import Link from 'next/link';
import { ArrowLeft, Users, CheckCircle2, XCircle, ArrowRight } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

const casosDeUso = [
  {
    id: 'UC-01',
    nome: 'Navegar Eventos',
    ator: 'Cliente (não autenticado)',
    preCondicoes: 'Nenhuma',
    posCondicoes: 'O sistema exibe a lista de eventos disponíveis',
    fluxoPrincipal: [
      'O usuário acessa a página inicial (/)',
      'O sistema carrega os eventos em destaque no carrossel (HeroSection)',
      'O sistema carrega o grid de eventos paginado',
      'O usuário visualiza os cards com imagem, título, data, local e preço',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - Nenhum evento encontrado', descricao: 'O sistema exibe mensagem "Nenhum evento encontrado"' },
      { nome: 'FA02 - Erro de carregamento', descricao: 'O sistema tenta carregar da API real. Se falhar, utiliza dados mockados como fallback' },
    ],
  },
  {
    id: 'UC-02',
    nome: 'Visualizar Detalhes do Evento',
    ator: 'Cliente (não autenticado)',
    preCondicoes: 'O evento deve existir no sistema',
    posCondicoes: 'O sistema exibe os detalhes completos do evento',
    fluxoPrincipal: [
      'O usuário clica em um card de evento na página inicial',
      'O sistema redireciona para /evento/{id}',
      'O sistema carrega os detalhes do evento (banner, data, horário, local, descrição)',
      'O sistema carrega os tipos de ingresso disponíveis com preços',
      'O sistema exibe o card do organizador',
      'O usuário visualiza as informações',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - Evento não encontrado', descricao: 'O sistema exibe mensagem "Evento não encontrado"' },
    ],
  },
  {
    id: 'UC-03',
    nome: 'Buscar Eventos',
    ator: 'Cliente (não autenticado)',
    preCondicoes: 'Nenhuma',
    posCondicoes: 'O sistema exibe os resultados da busca',
    fluxoPrincipal: [
      'O usuário digita um termo de busca no campo de pesquisa do header',
      'O sistema filtra eventos por título, descrição, cidade ou categoria',
      'O sistema exibe os resultados no grid',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - Nenhum resultado', descricao: 'O sistema exibe mensagem "Nenhum evento encontrado para sua busca"' },
      { nome: 'FA02 - Filtro por categoria', descricao: 'O usuário seleciona uma categoria no dropdown; o sistema filtra os eventos' },
      { nome: 'FA03 - Filtro por cidade', descricao: 'O usuário seleciona uma cidade no dropdown; o sistema filtra os eventos' },
    ],
  },
  {
    id: 'UC-04',
    nome: 'Registrar-se',
    ator: 'Cliente (não autenticado)',
    preCondicoes: 'O usuário não deve possuir cadastro prévio com o mesmo e-mail',
    posCondicoes: 'Uma nova conta de usuário é criada no sistema',
    fluxoPrincipal: [
      'O usuário acessa a página de login (/login)',
      'O sistema exibe o formulário de login',
      'O usuário clica em "Criar conta"',
      'O sistema alterna para o modo de cadastro',
      'O usuário preenche: nome, CPF (com formatação automática), e-mail, senha (com indicador de força), confirmação',
      'O usuário seleciona o perfil (Cliente ou Organizador) através de cards visuais',
      'O usuário clica em "Criar conta"',
      'O sistema valida os dados (Zod + FluentValidation)',
      'O sistema envia requisição POST /api/auth/register',
      'O sistema retorna token JWT e dados do usuário',
      'O sistema armazena o token no localStorage',
      'O sistema redireciona para a página inicial',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - E-mail já cadastrado', descricao: 'O sistema exibe toast de erro "E-mail já cadastrado"' },
      { nome: 'FA02 - CPF inválido', descricao: 'O sistema exibe erro de validação "CPF inválido"' },
      { nome: 'FA03 - Senha fraca', descricao: 'O sistema exibe erro "Senha deve conter no mínimo 8 caracteres, maiúsculas, minúsculas e números"' },
    ],
  },
  {
    id: 'UC-05',
    nome: 'Autenticar-se',
    ator: 'Cliente (não autenticado)',
    preCondicoes: 'O usuário deve possuir cadastro no sistema',
    posCondicoes: 'O usuário recebe um token JWT válido',
    fluxoPrincipal: [
      'O usuário acessa a página de login (/login)',
      'O sistema exibe o formulário de login (e-mail + senha)',
      'O usuário preenche e-mail e senha',
      'O usuário clica em "Entrar"',
      'O sistema valida os campos',
      'O sistema envia requisição POST /api/auth/login',
      'O sistema valida as credenciais (BCrypt)',
      'O sistema gera um token JWT com claims (Id, Name, Email, Role)',
      'O sistema retorna o token e dados do usuário',
      'O sistema armazena o token no localStorage',
      'O sistema redireciona para a página inicial',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - Credenciais inválidas', descricao: 'O sistema exibe toast de erro "E-mail ou senha inválidos"' },
      { nome: 'FA02 - Conta inativa', descricao: 'O sistema exibe toast de erro "Conta desativada. Contate o administrador"' },
      { nome: 'FA03 - Rate limit excedido', descricao: 'O sistema exibe toast de erro "Muitas tentativas. Tente novamente em 1 minuto"' },
    ],
  },
  {
    id: 'UC-06',
    nome: 'Comprar Ingresso',
    ator: 'Cliente (autenticado)',
    preCondicoes: 'Usuário autenticado. Evento com ingressos disponíveis.',
    posCondicoes: 'Um pedido é criado com status "Pending". A quantidade disponível do ingresso é reduzida.',
    fluxoPrincipal: [
      'O usuário está na página do evento (/evento/{id})',
      'O sistema exibe o seletor de ingressos (TicketSelector) com tipos, preços e quantidades',
      'O usuário seleciona os tipos e quantidades desejadas',
      'O sistema calcula o subtotal, taxa de serviço e total',
      'O usuário clica em "Continuar para Pagamento"',
      'O sistema verifica se o usuário está autenticado',
      'O sistema redireciona para /checkout/{id}',
      'O sistema exibe o formulário de checkout (CheckoutForm)',
      'O usuário preenche dados pessoais e seleciona método de pagamento',
      'O usuário confirma a compra',
      'O sistema valida os dados e envia POST /api/orders',
      'O backend inicia transação, cria pedido e itens, atualiza disponibilidade',
      'O backend confirma a transação',
      'O sistema exibe toast de sucesso',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - Usuário não autenticado', descricao: 'Redireciona para /login. Após autenticação, retorna ao checkout.' },
      { nome: 'FA02 - Ingresso esgotado', descricao: 'Backend detecta quantidade insuficiente, faz rollback. Toast "Ingressos esgotados".' },
      { nome: 'FA03 - Dados de pagamento inválidos', descricao: 'Sistema exibe erro de validação no campo específico.' },
    ],
  },
  {
    id: 'UC-07',
    nome: 'Visualizar Pedidos',
    ator: 'Cliente (autenticado)',
    preCondicoes: 'Usuário autenticado',
    posCondicoes: 'O sistema exibe a lista de pedidos do usuário',
    fluxoPrincipal: [
      'O usuário acessa a seção "Meus Pedidos" (via menu do usuário)',
      'O sistema envia requisição GET /api/orders/my com token JWT',
      'O sistema exibe a lista de pedidos com código, data, evento, valor e status',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - Nenhum pedido', descricao: 'O sistema exibe mensagem "Você ainda não possui pedidos"' },
    ],
  },
  {
    id: 'UC-08',
    nome: 'Gerenciar Perfil',
    ator: 'Cliente ou Organizador (autenticado)',
    preCondicoes: 'Usuário autenticado',
    posCondicoes: 'A ação selecionada é executada',
    fluxoPrincipal: [
      'O usuário acessa o menu do usuário no header',
      'O sistema exibe as opções: "Meus Pedidos" (cliente), "Criar Evento" (organizador), "Sair"',
      'O usuário seleciona uma opção',
      'O sistema executa a ação correspondente',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - Logout', descricao: 'O sistema remove o token do localStorage e redireciona para a página inicial' },
    ],
  },
  {
    id: 'UC-09',
    nome: 'Criar Evento',
    ator: 'Organizador (autenticado)',
    preCondicoes: 'Usuário autenticado com perfil "Organizador"',
    posCondicoes: 'Um novo evento é criado com status "Draft"',
    fluxoPrincipal: [
      'O usuário acessa a página de criação (/criar-evento)',
      'O sistema verifica se o usuário é organizador',
      'O sistema exibe o formulário dividido em 4 cards (Informações, Imagem, Data/Local, Ingressos)',
      'O usuário preenche as informações e seleciona uma categoria',
      'O usuário faz upload da imagem com preview',
      'O usuário preenche data, horário e endereço',
      'O usuário adiciona tipos de ingresso (mínimo 1)',
      'O usuário clica em "Criar Evento"',
      'O sistema valida e envia POST /api/events',
      'O sistema exibe toast de sucesso e redireciona para o evento criado',
    ],
    fluxosAlternativos: [
      { nome: 'FA01 - Não é organizador', descricao: 'O sistema redireciona para a página inicial' },
      { nome: 'FA02 - Imagem muito grande', descricao: 'O sistema exibe erro "Tamanho máximo: 5MB"' },
      { nome: 'FA03 - Nenhum ingresso', descricao: 'O sistema exibe erro "Adicione pelo menos um tipo de ingresso"' },
    ],
  },
  {
    id: 'UC-10',
    nome: 'Gerenciar Evento',
    ator: 'Organizador (autenticado)',
    preCondicoes: 'O evento deve pertencer ao organizador',
    posCondicoes: 'O evento é atualizado',
    fluxoPrincipal: [
      'O organizador acessa a página de gerenciamento do evento',
      'O sistema exibe as opções: editar, publicar, cancelar',
      'O organizador seleciona uma ação',
      'O sistema executa a ação (PUT /api/events/{id} ou alteração de status)',
      'O sistema exibe toast de confirmação',
    ],
    fluxosAlternativos: [],
  },
  {
    id: 'UC-11',
    nome: 'Gerenciar Ingressos',
    ator: 'Organizador (autenticado)',
    preCondicoes: 'O evento deve pertencer ao organizador',
    posCondicoes: 'Os tipos de ingresso são atualizados',
    fluxoPrincipal: [
      'O organizador acessa a seção de ingressos do evento',
      'O sistema exibe os tipos de ingresso existentes',
      'O organizador pode adicionar, remover ou editar tipos',
      'O sistema valida e persiste as alterações',
    ],
    fluxosAlternativos: [],
  },
  {
    id: 'UC-12',
    nome: 'Gerenciar Usuários',
    ator: 'Admin',
    preCondicoes: 'Usuário autenticado com perfil "Admin"',
    posCondicoes: 'O sistema de usuários é atualizado',
    fluxoPrincipal: [
      'O admin acessa o painel administrativo',
      'O sistema exibe a lista de usuários',
      'O admin pode ativar/desativar contas, alterar perfis',
      'O sistema persiste as alterações',
    ],
    fluxosAlternativos: [],
  },
  {
    id: 'UC-13',
    nome: 'Moderar Conteúdo',
    ator: 'Admin',
    preCondicoes: 'Usuário autenticado com perfil "Admin"',
    posCondicoes: 'O conteúdo é moderado',
    fluxoPrincipal: [
      'O admin acessa o painel de moderação',
      'O sistema exibe eventos pendentes de revisão',
      'O admin aprova ou rejeita eventos',
      'O sistema atualiza o status do evento',
    ],
    fluxosAlternativos: [],
  },
  {
    id: 'UC-14',
    nome: 'Enviar Notificação',
    ator: 'Sistema',
    preCondicoes: 'Um evento que dispara notificação deve ocorrer',
    posCondicoes: 'Um e-mail é enviado ao destinatário',
    fluxoPrincipal: [
      'Um evento de domínio ocorre (ex: pedido confirmado)',
      'O sistema dispara o serviço de e-mail (MailKit)',
      'O sistema envia o e-mail com template apropriado',
      'O sistema registra o envio no log (Serilog)',
    ],
    fluxosAlternativos: [],
  },
  {
    id: 'UC-15',
    nome: 'Processar Pagamento',
    ator: 'Sistema',
    preCondicoes: 'Um pedido com status "Pending" deve existir',
    posCondicoes: 'O pedido é confirmado ou cancelado',
    fluxoPrincipal: [
      'O sistema recebe a confirmação de pagamento do gateway',
      'O sistema atualiza o status do pedido para "Confirmed"',
      'O sistema atualiza a data de confirmação (ConfirmedAt)',
      'O sistema registra o ID do pagamento (PaymentId)',
    ],
    fluxosAlternativos: [],
  },
  {
    id: 'UC-16',
    nome: 'Registrar Log',
    ator: 'Sistema',
    preCondicoes: 'Uma operação relevante deve ocorrer',
    posCondicoes: 'Um log é registrado no arquivo e console',
    fluxoPrincipal: [
      'Uma operação ocorre (requisição, erro, alteração de dados)',
      'O sistema registra o log via Serilog',
      'O log é escrito no console e no arquivo com rotação diária',
    ],
    fluxosAlternativos: [],
  },
];

const matrizRastreabilidade = [
  { uc: 'UC-01', spec: 'S8', funcionalidade: 'F2', endpoint: 'GET /api/events, GET /api/events/featured' },
  { uc: 'UC-02', spec: 'S10', funcionalidade: 'F3', endpoint: 'GET /api/events/{id}' },
  { uc: 'UC-03', spec: 'S8', funcionalidade: 'F2', endpoint: 'GET /api/events (query params)' },
  { uc: 'UC-04', spec: 'S9', funcionalidade: 'F1', endpoint: 'POST /api/auth/register' },
  { uc: 'UC-05', spec: 'S9', funcionalidade: 'F1', endpoint: 'POST /api/auth/login' },
  { uc: 'UC-06', spec: 'S11', funcionalidade: 'F5', endpoint: 'POST /api/orders' },
  { uc: 'UC-07', spec: 'S11', funcionalidade: 'F8', endpoint: 'GET /api/orders/my' },
  { uc: 'UC-08', spec: 'S9', funcionalidade: 'F1', endpoint: '— (client-side)' },
  { uc: 'UC-09', spec: 'S12', funcionalidade: 'F6', endpoint: 'POST /api/events' },
  { uc: 'UC-10', spec: 'S12', funcionalidade: 'F6', endpoint: 'PUT /api/events/{id}' },
  { uc: 'UC-11', spec: 'S12', funcionalidade: 'F4', endpoint: 'PUT /api/events/{id}' },
  { uc: 'UC-12', spec: '—', funcionalidade: '—', endpoint: '— (planejado)' },
  { uc: 'UC-13', spec: '—', funcionalidade: '—', endpoint: '— (planejado)' },
  { uc: 'UC-14', spec: 'S14', funcionalidade: 'FP2', endpoint: '— (serviço interno)' },
  { uc: 'UC-15', spec: 'S11', funcionalidade: 'F5', endpoint: '— (serviço interno)' },
  { uc: 'UC-16', spec: 'S6', funcionalidade: '—', endpoint: '— (middleware)' },
];

export default function SpecCasosDeUsoPage() {
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
              <Users className="h-8 w-8 text-primary" />
              <h1 className="text-3xl font-bold tracking-tight">Casos de Uso</h1>
            </div>
            <p className="mt-2 text-muted-foreground">
              Documento de casos de uso do sistema <strong>BoraAli</strong>, detalhando as interações entre
              atores e o sistema. Cada caso de uso inclui fluxo principal, fluxos alternativos,
              pré-condições e pós-condições.
            </p>
          </div>

          {/* Atores */}
          <Card className="mb-8">
            <CardHeader>
              <CardTitle className="text-lg">Atores do Sistema</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="rounded-lg border border-border p-4">
                  <h4 className="mb-1 font-semibold">Cliente</h4>
                  <p className="text-sm text-muted-foreground">Usuário não autenticado ou autenticado com perfil "Cliente". Pode navegar, buscar eventos e comprar ingressos.</p>
                </div>
                <div className="rounded-lg border border-border p-4">
                  <h4 className="mb-1 font-semibold">Organizador</h4>
                  <p className="text-sm text-muted-foreground">Usuário autenticado com perfil "Organizador". Pode criar e gerenciar eventos.</p>
                </div>
                <div className="rounded-lg border border-border p-4">
                  <h4 className="mb-1 font-semibold">Admin</h4>
                  <p className="text-sm text-muted-foreground">Usuário autenticado com perfil "Admin". Pode gerenciar usuários, categorias e moderar conteúdo.</p>
                </div>
                <div className="rounded-lg border border-border p-4">
                  <h4 className="mb-1 font-semibold">Sistema</h4>
                  <p className="text-sm text-muted-foreground">Ator secundário que representa processos automáticos (envio de e-mail, processamento de pagamento, logging).</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Casos de Uso */}
          <div className="space-y-4">
            {casosDeUso.map((uc) => (
              <Card key={uc.id} className="transition-shadow hover:shadow-md">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-3">
                      <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary/10 text-sm font-bold text-primary">
                        {uc.id.replace('UC-', '')}
                      </span>
                      <div>
                        <CardTitle className="text-lg">{uc.nome}</CardTitle>
                        <p className="text-xs text-muted-foreground">Ator: {uc.ator}</p>
                      </div>
                    </div>
                  </div>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="grid gap-2 sm:grid-cols-2">
                    <div>
                      <h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Pré-condições</h4>
                      <p className="text-sm">{uc.preCondicoes}</p>
                    </div>
                    <div>
                      <h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Pós-condições</h4>
                      <p className="text-sm">{uc.posCondicoes}</p>
                    </div>
                  </div>

                  <div>
                    <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Fluxo Principal</h4>
                    <ol className="space-y-1">
                      {uc.fluxoPrincipal.map((passo, i) => (
                        <li key={i} className="flex items-start gap-2 text-sm">
                          <span className="mt-0.5 flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-medium text-primary">
                            {i + 1}
                          </span>
                          <span className="pt-0.5">{passo}</span>
                        </li>
                      ))}
                    </ol>
                  </div>

                  {uc.fluxosAlternativos.length > 0 && (
                    <div>
                      <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Fluxos Alternativos</h4>
                      <div className="space-y-1">
                        {uc.fluxosAlternativos.map((fa, i) => (
                          <div key={i} className="flex items-start gap-2 text-sm">
                            <ArrowRight className="mt-0.5 h-3.5 w-3.5 flex-shrink-0 text-amber-500" />
                            <span><strong>{fa.nome}:</strong> {fa.descricao}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Matriz de Rastreabilidade */}
          <Card className="mt-8">
            <CardHeader>
              <CardTitle className="text-lg">Matriz de Rastreabilidade</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border">
                      <th className="px-3 py-2 text-left font-medium">Caso de Uso</th>
                      <th className="px-3 py-2 text-left font-medium">Spec</th>
                      <th className="px-3 py-2 text-left font-medium">Funcionalidade</th>
                      <th className="px-3 py-2 text-left font-medium">Endpoint</th>
                    </tr>
                  </thead>
                  <tbody>
                    {matrizRastreabilidade.map((item) => (
                      <tr key={item.uc} className="border-b border-border/50">
                        <td className="px-3 py-2 font-medium">{item.uc}</td>
                        <td className="px-3 py-2 text-muted-foreground">{item.spec}</td>
                        <td className="px-3 py-2 text-muted-foreground">{item.funcionalidade}</td>
                        <td className="px-3 py-2 font-mono text-xs text-muted-foreground">{item.endpoint}</td>
                      </tr>
                    ))}
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
