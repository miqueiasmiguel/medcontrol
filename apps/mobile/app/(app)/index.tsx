import React, { useMemo, useState } from 'react';
import {
  FlatList,
  Platform,
  Pressable,
  StyleSheet,
  TextInput,
  View,
} from 'react-native';
import { Text } from 'react-native-paper';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTheme } from '@medcontrol/design-system/native';
import {
  MOCK_HEALTH_PLANS,
  MOCK_PAYMENTS,
  type MockPayment,
  type PaymentStatus,
} from '../../src/mocks/payments';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------
type StatusFilter = 'All' | PaymentStatus;
type PeriodFilter = 'All' | 'Today' | 'Week' | 'Month';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
function formatCurrency(value: number): string {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function formatDate(iso: string): string {
  const [year, month, day] = iso.split('-');
  return `${day}/${month}/${year}`;
}

function noop() { /* placeholder */ }

function getTodayIso(): string {
  return new Date().toISOString().split('T')[0] ?? '';
}

function isWithinDays(iso: string, days: number): boolean {
  const date = new Date(iso);
  const cutoff = new Date();
  cutoff.setDate(cutoff.getDate() - days);
  return date >= cutoff;
}

const STATUS_LABELS: Record<PaymentStatus, string> = {
  Pending: 'Pendente',
  Paid: 'Pago',
  Refused: 'Recusado',
};

const PERIOD_LABELS: Record<PeriodFilter, string> = {
  All: 'Todo período',
  Today: 'Hoje',
  Week: 'Últimos 7 dias',
  Month: 'Últimos 30 dias',
};

// ---------------------------------------------------------------------------
// Sub-components
// ---------------------------------------------------------------------------

function StatusBadge({ status }: { status: PaymentStatus }) {
  const t = useTheme();
  const s = t.colors.paymentStatus[
    status === 'Pending' ? 'pending' : status === 'Paid' ? 'paid' : 'refused'
  ];
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: s.bg,
        borderWidth: 1,
        borderColor: s.border,
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
          backgroundColor: s.dot,
        }}
      />
      <Text
        style={{
          fontSize: t.typography.fontSize.xs,
          fontWeight: t.typography.fontWeight.semibold,
          color: s.text,
          lineHeight: 16,
        }}
      >
        {STATUS_LABELS[status]}
      </Text>
    </View>
  );
}

