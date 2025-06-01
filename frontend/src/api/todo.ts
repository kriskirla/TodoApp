import axios from 'axios';
import { TodoList, ItemForm, Permission, AttributeType, OrderType } from '../types'

const API_BASE_URL = 'http://localhost:5286/api/todo';

// --- Helper: Add Bearer Token and param ---
function helper(token: string, params?: Record<string, string | number>) {
    return {
        headers: { Authorization: `Bearer ${token}` },
        params
    };
}

// --- API Methods ---
export async function createList(list: Partial<TodoList>, token: string): Promise<TodoList> {
    const response = await axios.post<TodoList>(`${API_BASE_URL}/list/create`, list, helper(token));
    return response.data;
}

export async function getList(listId: string, token: string): Promise<TodoList> {
    const response = await axios.get<TodoList>(`${API_BASE_URL}/list/${listId}`, helper(token));
    return response.data;
}

export async function updateList(
    listId: string,
    update: Partial<TodoList>,
    token: string
): Promise<TodoList> {
    const response = await axios.put<TodoList>(`${API_BASE_URL}/list/${listId}`, update, helper(token));
    return response.data;
}

export async function deleteList(listId: string, token: string): Promise<TodoList> {
    const response = await axios.delete<TodoList>(`${API_BASE_URL}/list/${listId}`, helper(token));
    return response.data;
}

export async function addItem(
    listId: string,
    itemForm: ItemForm,
    token: string
): Promise<TodoList> {
    const formData = new FormData();

    formData.append('name', itemForm.name);
    formData.append('description', itemForm.description);

    if (itemForm.dueDate) {
        formData.append('dueDate', itemForm.dueDate.toISOString());
    }

    if (itemForm.status !== undefined) {
        formData.append('status', itemForm.status.toString());
    }

    if (itemForm.priority !== undefined) {
        formData.append('priority', itemForm.priority.toString());
    }

    if (itemForm.media) {
        formData.append('media', itemForm.media);
    }

    const response = await axios.post<TodoList>(
        `${API_BASE_URL}/item/${listId}`,
        formData,
        {
            ...helper(token),
            headers: {
                ...helper(token).headers,
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
): Promise<TodoList> {
    const response = await axios.delete<TodoList>(
        `${API_BASE_URL}/item/${listId}/${itemId}`,
        helper(token)
    );
    return response.data;
}

export async function shareList(
    listId: string,
    userId: string,
    permission: Permission,
    token: string
): Promise<TodoList> {
    const response = await axios.post<TodoList>(
        `${API_BASE_URL}/list/share/${listId}`,
        { userId, permission },
        helper(token)
    );
    return response.data;
}

export async function unshareList(
    listId: string,
    userId: string,
    token: string
): Promise<TodoList> {
    const response = await axios.delete<TodoList>(
        `${API_BASE_URL}/list/unshare/${listId}/${userId}`,
        helper(token)
    );
    return response.data;
}

export async function getAllListsByUser(token: string): Promise<TodoList[]> {
    const response = await axios.get<TodoList[]>(`${API_BASE_URL}/list/user`, helper(token));
    return response.data;
}

export async function filterList(
    listId: string,
    filter: AttributeType,
    key: string,
    token: string
): Promise<TodoList> {
    const response = await axios.get<TodoList>(`${API_BASE_URL}/list/${listId}`,
        helper(token, { filter, key })
    );
    return response.data;
}

export async function sortList(
    listId: string,
    sort: AttributeType,
    order: OrderType,
    token: string): Promise<TodoList> {
    const response = await axios.get<TodoList>(`${API_BASE_URL}/list/${listId}`, 
        helper(token, { sort, order })
    );
    return response.data;
}

export async function sortFilteredList(
    listId: string,
    filter: AttributeType,
    key: string,
    sort: AttributeType,
    order: OrderType,
    token: string): Promise<TodoList> {
    const response = await axios.get<TodoList>(`${API_BASE_URL}/list/${listId}`,
        helper(token, { filter, key, sort, order })
    );
    return response.data;
}
