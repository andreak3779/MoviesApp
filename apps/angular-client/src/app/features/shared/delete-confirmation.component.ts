import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';

/**
 * Standalone confirmation modal used by the movies list and the lookup lists.
 * Pure presentational — parent owns the visibility state.
 */
@Component({
  selector: 'app-delete-confirmation',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (visible) {
      <div class="backdrop" (click)="cancel.emit()">
        <div class="dialog" (click)="$event.stopPropagation()" role="dialog" aria-modal="true">
          <h3>{{ title }}</h3>
          <p>{{ message }}</p>
          <div class="actions">
            <button type="button" class="secondary" (click)="cancel.emit()">Cancel</button>
            <button type="button" class="danger" (click)="confirm.emit()">Delete</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .backdrop {
      position: fixed; inset: 0;
      background: oklch(0% 0 0 / 0.4);
      display: flex; align-items: center; justify-content: center;
      z-index: 50;
    }
    .dialog {
      background: var(--surface, #fff);
      border-radius: 16px;
      padding: 28px;
      max-width: 380px; width: 90%;
      box-shadow: 0 8px 32px oklch(0% 0 0 / 0.25);
    }
    h3 { margin: 0 0 8px; font-size: 18px; font-weight: 700; }
    p  { margin: 0 0 24px; color: var(--ink-secondary, #555); font-size: 14px; line-height: 1.5; }
    .actions { display: flex; justify-content: flex-end; gap: 10px; }
    button {
      padding: 10px 20px;
      border-radius: 999px;
      font: 500 14px Roboto, sans-serif;
      cursor: pointer;
      border: 1px solid var(--surface-border, #e6e6e6);
    }
    .secondary { background: transparent; color: var(--ink, #222); }
    .danger    { background: var(--danger, #b03b3b); color: white; border: none; }
  `],
})
export class DeleteConfirmationComponent {
  @Input() visible = false;
  @Input() title = 'Delete?';
  @Input() message = 'This cannot be undone.';
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
}
