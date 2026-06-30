// Mock data for BoraAli events platform

export interface Event {
  id: string;
  title: string;
  description: string;
  fullDescription: string;
  date: string;
  time: string;
  location: string;
  address: string;
  city: string;
  category: string;
  image: string;
    organizer: {
    id: number;
    name: string;
    logo: string;
    followers: number;
  };
  tickets: {
    id: string;
    name: string;
    price: number;
    available: number;
    description?: string;
  }[];
  isFeatured?: boolean;
}

export const categories = [
  { id: 'shows', name: 'Shows', icon: '🎤' },
  { id: 'festas', name: 'Festas', icon: '🎉' },
  { id: 'tech', name: 'Tech', icon: '💻' },
  { id: 'gastronomia', name: 'Gastronomia', icon: '🍽️' },
  { id: 'esportes', name: 'Esportes', icon: '⚽' },
  { id: 'teatro', name: 'Teatro', icon: '🎭' },
  { id: 'workshops', name: 'Workshops', icon: '📚' },
  { id: 'networking', name: 'Networking', icon: '🤝' },
];

export const cities = [
  'São Paulo',
  'Rio de Janeiro', 
  'Belo Horizonte',
  'Curitiba',
  'Porto Alegre',
  'Salvador',
  'Recife',
  'Fortaleza',
];

