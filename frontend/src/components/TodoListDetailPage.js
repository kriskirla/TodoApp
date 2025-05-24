import { useCallback, useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { List, ListItem, ListItemText, TextField, Button, Typography, Box } from '@mui/material';
import * as todoApi from '../api/todo';

const TodoListDetailPage = ({ token }) => {
    const { id: listId } = useParams();
    const [list, setList] = useState(null);
    const [newItemTitle, setNewItemTitle] = useState('');
    const [newItemMedia, setNewItemMedia] = useState(null);
    const [error, setError] = useState(null);
    const navigate = useNavigate();
    const fileInputRef = useRef();

    const fetchListDetails = useCallback(async () => {
        setError(null);
        try {
            const data = await todoApi.getList(listId, token);
            setList(data);
        } catch (err) {
            console.error('Failed to fetch list details:', err);
            setError('Failed to fetch list details.');
        }
    }, [listId, token]);

    useEffect(() => {
        fetchListDetails();
    }, [fetchListDetails]);

    const handleBack = () => {
        navigate('/');
    };

    const handleAddItem = async () => {
        if (!newItemTitle.trim()) return;
        setError(null);

        try {
            const itemForm = { description: newItemTitle };
            if (newItemMedia) {
                itemForm.media = newItemMedia;
            }

            await todoApi.addItem(listId, itemForm, token);
            setNewItemTitle('');
            setNewItemMedia(null);
            if (fileInputRef.current) {
                fileInputRef.current.value = '';
            }

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

    if (error) return <Typography color="error">{error}</Typography>;
    if (!list) return null;

    return (
        <Box maxWidth={600} mx="auto" p={2}>
            <Button variant="outlined" onClick={handleBack} sx={{ mb: 2 }}>
                Back
            </Button>

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
                        <Box>
                            <ListItemText primary={item.description} />
                            {item.mediaUrl && item.mediaType === 0 && (
                                <img
                                    src={`http://localhost:5286${item.mediaUrl}`}
                                    alt="media"
                                    style={{ maxWidth: '100px', marginTop: 4 }}
                                />
                            )}
                            {item.mediaUrl && item.mediaType === 1 && (
                                <video controls width="200" style={{ marginTop: 4 }}>
                                    <source
                                        src={`http://localhost:5286${item.mediaUrl}`}
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
                    accept="image/*,video/mp4,video/mov"
                    ref={fileInputRef}
                    onChange={(e) => setNewItemMedia(e.target.files[0] || null)}
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
