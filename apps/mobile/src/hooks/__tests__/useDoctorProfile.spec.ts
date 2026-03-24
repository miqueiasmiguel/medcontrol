import { act, renderHook } from '@testing-library/react-native';
import { UserService } from '../../services/user.service';
import { useDoctorProfile } from '../useDoctorProfile';

jest.mock('../../services/user.service');

const mockUserService = UserService as jest.Mocked<typeof UserService>;

const fakeProfile = {
  id: 'd1',
  tenantId: 't1',
  userId: 'u1',
  name: 'Dr. João',
  crm: '123456',
  councilState: 'SP',
  specialty: 'Cardiologia',
};

beforeEach(() => {
  jest.clearAllMocks();
});

describe('useDoctorProfile', () => {
  it('começa com loading true e doctorProfile null', () => {
    mockUserService.getDoctorProfile.mockResolvedValue(fakeProfile);
    const { result } = renderHook(() => useDoctorProfile());

    expect(result.current.loading).toBe(true);
    expect(result.current.doctorProfile).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('carrega o perfil ao montar', async () => {
    mockUserService.getDoctorProfile.mockResolvedValue(fakeProfile);
    const { result } = renderHook(() => useDoctorProfile());

    await act(() => Promise.resolve());

    expect(result.current.loading).toBe(false);
    expect(result.current.doctorProfile).toEqual(fakeProfile);
    expect(result.current.error).toBeNull();
  });

  it('retorna null quando não há perfil vinculado', async () => {
    mockUserService.getDoctorProfile.mockResolvedValue(null);
    const { result } = renderHook(() => useDoctorProfile());

    await act(() => Promise.resolve());

    expect(result.current.loading).toBe(false);
    expect(result.current.doctorProfile).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it('define error quando getDoctorProfile falha', async () => {
    mockUserService.getDoctorProfile.mockRejectedValue(new Error('Unauthorized'));
    const { result } = renderHook(() => useDoctorProfile());

    await act(() => Promise.resolve());

    expect(result.current.loading).toBe(false);
    expect(result.current.doctorProfile).toBeNull();
    expect(result.current.error).toBe('Unauthorized');
  });

  it('refetch busca novamente o perfil', async () => {
    mockUserService.getDoctorProfile.mockResolvedValue(fakeProfile);
    const { result } = renderHook(() => useDoctorProfile());

    await act(() => Promise.resolve());
    expect(mockUserService.getDoctorProfile).toHaveBeenCalledTimes(1);

    await act(() => result.current.refetch());

    expect(mockUserService.getDoctorProfile).toHaveBeenCalledTimes(2);
  });
});
