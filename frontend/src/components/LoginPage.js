import { useState } from 'react';
import { TextField, Button, Container, Typography } from '@mui/material';
import { createUser, login } from '../api/auth';

const LoginPage = ({ onLogin }) => {
    const [email, setEmail] = useState('');
    const [error, setError] = useState(null);

    const handleSubmit = async () => {
        setError(null);
        try {
            // Going to assume there's no registration process
            // If the user doesn't exist, create them
            await createUser(email);

            // Login user
            const data = await login(email);
            const token = data.token || data.Token;
            
            // Store and persist token
            localStorage.setItem('token', token);
            onLogin(token);
        } catch (err) {
            console.error('Authentication failed:', err);
            setError('Authentication failed. Please try again.');
        }
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
                onChange={(e) => setEmail(e.target.value)}
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
