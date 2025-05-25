import { useState, ChangeEvent } from 'react';
import { TextField, Button, Container, Typography } from '@mui/material';
import { createUser, login } from '../api/auth';
import { LoginOutputDto  } from '../types';

interface LoginPageProps {
    onLogin: (token: string) => void;
}

const LoginPage: React.FC<LoginPageProps> = ({ onLogin }) => {
    const [email, setEmail] = useState<string>('');
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (): Promise<void> => {
        setError(null);
        try {
            // Create user and login. Backend will handle gracefully if user already exists
            await createUser(email);
            const result: LoginOutputDto = await login(email);
            onLogin(result.token);
        } catch (err) {
            console.error('Authentication failed:', err);
            setError('Authentication failed. Please try again.');
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
                margin="normal"
            />
            {error && (
                <Typography color="error" variant="body2" gutterBottom>
                    {error}
                </Typography>
            )}
            <Button variant="contained" color="primary" onClick={handleSubmit}>
                Submit
            </Button>
        </Container>
    );
};

export default LoginPage;
