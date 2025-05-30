import { useState, ChangeEvent, KeyboardEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { TextField, Button, Container, Typography, Stack } from '@mui/material';
import { login } from '../api/auth';
import { AuthenticateUserRequest, LoginResponse } from '../types';

interface LoginPageProps {
    onLogin: (token: string) => void;
}

const LoginPage: React.FC<LoginPageProps> = ({ onLogin }) => {
    const [email, setEmail] = useState('');
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleSubmit = async (): Promise<void> => {
        setError(null);
        try {
            const loginRequest: AuthenticateUserRequest = { email };
            const result: LoginResponse = await login(loginRequest);
            onLogin(result.token);
        } catch (err) {
            console.error('Login failed:', err);
            setError('Login failed. Please try again.');
        }
    };

    const handleChange = (e: ChangeEvent<HTMLInputElement>): void => {
        setEmail(e.target.value);
    };

    return (
        <Container maxWidth="sm">
            <Typography variant="h4" gutterBottom>
                Login
            </Typography>
            <TextField
                label="Email"
                variant="outlined"
                fullWidth
                value={email}
                onChange={handleChange}
                onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        handleSubmit();
                    }
                }}
                margin="normal"
            />
            {error && (
                <Typography color="error" variant="body2" gutterBottom>
                    {error}
                </Typography>
            )}
            <Stack direction="row" spacing={2}>
                <Button variant="contained" color="primary" onClick={handleSubmit}>
                    Login
                </Button>
                <Button variant="outlined" onClick={() => navigate('/register')}>
                    Register
                </Button>
            </Stack>
        </Container>
    );
};

export default LoginPage;
