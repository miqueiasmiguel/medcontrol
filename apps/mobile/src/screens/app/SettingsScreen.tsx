import React, { useEffect, useState } from 'react';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  View,
} from 'react-native';
import { Text } from 'react-native-paper';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { useForm, Controller } from 'react-hook-form';
import { useAppTheme, useThemePreference, type ThemePreference } from '../../contexts/ThemeContext';
import { useAuth } from '../../hooks/useAuth';
import { useCurrentUser } from '../../hooks/useCurrentUser';
import { useDoctorProfile } from '../../hooks/useDoctorProfile';
import { UserService } from '../../services/user.service';
import { AppTextInput } from '../../components/ui/AppTextInput';
import { AppButton } from '../../components/ui/AppButton';

interface EditProfileForm {
  displayName: string;
  name: string;
  crm: string;
  councilState: string;
  specialty: string;
}

const THEME_OPTIONS: { value: ThemePreference; label: string; icon: string }[] = [
  { value: 'system', label: 'Sistema', icon: 'phone-portrait-outline' },
  { value: 'light', label: 'Claro', icon: 'sunny-outline' },
  { value: 'dark', label: 'Escuro', icon: 'moon-outline' },
];

export default function SettingsScreen() {
  const t = useAppTheme();
  const insets = useSafeAreaInsets();
  const router = useRouter();
  const { logout } = useAuth();
  const { preference, setPreference } = useThemePreference();
  const { user } = useCurrentUser();
  const { doctorProfile } = useDoctorProfile();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<EditProfileForm>({
    defaultValues: {
      displayName: '',
      name: '',
      crm: '',
      councilState: '',
      specialty: '',
    },
  });

  useEffect(() => {
    reset({
      displayName: user?.displayName ?? '',
      name: doctorProfile?.name ?? '',
      crm: doctorProfile?.crm ?? '',
      councilState: doctorProfile?.councilState ?? '',
      specialty: doctorProfile?.specialty ?? '',
    });
  }, [user, doctorProfile, reset]);

  async function onSubmit(data: EditProfileForm) {
    setIsSubmitting(true);
    try {
      await Promise.all([
        UserService.updateProfile({ displayName: data.displayName || undefined }),
        UserService.updateMyDoctorProfile({
          name: data.name,
          crm: data.crm,
          councilState: data.councilState,
          specialty: data.specialty,
        }),
      ]);
      Alert.alert('Sucesso', 'Perfil atualizado com sucesso.');
    } catch (e) {
      Alert.alert('Erro', e instanceof Error ? e.message : 'Erro ao salvar perfil.');
    } finally {
      setIsSubmitting(false);
    }
  }

  function handleLogoutPress() {
    Alert.alert('Sair', 'Deseja realmente sair da sua conta?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Sair',
        style: 'destructive',
        onPress: async () => {
          await logout();
          router.replace('/(auth)/login');
        },
      },
    ]);
  }

  const s = makeStyles(t);

  return (
    <KeyboardAvoidingView
      style={{ flex: 1 }}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <View style={[s.header, { paddingTop: insets.top + 16 }]}>
        <Pressable onPress={() => router.back()} style={s.headerBtn}>
          <Ionicons name="arrow-back" size={22} color={t.colors.text.onDark} />
        </Pressable>
        <Text style={s.headerTitle}>Configurações</Text>
        <View style={s.headerBtn} />
      </View>

      <ScrollView
        style={{ flex: 1, backgroundColor: t.colors.surface.background }}
        contentContainerStyle={[s.content, { paddingBottom: insets.bottom + 24 }]}
        keyboardShouldPersistTaps="handled"
      >
        {/* ── Minha Conta ── */}
        <Text style={s.sectionTitle}>Minha Conta</Text>

        <Controller
          control={control}
          name="displayName"
          render={({ field: { onChange, value } }) => (
            <AppTextInput
              testID="field-displayName"
              label="Nome de exibição"
              value={value}
              onChangeText={onChange}
              placeholder="Como você quer ser chamado"
              errorMessage={errors.displayName?.message}
            />
          )}
        />

        <Text style={[s.sectionTitle, { marginTop: 24 }]}>Dados Profissionais</Text>

        <Controller
          control={control}
          name="name"
          rules={{ required: 'Nome profissional é obrigatório' }}
          render={({ field: { onChange, value } }) => (
            <AppTextInput
              testID="field-name"
              label="Nome profissional"
              value={value}
              onChangeText={onChange}
              placeholder="Nome completo com título"
              errorMessage={errors.name?.message}
            />
          )}
        />

        <Controller
          control={control}
          name="crm"
          rules={{ required: 'CRM é obrigatório' }}
          render={({ field: { onChange, value } }) => (
            <AppTextInput
              testID="field-crm"
              label="CRM"
              value={value}
              onChangeText={onChange}
              placeholder="Número do CRM"
              keyboardType="default"
              errorMessage={errors.crm?.message}
            />
          )}
        />

        <Controller
          control={control}
          name="councilState"
          rules={{
            required: 'Estado do conselho é obrigatório',
            maxLength: { value: 2, message: 'Máximo 2 caracteres (ex: SP)' },
          }}
          render={({ field: { onChange, value } }) => (
            <AppTextInput
              testID="field-councilState"
              label="Estado do Conselho"
              value={value}
              onChangeText={onChange}
              placeholder="UF (ex: SP)"
              autoCapitalize="characters"
              errorMessage={errors.councilState?.message}
            />
          )}
        />

        <Controller
          control={control}
          name="specialty"
          rules={{ required: 'Especialidade é obrigatória' }}
          render={({ field: { onChange, value } }) => (
            <AppTextInput
              testID="field-specialty"
              label="Especialidade"
              value={value}
              onChangeText={onChange}
              placeholder="Ex: Cardiologia"
              errorMessage={errors.specialty?.message}
            />
          )}
        />

        <AppButton
          testID="submit-button"
          onPress={handleSubmit(onSubmit)}
          loading={isSubmitting}
          style={{ marginTop: 32 }}
        >
          Salvar
        </AppButton>

        {/* ── Aparência ── */}
        <Text style={[s.sectionTitle, { marginTop: 32 }]}>Aparência</Text>
        <View style={s.themeSelector} testID="theme-selector">
          {THEME_OPTIONS.map((opt) => (
            <Pressable
              key={opt.value}
              testID={`theme-option-${opt.value}`}
              style={[s.themeOption, preference === opt.value && s.themeOptionActive]}
              onPress={() => setPreference(opt.value)}
            >
              <Ionicons
                name={opt.icon as keyof typeof Ionicons.glyphMap}
                size={18}
                color={preference === opt.value ? t.colors.text.onDark : t.colors.text.secondary}
              />
              <Text
                style={[
                  s.themeOptionLabel,
                  preference === opt.value && s.themeOptionLabelActive,
                ]}
              >
                {opt.label}
              </Text>
            </Pressable>
          ))}
        </View>

        {/* ── Legal ── */}
        <Text style={[s.sectionTitle, { marginTop: 32 }]}>Legal</Text>
        <Pressable
          testID="privacy-policy-button"
          onPress={() => router.push('/privacy-policy')}
          style={s.legalBtn}
        >
          <Ionicons name="shield-checkmark-outline" size={20} color={t.colors.text.secondary} />
          <Text style={s.legalBtnText}>Política de Privacidade</Text>
          <Ionicons name="chevron-forward" size={16} color={t.colors.text.tertiary} />
        </Pressable>

        {/* ── Sair ── */}
        <Pressable
          testID="logout-button"
          accessibilityLabel="Sair"
          onPress={handleLogoutPress}
          style={s.logoutBtn}
        >
          <Ionicons name="log-out-outline" size={20} color={t.colors.error.base} />
          <Text style={s.logoutBtnText}>Sair da conta</Text>
        </Pressable>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

function makeStyles(t: ReturnType<typeof useAppTheme>) {
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
      fontSize: t.typography.fontSize.lg,
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
    },
    sectionTitle: {
      fontSize: t.typography.fontSize.sm,
      fontWeight: t.typography.fontWeight.semibold,
      color: t.colors.text.secondary,
      textTransform: 'uppercase',
      letterSpacing: 0.5,
      marginBottom: 12,
    },
    themeSelector: {
      flexDirection: 'row',
      gap: 8,
    },
    themeOption: {
      flex: 1,
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 6,
      paddingVertical: 10,
      paddingHorizontal: 12,
      borderRadius: t.borderRadius.md,
      borderWidth: 1,
      borderColor: t.colors.border,
      backgroundColor: t.colors.surface.card,
    },
    themeOptionActive: {
      backgroundColor: t.colors.secondary,
      borderColor: t.colors.secondary,
    },
    themeOptionLabel: {
      fontSize: t.typography.fontSize.sm,
      fontWeight: t.typography.fontWeight.medium,
      color: t.colors.text.secondary,
    },
    themeOptionLabelActive: {
      color: t.colors.text.onDark,
    },
    legalBtn: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 12,
      paddingVertical: 14,
      paddingHorizontal: 16,
      borderRadius: t.borderRadius.md,
      borderWidth: 1,
      borderColor: t.colors.border,
      backgroundColor: t.colors.surface.card,
    },
    legalBtnText: {
      flex: 1,
      fontSize: t.typography.fontSize.md,
      fontWeight: t.typography.fontWeight.medium,
      color: t.colors.text.primary,
    },
    logoutBtn: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 8,
      marginTop: 16,
      paddingVertical: 14,
      borderRadius: t.borderRadius.md,
      borderWidth: 1,
      borderColor: t.colors.error.base,
      backgroundColor: t.colors.error.light,
    },
    logoutBtnText: {
      fontSize: t.typography.fontSize.md,
      fontWeight: t.typography.fontWeight.semibold,
      color: t.colors.error.base,
    },
  });
}
