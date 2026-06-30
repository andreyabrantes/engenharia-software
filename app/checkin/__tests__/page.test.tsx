import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

// ============================================================
// Mocks de módulos
// ============================================================

// Mock next/navigation
vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
    replace: vi.fn(),
    prefetch: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
  }),
}));

// Mock de componentes de layout
vi.mock('@/components/header', () => ({
  Header: () => <header data-testid="mock-header">Header</header>,
}));

vi.mock('@/components/footer', () => ({
  Footer: () => <footer data-testid="mock-footer">Footer</footer>,
}));

// Mock de next/link para evitar dependência de IntersectionObserver interno do Next.js
vi.mock('next/link', () => ({
  default: ({ href, children, ...props }: { href: string; children: React.ReactNode; [key: string]: unknown }) => (
    <a href={href} {...props}>{children}</a>
  ),
}));

// Mock de ícones do lucide-react usados na página
vi.mock('lucide-react', async () => {
  const actual = await vi.importActual('lucide-react');
  return {
    ...actual,
    Loader2: ({ className }: { className?: string }) => (
      <span data-testid="icon-loader" className={className}>Loader</span>
    ),
    Camera: ({ className }: { className?: string }) => (
      <span data-testid="icon-camera" className={className}>Camera</span>
    ),
  };
});

// Importamos a página APÓS os mocks
import CheckInPage from '../page';

// ============================================================
// Helpers
// ============================================================

/** Mock de resposta de check-in bem-sucedido */
function mockCheckInSuccess(eventTitle = 'Hackathon CCOMP 2026') {
  vi.mocked(fetch).mockResolvedValue({
    ok: true,
    json: async () => ({
      success: true,
      message: 'Entrada liberada com sucesso!',
      data: {
        orderCode: 'BA-20260615-A1B2C3D4',
        eventTitle,
        eventLocation: 'Campus Unifeso',
      },
    }),
  } as Response);
}

/** Mock de resposta de check-in falho (ingresso inválido) */
function mockCheckInInvalid(message = 'Pedido não encontrado ou já utilizado.') {
  vi.mocked(fetch).mockResolvedValue({
    ok: true,
    json: async () => ({
      success: false,
      message,
    }),
  } as Response);
}

/** Mock de erro de rede */
function mockNetworkError() {
  vi.mocked(fetch).mockRejectedValue(new Error('Network error'));
}

// ============================================================
// Testes
// ============================================================

