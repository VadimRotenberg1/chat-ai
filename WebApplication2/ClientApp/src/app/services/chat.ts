import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HttpClient } from '@angular/common/http';

export interface ChatMessage {
  kind: 'user' | 'assistant' | 'error';
  content: string;
  isHtml: boolean;
  responseId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private connection: signalR.HubConnection;
  private conversationId = crypto.randomUUID();
  
  public messages = signal<ChatMessage[]>([]);
  public connectionStatus = signal<string>('Connecting...');
  public isSending = signal<boolean>(false);

  constructor(private http: HttpClient) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/chatHub')
      .withAutomaticReconnect()
      .build();

    this.setupSignalRHandlers();
    this.startConnection();
  }

  private setupSignalRHandlers() {
    this.connection.on('AssistantStarted', ({ responseId }) => {
      this.messages.update(ms => [...ms, { kind: 'assistant', content: '', isHtml: false, responseId }]);
      this.isSending.set(true);
    });

    this.connection.on('AssistantChunk', ({ responseId, text }) => {
      this.messages.update(ms => ms.map(m => 
        m.responseId === responseId ? { ...m, content: m.content + text } : m
      ));
    });

    this.connection.on('AssistantCompleted', ({ responseId }) => {
      this.messages.update(ms => ms.map(m => 
        m.responseId === responseId ? { ...m, responseId: undefined } : m
      ));
      this.isSending.set(false);
    });

    this.connection.on('AssistantError', ({ responseId, error }) => {
      this.messages.update(ms => ms.map(m => 
        m.responseId === responseId ? { ...m, kind: 'error', content: error, responseId: undefined } : m
      ));
      this.isSending.set(false);
    });

    this.connection.onreconnecting(() => this.connectionStatus.set('Reconnecting...'));
    this.connection.onreconnected(() => this.connectionStatus.set('Connected'));
    this.connection.onclose(() => this.connectionStatus.set('Disconnected'));
  }

  private async startConnection() {
    try {
      await this.connection.start();
      this.connectionStatus.set('Connected');
    } catch (err) {
      this.connectionStatus.set('Connection failed');
      setTimeout(() => this.startConnection(), 2000);
    }
  }

  async sendMessage(html: string, text: string) {
    if (!text.trim() || this.connection.state !== signalR.HubConnectionState.Connected) return;

    // Add user message to UI
    this.messages.update(ms => [...ms, { kind: 'user', content: html, isHtml: true }]);
    this.isSending.set(true);

    try {
      await this.http.post('/api/chat', {
        connectionId: this.connection.connectionId,
        conversationId: this.conversationId,
        message: text
      }).toPromise();
    } catch (error: any) {
      this.messages.update(ms => [...ms, { kind: 'error', content: error.error?.error || 'Request failed.', isHtml: false }]);
      this.isSending.set(false);
    }
  }

  clearMessages() {
    this.messages.set([]);
  }
}
