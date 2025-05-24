import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import LoginPage from './components/LoginPage';
import TodoListPage from './components/TodoListPage';
import TodoDetailPage from './components/TodoListDetailPage';
import * as authApi from './api/auth'; // Make sure this path matches your structure

const isTokenExpired = (token) => {
    if (!token) return true;

    try {
        const { exp } = jwtDecode(token);
        if (!exp) return true;
        return Date.now() >= exp * 1000;
    } catch {
        return true;
    }
};

const App = () => {
    const [token, setToken] = useState(localStorage.getItem('token'));

    useEffect(() => {
        const validateTokenAndUser = async () => {
            if (!token || isTokenExpired(token)) {
                handleLogout();
                return;
            }

            try {
                const decoded = jwtDecode(token);
                const userId = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
                await authApi.getUserById(userId);
            } catch (err) {
                console.error('Token invalid or user not found:', err);
                handleLogout();
            }
        };

        const handleLogout = () => {
            localStorage.removeItem('token');
            setToken(null);
        };

        validateTokenAndUser();
    }, [token]);

    if (!token) {
        return <LoginPage onLogin={setToken} />;
    }

    return (
        <Router>
            <Routes>
                <Route path="/" element={<TodoListPage token={token} />} />
                <Route path="/list/:id" element={<TodoDetailPage token={token} />} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </Router>
    );
};

export default App;
