import React, { useEffect, useMemo, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, View } from 'react-native';
import { Text } from 'react-native-paper';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTheme } from '@medcontrol/design-system/native';
import { usePayment } from '../../../src/hooks/usePayment';
import {
  HealthPlanService,
  type HealthPlanDto,
} from '../../../src/services/health-plan.service';
import type { PaymentItemDto, PaymentStatus } from '../../../src/services/payment.service';

// ── Helpers ───────────────────────────────────────────────────────────────────

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}

function formatDatePtBr(iso: string): string {
  const [year, month, day] = iso.split('-').map(Number);
  const d = new Date(year, (month ?? 1) - 1, day);
  return d.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' });
}

const STATUS_LABELS: Record<PaymentStatus, string> = {
  Pending: 'Pendente',
  Paid: 'Pago',
  Refused: 'Recusado',
  PartiallyPending: 'Parcial',
  PartiallyRefused: 'Glosa parcial',
};

type BaseStatus = 'pending' | 'paid' | 'refused';

function resolveBaseStatus(status: PaymentStatus): BaseStatus {
  if (status === 'Paid') return 'paid';
  if (status === 'Refused' || status === 'PartiallyRefused') return 'refused';
  return 'pending';
}

// ── Sub-componentes ───────────────────────────────────────────────────────────

function StatusBadge({ status }: { status: PaymentStatus }) {
  const t = useTheme();
  const base = resolveBaseStatus(status);
  const colors = t.colors.paymentStatus[base];
  return (
    <View
      testID="status-badge"
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        alignSelf: 'flex-start',
        backgroundColor: colors.bg,
        borderWidth: 1,
        borderColor: colors.border,
        borderRadius: t.borderRadius.full,
        paddingHorizontal: t.spacing[2],
        paddingVertical: t.spacing[0.5],
        gap: t.spacing[1],
      }}
    >
      <View
        style={{
          width: 6,
          height: 6,
          borderRadius: t.borderRadius.full,
          backgroundColor: colors.dot,
        }}
      />
      <Text
        style={{
          fontSize: t.typography.fontSize.xs,
          fontWeight: t.typography.fontWeight.semibold,
          color: colors.text,
          lineHeight: 16,
        }}
      >
        {STATUS_LABELS[status]}
      </Text>
    </View>
  );
}

function SectionCard({ title, children }: { title: string; children: React.ReactNode }) {
  const t = useTheme();
  return (
    <View
      style={[
        {
          backgroundColor: t.colors.surface.card,
          borderRadius: t.borderRadius.xl,
          padding: t.spacing[4],
          borderWidth: 1,
          borderColor: t.colors.border,
        },
        t.shadows.sm,
      ]}
    >
      <Text
        style={{
          fontSize: t.typography.fontSize.xs,
          fontWeight: t.typography.fontWeight.semibold,
          color: t.colors.text.secondary,
          textTransform: 'uppercase',
          letterSpacing: 0.6,
          marginBottom: t.spacing[3],
        }}
      >
        {title}
      </Text>
      {children}
    </View>
  );
}

function DetailRow({
  label,
  value,
  testID,
}: {
  label: string;
  value: string;
  testID?: string;
}) {
  const t = useTheme();
  return (
    <View
      style={{
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'flex-start',
        paddingVertical: t.spacing[2],
        gap: t.spacing[4],
        borderBottomWidth: 1,
        borderBottomColor: t.colors.divider,
      }}
    >
      <Text
        style={{
          fontSize: t.typography.fontSize.sm,
          color: t.colors.text.secondary,
          flex: 1,
        }}
      >
        {label}
      </Text>
      <Text
        testID={testID}
        style={{
          fontSize: t.typography.fontSize.sm,
          color: t.colors.text.primary,
          fontWeight: t.typography.fontWeight.medium,
          flex: 1,
          textAlign: 'right',
        }}
      >
        {value}
      </Text>
    </View>
  );
}

