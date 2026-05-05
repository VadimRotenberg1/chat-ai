import { Component, ElementRef, ViewChild, effect } from '@angular/core';
import { ChatService } from './services/chat';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent {
  @ViewChild('editor') editor!: ElementRef<HTMLDivElement>;
  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;

  constructor(public chat: ChatService) {
    effect(() => {
      // Trigger scroll when messages change
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
