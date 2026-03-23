export type PaymentStatus = 'Pending' | 'Paid' | 'Refused';

export interface MockPayment {
  id: string;
  beneficiaryName: string;
  beneficiaryCard: string;
  healthPlan: string;
  healthPlanId: string;
  procedure: string;
  executionDate: string; // ISO date string
  value: number;
  status: PaymentStatus;
  appointmentNumber: string;
  authorizationCode?: string;
  executionLocation: string;
  notes?: string;
}

export const MOCK_HEALTH_PLANS = [
  { id: 'hp-1', name: 'Unimed' },
  { id: 'hp-2', name: 'Bradesco Saúde' },
  { id: 'hp-3', name: 'SulAmérica' },
  { id: 'hp-4', name: 'Amil' },
  { id: 'hp-5', name: 'Hapvida' },
] as const;

export const MOCK_PAYMENTS: MockPayment[] = [
  {
    id: 'pay-001',
    beneficiaryName: 'João Carlos Silva',
    beneficiaryCard: '0012345678901',
    healthPlan: 'Unimed',
    healthPlanId: 'hp-1',
    procedure: 'Consulta Cardiológica',
    executionDate: '2026-03-23',
    value: 320.0,
    status: 'Pending',
    appointmentNumber: '2026032301',
    authorizationCode: '987654',
    executionLocation: 'Clínica CardioVida',
    notes: 'Primeira consulta',
  },
  {
    id: 'pay-002',
    beneficiaryName: 'Maria Aparecida Santos',
    beneficiaryCard: '0098765432101',
    healthPlan: 'Bradesco Saúde',
    healthPlanId: 'hp-2',
    procedure: 'Ecocardiograma',
    executionDate: '2026-03-22',
    value: 680.0,
    status: 'Paid',
    appointmentNumber: '2026032201',
    authorizationCode: '654321',
    executionLocation: 'Hospital São Lucas',
  },
  {
    id: 'pay-003',
    beneficiaryName: 'Roberto Ferreira Lima',
    beneficiaryCard: '0011223344551',
    healthPlan: 'Amil',
    healthPlanId: 'hp-4',
    procedure: 'Eletrocardiograma',
    executionDate: '2026-03-22',
    value: 180.0,
    status: 'Refused',
    appointmentNumber: '2026032202',
    executionLocation: 'Clínica CardioVida',
    notes: 'Recusado por falta de autorização prévia',
  },
  {
    id: 'pay-004',
    beneficiaryName: 'Ana Paula Rodrigues',
    beneficiaryCard: '0055667788991',
    healthPlan: 'SulAmérica',
    healthPlanId: 'hp-3',
    procedure: 'Consulta Cardiológica',
    executionDate: '2026-03-21',
    value: 320.0,
    status: 'Paid',
    appointmentNumber: '2026032101',
    authorizationCode: '112233',
    executionLocation: 'Hospital São Lucas',
  },
  {
    id: 'pay-005',
    beneficiaryName: 'Carlos Eduardo Mendes',
    beneficiaryCard: '0077889900111',
    healthPlan: 'Unimed',
    healthPlanId: 'hp-1',
    procedure: 'Holter 24h',
    executionDate: '2026-03-20',
    value: 450.0,
    status: 'Pending',
    appointmentNumber: '2026032001',
    executionLocation: 'Clínica CardioVida',
  },
  {
    id: 'pay-006',
    beneficiaryName: 'Fernanda Costa Oliveira',
    beneficiaryCard: '0033445566771',
    healthPlan: 'Hapvida',
    healthPlanId: 'hp-5',
    procedure: 'Teste Ergométrico',
    executionDate: '2026-03-20',
    value: 560.0,
    status: 'Paid',
    appointmentNumber: '2026032002',
    authorizationCode: '445566',
    executionLocation: 'Hospital São Lucas',
  },
  {
    id: 'pay-007',
    beneficiaryName: 'Paulo Henrique Alves',
    beneficiaryCard: '0022334455661',
    healthPlan: 'Bradesco Saúde',
    healthPlanId: 'hp-2',
    procedure: 'Consulta Cardiológica',
    executionDate: '2026-03-19',
    value: 320.0,
    status: 'Refused',
    appointmentNumber: '2026031901',
    executionLocation: 'Clínica CardioVida',
    notes: 'Código de procedimento divergente',
  },
  {
    id: 'pay-008',
    beneficiaryName: 'Lúcia Maria Pereira',
    beneficiaryCard: '0099887766551',
    healthPlan: 'Unimed',
    healthPlanId: 'hp-1',
    procedure: 'Ecocardiograma',
    executionDate: '2026-03-18',
    value: 680.0,
    status: 'Paid',
    appointmentNumber: '2026031801',
    authorizationCode: '778899',
    executionLocation: 'Hospital São Lucas',
  },
  {
    id: 'pay-009',
    beneficiaryName: 'Bruno Nascimento Souza',
    beneficiaryCard: '0066778899001',
    healthPlan: 'Amil',
    healthPlanId: 'hp-4',
    procedure: 'Holter 24h',
    executionDate: '2026-03-17',
    value: 450.0,
    status: 'Pending',
    appointmentNumber: '2026031701',
    executionLocation: 'Clínica CardioVida',
  },
  {
    id: 'pay-010',
    beneficiaryName: 'Juliana Martins Castro',
    beneficiaryCard: '0044556677881',
    healthPlan: 'SulAmérica',
    healthPlanId: 'hp-3',
    procedure: 'Teste Ergométrico',
    executionDate: '2026-03-15',
    value: 560.0,
    status: 'Paid',
    appointmentNumber: '2026031501',
    authorizationCode: '223344',
    executionLocation: 'Hospital São Lucas',
  },
];
