import axios from 'axios';

const API_BASE_URL = 'http://localhost:5286/api/authentication';

export async function login(email) {
    const response = await axios.post(`${API_BASE_URL}/login`, { email });
    return response.data; // LoginOutputDto
}

export async function createUser(email) {
    const response = await axios.post(`${API_BASE_URL}/user/create`, null, { params: { email } });
    return response.data; // UserOutputDto
}

export async function getUserById(id) {
    const response = await axios.get(`${API_BASE_URL}/user/${id}`);
    return response.data; // User
}

export async function getUserByEmail(email) {
    const response = await axios.get(`${API_BASE_URL}/user/email/${email}`);
    return response.data; // User
}


/* For reference, the expected response structure:
LoginOutputDto
{
    token: string;
    expiration: DateTime;
}

UserOutputDto
{
    user: User object;
    email: string;
}

User
{
    id: string;
    username: string;
}
*/