import { useEffect, useState } from 'react';
import { List, ListItem, ListItemText, TextField, Button, Typography, Box } from '@mui/material';
import * as todoApi from '../api/todo';

const TodoListDetailPage = ({ token, listId }) => {
    const [list, setList] = useState(null);
    const [newItemTitle, setNewItemTitle] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchListDetails();
    }, [listId]);

    const fetchListDetails = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await todoApi.getList(listId, token);
            setList(data);
        } catch (err) {
            console.error('Failed to fetch list details:', err);
            setError('Failed to fetch list details.');
        } finally {
            setLoading(false);
        }
    };

    const handleAddItem = async () => {
        if (!newItemTitle.trim()) return;
        setError(null);
        try {
            await todoApi.addItem(listId, { title: newItemTitle }, token);
            setNewItemTitle('');
            fetchListDetails();
        } catch (err) {
            console.error('Failed to add item:', err);
            setError('Failed to add item.');
        }
    };

    const handleDeleteItem = async (itemId) => {
        setError(null);
        try {
            await todoApi.deleteItem(listId, itemId, token);
            fetchListDetails();
        } catch (err) {
            console.error('Failed to delete item:', err);
            setError('Failed to delete item.');
        }
    };

    if (loading) return <div>Loading...</div>;
    if (error) return <Typography color="error">{error}</Typography>;
    if (!list) return null;

    return (
        <Box maxWidth={600} mx="auto" p={2}>
            <Typography variant="h4" gutterBottom>
                {list.title}
            </Typography>

            <List>
                {list.items.map((item) => (
                    <ListItem
                        key={item.id}
                        secondaryAction={
                            <Button color="error" onClick={() => handleDeleteItem(item.id)}>
                                Delete
                            </Button>
                        }
                    >
                        <ListItemText primary={item.title} />
                    </ListItem>
                ))}
            </List>

            <Box display="flex" mt={2}>
                <TextField
                    label="New Item"
                    variant="outlined"
                    fullWidth
                    value={newItemTitle}
                    onChange={(e) => setNewItemTitle(e.target.value)}
                    onKeyPress={(e) => {
                        if (e.key === 'Enter') {
                            handleAddItem();
                            e.preventDefault();
                        }
                    }}
                />
                <Button variant="contained" color="primary" onClick={handleAddItem} sx={{ ml: 1 }}>
                    Add Item
                </Button>
            </Box>
        </Box>
    );
};

export default TodoListDetailPage;
