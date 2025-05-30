import axios from 'axios';
import { LoginResponse, User } from '../types';

const API_BASE_URL = 'http://localhost:5286/api/authentication';

// --- API Methods ---
export async function login(email: string): Promise<LoginResponse> {
    const response = await axios.post<LoginResponse>(`${API_BASE_URL}/login`, { email });
    return response.data;
}

export async function createUser(email: string): Promise<User> {
    const response = await axios.post<User>(`${API_BASE_URL}/user/create`, null, {
        params: { email },
    });
    return response.data;
}

export async function getUserById(id: string): Promise<User> {
    const response = await axios.get<User>(`${API_BASE_URL}/user/${id}`);
    return response.data;
}

export async function getUserByEmail(email: string): Promise<User> {
    const response = await axios.get<User>(`${API_BASE_URL}/user/email/${email}`);
    return response.data;
}
