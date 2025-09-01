import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { apiBaseUrl } from "../lib/constants";

// Extract base URL without /api/ path
const getSignalRBaseUrl = () => {
  return apiBaseUrl.replace("/api/", "");
};

export interface NotificationData {
  id?: string;
  type: string;
  message: string;
  data?: string;
  bugId?: string;
  isRead?: boolean;
  createdAt?: string;
  timestamp: string;
}

export class NotificationService {
  private connection: HubConnection | null = null;
  private readonly baseUrl: string;
  private listeners: Map<string, ((data: NotificationData) => void)[]> =
    new Map();

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl || getSignalRBaseUrl();
  }

  public async startConnection(accessToken?: string): Promise<void> {
    if (this.connection) {
      return;
    }

    interface ConnectionOptions {
      withCredentials: boolean;
      accessTokenFactory?: () => string;
    }

    const connectionOptions: ConnectionOptions = {
      withCredentials: true,
    };

    // Add authorization header if token is provided
    if (accessToken) {
      connectionOptions.accessTokenFactory = () => accessToken;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/hubs/notifications`, connectionOptions)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    try {
      await this.connection.start();
      console.log("SignalR connection established");
      this.setupEventListeners();
    } catch (error) {
      console.error("Failed to establish SignalR connection:", error);
      throw error;
    }
  }

  public getConnection(): HubConnection | null {
    return this.connection;
  }

  public async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log("SignalR connection closed");
    }
  }

  private setupEventListeners(): void {
    if (!this.connection) return;

    // Listen for general notifications
    this.connection.on("ReceiveNotification", (data: NotificationData) => {
      console.log("Received notification:", data);
      this.notifyListeners("notification", data);
    });

    // Listen for bug-specific notifications
    this.connection.on("ReceiveBugNotification", (data: NotificationData) => {
      console.log("Received bug notification:", data);
      this.notifyListeners("bugNotification", data);
    });
  }

  public onNotification(
    eventType: string,
    callback: (data: NotificationData) => void
  ): void {
    if (!this.listeners.has(eventType)) {
      this.listeners.set(eventType, []);
    }
    this.listeners.get(eventType)!.push(callback);
  }

  public offNotification(
    eventType: string,
    callback: (data: NotificationData) => void
  ): void {
    const eventListeners = this.listeners.get(eventType);
    if (eventListeners) {
      const index = eventListeners.indexOf(callback);
      if (index > -1) {
        eventListeners.splice(index, 1);
      }
    }
  }

  private notifyListeners(eventType: string, data: NotificationData): void {
    const eventListeners = this.listeners.get(eventType);
    if (eventListeners) {
      eventListeners.forEach((callback) => callback(data));
    }
  }

  public async joinUserGroup(userId: string): Promise<void> {
    if (this.connection && this.connection.state === "Connected") {
      try {
        await this.connection.invoke("JoinUserGroup", userId);
        console.log(`Joined user group: User_${userId}`);
      } catch (error) {
        console.error("Failed to join user group:", error);
      }
    }
  }

  public async leaveUserGroup(userId: string): Promise<void> {
    if (this.connection && this.connection.state === "Connected") {
      try {
        await this.connection.invoke("LeaveUserGroup", userId);
        console.log(`Left user group: User_${userId}`);
      } catch (error) {
        console.error("Failed to leave user group:", error);
      }
    }
  }

  public isConnected(): boolean {
    return this.connection?.state === "Connected";
  }

  public getConnectionState(): string {
    return this.connection?.state || "Disconnected";
  }
}

// Singleton instance
export const notificationService = new NotificationService();
