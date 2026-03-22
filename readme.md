# 🎟️ Sistema de Bilheteria Virtual 

Este projeto consiste em uma plataforma robusta para a comercialização e gestão de ingressos online. O foco principal é garantir uma experiência de compra fluida para o cliente e um controle gerencial estratégico para o organizador, mitigando riscos críticos como a concorrência de assentos.

****👥 Histórias de Usuário (Backlog)****
ID	| Papel |	Descrição | Critério de Aceite

01 - Cliente | Eu quero comprar ingressos online escolhendo setor e assento para garantir minha comodidade e segurança.	Mapa interativo de assentos com status de ocupação em tempo real.

02 - Cliente | Eu quero receber meu ingresso digital por e-mail para acessar o evento de forma rápida e sustentável.

03 - Organizador |	Eu quero cadastrar eventos com datas, setores e preços para vender ingressos de forma estruturada,interface administrativa para CRUD de eventos e precificação por lote.

04 - Organizador | Eu quero acompanhar relatórios de vendas em tempo real para monitorar o desempenho e tomar decisões.

📊 Matriz de Seleção de Ciclo de Vida
Abaixo, detalhamos a estratégia de engenharia de software adotada para este ecossistema.

🌐 Contexto do Sistema
O sistema opera em um ambiente de alta transacionalidade. A principal dor de sistemas de bilheteria que identificamos é o pico de acesso durante a abertura de vendas e a necessidade de consistência imediata dos dados (evitar que duas pessoas comprem o mesmo lugar. Para isso, criamos alguns critérios determinantes que irão 

🎯 Critérios Determinantes

    Concorrência: O sistema deve lidar com múltiplos pedidos simultâneos, evitando fila de requisição e lentidões no sistema.

    Segurança: Proteção de dados sensíveis de clientes e autenticidade do ID do ingresso.

    Usabilidade: O fluxo de compra deve ser concluído em poucos cliques para reduzir a taxa de abandono de carrinho.

⚠️ Maiores Riscos Identificados

    Overbooking/Race Condition: Possibilidade de dois usuários reservarem o mesmo assento simultaneamente.

    Falha na Integração de E-mail: O cliente pagar e não receber o ingresso, gerando suporte excessivo.

    Indisponibilidade: O servidor cair durante o lançamento de um evento popular.

    Fraude: Possibilidade de um usuário utilizar o ingresso de outro comprador.

🔄 Modelo de Ciclo utilizado
Modelo Incremental e Iterativo (Scrum/Ágil).

💡 Justificativa Técnica
Diferente do modelo cascata, o ciclo iterativo permitirá que a compra e reserva seja testada e validada exaustivamente antes de avançarmos para as demais funcionalidades da interface gráfica.

    Feedback Antecipado: Como se trata de um projeto de graduação, as entregas incrementais irá nos permitir ajustes de rota conforme as críticas e sugestões do professor e do grupo.

    Mitigação de Riscos: O risco de race condition pode ser atacado na primeira Sprint, garantindo que a base do sistema seja sólida.
    
****Componentes do Projeto****

* Andrey Campos
* Gustavo Ramos
* Nathan Salles
* Cristiano Cordeiro
* Lucas Gabriel

O projeto está sendo feito por alunos do 5°- Período de Ciência da Computação da Unifeso, para agregar à materia de Engenharia de Software do professor Andŕe Campos.
