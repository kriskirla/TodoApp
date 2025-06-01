import axios from 'axios';
import { AuthenticateUserRequest, LoginResponse, RegistrationRequest, User } from '../types';

const API_BASE_URL = 'http://localhost:5286/api/authentication';

// --- API Methods ---
export async function login(request: AuthenticateUserRequest): Promise<LoginResponse> {
    const response = await axios.post<LoginResponse>(`${API_BASE_URL}/login`, request);
    return response.data;
}

export async function createUser(request: RegistrationRequest): Promise<User> {
    const response = await axios.post<User>(`${API_BASE_URL}/user/create`, request);
    return response.data;
}

export async function getUserById(id: string): Promise<User> {
    const response = await axios.get<User>(`${API_BASE_URL}/user/${id}`);
    return response.data;
}

export async function getUserByEmail(email: string): Promise<User> {
    const response = await axios.get<User>(`${API_BASE_URL}/user/email`, {
        params: { email }
    });
    return response.data;
}
