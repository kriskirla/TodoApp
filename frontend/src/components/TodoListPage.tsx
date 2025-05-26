import { useEffect, useState, KeyboardEvent, ChangeEvent } from 'react';
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
    ButtonGroup,
    RadioGroup,
    FormControlLabel,
    Radio
} from '@mui/material';
import { toast } from 'material-react-toastify';
import { useNavigate } from 'react-router-dom';
import * as authApi from '../api/auth';
import * as todoApi from '../api/todo';
import { TodoList, User, Permission } from '../types'
import { useSignalR } from '../hooks/useSignalR';

interface TodoListPageProps {
    token: string;
}

const TodoListPage: React.FC<TodoListPageProps> = ({ token }) => {
    const [lists, setLists] = useState<TodoList[]>([]);
    const [newListTitle, setNewListTitle] = useState<string>('');
    const [confirmDeleteOpen, setConfirmDeleteOpen] = useState<boolean>(false);
    const [listIdToDelete, setListIdToDelete] = useState<string | null>(null);
    const [shareDialogOpen, setShareDialogOpen] = useState<boolean>(false);
    const [dialogMode, setDialogMode] = useState<'share' | 'unshare'>('share');
    const [selectedListId, setSelectedListId] = useState<string | null>(null);
    const [shareEmail, setShareEmail] = useState<string>('');
    const [permission, setPermission] = useState<Permission>(Permission.View);
    const [error, setError] = useState<string | null>(null);
    const { connection, joinedGroups } = useSignalR();

    const navigate = useNavigate();

    useEffect(() => {
        if (!connection) return;

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

        const showToastAndRefresh = (msg: string) => {
            toast.info(msg);
            fetchLists();
        };

        const onListCreated = async (list: TodoList) => {
            showToastAndRefresh("A list was created");
            if (!joinedGroups.current.has(list.id)) {
                await connection.invoke("JoinListGroup", list.id.toString());
                joinedGroups.current.add(list.id);
            }
        };

        const onListUpdated = () => showToastAndRefresh("A list was updated");

        const onListDeleted = async (list: TodoList) => {
            showToastAndRefresh("A list was deleted");
            if (joinedGroups.current.has(list.id)) {
                await connection.invoke("LeaveListGroup", list.id.toString());
                joinedGroups.current.delete(list.id);
            }
        };
        const onListShared = async (list: TodoList) => {
            showToastAndRefresh("A list was shared with you");
            if (!joinedGroups.current.has(list.id)) {
                await connection.invoke("JoinListGroup", list.id.toString());
                joinedGroups.current.add(list.id);
            }
        };
        const onListUnshared = async (list: TodoList) => {
            showToastAndRefresh("A list was unshared");
            if (joinedGroups.current.has(list.id)) {
                await connection.invoke("LeaveListGroup", list.id.toString());
                joinedGroups.current.delete(list.id);
            }
        };

        connection.on("ListCreated", onListCreated);
        connection.on("ListUpdated", onListUpdated);
        connection.on("ListDeleted", onListDeleted);
        connection.on("ListShared", onListShared);
        connection.on("ListUnshared", onListUnshared);

        (async () => {
            const data = await todoApi.getAllListsByUser(token);
            setLists(data);
            for (const list of data) {
                if (!joinedGroups.current.has(list.id)) {
                    await connection.invoke("JoinListGroup", list.id.toString());
                    joinedGroups.current.add(list.id);
                }
            }
        })();

        return () => {
            connection.off("ListCreated", onListCreated);
            connection.off("ListUpdated", onListUpdated);
            connection.off("ListDeleted", onListDeleted);
            connection.off("ListShared", onListShared);
            connection.off("ListUnshared", onListUnshared);
        };
    }, [connection, joinedGroups, token]);

    const handleCreateList = async () => {
        if (!newListTitle.trim()) return;
        setError(null);
        try {
            await todoApi.createList({ title: newListTitle }, token);
            setNewListTitle('');
        } catch (err) {
            console.error('Failed to create list:', err);
            setError('Failed to create list.');
        }
    };

    const handleDeleteList = async (listId: string | null) => {
        if (listId === null) return;
        setError(null);
        try {
            await todoApi.deleteList(listId, token);
        } catch (err) {
            console.error('Failed to delete list:', err);
            setError('Failed to delete list.');
        }
    };

    const handleShareList = async () => {
        if (!shareEmail.trim() || selectedListId === null) return;
        setError(null);
        try {
            const user: User = await authApi.getUserByEmail(shareEmail);

            await todoApi.shareList(selectedListId, user.id, permission, token);

            setShareDialogOpen(false);
            setShareEmail('');
            setPermission(Permission.View);
        } catch (err) {
            console.error('Failed to share list:', err);
            setError('Failed to share list.');
        }
    };

    const handleUnshareList = async () => {
        if (!shareEmail.trim() || selectedListId === null) return;
        setError(null);
        try {
            const user: User = await authApi.getUserByEmail(shareEmail);

            await todoApi.unshareList(selectedListId, user.id, token);

            setShareDialogOpen(false);
            setShareEmail('');
        } catch (err) {
            console.error('Failed to unshare list:', err);
            setError('Failed to unshare list.');
        }
    };

    const handleViewDetails = (listId: string) => {
        navigate(`/list/${listId}`);
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
                                <Button
                                    color="primary"
                                    onClick={() => handleViewDetails(list.id)}
                                    sx={{ mr: 1 }}
                                >
                                    View
                                </Button>
                                <Button
                                    color="error"
                                    onClick={() => {
                                        setListIdToDelete(list.id);
                                        setConfirmDeleteOpen(true);
                                    }}
                                    sx={{ mr: 1 }}
                                >
                                    Delete
                                </Button>
                                <ButtonGroup variant="outlined" color="secondary" sx={{ mr: 1 }}>
                                    <Button
                                        onClick={() => {
                                            setSelectedListId(list.id);
                                            setShareEmail('');
                                            setDialogMode('share');
                                            setShareDialogOpen(true);
                                        }}
                                    >
                                        Share
                                    </Button>
                                    <Button
                                        onClick={() => {
                                            setSelectedListId(list.id);
                                            setShareEmail('');
                                            setDialogMode('unshare');
                                            setShareDialogOpen(true);
                                        }}
                                    >
                                        Unshare
                                    </Button>
                                </ButtonGroup>
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
                    onChange={(e: ChangeEvent<HTMLInputElement>) => setNewListTitle(e.target.value)}
                    onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
                        if (e.key === 'Enter') {
                            handleCreateList();
                            e.preventDefault();
                        }
                    }}
                />
                <Button
                    variant="contained"
                    color="primary"
                    disabled={!newListTitle.trim()}
                    onClick={handleCreateList}>
                    Create
                </Button>
            </Box>

            <Dialog open={confirmDeleteOpen} onClose={() => setConfirmDeleteOpen(false)}>
                <DialogTitle>Confirm Delete</DialogTitle>
                <DialogContent>
                    <Typography>Are you sure you want to delete this list?</Typography>
                </DialogContent>
                <DialogActions>
                    <Button
                        onClick={async () => {
                            await handleDeleteList(listIdToDelete);
                            setConfirmDeleteOpen(false);
                        }}
                        color="error"
                        variant="contained"
                    >
                        Delete
                    </Button>
                    <Button onClick={() => setConfirmDeleteOpen(false)}>Cancel</Button>
                </DialogActions>
            </Dialog>

            <Dialog open={shareDialogOpen} onClose={() => setShareDialogOpen(false)}>
                <DialogTitle>{dialogMode === 'share' ? 'Share List' : 'Unshare List'}</DialogTitle>
                <DialogContent>
                    <TextField
                        label="User Email"
                        variant="outlined"
                        value={shareEmail}
                        onChange={(e: ChangeEvent<HTMLInputElement>) => setShareEmail(e.target.value)}
                        fullWidth
                        autoFocus
                        onKeyDown={(e: KeyboardEvent<HTMLInputElement>) => {
                            if (e.key === 'Enter') {
                                if (dialogMode === 'share') {
                                    handleShareList();
                                } else {
                                    handleUnshareList();
                                }
                                e.preventDefault();
                            }
                        }}
                    />
                    {dialogMode === 'share' && (
                        <>
                            <Typography sx={{ mt: 2, mb: 1 }}>Permission</Typography>
                            <RadioGroup
                                row
                                value={permission}
                                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                                    setPermission(Number(e.target.value) as Permission)
                                }
                            >
                                <FormControlLabel
                                    value={Permission.View}
                                    control={<Radio />}
                                    label="View"
                                />
                                <FormControlLabel
                                    value={Permission.Edit}
                                    control={<Radio />}
                                    label="Edit"
                                />
                            </RadioGroup>
                        </>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button
                        onClick={dialogMode === 'share' ? handleShareList : handleUnshareList}
                        variant="contained"
                        color="primary"
                        disabled={!shareEmail.trim()}
                        sx={{ mr: 1 }}
                    >
                        {dialogMode === 'share' ? 'Share' : 'Unshare'}
                    </Button>
                    <Button onClick={() => setShareDialogOpen(false)}>Cancel</Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default TodoListPage;
