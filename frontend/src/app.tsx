import { useState, useEffect } from 'react';
import {
    BrowserRouter as Router,
    Routes,
    Route,
    Navigate,
} from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import { ToastContainer } from 'material-react-toastify';
import 'material-react-toastify/dist/ReactToastify.css';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import TodoListPage from './components/TodoListPage';
import TodoDetailPage from './components/TodoListDetailPage';
import * as authApi from './api/auth';
import { SignalRProvider } from './contexts/SignalRContext';

interface JwtPayload {
    exp: number;
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string;
}

const isTokenExpired = (token: string | null): boolean => {
    if (!token) return true;

    try {
        const { exp } = jwtDecode<JwtPayload>(token);
        return !exp || Date.now() >= exp * 1000;
    } catch {
        return true;
    }
};

const App = () => {
    const [token, setToken] = useState<string | null>(localStorage.getItem('token'));

    useEffect(() => {
        const validateTokenAndUser = async () => {
            if (!token || isTokenExpired(token)) {
                handleLogout();
                return;
            }

            try {
                const decoded = jwtDecode<JwtPayload>(token);
                const userId =
                    decoded[
                    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
                    ];
                await authApi.getUserById(userId);
            } catch (err) {
                console.error('Token invalid or user not found:', err);
                handleLogout();
            }
        };

        validateTokenAndUser();
    }, [token]);

    const handleLogout = () => {
        if (document.activeElement instanceof HTMLElement) {
            document.activeElement.blur();
        }

        localStorage.removeItem('token');
        setToken(null);
    };

    const onLoginSuccess = (newToken: string) => {
        if (document.activeElement instanceof HTMLElement) {
            document.activeElement.blur();
        }

        localStorage.setItem('token', newToken);
        setToken(newToken);
    };

    return (
        <SignalRProvider token={token!}>
            <Router>
                <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
                <Routes>
                    {!token ? (
                        <>
                            <Route path="/" element={<LoginPage onLogin={onLoginSuccess} />} />
                            <Route path="/register" element={<RegisterPage />} />
                            <Route path="*" element={<Navigate to="/" replace />} />
                        </>
                    ) : (
                        <>
                            <Route path="/" element={<TodoListPage token={token} />} />
                            <Route path="/list/:id" element={<TodoDetailPage token={token} />} />
                            <Route path="*" element={<Navigate to="/" replace />} />
                        </>
                    )}
                </Routes>
            </Router>
        </SignalRProvider>
    );
};

export default App;
