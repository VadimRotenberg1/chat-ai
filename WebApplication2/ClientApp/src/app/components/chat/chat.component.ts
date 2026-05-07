import { Component, ElementRef, ViewChild, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatService } from '../../services/chat';
import { HeaderComponent } from '../header/header.component';

@Component({
    selector: 'app-chat',
    standalone: true,
    imports: [CommonModule, HeaderComponent],
    templateUrl: './chat.component.html',
    styleUrls: ['./chat.component.css']
})
export class ChatComponent {
    @ViewChild('editor') editor!: ElementRef<HTMLDivElement>;
    @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;

    protected readonly chat = inject(ChatService);

    constructor() {
        // Ensure the SignalR connection is established once the user lands on chat.
        this.chat.connect();

        effect(() => {
            this.chat.messages();
            setTimeout(() => this.scrollToBottom(), 0);
        });
    }

    onSubmit(event: Event) {
        event.preventDefault();
        const html = this.editor.nativeElement.innerHTML.trim();
        const text = this.editor.nativeElement.innerText.trim();

        if (text) {
            this.chat.sendMessage(html, text);
            this.editor.nativeElement.innerHTML = '';
        }
    }

    onKeyDown(event: KeyboardEvent) {
        if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) {
            this.onSubmit(event);
        }
    }

    onPaste(event: ClipboardEvent) {
        event.preventDefault();
        const text = event.clipboardData?.getData('text/plain') ?? '';
        document.execCommand('insertText', false, text);
    }

    execCommand(command: string) {
        document.execCommand(command, false, undefined);
    }

    private scrollToBottom() {
        if (this.messagesContainer) {
            const el = this.messagesContainer.nativeElement;
            el.scrollTop = el.scrollHeight;
        }
    }
}