function PaymentItemRow({ item }: { item: PaymentItemDto }) {
  const t = useTheme();
  const base = resolveBaseStatus(item.status as PaymentStatus);
  const colors = t.colors.paymentStatus[base];
  return (
    <View
      testID="payment-item"
      style={{
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        paddingVertical: t.spacing[3],
        gap: t.spacing[3],
        borderBottomWidth: 1,
        borderBottomColor: t.colors.divider,
      }}
    >
      <View style={{ flex: 1 }}>
        <Text
          style={{
            fontSize: t.typography.fontSize.sm,
            color: t.colors.text.primary,
            fontWeight: t.typography.fontWeight.medium,
          }}
        >
          {formatCurrency(item.value)}
        </Text>
        {item.notes ? (
          <Text
            style={{
              fontSize: t.typography.fontSize.xs,
              color: t.colors.text.tertiary,
              marginTop: t.spacing[0.5],
            }}
          >
            {item.notes}
          </Text>
        ) : null}
      </View>
      <View
        style={{
          flexDirection: 'row',
          alignItems: 'center',
          gap: t.spacing[1],
          paddingHorizontal: t.spacing[2],
          paddingVertical: t.spacing[0.5],
          borderRadius: t.borderRadius.full,
          backgroundColor: colors.bg,
          borderWidth: 1,
          borderColor: colors.border,
        }}
      >
        <View
          style={{
            width: 5,
            height: 5,
            borderRadius: t.borderRadius.full,
            backgroundColor: colors.dot,
          }}
        />
        <Text
          style={{
            fontSize: t.typography.fontSize.xs,
            color: colors.text,
            fontWeight: t.typography.fontWeight.semibold,
          }}
        >
          {STATUS_LABELS[item.status as PaymentStatus]}
        </Text>
      </View>
    </View>
  );
}

function LoadingScreen() {
  const t = useTheme();
  return (
    <View
      testID="loading-indicator"
      style={{
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: t.colors.surface.background,
      }}
    >
      <ActivityIndicator size="large" color={t.colors.primary} />
    </View>
  );
}

