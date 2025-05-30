import {
    useCallback,
    useEffect,
    useState,
    useRef
} from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    TextField,
    Button,
    Typography,
    Box,
    Table,
    TableHead,
    TableRow,
    TableCell,
    TableBody,
    Select,
    MenuItem,
    FormControl,
    InputLabel,
    Dialog,
    DialogContent,
    DialogTitle,
    DialogActions
} from '@mui/material';
import { toast } from 'material-react-toastify';
import * as todoApi from '../api/todo';
import { useSignalR } from '../hooks/useSignalR';
import {
    TodoList,
    ItemForm,
    MediaType,
    AttributeType,
    OrderType,
    StatusType,
    PriorityType
} from '../types';
import { StatusTypeLabel, PriorityTypeLabel } from '../helpers/enums';

interface TodoListDetailPageProps {
    token: string;
}

const API_BASE_URL = 'http://localhost:5286';

const TodoListDetailPage = ({ token }: TodoListDetailPageProps) => {
    const { id: listId } = useParams<{ id: string }>();
    const [list, setList] = useState<TodoList | null>(null);
    const [newItemName, setNewItemName] = useState('');
    const [newItemDescription, setNewItemDescription] = useState('');
    const [newItemDueDate, setNewItemDueDate] = useState<string | null>(null);
    const [newItemStatus, setNewItemStatus] = useState<StatusType>(StatusType.NotStarted);
    const [newItemPriority, setNewItemPriority] = useState<PriorityType>(PriorityType.Low);
    const [newItemMedia, setNewItemMedia] = useState<File | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [filterAttribute, setFilterAttribute] = useState<AttributeType | ''>('');
    const [filterKey, setFilterKey] = useState('');
    const [sortAttribute, setSortAttribute] = useState<AttributeType | null>(null);
    const [sortOrder, setSortOrder] = useState<OrderType>(OrderType.Ascending);
    const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
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

    useEffect(() => {
        const handleFilter = async () => {
            try {
                if (filterAttribute && filterKey !== '') {
                    let valueToUse: string = filterKey;

                    if (
                        filterAttribute === AttributeType.Status ||
                        filterAttribute === AttributeType.Priority
                    ) {
                        valueToUse = String(Number(filterKey));
                    }

                    const filtered = await todoApi.filterList(
                        listId!,
                        filterAttribute,
                        valueToUse,
                        token
                    );
                    setList(filtered);
                    return;
                }
                fetchListDetails();
            } catch (err) {
                console.error('Failed to filter list:', err);
                setError('Failed to filter list.');
            }
        }
        handleFilter();
    }, [fetchListDetails, filterAttribute, filterKey, listId, token]);

    const handleBack = () => navigate('/');

    if (!list) {
        navigate('/');
        return null;
    }

    const handleAddItem = async () => {
        if (!newItemName.trim()) return;
        setError(null);

        try {
            const itemForm: ItemForm = {
                name: newItemName,
                description: newItemDescription,
                dueDate: newItemDueDate ? new Date(newItemDueDate) : undefined,
                status: newItemStatus,
                priority: newItemPriority
            };

            if (newItemMedia) {
                itemForm.media = newItemMedia;
            }

            await todoApi.addItem(listId!, itemForm, token);

            // Reset form and close dialog
            setNewItemName('');
            setNewItemDescription('');
            setNewItemDueDate(null);
            setNewItemStatus(StatusType.NotStarted);
            setNewItemPriority(PriorityType.Low);
            setNewItemMedia(null);
            setIsAddDialogOpen(false);
            if (fileInputRef.current) fileInputRef.current.value = '';
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

    const handleSort = async (sort: AttributeType) => {
        const newOrder = sortAttribute === sort && sortOrder === OrderType.Ascending ? OrderType.Descending : OrderType.Ascending;
        setSortAttribute(sort);
        setSortOrder(newOrder);
        try {
            if (filterAttribute !== '' && filterKey !== '') {
                const sorted = await todoApi.sortFilteredList(
                    listId!,
                    filterAttribute as AttributeType,
                    filterKey,
                    sort as AttributeType,
                    newOrder as OrderType,
                    token
                );
                setList(sorted);
            } else {
                const sorted = await todoApi.sortList(listId!, sort, newOrder, token);
                setList(sorted);
            }
        } catch (err) {
            console.error('Failed to sort list:', err);
            setError('Failed to sort list.');
        }
    };

    const handleResetFilter = async () => {
        setFilterAttribute('');
        setFilterKey('');
        fetchListDetails();
    }

    const formatDate = (date: string | Date): string => new Date(date).toISOString();

    return (
        <Box maxWidth={1000} mx="auto" p={2}>
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

            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                <Box display="flex" gap={2}>
                    <FormControl sx={{ minWidth: 160 }}>
                        <InputLabel>Filter By</InputLabel>
                        <Select
                            value={filterAttribute}
                            label="Filter By"
                            onChange={(e) => setFilterAttribute(e.target.value as AttributeType)}
                        >
                            <MenuItem value={AttributeType.Name}>Name</MenuItem>
                            <MenuItem value={AttributeType.Description}>Description</MenuItem>
                            <MenuItem value={AttributeType.DueDate}>Due Date</MenuItem>
                            <MenuItem value={AttributeType.Status}>Status</MenuItem>
                            <MenuItem value={AttributeType.Priority}>Priority</MenuItem>
                        </Select>
                    </FormControl>

                    {filterAttribute === AttributeType.Status ? (
                        <FormControl>
                            <InputLabel>Status</InputLabel>
                            <Select
                                value={filterKey}
                                onChange={(e) => setFilterKey(e.target.value)}
                                label="Status"
                                sx={{ minWidth: 120 }}
                            >
                                <MenuItem value={StatusType.NotStarted}>Not Started</MenuItem>
                                <MenuItem value={StatusType.InProgress}>In Progress</MenuItem>
                                <MenuItem value={StatusType.Completed}>Completed</MenuItem>
                            </Select>
                        </FormControl>
                    ) : filterAttribute === AttributeType.Priority ? (
                        <FormControl>
                            <InputLabel>Priority</InputLabel>
                            <Select
                                value={filterKey}
                                onChange={(e) => setFilterKey(e.target.value)}
                                label="Priority"
                                sx={{ minWidth: 120 }}
                            >
                                <MenuItem value={PriorityType.Low}>Low</MenuItem>
                                <MenuItem value={PriorityType.Medium}>Medium</MenuItem>
                                <MenuItem value={PriorityType.High}>High</MenuItem>
                                <MenuItem value={PriorityType.Critical}>Critical</MenuItem>
                            </Select>
                        </FormControl>
                    ) : filterAttribute === AttributeType.DueDate ? (
                        <TextField
                            type="date"
                            label="Due Date"
                            InputLabelProps={{ shrink: true }}
                            value={filterKey}
                            onChange={(e) => setFilterKey(e.target.value)}
                        />
                    ) : (
                        <TextField
                            label="Filter Value"
                            value={filterKey}
                            onChange={(e) => setFilterKey(e.target.value)}
                            onBlur={() => { }}
                        />
                    )}

                    <Button variant="outlined" color="error" onClick={handleResetFilter}>
                        Reset
                    </Button>
                </Box>

                <Button variant="contained" onClick={() => setIsAddDialogOpen(true)}>
                    Add Item
                </Button>
            </Box>

            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell
                            onClick={() => handleSort(AttributeType.Name)}
                            sx={{ cursor: 'pointer' }}
                        >
                            Name
                        </TableCell>
                        <TableCell
                            onClick={() => handleSort(AttributeType.Description)}
                            sx={{ cursor: 'pointer' }}
                        >
                            Description
                        </TableCell>
                        <TableCell
                            onClick={() => handleSort(AttributeType.DueDate)}
                            sx={{ cursor: 'pointer' }}
                        >
                            Due Date
                        </TableCell>
                        <TableCell
                            onClick={() => handleSort(AttributeType.Status)}
                            sx={{ cursor: 'pointer' }}
                        >
                            Status
                        </TableCell>
                        <TableCell
                            onClick={() => handleSort(AttributeType.Priority)}
                            sx={{ cursor: 'pointer' }}
                        >
                            Priority
                        </TableCell>
                        <TableCell>Media</TableCell>
                        <TableCell>Actions</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {list.items.map((item) => (
                        <TableRow key={item.id}>
                            <TableCell>{item.name}</TableCell>
                            <TableCell>{item.description}</TableCell>
                            <TableCell>{formatDate(item.dueDate)}</TableCell>
                            <TableCell>{StatusTypeLabel[item.status as StatusType]}</TableCell>
                            <TableCell>{PriorityTypeLabel[item.priority as PriorityType]}</TableCell>
                            <TableCell>
                                {item.mediaUrl && item.mediaType === MediaType.Image && (
                                    <img
                                        src={API_BASE_URL + item.mediaUrl}
                                        alt="media"
                                        style={{ maxWidth: '100px' }}
                                    />
                                )}
                                {item.mediaUrl && item.mediaType === MediaType.Video && (
                                    <video controls width="200">
                                        <source
                                            src={API_BASE_URL + item.mediaUrl}
                                            type={item.mediaUrl.toLowerCase().endsWith('.mov') ? 'video/quicktime' : 'video/mp4'}
                                        />
                                        Your browser does not support the video tag.
                                    </video>
                                )}
                            </TableCell>
                            <TableCell>
                                <Button color="error" onClick={() => handleDeleteItem(item.id)}>Delete</Button>
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>

            <Dialog open={isAddDialogOpen} onClose={() => setIsAddDialogOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Add New Item</DialogTitle>
                <DialogContent>
                    <Box display="flex" flexDirection="column" gap={2} mt={1}>
                        <TextField
                            label="Name"
                            fullWidth
                            value={newItemName}
                            onChange={(e) => setNewItemName(e.target.value)}
                        />
                        <TextField
                            label="Description"
                            fullWidth
                            value={newItemDescription}
                            onChange={(e) => setNewItemDescription(e.target.value)}
                        />
                        <TextField
                            type="date"
                            label="Due Date"
                            InputLabelProps={{ shrink: true }}
                            value={newItemDueDate}
                            onChange={(e) => setNewItemDueDate(e.target.value)}
                        />
                        <FormControl fullWidth>
                            <InputLabel>Status</InputLabel>
                            <Select
                                value={newItemStatus}
                                onChange={(e) => setNewItemStatus(Number(e.target.value))}
                                label="Status"
                            >
                                <MenuItem value={StatusType.NotStarted}>Not Started</MenuItem>
                                <MenuItem value={StatusType.InProgress}>In Progress</MenuItem>
                                <MenuItem value={StatusType.Completed}>Completed</MenuItem>
                            </Select>
                        </FormControl>
                        <FormControl fullWidth>
                            <InputLabel>Priority</InputLabel>
                            <Select
                                value={newItemPriority}
                                onChange={(e) => setNewItemPriority(Number(e.target.value))}
                                label="Priority"
                            >
                                <MenuItem value={PriorityType.Low}>Low</MenuItem>
                                <MenuItem value={PriorityType.Medium}>Medium</MenuItem>
                                <MenuItem value={PriorityType.High}>High</MenuItem>
                                <MenuItem value={PriorityType.Critical}>Critical</MenuItem>
                            </Select>
                        </FormControl>
                        <input
                            type="file"
                            accept="image/*,video/mp4,.mov"
                            ref={fileInputRef}
                            onChange={(e) => setNewItemMedia(e.target.files?.[0] || null)}
                            style={{ cursor: 'pointer' }}
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button color="error" onClick={() => setIsAddDialogOpen(false)}>Cancel</Button>
                    <Button
                        onClick={handleAddItem}
                        disabled={!newItemName.trim()}
                        variant="contained"
                    >
                        Add
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default TodoListDetailPage;