describe('CheckInPage — app/checkin', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Renderização inicial', () => {
    it('deve renderizar o layout com Header, Footer e título da página', () => {
      render(<CheckInPage />);

      expect(screen.getByTestId('mock-header')).toBeInTheDocument();
      expect(screen.getByTestId('mock-footer')).toBeInTheDocument();
      expect(screen.getByText('Check-in de Ingressos')).toBeInTheDocument();
    });

    it('deve renderizar o campo de código do pedido e o botão Validar', () => {
      render(<CheckInPage />);

      expect(screen.getByLabelText('Código do Pedido')).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /validar/i })).toBeInTheDocument();
    });

    it('deve renderizar as instruções de uso', () => {
      render(<CheckInPage />);

      expect(screen.getByText('Como usar:')).toBeInTheDocument();
      expect(screen.getByText(/Peça ao participante/)).toBeInTheDocument();
      expect(screen.getByText(/Aponte a câmera/)).toBeInTheDocument();
      expect(screen.getByText(/O sistema validará/)).toBeInTheDocument();
      expect(screen.getByText(/O ingresso será marcado/)).toBeInTheDocument();
    });

    it('deve renderizar o link de voltar para home', () => {
      render(<CheckInPage />);

      const backLink = screen.getByText('Voltar');
      expect(backLink).toBeInTheDocument();
      expect(backLink.closest('a')).toHaveAttribute('href', '/');
    });
  });

  describe('Validação do input', () => {
    it('deve desabilitar o botão Validar quando o input está vazio', () => {
      render(<CheckInPage />);

      const button = screen.getByRole('button', { name: /validar/i });
      expect(button).toBeDisabled();
    });

    it('deve habilitar o botão Validar quando o input tem texto', async () => {
      const user = userEvent.setup();
      render(<CheckInPage />);

      const input = screen.getByLabelText('Código do Pedido');
      await user.type(input, 'BA-20260615-A1B2C3D4');

      const button = screen.getByRole('button', { name: /validar/i });
      expect(button).toBeEnabled();
    });

    it('deve converter o input para maiúsculas automaticamente', async () => {
      const user = userEvent.setup();
      render(<CheckInPage />);

      const input = screen.getByLabelText('Código do Pedido');
      await user.type(input, 'ba-20260615-a1b2c3d4');

      expect(input).toHaveValue('BA-20260615-A1B2C3D4');
    });
  });

  describe('Submissão do check-in — fluxo de sucesso', () => {
    it('deve enviar requisição POST para /api/orders/public-checkin', async () => {
      const user = userEvent.setup();
      mockCheckInSuccess();
      render(<CheckInPage />);

      await user.type(screen.getByLabelText('Código do Pedido'), 'BA-20260615-A1B2C3D4');
      await user.click(screen.getByRole('button', { name: /validar/i }));

      expect(fetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/orders/public-checkin'),
        expect.objectContaining({
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ orderCode: 'BA-20260615-A1B2C3D4' }),
        })
      );
    });

    it('deve exibir mensagem de "Entrada Liberada!" após check-in bem-sucedido', async () => {
      const user = userEvent.setup();
      mockCheckInSuccess();
      render(<CheckInPage />);

      await user.type(screen.getByLabelText('Código do Pedido'), 'BA-20260615-A1B2C3D4');
      await user.click(screen.getByRole('button', { name: /validar/i }));

      await waitFor(() => {
        expect(screen.getByText('✅ Entrada Liberada!')).toBeInTheDocument();
      });
    });

    it('deve exibir detalhes do pedido após check-in bem-sucedido', async () => {
      const user = userEvent.setup();
      mockCheckInSuccess();
      render(<CheckInPage />);

      await user.type(screen.getByLabelText('Código do Pedido'), 'BA-20260615-A1B2C3D4');
      await user.click(screen.getByRole('button', { name: /validar/i }));

      await waitFor(() => {
        expect(screen.getByText(/#BA-20260615-A1B2C3D4/)).toBeInTheDocument();
        expect(screen.getByText(/Hackathon CCOMP 2026/)).toBeInTheDocument();
        expect(screen.getByText(/Campus Unifeso/)).toBeInTheDocument();
      });
    });

    it('deve limpar o campo de input após check-in bem-sucedido', async () => {
      const user = userEvent.setup();
      mockCheckInSuccess();
      render(<CheckInPage />);

      await user.type(screen.getByLabelText('Código do Pedido'), 'BA-20260615-A1B2C3D4');
      await user.click(screen.getByRole('button', { name: /validar/i }));

      await waitFor(() => {
        expect(screen.getByLabelText('Código do Pedido')).toHaveValue('');
      });
    });
  });

  describe('Submissão do check-in — fluxo de erro', () => {
    it('deve exibir "Acesso Negado" quando o pedido é inválido', async () => {
      const user = userEvent.setup();
      mockCheckInInvalid();
      render(<CheckInPage />);

      await user.type(screen.getByLabelText('Código do Pedido'), 'BA-INVALIDO');
      await user.click(screen.getByRole('button', { name: /validar/i }));

      await waitFor(() => {
        expect(screen.getByText('❌ Acesso Negado')).toBeInTheDocument();
      });
    });

    it('deve exibir mensagem de erro específica do servidor', async () => {
      const user = userEvent.setup();
      mockCheckInInvalid('Este ingresso já foi utilizado.');
      render(<CheckInPage />);

      await user.type(screen.getByLabelText('Código do Pedido'), 'BA-JA-USADO');
      await user.click(screen.getByRole('button', { name: /validar/i }));

      await waitFor(() => {
        expect(screen.getByText('Este ingresso já foi utilizado.')).toBeInTheDocument();
      });
    });

    it('deve exibir erro de conexão quando o servidor está offline', async () => {
      const user = userEvent.setup();
      mockNetworkError();
      render(<CheckInPage />);

      await user.type(screen.getByLabelText('Código do Pedido'), 'BA-QUALQUER');
      await user.click(screen.getByRole('button', { name: /validar/i }));

      await waitFor(() => {
        expect(screen.getByText('Erro ao conectar com o servidor')).toBeInTheDocument();
      });
    });
  });

  describe('Submissão via tecla Enter', () => {
    it('deve disparar o check-in ao pressionar Enter no input', async () => {
      const user = userEvent.setup();
      mockCheckInSuccess();
      render(<CheckInPage />);

      const input = screen.getByLabelText('Código do Pedido');
      await user.type(input, 'BA-20260615-A1B2C3D4');
      await user.keyboard('{Enter}');

      await waitFor(() => {
        expect(screen.getByText('✅ Entrada Liberada!')).toBeInTheDocument();
      });
    });
  });
});
