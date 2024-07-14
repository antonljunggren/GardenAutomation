import { Component, For } from "solid-js";
import { UserNotification } from "../models/Notification";


interface NotificationContainerProps {
    notifications: UserNotification[];
}

export const NotificationContainer: Component<NotificationContainerProps> = (props) => {
    return (
        <div class="notification-container">
            <For each={props.notifications}>
                {(notification) => (
                    <div class={`notification ${notification.type}`}>
                        {notification.message}
                    </div>
                )}
            </For>
        </div>
    );
};