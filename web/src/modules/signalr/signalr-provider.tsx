import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";
import { SignalRContext } from "./signalr-context";

export const SignalRProvider = ({
    children,
}: {
    children: React.ReactNode;
}) => {
    const isSignalRDisabled = import.meta.env.VITE_DISABLE_SIGNALR === "true";
    const connectionRef = useRef<HubConnection | null>(null);
    const [connectionState, setConnectionState] =
        useState<HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        if (isSignalRDisabled) {
            return;
        }

        let isMounted = true;

        if (!connectionRef.current) {
            connectionRef.current = new HubConnectionBuilder()
                .withUrl("/hubs/logsheets")
                .withAutomaticReconnect()
                .build();

            setConnectionState(connectionRef.current);
        }

        const connection = connectionRef.current;

        async function startConnection() {
            try {
                if (connection.state === "Disconnected") {
                    await connection.start();
                }
                if (isMounted) {
                    setIsConnected(connection.state === "Connected");
                    console.log("SignalR Connected!");
                }
            } catch (err) {
                if (isMounted) {
                    console.error("SignalR Connection Error: ", err);
                }
            }
        }

        const onReconnecting = () => {
            if (isMounted) setIsConnected(false);
        };

        const onReconnected = () => {
            if (isMounted) setIsConnected(true);
        };

        const onClose = () => {
            if (isMounted) setIsConnected(false);
        };

        connection.onreconnecting(onReconnecting);
        connection.onreconnected(onReconnected);
        connection.onclose(onClose);

        void startConnection();

        return () => {
            isMounted = false;
        };
    }, [isSignalRDisabled]);

    return (
        <SignalRContext.Provider
            value={{ connection: connectionState, isConnected }}
        >
            {children}
        </SignalRContext.Provider>
    );
};
