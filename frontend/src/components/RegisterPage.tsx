import { useState, ChangeEvent, KeyboardEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { TextField, Button, Container, Typography } from '@mui/material';
import { createUser } from '../api/auth';
import { RegistrationRequest } from '../types';

const RegisterPage: React.FC = () => {
    const [email, setEmail] = useState('');
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleRegister = async () => {
        setError(null);
        try {
            const registerRequest: RegistrationRequest = { email };
            await createUser(registerRequest);
            // For now my design is to route back to login page after registration
            navigate('/');
        } catch (err) {
            console.error('Registration failed:', err);
            setError('Registration failed. Please try again.');
        }
    };

    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        setEmail(e.target.value);
    };

    return (
        <Container maxWidth="sm">
            <Typography variant="h4" gutterBottom>
                Register
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
                        handleRegister();
                    }
                }}
                margin="normal"
            />
            {error && (
                <Typography color="error" variant="body2" gutterBottom>
                    {error}
                </Typography>
            )}
            <Button variant="contained" color="primary" onClick={handleRegister}>
                Register
            </Button>
        </Container>
    );
};

export default RegisterPage;
