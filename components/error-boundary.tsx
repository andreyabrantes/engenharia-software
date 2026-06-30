'use client';

import { Component, ReactNode } from 'react';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: any) {
    console.error('ErrorBoundary capturou erro:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback || (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <p className="mb-2 text-lg font-medium text-foreground">
            Algo deu errado ao carregar esta seção
          </p>
          <button
            onClick={() => this.setState({ hasError: false })}
            className="text-sm text-muted-foreground hover:text-primary"
          >
            Tentar novamente
          </button>
        </div>
      );
    }

    return this.props.children;
  }
}
