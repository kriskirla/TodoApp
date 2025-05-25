import {
    createContext,
    useState,
    useEffect,
    useRef,
    ReactNode,
    MutableRefObject
} from 'react';
import { HubConnection } from '@microsoft/signalr';
import { startConnection } from '../services/signalRService';

interface SignalRContextType {
    connection: HubConnection | null;
    joinedGroups: MutableRefObject<Set<string>>;
}

export const SignalRContext = createContext<SignalRContextType>(null as any);

interface SignalRProviderProps {
    token: string;
    children: ReactNode;
}

export const SignalRProvider: React.FC<SignalRProviderProps> = ({ token, children }) => {
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const joinedGroups = useRef<Set<string>>(new Set());
    const connectionRef = useRef<HubConnection | null>(null);

    useEffect(() => {
        if (!token) return;

        let isMounted = true;

        const setupConnection = async () => {
            try {
                const conn = await startConnection(token);

                if (!isMounted) {
                    await conn.stop();
                    return;
                }

                connectionRef.current = conn;
                setConnection(conn);
            } catch (err) {
                console.error('SignalR connection failed:', err);
            }
        };

        setupConnection();

        return () => {
            isMounted = false;
            if (connectionRef.current) {
                connectionRef.current.stop();
                connectionRef.current = null;
            }
            setConnection(null);
        };
    }, [token]);

    return (
        <SignalRContext.Provider value={{ connection, joinedGroups }
        }>
            {children}
        </SignalRContext.Provider>
    );
};
