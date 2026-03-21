import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { AuthStackParamList } from './types';
import { LoginScreen } from '../screens/auth/LoginScreen';
import { MagicLinkSentScreen } from '../screens/auth/MagicLinkSentScreen';

const Stack = createNativeStackNavigator<AuthStackParamList>();

export function RootNavigator() {
  return (
    <NavigationContainer>
      <Stack.Navigator
        initialRouteName="Login"
        screenOptions={{ headerShown: false }}
      >
        <Stack.Screen name="Login" component={LoginScreen} />
        <Stack.Screen name="MagicLinkSent" component={MagicLinkSentScreen} />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