function SummaryCard({
  label,
  value,
  color,
  icon,
}: {
  label: string;
  value: string;
  color: string;
  icon: keyof typeof Ionicons.glyphMap;
}) {
  const t = useTheme();
  return (
    <View
      style={[
        {
          flex: 1,
          backgroundColor: t.colors.surface.card,
          borderRadius: t.borderRadius.xl,
          padding: t.spacing[4],
          gap: t.spacing[2],
          borderWidth: 1,
          borderColor: t.colors.border,
        },
        t.shadows.sm,
      ]}
    >
      <View
        style={{
          width: 32,
          height: 32,
          borderRadius: t.borderRadius.lg,
          backgroundColor: `${color}1A`,
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Ionicons name={icon} size={16} color={color} />
      </View>
      <Text
        style={{
          fontSize: t.typography.fontSize.xs,
          color: t.colors.text.secondary,
          fontWeight: t.typography.fontWeight.medium,
        }}
        numberOfLines={1}
      >
        {label}
      </Text>
      <Text
        style={{
          fontSize: t.typography.fontSize.md,
          fontWeight: t.typography.fontWeight.bold,
          color: t.colors.text.primary,
          lineHeight: 22,
        }}
        numberOfLines={1}
        adjustsFontSizeToFit
      >
        {value}
      </Text>
    </View>
  );
}

function PaymentCard({
  payment,
  onPress,
}: {
  payment: MockPayment;
  onPress: () => void;
}) {
  const t = useTheme();
  return (
    <Pressable
      onPress={onPress}
      style={({ pressed }) => [
        {
          backgroundColor: pressed
            ? t.colors.surface.cardPressed
            : t.colors.surface.card,
          borderRadius: t.borderRadius.xl,
          padding: t.spacing[4],
          marginHorizontal: t.spacing[4],
          marginBottom: t.spacing[3],
          borderWidth: 1,
          borderColor: t.colors.border,
        },
        t.shadows.sm,
      ]}
      android_ripple={{ color: t.colors.surface.cardPressed }}
    >
      {/* Top row: name + status */}
      <View
        style={{
          flexDirection: 'row',
          alignItems: 'flex-start',
          justifyContent: 'space-between',
          marginBottom: t.spacing[2],
          gap: t.spacing[2],
        }}
      >
        <Text
          style={{
            flex: 1,
            fontSize: t.typography.fontSize.md,
            fontWeight: t.typography.fontWeight.semibold,
            color: t.colors.text.primary,
            lineHeight: 22,
          }}
          numberOfLines={1}
        >
          {payment.beneficiaryName}
        </Text>
        <StatusBadge status={payment.status} />
      </View>

      {/* Procedure */}
      <Text
        style={{
          fontSize: t.typography.fontSize.sm,
          color: t.colors.text.secondary,
          marginBottom: t.spacing[3],
        }}
        numberOfLines={1}
      >
        {payment.procedure}
      </Text>

      {/* Divider */}
      <View
        style={{
          height: 1,
          backgroundColor: t.colors.divider,
          marginBottom: t.spacing[3],
        }}
      />

      {/* Bottom row: health plan + date + value */}
      <View
        style={{
          flexDirection: 'row',
          alignItems: 'center',
          justifyContent: 'space-between',
        }}
      >
        <View
          style={{
            flexDirection: 'row',
            alignItems: 'center',
            gap: t.spacing[1],
            flex: 1,
          }}
        >
          <Ionicons
            name="shield-checkmark-outline"
            size={13}
            color={t.colors.text.tertiary}
          />
          <Text
            style={{
              fontSize: t.typography.fontSize.xs,
              color: t.colors.text.tertiary,
            }}
            numberOfLines={1}
          >
            {payment.healthPlan}
          </Text>
        </View>

        <View style={{ flexDirection: 'row', alignItems: 'center', gap: t.spacing[3] }}>
          <View style={{ flexDirection: 'row', alignItems: 'center', gap: t.spacing[1] }}>
            <Ionicons
              name="calendar-outline"
              size={13}
              color={t.colors.text.tertiary}
            />
            <Text
              style={{
                fontSize: t.typography.fontSize.xs,
                color: t.colors.text.tertiary,
              }}
            >
              {formatDate(payment.executionDate)}
            </Text>
          </View>

          <Text
            style={{
              fontSize: t.typography.fontSize.sm,
              fontWeight: t.typography.fontWeight.bold,
              color: t.colors.text.primary,
            }}
          >
            {formatCurrency(payment.value)}
          </Text>
        </View>
      </View>
    </Pressable>
  );
}

function FilterChip({
  label,
  selected,
  onPress,
  color,
}: {
  label: string;
  selected: boolean;
  onPress: () => void;
  color?: string;
}) {
  const t = useTheme();
  const activeColor = color ?? t.colors.primary;
  return (
    <Pressable
      onPress={onPress}
      style={[
        {
          flexDirection: 'row',
          alignItems: 'center',
          paddingHorizontal: t.spacing[3],
          paddingVertical: t.spacing[1.5],
          borderRadius: t.borderRadius.full,
          borderWidth: 1,
          borderColor: selected ? activeColor : t.colors.border,
          backgroundColor: selected ? `${activeColor}1A` : t.colors.surface.card,
        },
      ]}
    >
      {selected && (
        <Ionicons
          name="checkmark"
          size={12}
          color={activeColor}
          style={{ marginRight: t.spacing[1] }}
        />
      )}
      <Text
        style={{
          fontSize: t.typography.fontSize.xs,
          fontWeight: selected
            ? t.typography.fontWeight.semibold
            : t.typography.fontWeight.regular,
          color: selected ? activeColor : t.colors.text.secondary,
        }}
      >
        {label}
      </Text>
    </Pressable>
  );
}

function DropdownButton({
  label,
  value,
  onPress,
}: {
  label: string;
  value: string;
  onPress: () => void;
}) {
  const t = useTheme();
  const isActive = value !== label;
  return (
    <Pressable
      onPress={onPress}
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        gap: t.spacing[1.5],
        paddingHorizontal: t.spacing[3],
        paddingVertical: t.spacing[2],
        borderRadius: t.borderRadius.lg,
        borderWidth: 1,
        borderColor: isActive ? t.colors.primary : t.colors.border,
        backgroundColor: isActive
          ? `${t.colors.primary}1A`
          : t.colors.surface.card,
      }}
    >
      <Text
        style={{
          fontSize: t.typography.fontSize.sm,
          fontWeight: isActive
            ? t.typography.fontWeight.semibold
            : t.typography.fontWeight.regular,
          color: isActive ? t.colors.primary : t.colors.text.secondary,
        }}
        numberOfLines={1}
      >
        {value}
      </Text>
      <Ionicons
        name="chevron-down"
        size={14}
        color={isActive ? t.colors.primary : t.colors.text.tertiary}
      />
    </Pressable>
  );
}

