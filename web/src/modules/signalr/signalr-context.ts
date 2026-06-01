import { HubConnection } from "@microsoft/signalr";
import { createContext, useContext } from "react";

export type SignalRContextType = {
    connection: HubConnection | null;
    isConnected: boolean;
};

export const SignalRContext = createContext<SignalRContextType>({
    connection: null,
    isConnected: false,
});

export const useSignalR = () => useContext(SignalRContext);
