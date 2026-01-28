import { Injectable } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { AuthService } from '../auth-services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RealtimeHubService {
  private connection?: HubConnection;
  private hubUrl?: string;
  private startPromise?: Promise<void>;
  private handlers = new Map<string, Set<(...args: any[]) => void>>();

  constructor(private readonly auth: AuthService) {}

  start(hubUrl: string): Promise<void> {
    this.hubUrl = hubUrl;
    this.ensureConnection();

    if (!this.connection) {
      return Promise.reject(new Error('SignalR connection is not initialized.'));
    }

    if (this.connection.state === HubConnectionState.Connected) {
      return Promise.resolve();
    }

    if (this.startPromise) {
      return this.startPromise;
    }

    this.startPromise = this.connection
      .start()
      .catch(err => {
        console.error('SignalR connection failed to start.', err);
        throw err;
      })
      .finally(() => {
        this.startPromise = undefined;
      });

    return this.startPromise;
  }

  stop(): Promise<void> {
    this.cleanupHandlers();
    if (!this.connection) {
      return Promise.resolve();
    }

    const stopPromise = this.connection.stop();
    this.connection = undefined;
    return stopPromise;
  }

  on<T>(eventName: string): Observable<T> {
    this.ensureConnection();
    if (!this.connection) {
      throw new Error('SignalR connection is not initialized. Call start() first.');
    }

    return new Observable<T>(subscriber => {
      const handler = (payload: T) => subscriber.next(payload);

      this.connection?.on(eventName, handler);
      this.registerHandler(eventName, handler);

      return () => {
        this.connection?.off(eventName, handler);
        this.unregisterHandler(eventName, handler);
      };
    });
  }

  off(eventName: string): void {
    const handlers = this.handlers.get(eventName);
    if (!handlers || !this.connection) return;

    handlers.forEach(handler => this.connection?.off(eventName, handler));
    this.handlers.delete(eventName);
  }

  invoke<TResult = void>(methodName: string, payload?: unknown): Promise<TResult> {
    if (!this.connection) {
      return Promise.reject(new Error('SignalR connection is not initialized.'));
    }

    return payload === undefined
      ? this.connection.invoke<TResult>(methodName)
      : this.connection.invoke<TResult>(methodName, payload);
  }

  private ensureConnection(): void {
    if (this.connection) return;
    if (!this.hubUrl) return;

    this.connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.auth.getAccessToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.onreconnecting(error => {
      console.warn('SignalR reconnecting...', error);
    });

    this.connection.onreconnected(connectionId => {
      console.info('SignalR reconnected.', connectionId);
    });

    this.connection.onclose(error => {
      if (error) {
        console.warn('SignalR closed with error.', error);
      }
    });
  }

  private cleanupHandlers(): void {
    if (!this.connection) return;

    this.handlers.forEach((handlers, eventName) => {
      handlers.forEach(handler => this.connection?.off(eventName, handler));
    });

    this.handlers.clear();
  }

  private registerHandler(eventName: string, handler: (...args: any[]) => void): void {
    const handlers = this.handlers.get(eventName) ?? new Set<(...args: any[]) => void>();
    handlers.add(handler);
    this.handlers.set(eventName, handlers);
  }

  private unregisterHandler(eventName: string, handler: (...args: any[]) => void): void {
    const handlers = this.handlers.get(eventName);
    if (!handlers) return;

    handlers.delete(handler);
    if (handlers.size === 0) {
      this.handlers.delete(eventName);
    }
  }
}