// ---------------------------------------------------------------------------
// Modals (Period + Health Plan pickers)
// ---------------------------------------------------------------------------
import { Modal } from 'react-native';

function PickerModal<T extends string>({
  visible,
  title,
  options,
  selected,
  onSelect,
  onClose,
}: {
  visible: boolean;
  title: string;
  options: { value: T; label: string }[];
  selected: T;
  onSelect: (v: T) => void;
  onClose: () => void;
}) {
  const t = useTheme();
  return (
    <Modal
      visible={visible}
      transparent
      animationType="slide"
      onRequestClose={onClose}
    >
      <Pressable
        style={{
          flex: 1,
          backgroundColor: t.colors.surface.overlay,
          justifyContent: 'flex-end',
        }}
        onPress={onClose}
      >
        <Pressable
          style={{
            backgroundColor: t.colors.surface.card,
            borderTopLeftRadius: t.borderRadius['3xl'],
            borderTopRightRadius: t.borderRadius['3xl'],
            paddingBottom: Platform.OS === 'ios' ? 34 : t.spacing[6],
          }}
          onPress={noop}
        >
          {/* Handle */}
          <View
            style={{
              alignSelf: 'center',
              width: 40,
              height: 4,
              borderRadius: t.borderRadius.full,
              backgroundColor: t.colors.borderStrong,
              marginTop: t.spacing[3],
              marginBottom: t.spacing[4],
            }}
          />

          <Text
            style={{
              fontSize: t.typography.fontSize.lg,
              fontWeight: t.typography.fontWeight.semibold,
              color: t.colors.text.primary,
              paddingHorizontal: t.spacing[6],
              marginBottom: t.spacing[4],
            }}
          >
            {title}
          </Text>

          {options.map((opt) => (
            <Pressable
              key={opt.value}
              onPress={() => {
                onSelect(opt.value);
                onClose();
              }}
              style={({ pressed }) => ({
                flexDirection: 'row',
                alignItems: 'center',
                justifyContent: 'space-between',
                paddingVertical: t.spacing[4],
                paddingHorizontal: t.spacing[6],
                backgroundColor: pressed
                  ? t.colors.surface.cardPressed
                  : 'transparent',
              })}
            >
              <Text
                style={{
                  fontSize: t.typography.fontSize.md,
                  color:
                    selected === opt.value
                      ? t.colors.primary
                      : t.colors.text.primary,
                  fontWeight:
                    selected === opt.value
                      ? t.typography.fontWeight.semibold
                      : t.typography.fontWeight.regular,
                }}
              >
                {opt.label}
              </Text>
              {selected === opt.value && (
                <Ionicons
                  name="checkmark-circle"
                  size={20}
                  color={t.colors.primary}
                />
              )}
            </Pressable>
          ))}
        </Pressable>
      </Pressable>
    </Modal>
  );
}

// ---------------------------------------------------------------------------
// Empty state
// ---------------------------------------------------------------------------
function EmptyState({ hasFilters }: { hasFilters: boolean }) {
  const t = useTheme();
  return (
    <View
      style={{
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: t.spacing[8],
        paddingTop: t.spacing[12],
      }}
    >
      <View
        style={{
          width: 72,
          height: 72,
          borderRadius: t.borderRadius['2xl'],
          backgroundColor: t.colors.primaryLight,
          alignItems: 'center',
          justifyContent: 'center',
          marginBottom: t.spacing[4],
        }}
      >
        <Ionicons
          name={hasFilters ? 'search-outline' : 'receipt-outline'}
          size={32}
          color={t.colors.primary}
        />
      </View>
      <Text
        style={{
          fontSize: t.typography.fontSize.lg,
          fontWeight: t.typography.fontWeight.semibold,
          color: t.colors.text.primary,
          textAlign: 'center',
          marginBottom: t.spacing[2],
        }}
      >
        {hasFilters ? 'Nenhum resultado' : 'Sem pagamentos'}
      </Text>
      <Text
        style={{
          fontSize: t.typography.fontSize.sm,
          color: t.colors.text.secondary,
          textAlign: 'center',
          lineHeight: 20,
        }}
      >
        {hasFilters
          ? 'Tente ajustar os filtros para encontrar o que procura.'
          : 'Seus pagamentos aparecerão aqui quando forem registrados.'}
      </Text>
    </View>
  );
}

