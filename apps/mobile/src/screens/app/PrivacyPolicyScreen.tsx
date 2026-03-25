import React from 'react';
import { Linking, Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { Text } from 'react-native-paper';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { theme as lightTheme } from '@medcontrol/design-system/native';
import { useAppTheme } from '../../contexts/ThemeContext';

const LAST_UPDATED = '25 de março de 2026';

interface SectionProps {
  title: string;
  children: React.ReactNode;
  styles: ReturnType<typeof makeStyles>;
}

function Section({ title, children, styles }: SectionProps) {
  return (
    <View style={styles.section}>
      <Text style={styles.sectionTitle}>{title}</Text>
      {children}
    </View>
  );
}

interface BulletProps {
  children: string;
  styles: ReturnType<typeof makeStyles>;
}

function Bullet({ children, styles }: BulletProps) {
  return (
    <View style={styles.bulletRow}>
      <Text style={styles.bulletDot}>•</Text>
      <Text style={styles.bulletText}>{children}</Text>
    </View>
  );
}

export default function PrivacyPolicyScreen() {
  const t = useAppTheme();
  const insets = useSafeAreaInsets();
  const router = useRouter();
  const s = makeStyles(t);

  return (
    <View style={{ flex: 1, backgroundColor: t.colors.surface.background }}>
      {/* Header */}
      <View style={[s.header, { paddingTop: insets.top + 16 }]}>
        <Pressable onPress={() => router.back()} style={s.headerBtn} testID="back-button">
          <Ionicons name="arrow-back" size={22} color={t.colors.text.onDark} />
        </Pressable>
        <Text style={s.headerTitle}>Política de Privacidade</Text>
        <View style={s.headerBtn} />
      </View>

      <ScrollView
        style={{ flex: 1 }}
        contentContainerStyle={[s.content, { paddingBottom: insets.bottom + 32 }]}
      >
        {/* Meta */}
        <View style={s.metaBox}>
          <Text style={s.metaText}>Última atualização: {LAST_UPDATED}</Text>
        </View>

        {/* Lead */}
        <Text style={s.lead}>
          A MedControl leva a privacidade dos seus dados a sério. Esta política descreve como
          coletamos, usamos e protegemos seus dados pessoais, em conformidade com a{' '}
          <Text style={s.bold}>Lei Geral de Proteção de Dados (LGPD — Lei n.º 13.709/2018)</Text>.
        </Text>

        {/* 1 */}
        <Section title="1. Controlador dos Dados" styles={s}>
          <Text style={s.body}>O controlador responsável é:</Text>
          <View style={s.infoBox}>
            <Text style={s.infoText}>
              <Text style={s.bold}>MedControl Tecnologia Ltda.</Text>
            </Text>
            <Pressable onPress={() => Linking.openURL('mailto:privacidade@medcontrol.app')}>
              <Text style={[s.infoText, s.link]}>privacidade@medcontrol.app</Text>
            </Pressable>
          </View>
        </Section>

        {/* 2 */}
        <Section title="2. Dados Coletados" styles={s}>
          <Text style={s.subsectionTitle}>Identificação e acesso</Text>
          <Bullet styles={s}>Nome completo e e-mail (cadastro ou Google OAuth)</Bullet>
          <Bullet styles={s}>Foto de perfil (quando autenticado via Google)</Bullet>
          <Bullet styles={s}>Identificadores de sessão (cookies HttpOnly)</Bullet>

          <Text style={[s.subsectionTitle, { marginTop: 12 }]}>Dados profissionais (médicos)</Text>
          <Bullet styles={s}>Número de CRM e estado do conselho regional</Bullet>
          <Bullet styles={s}>Especialidade médica</Bullet>

          <Text style={[s.subsectionTitle, { marginTop: 12 }]}>Dados operacionais</Text>
          <Bullet styles={s}>Nome e carteira do beneficiário (paciente)</Bullet>
          <Bullet styles={s}>Número de atendimento e código de autorização</Bullet>
          <Bullet styles={s}>Data e local de execução, procedimentos e valores</Bullet>

          <View style={s.warningBox}>
            <Ionicons name="warning-outline" size={16} color={t.colors.warning.dark} />
            <Text style={s.warningText}>
              <Text style={s.bold}>Dados sensíveis: </Text>
              informações de saúde são tratadas com controles de acesso restritos conforme LGPD art. 11.
            </Text>
          </View>
        </Section>

        {/* 3 */}
        <Section title="3. Finalidade e Base Legal" styles={s}>
          {[
            { purpose: 'Autenticação e controle de acesso', basis: 'Execução de contrato (art. 7º, V)' },
            { purpose: 'Prestação do serviço de pagamentos', basis: 'Execução de contrato (art. 7º, V)' },
            { purpose: 'Gerenciamento de membros', basis: 'Execução de contrato (art. 7º, V)' },
            { purpose: 'E-mails transacionais (magic link)', basis: 'Legítimo interesse (art. 7º, IX)' },
            { purpose: 'Obrigações legais', basis: 'Obrigação legal (art. 7º, II)' },
          ].map((row) => (
            <View key={row.purpose} style={s.tableRow}>
              <Text style={s.tableCell}>{row.purpose}</Text>
              <Text style={[s.tableCell, s.tableCellBasis]}>{row.basis}</Text>
            </View>
          ))}
        </Section>

        {/* 4 */}
        <Section title="4. Compartilhamento de Dados" styles={s}>
          <Text style={s.body}>
            Não vendemos dados. Compartilhamos apenas com fornecedores de infraestrutura:
          </Text>
          <Bullet styles={s}>Google LLC — autenticação OAuth</Bullet>
          <Bullet styles={s}>Cloudflare, Inc. — CDN e armazenamento</Bullet>
          <Bullet styles={s}>Upstash, Inc. — cache Redis (tokens TTL 15 min)</Bullet>
          <Bullet styles={s}>Resend, Inc. — envio de e-mails transacionais</Bullet>
          <Bullet styles={s}>Provedor PostgreSQL — armazenamento persistente</Bullet>
        </Section>

        {/* 5 */}
        <Section title="5. Retenção de Dados" styles={s}>
          {[
            { type: 'Tokens de magic link', period: '15 minutos (TTL automático)' },
            { type: 'Cookies de sessão', period: 'Até logout ou expiração do JWT' },
            { type: 'Dados de conta', period: 'Conta ativa + 5 anos após encerramento' },
            { type: 'Pagamentos e procedimentos', period: 'Conta ativa + 5 anos (prazo fiscal)' },
          ].map((row) => (
            <View key={row.type} style={s.tableRow}>
              <Text style={s.tableCell}>{row.type}</Text>
              <Text style={[s.tableCell, s.tableCellBasis]}>{row.period}</Text>
            </View>
          ))}
        </Section>

        {/* 6 */}
        <Section title="6. Seus Direitos (LGPD art. 18)" styles={s}>
          <Bullet styles={s}>Confirmação e acesso aos seus dados</Bullet>
          <Bullet styles={s}>Correção de dados incompletos ou inexatos</Bullet>
          <Bullet styles={s}>Anonimização, bloqueio ou eliminação</Bullet>
          <Bullet styles={s}>Portabilidade dos dados</Bullet>
          <Bullet styles={s}>Eliminação dos dados tratados com consentimento</Bullet>
          <Bullet styles={s}>Revogação do consentimento</Bullet>
          <Bullet styles={s}>Oposição ao tratamento por legítimo interesse</Bullet>
          <Text style={[s.body, { marginTop: 8 }]}>
            Para exercer esses direitos, envie e-mail para:
          </Text>
          <Pressable onPress={() => Linking.openURL('mailto:privacidade@medcontrol.app')}>
            <Text style={[s.body, s.link]}>privacidade@medcontrol.app</Text>
          </Pressable>
          <Text style={s.body}>Prazo de resposta: até 15 dias úteis.</Text>
        </Section>

        {/* 7 */}
        <Section title="7. Segurança" styles={s}>
          <Bullet styles={s}>Transmissão cifrada via HTTPS/TLS</Bullet>
          <Bullet styles={s}>JWT com chave mínima de 256 bits</Bullet>
          <Bullet styles={s}>Cookies HttpOnly e SameSite=Strict</Bullet>
          <Bullet styles={s}>Tokens de magic link de uso único (TTL 15 min)</Bullet>
          <Bullet styles={s}>Isolamento por tenant (multi-tenancy row-level)</Bullet>
          <Bullet styles={s}>Controle de acesso por roles (operator, doctor, admin, owner)</Bullet>
        </Section>

        {/* 8 */}
        <Section title="8. Encarregado (DPO)" styles={s}>
          <View style={s.infoBox}>
            <Text style={s.infoText}>Para dúvidas, solicitações ou reclamações:</Text>
            <Pressable onPress={() => Linking.openURL('mailto:privacidade@medcontrol.app')}>
              <Text style={[s.infoText, s.link]}>privacidade@medcontrol.app</Text>
            </Pressable>
            <Text style={[s.infoText, { marginTop: 8 }]}>
              Você também pode registrar reclamações na ANPD:
            </Text>
            <Pressable onPress={() => Linking.openURL('https://www.gov.br/anpd')}>
              <Text style={[s.infoText, s.link]}>gov.br/anpd</Text>
            </Pressable>
          </View>
        </Section>
      </ScrollView>
    </View>
  );
}

function makeStyles(t: ReturnType<typeof useAppTheme>) {
  const isDark = t !== lightTheme;
  return StyleSheet.create({
    header: {
      backgroundColor: t.colors.secondary,
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      paddingHorizontal: 16,
      paddingBottom: 16,
    },
    headerTitle: {
      fontSize: t.typography.fontSize.md,
      fontWeight: t.typography.fontWeight.semibold,
      color: t.colors.text.onDark,
    },
    headerBtn: {
      width: 40,
      height: 40,
      borderRadius: 20,
      backgroundColor: 'rgba(255,255,255,0.10)',
      alignItems: 'center',
      justifyContent: 'center',
    },
    content: {
      padding: 16,
      gap: 0,
    },
    metaBox: {
      backgroundColor: t.colors.surface.card,
      borderRadius: t.borderRadius.md,
      borderWidth: 1,
      borderColor: t.colors.border,
      paddingVertical: 10,
      paddingHorizontal: 14,
      marginBottom: 20,
      alignSelf: 'flex-start',
    },
    metaText: {
      fontSize: t.typography.fontSize.xs,
      color: t.colors.text.tertiary,
    },
    lead: {
      fontSize: t.typography.fontSize.md,
      lineHeight: t.typography.fontSize.md * 1.7,
      color: t.colors.text.secondary,
      marginBottom: 24,
    },
    bold: {
      fontWeight: t.typography.fontWeight.semibold,
      color: t.colors.text.primary,
    },
    section: {
      marginBottom: 28,
    },
    sectionTitle: {
      fontSize: t.typography.fontSize.lg,
      fontWeight: t.typography.fontWeight.semibold,
      color: t.colors.text.primary,
      marginBottom: 12,
      paddingBottom: 8,
      borderBottomWidth: 2,
      // primaryLight adapts: #FFF4ED (light) → rgba(249,115,22,0.14) (dark)
      borderBottomColor: t.colors.primaryLight,
    },
    subsectionTitle: {
      fontSize: t.typography.fontSize.sm,
      fontWeight: t.typography.fontWeight.semibold,
      color: t.colors.text.secondary,
      textTransform: 'uppercase',
      letterSpacing: 0.5,
      marginBottom: 6,
    },
    body: {
      fontSize: t.typography.fontSize.md,
      lineHeight: t.typography.fontSize.md * 1.6,
      color: t.colors.text.primary,
      marginBottom: 6,
    },
    link: {
      color: t.colors.text.link,
      textDecorationLine: 'underline',
    },
    bulletRow: {
      flexDirection: 'row',
      gap: 8,
      marginBottom: 6,
    },
    bulletDot: {
      fontSize: t.typography.fontSize.md,
      color: t.colors.primary,
      lineHeight: t.typography.fontSize.md * 1.6,
    },
    bulletText: {
      flex: 1,
      fontSize: t.typography.fontSize.md,
      lineHeight: t.typography.fontSize.md * 1.6,
      color: t.colors.text.primary,
    },
    infoBox: {
      // secondaryLight/navy[*] não são sobrescritos no darkTheme — usar valores condicionais
      backgroundColor: isDark ? 'rgba(99, 102, 241, 0.10)' : t.colors.secondaryLight,
      borderRadius: t.borderRadius.md,
      borderWidth: 1,
      borderColor: isDark ? 'rgba(99, 102, 241, 0.25)' : t.colors.navy[200],
      borderLeftWidth: 4,
      borderLeftColor: isDark ? t.colors.navy[400] : t.colors.secondary,
      padding: 14,
      gap: 4,
    },
    infoText: {
      fontSize: t.typography.fontSize.sm,
      color: t.colors.text.primary,
      lineHeight: t.typography.fontSize.sm * 1.6,
    },
    warningBox: {
      flexDirection: 'row',
      gap: 8,
      backgroundColor: t.colors.warning.light,
      borderRadius: t.borderRadius.md,
      borderWidth: 1,
      // warning.light adapta, mas a borda não — usar valor condicional
      borderColor: isDark ? 'rgba(251, 191, 36, 0.28)' : '#FDE68A',
      padding: 12,
      marginTop: 12,
      alignItems: 'flex-start',
    },
    warningText: {
      flex: 1,
      fontSize: t.typography.fontSize.sm,
      lineHeight: t.typography.fontSize.sm * 1.6,
      color: t.colors.warning.dark,
    },
    tableRow: {
      flexDirection: 'row',
      gap: 8,
      paddingVertical: 8,
      borderBottomWidth: 1,
      borderBottomColor: t.colors.divider,
    },
    tableCell: {
      flex: 1,
      fontSize: t.typography.fontSize.sm,
      color: t.colors.text.primary,
      lineHeight: t.typography.fontSize.sm * 1.5,
    },
    tableCellBasis: {
      color: t.colors.text.secondary,
    },
  });
}
