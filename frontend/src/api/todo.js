import axios from 'axios';

const API_BASE_URL = 'http://localhost:5286/api/todo';

// Helper: attach Bearer token to headers
function authHeaders(token) {
    return { headers: { Authorization: `Bearer ${token}` } };
}

export async function createList(list, token) {
    const response = await axios.post(`${API_BASE_URL}/list/create`, list, authHeaders(token));
    return response.data; // TodoListOutputDto
}

export async function getList(listId, token) {
    const response = await axios.get(`${API_BASE_URL}/list/${listId}`, authHeaders(token));
    return response.data; // TodoList
}

export async function updateList(listId, update, token) {
    const response = await axios.put(`${API_BASE_URL}/list/${listId}`, update, authHeaders(token));
    return response.data; // TodoListOutputDto
}

export async function deleteList(listId, token) {
    const response = await axios.delete(`${API_BASE_URL}/list/${listId}`, authHeaders(token));
    return response.data; // TodoListOutputDto
}

export async function addItem(listId, itemForm, token) {
    const formData = new FormData();
    for (const key in itemForm) {
        formData.append(key, itemForm[key]);
    }
    const response = await axios.post(`${API_BASE_URL}/item/${listId}`, formData, {
        ...authHeaders(token),
        headers: {
            ...authHeaders(token).headers,
            'Content-Type': 'multipart/form-data'
        }
    });
    return response.data; // TodoListOutputDto
}

export async function deleteItem(listId, itemId, token) {
    const response = await axios.delete(`${API_BASE_URL}/item/${listId}/${itemId}`, authHeaders(token));
    return response.data; // TodoListOutputDto
}

export async function shareList(listId, userId, permission, token) {
    const response = await axios.post(
        `${API_BASE_URL}/share/${listId}`,
        { userId, permission },
        authHeaders(token)
    );
    return response.data; // TodoListOutputDto
}


export async function unshareList(listId, userId, token) {
    const response = await axios.post(`${API_BASE_URL}/unshare/${listId}`, { userId }, authHeaders(token));
    return response.data; // TodoListOutputDto
}

export async function getAllListsByUser(token) {
    const response = await axios.get(`${API_BASE_URL}/list/user`, authHeaders(token));
    return response.data; // IEnumerable<TodoList>
}

/* For reference, the expected response structure:
TodoListOutputDto
{
    list: TodoList,
    items: TodoItem,
    Message: string,
    Success: boolean
}

TodoList
{
    id: Guid;
    title: string;
    ownerId: Guid;
    items: TodoItem[];
    sharedWith: User[];
}

TodoItem
{
    id: string,
    description: string,
    mediaUrl: string,
    mediaType: string,
    todoListId: Guid
}

TodoListShare
{
    id: Guid,
    todoListId: Guid,
    shareWithUserId: Guid,
    permission: string // 'View' or 'Edit'
}
*/