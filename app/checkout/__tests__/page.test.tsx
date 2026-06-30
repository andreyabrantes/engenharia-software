import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { Suspense } from 'react';

// ============================================================
// Mocks de módulos
// ============================================================

// Mock next/navigation
const mockPush = vi.fn();
vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
    replace: vi.fn(),
    prefetch: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
  }),
}));

// Mock de componentes de layout pesados
vi.mock('@/components/header', () => ({
  Header: () => <header data-testid="mock-header">Header</header>,
}));

vi.mock('@/components/footer', () => ({
  Footer: () => <footer data-testid="mock-footer">Footer</footer>,
}));

vi.mock('@/components/checkout-form', () => ({
  CheckoutForm: ({ event }: { event: { title: string }; selectedTickets: Record<string, number> }) => (
    <div data-testid="mock-checkout-form">
      Checkout: {event.title}
    </div>
  ),
}));

// Importamos a página APÓS os mocks estarem configurados
import CheckoutPage from '../[id]/page';

// ============================================================
// Helpers
// ============================================================

/** Cria uma resposta mock da API de evento */
function createMockEvent(overrides: Record<string, unknown> = {}) {
  return {
    id: 1,
    title: 'Hackathon CCOMP 2026',
    description: 'Maior maratona de programação',
    fullDescription: 'Descrição completa',
    eventDate: '2026-10-15',
    time: '08:00',
    location: 'Campus Unifeso',
    address: 'Av. Alberto Torres, 111',
    city: 'Teresópolis',
    isFeatured: true,
    status: 'Published',
    categoryId: 7,
    categoryName: 'Tecnologia',
    organizerId: 4,
    organizerName: 'Carlos Eventos',
    organizerAvatar: null,
    organizerFollowers: 120,
    tickets: [
      { id: 1, name: 'Equipe (até 4 pessoas)', price: 50, availableQuantity: 95, totalQuantity: 100, description: 'Inscrição para equipe' },
      { id: 2, name: 'Individual', price: 15, availableQuantity: 190, totalQuantity: 200, description: 'Inscrição individual' },
    ],
    createdAt: '2026-06-01T00:00:00Z',
    ...overrides,
  };
}

/** Configura sessionStorage com tickets selecionados */
function setSelectedTickets(eventId: number, tickets: Record<string, number>) {
  sessionStorage.setItem('selectedTickets', JSON.stringify(tickets));
  sessionStorage.setItem('eventId', String(eventId));
}

/** Configura mock do fetch para retornar sucesso */
function mockFetchSuccess(eventData = createMockEvent()) {
  vi.mocked(fetch).mockResolvedValue({
    ok: true,
    json: async () => ({ success: true, data: eventData }),
  } as Response);
}

/** Configura mock do fetch para retornar falha */
function mockFetchFailure() {
  vi.mocked(fetch).mockRejectedValue(new Error('Network error'));
}

// ============================================================
// Fixture: wrapper para renderizar a página com params
// ============================================================

async function renderWithParams(id: string) {
  const params = Promise.resolve({ id });
  // Envolvemos em <Suspense> porque a página usa use(params) do React 19,
  // que dispara Suspense mesmo quando a Promise já está resolvida.
  const renderResult = render(
    <Suspense fallback={<div>Loading...</div>}>
      <CheckoutPage params={params} />
    </Suspense>
  );
  // Flush de microtasks: o React 19 precisa de um ciclo extra para resolver
  // a Promise já resolvida dentro de use() e fazer o re-render.
  await Promise.resolve();
  return renderResult;
}

// ============================================================
// Testes
// ============================================================

describe('CheckoutPage — app/checkout/[id]', () => {
  beforeEach(() => {
    mockPush.mockClear();
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  describe('Estado de carregamento (loading)', () => {
    it('deve exibir spinner de carregamento enquanto o evento é buscado', async () => {
      setSelectedTickets(1, { '1': 2 });
      mockFetchSuccess();

      await renderWithParams('1');

      // O loading aparece imediatamente
      expect(screen.getByText('Carregando...')).toBeInTheDocument();

      // Aguarda o loading desaparecer quando a fetch resolver
      await waitFor(() => {
        expect(screen.queryByText('Carregando...')).not.toBeInTheDocument();
      });
    });
  });

  describe('Estado de erro / evento não encontrado', () => {
    it('deve exibir "Evento não encontrado" quando a API falha', async () => {
      setSelectedTickets(1, { '1': 1 });
      mockFetchFailure();

      await renderWithParams('1');

      await waitFor(() => {
        expect(screen.getByText('Evento não encontrado')).toBeInTheDocument();
      });

      expect(screen.getByText(/O evento que você está procurando/)).toBeInTheDocument();
    });

    it('deve exibir "Evento não encontrado" quando a API retorna success:false', async () => {
      setSelectedTickets(1, { '1': 1 });
      vi.mocked(fetch).mockResolvedValue({
        ok: true,
        json: async () => ({ success: false, data: null }),
      } as Response);

      await renderWithParams('1');

      await waitFor(() => {
        expect(screen.getByText('Evento não encontrado')).toBeInTheDocument();
      });
    });
  });

  describe('Redirecionamento por falta de ingressos', () => {
    it('deve redirecionar para /evento/:id quando não há tickets no sessionStorage', async () => {
      mockFetchSuccess();

      await renderWithParams('42');

      // O router.push deve ser chamado de volta para a página do evento
      await waitFor(() => {
        expect(mockPush).toHaveBeenCalledWith('/evento/42');
      });
    });

    it('deve redirecionar quando o eventId armazenado não corresponde ao id da URL', async () => {
      // Tickets existem, mas para outro evento
      setSelectedTickets(99, { '1': 1 });
      mockFetchSuccess();

      await renderWithParams('42');

      await waitFor(() => {
        expect(mockPush).toHaveBeenCalledWith('/evento/42');
      });
    });
  });

  describe('Fluxo feliz: checkout com evento e tickets', () => {
    it('deve renderizar o CheckoutForm com os dados do evento', async () => {
      setSelectedTickets(1, { '1': 2 });
      mockFetchSuccess();

      await renderWithParams('1');

      await waitFor(() => {
        expect(screen.getByTestId('mock-checkout-form')).toBeInTheDocument();
      });

      // Verifica que o Header e Footer foram renderizados
      expect(screen.getByTestId('mock-header')).toBeInTheDocument();
      expect(screen.getByTestId('mock-footer')).toBeInTheDocument();

      // Verifica que o formulário recebeu o título do evento
      expect(screen.getByText('Checkout: Hackathon CCOMP 2026')).toBeInTheDocument();
    });

    it('deve fazer fetch da API com a URL correta', async () => {
      setSelectedTickets(1, { '1': 2 });
      mockFetchSuccess();

      await renderWithParams('1');

      await waitFor(() => {
        expect(fetch).toHaveBeenCalledWith(
          expect.stringContaining('/api/events/1')
        );
      });
    });
  });

  describe('Estrutura da página', () => {
    it('deve conter Header, Footer e conteúdo principal', async () => {
      setSelectedTickets(1, { '1': 1 });
      mockFetchSuccess();

      await renderWithParams('1');

      await waitFor(() => {
        expect(screen.getByTestId('mock-header')).toBeInTheDocument();
        expect(screen.getByTestId('mock-footer')).toBeInTheDocument();
      });
    });
  });
});
