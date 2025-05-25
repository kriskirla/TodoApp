import ReactDOM from 'react-dom/client';
import App from './app';
import CssBaseline from '@mui/material/CssBaseline';
import { ThemeProvider, createTheme } from '@mui/material/styles';

const theme = createTheme({
    palette: {
        mode: 'light',
        primary: {
            main: '#1976d2',
        },
        secondary: {
            main: '#dc004e',
        },
    },
});

const rootElement = document.getElementById('root');

if (!rootElement) {
    throw new Error("Failed to find the root element");
}

const root = ReactDOM.createRoot(rootElement);

root.render(
    <ThemeProvider theme={theme}>
        <CssBaseline />
        <App />
    </ThemeProvider>
);

