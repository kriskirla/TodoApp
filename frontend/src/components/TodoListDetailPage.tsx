import { useCallback, useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { List, ListItem, ListItemText, TextField, Button, Typography, Box } from '@mui/material';
import { toast } from 'material-react-toastify';
import * as todoApi from '../api/todo';
import { useSignalR } from '../hooks/useSignalR';
import { TodoList, ItemForm, TodoItem, MediaType } from '../types';

interface TodoListDetailPageProps {
    token: string;
}

const API_BASE_URL = 'http://localhost:5286';

const TodoListDetailPage = ({ token }: TodoListDetailPageProps) => {
    const { id: listId } = useParams<{ id: string }>();
    const [list, setList] = useState<TodoList | null>(null);
    const [newItemTitle, setNewItemTitle] = useState('');
    const [newItemMedia, setNewItemMedia] = useState<File | null>(null);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();
    const fileInputRef = useRef<HTMLInputElement | null>(null);
    const { connection, joinedGroups } = useSignalR();

    const fetchListDetails = useCallback(async () => {
        setError(null);
        try {
            const data = await todoApi.getList(listId!, token);
            setList(data);
        } catch (err) {
            console.error('Failed to fetch list details:', err);
            setError('Failed to fetch list details.');
        }
    }, [listId, token]);

    useEffect(() => {
        if (!connection) return;

        const joinGroupIfNeeded = async () => {
            try {
                if (!joinedGroups.current.has(listId!)) {
                    await connection.invoke('JoinListGroup', listId);
                    joinedGroups.current.add(listId!);
                }
            } catch (err) {
                console.error('Failed to join list in Detail Page:', err);
            }
        };

        const showToastAndRefresh = (msg: string) => {
            toast.info(msg);
            fetchListDetails();
        };

        const onItemAdded = () => showToastAndRefresh('An item was added');
        const onItemDeleted = () => showToastAndRefresh('An item was deleted');

        connection.on('ItemAdded', onItemAdded);
        connection.on('ItemDeleted', onItemDeleted);

        fetchListDetails();
        joinGroupIfNeeded();

        return () => {
            connection.off('ItemAdded', onItemAdded);
            connection.off('ItemDeleted', onItemDeleted);
        };
    }, [connection, fetchListDetails, joinedGroups, listId]);

    const handleBack = () => {
        navigate('/');
    };

    if (!list) {
        navigate('/');
        return null;
    }

    const handleAddItem = async () => {
        if (!newItemTitle.trim()) return;
        setError(null);

        try {
            const itemForm: ItemForm = { description: newItemTitle };
            if (newItemMedia) {
                itemForm.media = newItemMedia;
            }

            await todoApi.addItem(listId!, itemForm, token);
            setNewItemTitle('');
            setNewItemMedia(null);
            if (fileInputRef.current) {
                fileInputRef.current.value = '';
            }
        } catch (err) {
            console.error('Failed to add item:', err);
            setError('Failed to add item.');
        }
    };

    const handleDeleteItem = async (itemId: string) => {
        setError(null);
        try {
            await todoApi.deleteItem(listId!, itemId, token);
        } catch (err) {
            console.error('Failed to delete item:', err);
            setError('Failed to delete item.');
        }
    };

    return (
        <Box maxWidth={600} mx="auto" p={2}>
            <Button variant="outlined" onClick={handleBack} sx={{ mb: 2 }}>
                Back
            </Button>

            <Typography variant="h4" gutterBottom>
                {list.title}
            </Typography>

            {error && (
                <Typography color="error" gutterBottom>
                    {error}
                </Typography>
            )}

            <List>
                {list.items.map((item: TodoItem) => (
                    <ListItem
                        key={item.id}
                        secondaryAction={
                            <Button color="error" onClick={() => handleDeleteItem(item.id)}>
                                Delete
                            </Button>
                        }
                    >
                        <Box>
                            <ListItemText primary={item.description} />
                            {item.mediaUrl && item.mediaType === MediaType.Image && (
                                <img
                                    src={API_BASE_URL + item.mediaUrl}
                                    alt="media"
                                    style={{ maxWidth: '100px', marginTop: 4 }}
                                />
                            )}
                            {item.mediaUrl && item.mediaType === MediaType.Video && (
                                <video controls width="200" style={{ marginTop: 4 }}>
                                    <source
                                        src={API_BASE_URL + item.mediaUrl}
                                        type={item.mediaUrl.toLowerCase().endsWith('.mov') ? 'video/quicktime' : 'video/mp4'}
                                    />
                                    Your browser does not support the video tag.
                                </video>
                            )}
                        </Box>
                    </ListItem>
                ))}
            </List>

            <Box display="flex" mt={2} alignItems="center" gap={1}>
                <TextField
                    label="New Item"
                    variant="outlined"
                    fullWidth
                    value={newItemTitle}
                    onChange={(e) => setNewItemTitle(e.target.value)}
                    onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                            handleAddItem();
                            e.preventDefault();
                        }
                    }}
                />

                <input
                    type="file"
                    accept="image/*,video/mp4,.mov"
                    ref={fileInputRef}
                    onChange={(e) => setNewItemMedia(e.target.files?.[0] || null)}
                    style={{ cursor: 'pointer' }}
                />

                <Button
                    variant="contained"
                    color="primary"
                    onClick={handleAddItem}
                    sx={{ ml: 1 }}
                    disabled={!newItemTitle.trim()}
                >
                    Add
                </Button>
            </Box>
        </Box>
    );
};

export default TodoListDetailPage;
