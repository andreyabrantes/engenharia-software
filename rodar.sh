#!/bin/bash

export PATH="$HOME/.dotnet:$PATH"

echo "🎫 Iniciando Bilheteria Virtual - Blazor WebAssembly"
echo "=================================================="
echo ""

cd "$(dirname "$0")/BilheteriaVirtualBlazor"

echo "🔨 Compilando o projeto..."
dotnet build

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Compilação concluída com sucesso!"
    echo ""
    echo "🚀 Iniciando o servidor..."
    echo ""
    echo "📱 Acesse no navegador:"
    echo "   http://localhost:5000"
    echo "   ou"
    echo "   https://localhost:5001"
    echo ""
    echo "⚠️  Pressione Ctrl+C para parar o servidor"
    echo ""
    
    dotnet run
else
    echo ""
    echo "❌ Erro na compilação!"
    exit 1
fi
