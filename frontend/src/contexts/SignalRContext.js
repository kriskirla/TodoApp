import { createContext, useState, useEffect, useRef } from 'react';
import { startConnection } from '../services/signalRService';

export const SignalRContext = createContext(null);

export const SignalRProvider = ({ token, children }) => {
    const [connection, setConnection] = useState(null);
    const joinedGroups = useRef(new Set());

    const connectionRef = useRef(null);

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

    // Expose connection and joinedGroups (for group join/leave management)
    return (
        <SignalRContext.Provider value={{ connection, joinedGroups }}>
            {children}
        </SignalRContext.Provider>
    );
};
