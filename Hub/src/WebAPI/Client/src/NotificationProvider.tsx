import { createContext, createSignal, useContext } from "solid-js";
import { NotificationType as UserNotificationType, UserNotification } from "./models/Notification";
import { NotificationContainer } from "./components/NotificationContainer";

interface NotificationContextProps {
    createNotification: (type: UserNotificationType, message: string) => void;
}

const NotificationContext = createContext<NotificationContextProps>();

let notificationId = 0;

export const NotificationProvider = (props: any) => {
    const [notifications, setNotifications] = createSignal<UserNotification[]>([]);

    const createNotification = (type: UserNotificationType, message: string) => {
        const id = notificationId++;
        setNotifications((prev) => [...prev, { id, type, message }]);
        setTimeout(() => {
            setNotifications((prev) => prev.filter((notification) => notification.id !== id));
        }, 3000);
    };

    return (
        <NotificationContext.Provider value={{ createNotification }}>
            {props.children}
            <NotificationContainer notifications={notifications()} />
        </NotificationContext.Provider>
    );
}

export const useNotification = () => useContext(NotificationContext);