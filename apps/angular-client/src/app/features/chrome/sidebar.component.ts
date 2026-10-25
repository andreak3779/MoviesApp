import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

/**
 * Mockup-shaped left rail. Lives outside the /login route. Highlights the
 * nav item that matches the current URL prefix.
 */
@Component({
  selector: 'app-sidebar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <aside>
      <div class="brand">MoviesApp</div>

      <a routerLink="/movies" routerLinkActive="active" class="nav">Movies</a>

      <a routerLink="/genres" routerLinkActive="active" class="nav">Genres</a>
      <a routerLink="/media-types" routerLinkActive="active" class="nav">Media Types</a>
      <a routerLink="/formats" routerLinkActive="active" class="nav">Formats</a>

      <div class="footer">
        @if (authed()) {
          <button type="button" class="signout" (click)="signOut()">Sign out</button>
        }
      </div>
    </aside>
  `,
  styles: [`
    aside {
      width: 220px;
      flex-shrink: 0;
      background: var(--surface, #fff);
      border-right: 1px solid var(--surface-border, #e6e6e6);
      padding: 24px 14px;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .brand { padding: 8px 12px 24px; font-size: 19px; font-weight: 700; color: var(--primary, #7a1256); letter-spacing: -0.02em; }
    .nav {
      display: block;
      padding: 10px 14px;
      border-radius: 20px;
      color: var(--ink-secondary, #555);
      font: 500 14px Roboto, sans-serif;
      text-decoration: none;
    }
    .nav:hover { background: color-mix(in srgb, var(--primary, #7a1256) 6%, transparent); }
    .nav.active {
      background: var(--primary-tint, #f4e3ee);
      color: var(--primary, #7a1256);
    }
    .footer { margin-top: auto; padding: 12px; }
    .signout {
      width: 100%;
      padding: 8px 14px;
      border-radius: 999px;
      background: transparent;
      border: 1px solid var(--surface-border, #e6e6e6);
      color: var(--ink-secondary, #555);
      font: 500 13px Roboto, sans-serif;
      cursor: pointer;
    }
  `],
})
export class SidebarComponent {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  readonly authed = this.auth.isAuthenticated;

  signOut(): void {
    this.auth.logout().subscribe({
      next: () => void this.router.navigate(['/login']),
      error: () => void this.router.navigate(['/login']),
    });
  }
}
