import { useCallback, useReducer } from "react";

type HistoryState<T> = {
    past: T[];
    present: T;
    future: T[];
};

type Action<T> =
    | { type: "SET"; payload: T | ((prev: T) => T); depth: number }
    | { type: "UNDO" }
    | { type: "REDO" };

const historyReducer = <T>(
    state: HistoryState<T>,
    action: Action<T>
): HistoryState<T> => {
    switch (action.type) {
        case "SET": {
            const { past, present } = state;
            const newState =
                typeof action.payload === "function"
                    ? (action.payload as (prev: T) => T)(present)
                    : action.payload;

            if (newState === present) {
                return state;
            }

            return {
                past: [...past, present].slice(-action.depth),
                present: newState,
                future: [],
            };
        }
        case "UNDO": {
            const { past, present, future } = state;
            if (past.length === 0) {
                return state;
            }

            const previous = past[past.length - 1];
            const newPast = past.slice(0, past.length - 1);

            return {
                past: newPast,
                present: previous,
                future: [present, ...future],
            };
        }
        case "REDO": {
            const { past, present, future } = state;
            if (future.length === 0) {
                return state;
            }

            const next = future[0];
            const newFuture = future.slice(1);

            return {
                past: [...past, present],
                present: next,
                future: newFuture,
            };
        }
        default:
            return state;
    }
};

export const useHistory = <T>(initialState: T, depth = 20) => {
    const [state, dispatch] = useReducer(historyReducer, {
        past: [],
        present: initialState,
        future: [],
    }) as [HistoryState<T>, React.Dispatch<Action<T>>];

    const set = useCallback(
        (newState: T | ((prev: T) => T)) => {
            dispatch({ type: "SET", payload: newState, depth });
        },
        [depth, dispatch]
    );

    const undo = useCallback(() => {
        dispatch({ type: "UNDO" });
    }, [dispatch]);

    const redo = useCallback(() => {
        dispatch({ type: "REDO" });
    }, [dispatch]);

    return {
        state: state.present,
        set,
        undo,
        redo,
        canUndo: state.past.length > 0,
        canRedo: state.future.length > 0,
    };
};