// ---------------------------------------------------------------------------
// Main screen
// ---------------------------------------------------------------------------
const STATUS_FILTER_OPTIONS: { value: StatusFilter; label: string }[] = [
  { value: 'All', label: 'Todos' },
  { value: 'Pending', label: 'Pendente' },
  { value: 'Paid', label: 'Pago' },
  { value: 'Refused', label: 'Recusado' },
];

const PERIOD_OPTIONS: { value: PeriodFilter; label: string }[] = [
  { value: 'All', label: 'Todo período' },
  { value: 'Today', label: 'Hoje' },
  { value: 'Week', label: 'Últimos 7 dias' },
  { value: 'Month', label: 'Últimos 30 dias' },
];

const HEALTH_PLAN_OPTIONS = [
  { value: 'all', label: 'Todos os convênios' },
  ...MOCK_HEALTH_PLANS.map((hp) => ({ value: hp.id, label: hp.name })),
];

export default function HomeScreen() {
  const t = useTheme();
  const insets = useSafeAreaInsets();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [periodFilter, setPeriodFilter] = useState<PeriodFilter>('All');
  const [healthPlanFilter, setHealthPlanFilter] = useState('all');
  const [showPeriodModal, setShowPeriodModal] = useState(false);
  const [showHealthPlanModal, setShowHealthPlanModal] = useState(false);

  const today = getTodayIso();
  const todayFormatted = formatDate(today);

  // Apply filters
  const filteredPayments = useMemo(() => {
    return MOCK_PAYMENTS.filter((p) => {
      if (
        searchQuery &&
        !p.beneficiaryName.toLowerCase().includes(searchQuery.toLowerCase())
      ) {
        return false;
      }
      if (statusFilter !== 'All' && p.status !== statusFilter) return false;
      if (healthPlanFilter !== 'all' && p.healthPlanId !== healthPlanFilter)
        return false;
      if (periodFilter === 'Today' && p.executionDate !== today) return false;
      if (periodFilter === 'Week' && !isWithinDays(p.executionDate, 7))
        return false;
      if (periodFilter === 'Month' && !isWithinDays(p.executionDate, 30))
        return false;
      return true;
    });
  }, [searchQuery, statusFilter, healthPlanFilter, periodFilter, today]);

  // Summary totals (from filtered payments)
  const summary = useMemo(() => {
    const total = filteredPayments.reduce((acc, p) => acc + p.value, 0);
    const pending = filteredPayments.filter((p) => p.status === 'Pending');
    const paid = filteredPayments.filter((p) => p.status === 'Paid');
    const refused = filteredPayments.filter((p) => p.status === 'Refused');
    return {
      total,
      pendingCount: pending.length,
      pendingValue: pending.reduce((acc, p) => acc + p.value, 0),
      paidValue: paid.reduce((acc, p) => acc + p.value, 0),
      refusedCount: refused.length,
      refusedValue: refused.reduce((acc, p) => acc + p.value, 0),
    };
  }, [filteredPayments]);

  const hasActiveFilters =
    statusFilter !== 'All' ||
    periodFilter !== 'All' ||
    healthPlanFilter !== 'all' ||
    searchQuery.length > 0;

  const selectedHealthPlanLabel =
    healthPlanFilter === 'all'
      ? 'Convênio'
      : MOCK_HEALTH_PLANS.find((hp) => hp.id === healthPlanFilter)?.name ??
        'Convênio';

  const s = makeStyles(t);

  const ListHeader = (
    <>
      {/* ── Hero header ──────────────────────────────────────── */}
      <View style={[s.hero, { paddingTop: t.spacing[6] + insets.top }]}>
        <View style={s.heroInner}>
          <View style={s.heroAvatar}>
            <Ionicons name="person" size={20} color={t.colors.secondaryText} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={s.heroGreeting}>Olá, Dr. Carlos</Text>
            <Text style={s.heroDate}>{todayFormatted}</Text>
          </View>
          <Pressable style={s.notificationBtn}>
            <Ionicons
              name="notifications-outline"
              size={22}
              color={t.colors.text.onDark}
            />
            {/* Unread dot */}
            <View style={s.notificationDot} />
          </Pressable>
        </View>

        {/* ── Summary cards ──────────────────────────────────── */}
        <View style={s.summaryGrid}>
          <View style={s.summaryRow}>
            <SummaryCard
              label="Total recebido"
              value={formatCurrency(summary.paidValue)}
              color={t.colors.success.base}
              icon="trending-up"
            />
            <SummaryCard
              label="Pendente"
              value={formatCurrency(summary.pendingValue)}
              color={t.colors.warning.base}
              icon="time-outline"
            />
          </View>
          <View style={s.summaryRow}>
            <SummaryCard
              label="Glosas"
              value={`${summary.refusedCount} lançamento${summary.refusedCount !== 1 ? 's' : ''}`}
              color={t.colors.error.base}
              icon="close-circle-outline"
            />
            <SummaryCard
              label="Total do período"
              value={formatCurrency(summary.total)}
              color={t.colors.primary}
              icon="wallet-outline"
            />
          </View>
        </View>
      </View>

      {/* ── Search + Filters ──────────────────────────────────── */}
      <View style={s.filtersSection}>
        {/* Search bar */}
        <View style={s.searchBar}>
          <Ionicons
            name="search-outline"
            size={18}
            color={t.colors.text.tertiary}
          />
          <TextInput
            value={searchQuery}
            onChangeText={setSearchQuery}
            placeholder="Buscar beneficiário..."
            placeholderTextColor={t.colors.text.tertiary}
            style={s.searchInput}
            returnKeyType="search"
            clearButtonMode="while-editing"
          />
          {searchQuery.length > 0 && Platform.OS !== 'ios' && (
            <Pressable onPress={() => setSearchQuery('')} hitSlop={8}>
              <Ionicons
                name="close-circle"
                size={18}
                color={t.colors.text.tertiary}
              />
            </Pressable>
          )}
        </View>

        {/* Status chips */}
        <View style={s.chipsRow}>
          {STATUS_FILTER_OPTIONS.map((opt) => (
            <FilterChip
              key={opt.value}
              label={opt.label}
              selected={statusFilter === opt.value}
              onPress={() => setStatusFilter(opt.value)}
              color={
                opt.value === 'Pending'
                  ? t.colors.paymentStatus.pending.dot
                  : opt.value === 'Paid'
                    ? t.colors.paymentStatus.paid.dot
                    : opt.value === 'Refused'
                      ? t.colors.paymentStatus.refused.dot
                      : t.colors.primary
              }
            />
          ))}
        </View>

        {/* Period + Health plan dropdowns */}
        <View style={s.dropdownRow}>
          <DropdownButton
            label="Período"
            value={PERIOD_LABELS[periodFilter]}
            onPress={() => setShowPeriodModal(true)}
          />
          <DropdownButton
            label="Convênio"
            value={selectedHealthPlanLabel}
            onPress={() => setShowHealthPlanModal(true)}
          />
          {hasActiveFilters && (
            <Pressable
              onPress={() => {
                setSearchQuery('');
                setStatusFilter('All');
                setPeriodFilter('All');
                setHealthPlanFilter('all');
              }}
              style={s.clearFiltersBtn}
            >
              <Ionicons
                name="refresh-outline"
                size={14}
                color={t.colors.error.base}
              />
              <Text style={[s.clearFiltersText, { color: t.colors.error.base }]}>
                Limpar
              </Text>
            </Pressable>
          )}
        </View>
      </View>

      {/* ── List header ──────────────────────────────────────── */}
      <View style={s.listHeader}>
        <Text style={s.listTitle}>Pagamentos</Text>
        <View style={s.countBadge}>
          <Text style={s.countText}>{filteredPayments.length}</Text>
        </View>
      </View>
    </>
  );

  return (
    // Root com fundo navy — cobre a status bar quando headerShown: false
    <View style={[s.root, { backgroundColor: t.colors.secondary }]}>
      <FlatList
        data={filteredPayments}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <PaymentCard payment={item} onPress={noop} />
        )}
        ListHeaderComponent={ListHeader}
        ListEmptyComponent={<EmptyState hasFilters={hasActiveFilters} />}
        contentContainerStyle={
          filteredPayments.length === 0
            ? { flexGrow: 1, paddingBottom: insets.bottom + 24 }
            : { paddingBottom: insets.bottom + 24 }
        }
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="handled"
        keyboardDismissMode="on-drag"
        style={s.flatList}
      />

      <PickerModal
        visible={showPeriodModal}
        title="Período"
        options={PERIOD_OPTIONS}
        selected={periodFilter}
        onSelect={setPeriodFilter}
        onClose={() => setShowPeriodModal(false)}
      />

      <PickerModal
        visible={showHealthPlanModal}
        title="Convênio"
        options={HEALTH_PLAN_OPTIONS}
        selected={healthPlanFilter}
        onSelect={setHealthPlanFilter}
        onClose={() => setShowHealthPlanModal(false)}
      />
    </View>
  );
}

