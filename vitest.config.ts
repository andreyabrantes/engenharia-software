import { defineConfig } from 'vitest/config';
import path from 'path';

export default defineConfig({
  test: {
    // Ambiente jsdom para simular o navegador
    environment: 'jsdom',

    // Arquivo de setup global (carregado antes de cada arquivo de teste)
    setupFiles: ['./__tests__/setup.ts'],

    // Padrão de glob para encontrar arquivos de teste
    include: ['**/*.{test,spec}.{ts,tsx}'],

    // Excluir node_modules e .next
    exclude: ['node_modules', '.next'],

    // Limpar mocks entre testes automaticamente
    mockReset: true,
    restoreMocks: true,

    // Configurações de globals para compatibilidade com jest-style
    globals: true,
  },

  resolve: {
    alias: {
      // Espelha o path alias @/* do tsconfig.json
      '@': path.resolve(__dirname, '.'),
    },
  },
});
