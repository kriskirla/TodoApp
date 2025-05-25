import axios from 'axios';
import { TodoListOutputDto, TodoList, ItemForm, Permission } from '../types'

const API_BASE_URL = 'http://localhost:5286/api/todo';

// --- Helper: Add Bearer Token ---
function authHeaders(token: string) {
    return { headers: { Authorization: `Bearer ${token}` } };
}

// --- API Methods ---
export async function createList(list: Partial<TodoList>, token: string): Promise<TodoListOutputDto> {
    const response = await axios.post<TodoListOutputDto>(`${API_BASE_URL}/list/create`, list, authHeaders(token));
    return response.data;
}

export async function getList(listId: string, token: string): Promise<TodoList> {
    const response = await axios.get<TodoList>(`${API_BASE_URL}/list/${listId}`, authHeaders(token));
    return response.data;
}

export async function updateList(
    listId: string,
    update: Partial<TodoList>,
    token: string
): Promise<TodoListOutputDto> {
    const response = await axios.put<TodoListOutputDto>(`${API_BASE_URL}/list/${listId}`, update, authHeaders(token));
    return response.data;
}

export async function deleteList(listId: string, token: string): Promise<TodoListOutputDto> {
    const response = await axios.delete<TodoListOutputDto>(`${API_BASE_URL}/list/${listId}`, authHeaders(token));
    return response.data;
}

export async function addItem(
    listId: string,
    itemForm: ItemForm,
    token: string
): Promise<TodoListOutputDto> {
    const formData = new FormData();
    formData.append('description', itemForm.description);
    if (itemForm.media) {
        formData.append('media', itemForm.media);
    }

    const response = await axios.post<TodoListOutputDto>(
        `${API_BASE_URL}/item/${listId}`,
        formData,
        {
            ...authHeaders(token),
            headers: {
                ...authHeaders(token).headers,
                'Content-Type': 'multipart/form-data',
            },
        }
    );
    return response.data;
}

export async function deleteItem(
    listId: string,
    itemId: string,
    token: string
): Promise<TodoListOutputDto> {
    const response = await axios.delete<TodoListOutputDto>(
        `${API_BASE_URL}/item/${listId}/${itemId}`,
        authHeaders(token)
    );
    return response.data;
}

export async function shareList(
    listId: string,
    userId: string,
    permission: Permission,
    token: string
): Promise<TodoListOutputDto> {
    const response = await axios.post<TodoListOutputDto>(
        `${API_BASE_URL}/share/${listId}`,
        { userId, permission },
        authHeaders(token)
    );
    return response.data;
}

export async function unshareList(
    listId: string,
    userId: string,
    token: string
): Promise<TodoListOutputDto> {
    const response = await axios.post<TodoListOutputDto>(
        `${API_BASE_URL}/unshare/${listId}`,
        { userId },
        authHeaders(token)
    );
    return response.data;
}

export async function getAllListsByUser(token: string): Promise<TodoList[]> {
    const response = await axios.get<TodoList[]>(`${API_BASE_URL}/list/user`, authHeaders(token));
    return response.data;
}