// ---------------------------------------------------------------------------
// StyleSheet factory
// ---------------------------------------------------------------------------
function makeStyles(t: ReturnType<typeof useTheme>) {
  return StyleSheet.create({
    root: {
      flex: 1,
      backgroundColor: t.colors.surface.background,
    },
    flatList: {
      backgroundColor: t.colors.surface.background,
    },

    // Hero
    hero: {
      backgroundColor: t.colors.secondary,
      paddingTop: t.spacing[6],
      paddingBottom: t.spacing[6],
      paddingHorizontal: t.spacing[4],
    },
    heroInner: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: t.spacing[3],
      marginBottom: t.spacing[5],
    },
    heroAvatar: {
      width: t.components.avatarMd,
      height: t.components.avatarMd,
      borderRadius: t.borderRadius.full,
      backgroundColor: 'rgba(255,255,255,0.15)',
      alignItems: 'center',
      justifyContent: 'center',
    },
    heroGreeting: {
      fontSize: t.typography.fontSize.lg,
      fontWeight: t.typography.fontWeight.semibold,
      color: t.colors.text.onDark,
      lineHeight: 24,
    },
    heroDate: {
      fontSize: t.typography.fontSize.sm,
      color: t.colors.text.onDarkSubtle,
      marginTop: 1,
    },
    notificationBtn: {
      width: 40,
      height: 40,
      borderRadius: t.borderRadius.full,
      backgroundColor: 'rgba(255,255,255,0.10)',
      alignItems: 'center',
      justifyContent: 'center',
    },
    notificationDot: {
      position: 'absolute',
      top: 8,
      right: 8,
      width: 8,
      height: 8,
      borderRadius: t.borderRadius.full,
      backgroundColor: t.colors.primary,
      borderWidth: 1.5,
      borderColor: t.colors.secondary,
    },

    // Summary
    summaryGrid: {
      gap: t.spacing[3],
    },
    summaryRow: {
      flexDirection: 'row',
      gap: t.spacing[3],
    },

    // Filters
    filtersSection: {
      paddingHorizontal: t.spacing[4],
      paddingTop: t.spacing[4],
      paddingBottom: t.spacing[2],
      gap: t.spacing[3],
      backgroundColor: t.colors.surface.background,
    },
    searchBar: {
      flexDirection: 'row',
      alignItems: 'center',
      backgroundColor: t.colors.surface.card,
      borderRadius: t.borderRadius.lg,
      borderWidth: 1,
      borderColor: t.colors.border,
      paddingHorizontal: t.spacing[3],
      height: t.components.inputHeight,
      gap: t.spacing[2],
    },
    searchInput: {
      flex: 1,
      fontSize: t.typography.fontSize.sm,
      color: t.colors.text.primary,
      paddingVertical: 0,
    },
    chipsRow: {
      flexDirection: 'row',
      gap: t.spacing[2],
      flexWrap: 'nowrap',
    },
    dropdownRow: {
      flexDirection: 'row',
      gap: t.spacing[2],
      alignItems: 'center',
    },
    clearFiltersBtn: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: t.spacing[1],
      marginLeft: 'auto',
      paddingHorizontal: t.spacing[2],
      paddingVertical: t.spacing[1.5],
    },
    clearFiltersText: {
      fontSize: t.typography.fontSize.xs,
      fontWeight: t.typography.fontWeight.medium,
    },

    // List header
    listHeader: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: t.spacing[2],
      paddingHorizontal: t.spacing[4],
      paddingTop: t.spacing[2],
      paddingBottom: t.spacing[3],
    },
    listTitle: {
      fontSize: t.typography.fontSize.md,
      fontWeight: t.typography.fontWeight.semibold,
      color: t.colors.text.primary,
    },
    countBadge: {
      backgroundColor: t.colors.primaryLight,
      borderRadius: t.borderRadius.full,
      paddingHorizontal: t.spacing[2],
      paddingVertical: 2,
      minWidth: 24,
      alignItems: 'center',
    },
    countText: {
      fontSize: t.typography.fontSize.xs,
      fontWeight: t.typography.fontWeight.bold,
      color: t.colors.primary,
    },
  });
}