function ErrorScreen({ error, onRetry }: { error: string; onRetry: () => void }) {
  const t = useTheme();
  return (
    <View
      style={{
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        padding: t.spacing[6],
        backgroundColor: t.colors.surface.background,
      }}
    >
      <View
        style={{
          width: 56,
          height: 56,
          borderRadius: t.borderRadius.full,
          backgroundColor: t.colors.error.light,
          alignItems: 'center',
          justifyContent: 'center',
          marginBottom: t.spacing[4],
        }}
      >
        <Ionicons name="alert-circle-outline" size={t.components.iconXl} color={t.colors.error.base} />
      </View>
      <Text
        testID="error-message"
        style={{
          fontSize: t.typography.fontSize.md,
          color: t.colors.text.primary,
          fontWeight: t.typography.fontWeight.semibold,
          textAlign: 'center',
          marginBottom: t.spacing[2],
        }}
      >
        Não foi possível carregar
      </Text>
      <Text
        style={{
          fontSize: t.typography.fontSize.sm,
          color: t.colors.text.secondary,
          textAlign: 'center',
          marginBottom: t.spacing[6],
        }}
      >
        {error}
      </Text>
      <Pressable
        testID="retry-button"
        onPress={onRetry}
        style={{
          height: t.components.buttonHeight,
          paddingHorizontal: t.spacing[6],
          backgroundColor: t.colors.primary,
          borderRadius: t.borderRadius.md,
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Text
          style={{
            color: t.colors.primaryText,
            fontSize: t.typography.fontSize.sm,
            fontWeight: t.typography.fontWeight.semibold,
          }}
        >
          Tentar novamente
        </Text>
      </Pressable>
    </View>
  );
}

// ── Tela principal ────────────────────────────────────────────────────────────

export default function PaymentDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const t = useTheme();
  const { payment, loading, error, refetch } = usePayment(id);
  const [healthPlans, setHealthPlans] = useState<HealthPlanDto[]>([]);

  useEffect(() => {
    HealthPlanService.listHealthPlans()
      .then(setHealthPlans)
      .catch(() => {});
  }, []);

  const healthPlanName = useMemo(
    () => healthPlans.find((hp) => hp.id === payment?.healthPlanId)?.name ?? '—',
    [healthPlans, payment?.healthPlanId],
  );

  if (loading) return <LoadingScreen />;
  if (error || !payment) return <ErrorScreen error={error ?? 'Pagamento não encontrado'} onRetry={refetch} />;

  return (
    <View style={{ flex: 1, backgroundColor: t.colors.surface.background }}>

      {/* ── Hero header (mesmo padrão do HomeScreen) ─────────────────────── */}
      <View
        style={{
          backgroundColor: t.colors.secondary,
          paddingTop: insets.top + t.spacing[4],
          paddingBottom: t.spacing[6],
          paddingHorizontal: t.spacing[4],
        }}
      >
        {/* Linha: botão voltar + título */}
        <View
          style={{
            flexDirection: 'row',
            alignItems: 'center',
            gap: t.spacing[3],
            marginBottom: t.spacing[5],
          }}
        >
          <Pressable
            testID="back-button"
            onPress={() => router.back()}
            style={{
              width: 40,
              height: 40,
              borderRadius: t.borderRadius.full,
              backgroundColor: 'rgba(255,255,255,0.10)',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <Ionicons name="arrow-back" size={t.components.iconLg} color={t.colors.text.onDark} />
          </Pressable>
          <Text
            style={{
              flex: 1,
              fontSize: t.typography.fontSize.lg,
              fontWeight: t.typography.fontWeight.semibold,
              color: t.colors.text.onDark,
            }}
          >
            Detalhes do Pagamento
          </Text>
        </View>

        {/* Nome do beneficiário */}
        <Text
          testID="beneficiary-name"
          numberOfLines={2}
          style={{
            fontSize: t.typography.fontSize.xl,
            fontWeight: t.typography.fontWeight.bold,
            color: t.colors.text.onDark,
            lineHeight: 28,
            marginBottom: t.spacing[3],
          }}
        >
          {payment.beneficiaryName}
        </Text>

        {/* Badge de status */}
        <StatusBadge status={payment.status} />
      </View>

      {/* ── Conteúdo rolável ──────────────────────────────────────────────── */}
      <ScrollView
        contentContainerStyle={{
          padding: t.spacing[4],
          gap: t.spacing[3],
          paddingBottom: insets.bottom + t.spacing[6],
        }}
        showsVerticalScrollIndicator={false}
      >
        {/* Informações do atendimento */}
        <SectionCard title="Informações do Atendimento">
          <DetailRow label="Convênio" value={healthPlanName} testID="health-plan-name" />
          <DetailRow
            label="Data de execução"
            value={formatDatePtBr(payment.executionDate)}
            testID="execution-date"
          />
          <DetailRow
            label="Nº Atendimento"
            value={payment.appointmentNumber}
            testID="appointment-number"
          />
          {payment.authorizationCode ? (
            <DetailRow
              label="Código de autorização"
              value={payment.authorizationCode}
              testID="authorization-code"
            />
          ) : null}
        </SectionCard>

        {/* Carteirinha */}
        <SectionCard title="Beneficiário">
          <DetailRow label="Carteirinha" value={payment.beneficiaryCard} />
        </SectionCard>

        {/* Locais */}
        <SectionCard title="Locais">
          <DetailRow
            label="Local de execução"
            value={payment.executionLocation}
            testID="execution-location"
          />
          <DetailRow
            label="Local de pagamento"
            value={payment.paymentLocation}
            testID="payment-location"
          />
        </SectionCard>

        {/* Procedimentos */}
        <SectionCard title="Procedimentos">
          <View
            style={{
              flexDirection: 'row',
              justifyContent: 'space-between',
              alignItems: 'center',
              paddingBottom: t.spacing[3],
              borderBottomWidth: 1,
              borderBottomColor: t.colors.border,
              marginBottom: t.spacing[1],
            }}
          >
            <Text
              style={{
                fontSize: t.typography.fontSize.sm,
                color: t.colors.text.secondary,
              }}
            >
              Total
            </Text>
            <Text
              testID="total-value"
              style={{
                fontSize: t.typography.fontSize.md,
                fontWeight: t.typography.fontWeight.bold,
                color: t.colors.text.primary,
              }}
            >
              {formatCurrency(payment.totalValue)}
            </Text>
          </View>
          {payment.items.map((item) => (
            <PaymentItemRow key={item.id} item={item} />
          ))}
        </SectionCard>

        {/* Observações (condicional) */}
        {payment.notes ? (
          <SectionCard title="Observações">
            <Text
              testID="payment-notes"
              style={{
                fontSize: t.typography.fontSize.sm,
                color: t.colors.text.primary,
                lineHeight: 22,
              }}
            >
              {payment.notes}
            </Text>
          </SectionCard>
        ) : null}
      </ScrollView>
    </View>
  );
}
