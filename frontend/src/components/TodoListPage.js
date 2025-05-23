import { useEffect, useState } from 'react';
import {
    List,
    ListItem,
    ListItemText,
    Button,
    TextField,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Typography,
    Box,
} from '@mui/material';
import * as todoApi from '../api/todo';
import * as authApi from '../api/auth';

const TodoListPage = ({ token }) => {
    const [lists, setLists] = useState([]);
    const [newListTitle, setNewListTitle] = useState('');
    const [shareDialogOpen, setShareDialogOpen] = useState(false);
    const [selectedListId, setSelectedListId] = useState(null);
    const [shareEmail, setShareEmail] = useState('');
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchLists();
    }, []);

    const fetchLists = async () => {
        setError(null);
        try {
            const data = await todoApi.getAllListsByUser(token);
            setLists(data);
        } catch (err) {
            console.error('Failed to fetch lists:', err);
            setError('Failed to fetch lists.');
        }
    };

    const handleCreateList = async () => {
        if (!newListTitle.trim()) return;
        setError(null);
        try {
            await todoApi.createList({ title: newListTitle }, token);
            setNewListTitle('');
            fetchLists();
        } catch (err) {
            console.error('Failed to create list:', err);
            setError('Failed to create list.');
        }
    };

    const handleDeleteList = async (listId) => {
        setError(null);
        try {
            await todoApi.deleteList(listId, token);
            fetchLists();
        } catch (err) {
            console.error('Failed to delete list:', err);
            setError('Failed to delete list.');
        }
    };

    const handleShareList = async () => {
        if (!shareEmail.trim()) return;
        setError(null);
        try {
            // Get user by email
            const user = await authApi.getUserByEmail(shareEmail);
            const userId = user.id;

            // Share list with user (assuming permission is always 'View')
            await todoApi.shareList(selectedListId, userId, token);

            setShareDialogOpen(false);
            setShareEmail('');
        } catch (err) {
            console.error('Failed to share list:', err);
            setError('Failed to share list.');
        }
    };

    return (
        <Box maxWidth={600} mx="auto" p={2}>
            <Typography variant="h4" gutterBottom>
                Your Todo Lists
            </Typography>

            {error && (
                <Typography color="error" gutterBottom>
                    {error}
                </Typography>
            )}

            <List>
                {lists.map((list) => (
                    <ListItem
                        key={list.id}
                        secondaryAction={
                            <>
                                <Button color="error" onClick={() => handleDeleteList(list.id)}>
                                    Delete
                                </Button>
                                <Button
                                    onClick={() => {
                                        setSelectedListId(list.id);
                                        setShareDialogOpen(true);
                                    }}
                                >
                                    Share
                                </Button>
                            </>
                        }
                    >
                        <ListItemText primary={list.title} />
                    </ListItem>
                ))}
            </List>

            <Box display="flex" mt={2} gap={1}>
                <TextField
                    label="New List Title"
                    variant="outlined"
                    fullWidth
                    value={newListTitle}
                    onChange={(e) => setNewListTitle(e.target.value)}
                    onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                            handleCreateList();
                            e.preventDefault();
                        }
                    }}
                />
                <Button variant="contained" color="primary" onClick={handleCreateList}>
                    Create List
                </Button>
            </Box>

            <Dialog open={shareDialogOpen} onClose={() => setShareDialogOpen(false)}>
                <DialogTitle>Share List</DialogTitle>
                <DialogContent>
                    <TextField
                        label="User Email"
                        variant="outlined"
                        value={shareEmail}
                        onChange={(e) => setShareEmail(e.target.value)}
                        fullWidth
                        autoFocus
                        onKeyDown={(e) => {
                            if (e.key === 'Enter') {
                                handleShareList();
                                e.preventDefault();
                            }
                        }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleShareList} variant="contained" color="primary">
                        Share
                    </Button>
                    <Button onClick={() => setShareDialogOpen(false)}>Cancel</Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default TodoListPage;