export const events: Event[] = [
  {
    id: '1',
    title: 'Festival de Música Eletrônica 2024',
    description: 'O maior festival de música eletrônica do Brasil com os melhores DJs internacionais.',
    fullDescription: `
# Festival de Música Eletrônica 2024

Prepare-se para uma experiência única! O maior festival de música eletrônica do Brasil está de volta, reunindo os melhores DJs internacionais em uma noite inesquecível.

## Line-up Confirmado
- DJ Internacional 1
- DJ Internacional 2
- DJ Nacional 1
- DJ Nacional 2

## O que esperar
- 12 horas de música non-stop
- 3 palcos diferentes
- Área VIP exclusiva
- Food trucks selecionados
- Open bar premium (área VIP)

## Informações importantes
- Evento para maiores de 18 anos
- Documento com foto obrigatório
- Proibido entrada com bebidas e alimentos
    `,
    date: '15 Mar 2024',
    time: '22:00',
    location: 'Allianz Parque',
    address: 'Av. Francisco Matarazzo, 1705',
    city: 'São Paulo',
    category: 'festas',
    image: 'https://images.unsplash.com/photo-1470225620780-dba8ba36b745?w=800&h=400&fit=crop',
    organizer: {
      name: 'Eletronic Events BR',
      logo: 'https://images.unsplash.com/photo-1493225457124-a3eb161ffa5f?w=100&h=100&fit=crop',
      followers: 45000,
    },
    tickets: [
      { id: 't1', name: '1º Lote', price: 150, available: 500, description: 'Ingresso pista' },
      { id: 't2', name: '2º Lote', price: 200, available: 1000, description: 'Ingresso pista' },
      { id: 't3', name: 'VIP', price: 450, available: 200, description: 'Área VIP com open bar' },
    ],
    isFeatured: true,
  },
  {
    id: '2',
    title: 'Tech Summit Brasil 2024',
    description: 'Conferência de tecnologia com palestras sobre IA, Cloud e desenvolvimento.',
    fullDescription: `
# Tech Summit Brasil 2024

O maior evento de tecnologia do Brasil reúne os maiores especialistas do mercado para discutir as tendências que estão moldando o futuro da tecnologia.

## Trilhas de conteúdo
- Inteligência Artificial e Machine Learning
- Cloud Computing e DevOps
- Desenvolvimento Web e Mobile
- Cybersecurity
- Startups e Inovação

## Palestrantes confirmados
- CEO de uma Big Tech
- CTO de Startup Unicórnio
- Especialistas em IA
- Desenvolvedores influenciadores

## Incluso no ingresso
- Acesso a todas as palestras
- Coffee break
- Almoço
- Material exclusivo
- Certificado de participação
    `,
    date: '20 Abr 2024',
    time: '09:00',
    location: 'Centro de Convenções',
    address: 'Av. Paulista, 1500',
    city: 'São Paulo',
    category: 'tech',
    image: 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800&h=400&fit=crop',
    organizer: {
      name: 'TechBR Events',
      logo: 'https://images.unsplash.com/photo-1535303311164-664fc9ec6532?w=100&h=100&fit=crop',
      followers: 28000,
    },
    tickets: [
      { id: 't1', name: 'Early Bird', price: 299, available: 100, description: 'Acesso completo' },
      { id: 't2', name: 'Regular', price: 499, available: 500, description: 'Acesso completo' },
      { id: 't3', name: 'Premium', price: 899, available: 50, description: 'Acesso completo + workshop exclusivo' },
    ],
    isFeatured: true,
  },
  {
    id: '3',
    title: 'Stand Up Comedy Night',
    description: 'Uma noite de muitas risadas com os melhores comediantes do momento.',
    fullDescription: `
# Stand Up Comedy Night

Uma noite imperdível de humor com os comediantes mais engraçados do Brasil. Prepare-se para rir muito!

## Comediantes confirmados
- Comediante 1
- Comediante 2
- Comediante 3
- Comediante surpresa!

## Sobre o evento
- 3 horas de show
- Intervalo com bar disponível
- Meet & greet após o show (ingresso VIP)
    `,
    date: '10 Mar 2024',
    time: '21:00',
    location: 'Teatro Municipal',
    address: 'Praça Ramos de Azevedo, s/n',
    city: 'São Paulo',
    category: 'teatro',
    image: 'https://images.unsplash.com/photo-1585699324551-f6c309eedeca?w=800&h=400&fit=crop',
    organizer: {
      name: 'Risos Produções',
      logo: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&h=100&fit=crop',
      followers: 15000,
    },
    tickets: [
      { id: 't1', name: 'Plateia', price: 80, available: 300 },
      { id: 't2', name: 'Camarote', price: 150, available: 100 },
      { id: 't3', name: 'VIP + Meet & Greet', price: 250, available: 30 },
    ],
  },
  {
    id: '4',
    title: 'Workshop de Fotografia',
    description: 'Aprenda técnicas profissionais de fotografia com especialistas renomados.',
    fullDescription: `
# Workshop de Fotografia Profissional

Desenvolva suas habilidades fotográficas com nosso workshop intensivo ministrado por fotógrafos premiados.

## Conteúdo programático
- Fundamentos da fotografia
- Composição e enquadramento
- Iluminação natural e artificial
- Edição e pós-produção
- Portfolio profissional

## Material incluso
- Apostila completa
- Certificado
- Coffee break
    `,
    date: '25 Mar 2024',
    time: '14:00',
    location: 'Espaço Cultural',
    address: 'Rua Augusta, 500',
    city: 'São Paulo',
    category: 'workshops',
    image: 'https://images.unsplash.com/photo-1606983340126-99ab4feaa64a?w=800&h=400&fit=crop',
    organizer: {
      name: 'FotoArte Academy',
      logo: 'https://images.unsplash.com/photo-1552168324-d612d77725e3?w=100&h=100&fit=crop',
      followers: 8500,
    },
    tickets: [
      { id: 't1', name: 'Individual', price: 350, available: 20 },
      { id: 't2', name: 'Dupla', price: 600, available: 10, description: '2 ingressos' },
    ],
  },
  {
    id: '5',
    title: 'Feira Gastronômica Internacional',
    description: 'Sabores do mundo inteiro em um único lugar. Venha experimentar!',
    fullDescription: `
# Feira Gastronômica Internacional

Uma jornada gastronômica pelos sabores do mundo! Mais de 50 estandes com comidas típicas de diversos países.

## Países representados
- Itália
- Japão
- México
- Índia
- França
- Peru
- E muito mais!

## Atrações
- Shows de culinária ao vivo
- Degustações
- Competições gastronômicas
- Área kids
    `,
    date: '05 Abr 2024',
    time: '11:00',
    location: 'Parque Ibirapuera',
    address: 'Av. Pedro Álvares Cabral',
    city: 'São Paulo',
    category: 'gastronomia',
    image: 'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=800&h=400&fit=crop',
    organizer: {
      name: 'Sabores do Mundo',
      logo: 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=100&h=100&fit=crop',
      followers: 32000,
    },
    tickets: [
      { id: 't1', name: 'Entrada Básica', price: 25, available: 5000 },
      { id: 't2', name: 'Passaporte Degustação', price: 75, available: 1000, description: 'Inclui 10 degustações' },
    ],
    isFeatured: true,
  },
  {
    id: '6',
    title: 'Corrida Noturna 10K',
    description: 'Corra sob as estrelas na corrida noturna mais animada da cidade.',
    fullDescription: `
# Corrida Noturna 10K

Uma experiência única de corrida sob as estrelas! Percorra 10km pelas ruas da cidade em um trajeto iluminado e seguro.

## Kit do corredor
- Camiseta oficial
- Número de peito com chip
- Medalha de conclusão
- Sacola ecológica

## Estrutura
- Guarda-volumes
- Área de aquecimento
- Posto médico
- Hidratação no percurso
    `,
    date: '12 Abr 2024',
    time: '20:00',
    location: 'Marginal Pinheiros',
    address: 'Ponte Estaiada',
    city: 'São Paulo',
    category: 'esportes',
    image: 'https://images.unsplash.com/photo-1552674605-db6ffd4facb5?w=800&h=400&fit=crop',
    organizer: {
      name: 'Run SP',
      logo: 'https://images.unsplash.com/photo-1571008887538-b36bb32f4571?w=100&h=100&fit=crop',
      followers: 18000,
    },
    tickets: [
      { id: 't1', name: 'Inscrição Individual', price: 120, available: 3000 },
      { id: 't2', name: 'Kit Premium', price: 180, available: 500, description: 'Kit + camiseta extra + mochila' },
    ],
  },
  {
    id: '7',
    title: 'Show Sertanejo Universitário',
    description: 'Os maiores nomes do sertanejo universitário em uma noite épica.',
    fullDescription: `
# Show Sertanejo Universitário

Uma noite de muito sertanejo com as duplas mais queridas do Brasil!

## Atrações
- Dupla Sertaneja 1
- Dupla Sertaneja 2
- DJ convidado
- Participações especiais

## Estrutura
- 2 palcos
- Área de alimentação
- Estacionamento
    `,
    date: '22 Mar 2024',
    time: '19:00',
    location: 'Espaço das Américas',
    address: 'Rua Tagipuru, 795',
    city: 'São Paulo',
    category: 'shows',
    image: 'https://images.unsplash.com/photo-1493225457124-a3eb161ffa5f?w=800&h=400&fit=crop',
    organizer: {
      name: 'Country Music BR',
      logo: 'https://images.unsplash.com/photo-1511367461989-f85a21fda167?w=100&h=100&fit=crop',
      followers: 55000,
    },
    tickets: [
      { id: 't1', name: 'Pista', price: 90, available: 2000 },
      { id: 't2', name: 'Frontstage', price: 180, available: 500 },
      { id: 't3', name: 'Camarote', price: 350, available: 200 },
    ],
  },
  {
    id: '8',
    title: 'Networking Startups & Investidores',
    description: 'Conecte-se com investidores e empreendedores do ecossistema de startups.',
    fullDescription: `
# Networking Startups & Investidores

O evento perfeito para conectar startups com potenciais investidores e parceiros estratégicos.

## Formato
- Palestras inspiradoras
- Rodadas de pitch
- Happy hour networking
- Speed dating com investidores

## Para quem é
- Fundadores de startups
- Investidores anjo
- VCs
- Mentores
    `,
    date: '18 Abr 2024',
    time: '18:00',
    location: 'Google Campus',
    address: 'Rua Coronel Oscar Porto, 70',
    city: 'São Paulo',
    category: 'networking',
    image: 'https://images.unsplash.com/photo-1511578314322-379afb476865?w=800&h=400&fit=crop',
    organizer: {
      name: 'Startup Hub BR',
      logo: 'https://images.unsplash.com/photo-1560179707-f14e90ef3623?w=100&h=100&fit=crop',
      followers: 12000,
    },
    tickets: [
      { id: 't1', name: 'Startup', price: 50, available: 100 },
      { id: 't2', name: 'Investidor', price: 0, available: 30, description: 'Gratuito para investidores' },
      { id: 't3', name: 'Premium', price: 150, available: 20, description: 'Inclui 1 rodada de pitch garantida' },
    ],
  },
];

export function getEventById(id: string): Event | undefined {
  return events.find(event => event.id === id);
}

export function getEventsByCategory(category: string): Event[] {
  return events.filter(event => event.category === category);
}

export function getEventsByCity(city: string): Event[] {
  return events.filter(event => event.city === city);
}

export function getFeaturedEvents(): Event[] {
  return events.filter(event => event.isFeatured);
}

export function searchEvents(query: string): Event[] {
  const lowerQuery = query.toLowerCase();
  return events.filter(event => 
    event.title.toLowerCase().includes(lowerQuery) ||
    event.description.toLowerCase().includes(lowerQuery) ||
    event.category.toLowerCase().includes(lowerQuery) ||
    event.city.toLowerCase().includes(lowerQuery)
  );
}
