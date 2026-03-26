import React, { useState } from 'react';
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
import { useRouter } from 'expo-router';
import { useForm, Controller } from 'react-hook-form';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { useAppTheme } from '../../contexts/ThemeContext';
import { UserService } from '../../services/user.service';
import { AppTextInput } from '../../components/ui/AppTextInput';
import { AppButton } from '../../components/ui/AppButton';

interface OnboardingForm {
  name: string;
  crm: string;
  councilState: string;
  specialty: string;
}

export default function DoctorOnboardingScreen() {
  const t = useAppTheme();
  const insets = useSafeAreaInsets();
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<OnboardingForm>({
    defaultValues: { name: '', crm: '', councilState: '', specialty: '' },
  });

  async function onSubmit(data: OnboardingForm) {
    setIsSubmitting(true);
    try {
      await UserService.createMyDoctorProfile(data);
      router.replace('/(app)');
    } catch (e) {
      Alert.alert('Erro', e instanceof Error ? e.message : 'Erro ao salvar perfil. Tente novamente.');
    } finally {
      setIsSubmitting(false);
    }
  }

  async function skip() {
    await AsyncStorage.setItem('mmc_onboarding_skip', '1');
    router.replace('/(app)');
  }

  const s = makeStyles(t);

  return (
    <KeyboardAvoidingView
      style={{ flex: 1 }}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <View style={[s.header, { paddingTop: insets.top + 16 }]}>
        <Text style={s.headerTitle}>Complete seu perfil médico</Text>
        <Text style={s.headerSubtitle}>
          Preencha seus dados para acessar pagamentos e relatórios.
        </Text>
      </View>

      <ScrollView
        style={{ flex: 1, backgroundColor: t.colors.surface.background }}
        contentContainerStyle={[s.content, { paddingBottom: insets.bottom + 24 }]}
        keyboardShouldPersistTaps="handled"
      >
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
          Concluir cadastro
        </AppButton>

        <Pressable testID="skip-button" onPress={skip} style={s.skipBtn}>
          <Text style={s.skipText}>Fazer depois</Text>
        </Pressable>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

function makeStyles(t: ReturnType<typeof useAppTheme>) {
  return StyleSheet.create({
    header: {
      backgroundColor: t.colors.secondary,
      paddingHorizontal: 24,
      paddingBottom: 24,
    },
    headerTitle: {
      fontSize: t.typography.fontSize.lg,
      fontWeight: t.typography.fontWeight.bold,
      color: t.colors.text.onDark,
      marginBottom: 8,
    },
    headerSubtitle: {
      fontSize: t.typography.fontSize.sm,
      color: t.colors.text.onDarkSubtle,
    },
    content: {
      padding: 16,
    },
    skipBtn: {
      alignItems: 'center',
      marginTop: 16,
      paddingVertical: 12,
    },
    skipText: {
      fontSize: t.typography.fontSize.sm,
      color: t.colors.text.secondary,
    },
  });
}
