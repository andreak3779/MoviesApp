import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { SidebarComponent } from './features/chrome/sidebar.component';

/**
 * Root layout: shows the sidebar on every route except /login.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, SidebarComponent],
  template: `
    @if (showChrome()) {
      <div class="shell">
        <app-sidebar />
        <main><router-outlet /></main>
      </div>
    } @else {
      <router-outlet />
    }
  `,
  styles: [`
    :host { display: block; }
    .shell {
      display: flex;
      min-height: 100vh;
      background: var(--bg, oklch(97.5% 0.004 320));
      color: var(--ink, #222);
      font-family: Roboto, -apple-system, BlinkMacSystemFont, sans-serif;
    }
    main {
      flex: 1;
      min-width: 0;
      padding: 40px 48px;
      max-width: 1200px;
    }
  `],
})
export class App {
  private readonly router = inject(Router);

  readonly showChrome = toSignal(
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map((e) => !e.urlAfterRedirects.startsWith('/login')),
      startWith(!this.router.url.startsWith('/login')),
    ),
    { initialValue: !this.router.url.startsWith('/login') },
  );
}
