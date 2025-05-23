import { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import LoginPage from './components/LoginPage';
import TodoListPage from './components/TodoListPage';
import TodoDetailPage from './components/TodoListDetailPage';

const App = () => {
    const [token, setToken] = useState(localStorage.getItem('token'));

    if (!token) {
        return <LoginPage onLogin={setToken} />;
    }

    return (
        <Router>
            <Routes>
                <Route path="/" element={<TodoListPage token={token} />} />
                <Route path="/list/:id" element={<TodoDetailPage token={token} />} />
            </Routes>
        </Router>
    );
};

export default App;
