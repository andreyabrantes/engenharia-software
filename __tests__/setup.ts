import '@testing-library/jest-dom/vitest';
import { vi } from 'vitest';

// ============================================================
// Setup global para testes do BoraAli Frontend
// ============================================================

// --- Mock de sessionStorage ---
const sessionStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: vi.fn((key: string) => store[key] ?? null),
    setItem: vi.fn((key: string, value: string) => {
      store[key] = value;
    }),
    removeItem: vi.fn((key: string) => {
      delete store[key];
    }),
    clear: vi.fn(() => {
      store = {};
    }),
    get length() {
      return Object.keys(store).length;
    },
    key: vi.fn((index: number) => Object.keys(store)[index] ?? null),
  };
})();

Object.defineProperty(window, 'sessionStorage', { value: sessionStorageMock });

// --- Mock de localStorage ---
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: vi.fn((key: string) => store[key] ?? null),
    setItem: vi.fn((key: string, value: string) => {
      store[key] = value;
    }),
    removeItem: vi.fn((key: string) => {
      delete store[key];
    }),
    clear: vi.fn(() => {
      store = {};
    }),
    get length() {
      return Object.keys(store).length;
    },
    key: vi.fn((index: number) => Object.keys(store)[index] ?? null),
  };
})();

Object.defineProperty(window, 'localStorage', { value: localStorageMock });

// --- Mock de fetch global ---
// Cada teste pode sobrescrever com vi.fn() conforme necessário
global.fetch = vi.fn();

// --- Mock de matchMedia (usado por componentes Radix UI) ---
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// --- Mock de IntersectionObserver ---
Object.defineProperty(window, 'IntersectionObserver', {
  writable: true,
  value: vi.fn().mockImplementation(() => ({
    observe: vi.fn(),
    unobserve: vi.fn(),
    disconnect: vi.fn(),
    root: null,
    rootMargin: '',
    thresholds: [],
    takeRecords: vi.fn(),
  })),
});

// --- Mock de scrollTo ---
window.scrollTo = vi.fn() as unknown as typeof window.scrollTo;

// --- Mock de requestAnimationFrame ---
window.requestAnimationFrame = vi.fn((cb: FrameRequestCallback) => {
  setTimeout(cb, 0);
  return 0;
});

// ============================================================
// Limpeza automática entre testes
// ============================================================
beforeEach(() => {
  // Limpa storage mocks
  sessionStorage.clear();
  localStorage.clear();
  // Reseta fetch mock
  vi.resetAllMocks();
});
