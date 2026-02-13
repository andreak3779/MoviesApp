# Angular Material Guidelines for Agents

This file describes repository-specific patterns and recommendations for using Angular Material in `movies-app`.

Why this matters
- Angular Material provides accessible, themed UI components. Use it consistently to keep the UI cohesive and maintainable.

Where to look
- Dependency: `movies-app/package.json` includes `@angular/material`.
- Global styles and theme: `movies-app/src/styles.scss` and `movies-app/angular.json` (styles array).

Recommended practices
- Import only the Material modules you need in each feature module. Do NOT create a grab-bag `MaterialModule` that imports/exports the entire library — this prevents tree-shaking.

Example (feature module imports):

```ts
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@NgModule({
  imports: [MatButtonModule, MatIconModule],
  exports: [MatButtonModule, MatIconModule]
})
export class MoviesMaterialModule {}
```

Theming (SCSS)
- Use the Angular Material theming API in `movies-app/src/styles.scss` to create a single app theme, then include it globally.

Minimal theme example (SCSS):

```scss
@use '@angular/material' as mat;

$app-primary: mat.define-palette(mat.$indigo-palette);
$app-accent: mat.define-palette(mat.$pink-palette, A200, A100, A400);
$app-theme: mat.define-light-theme((
  color: (primary: $app-primary, accent: $app-accent),
));

@include mat.core();
@include mat.all-component-themes($app-theme);
```

Icons
- Register the icon font in `index.html` or use `MatIconRegistry` for SVG icons. Prefer SVG icon sets for offline/localized builds.

Accessibility
- Prefer semantic markup and supply `aria-*` attributes where applicable (buttons, dialogs, navigation).
- Test keyboard navigation and focus behavior for Material components (menus, dialogs, form fields).

Performance
- Use `ChangeDetectionStrategy.OnPush` for presentational components.
- Use `trackBy` for `*ngFor` lists backed by large arrays.

Forms & Inputs
- Use `mat-form-field` with appropriate `appearance` (outline by default) and proper `formControl` bindings.

Testing
- In unit tests, import only the specific Material modules used by the component.
- For shallow tests, mock complex Material components with simple stubs if behavior isn't required.

Style & patterns
- Keep Material styling in component SCSS files or the global theme; avoid inline styles that break theme tokens.
- Follow the project's Prettier settings in `movies-app/package.json` for formatting.

Where to add new components
- Add components under `movies-app/src/app/` and prefer `--style=scss` when generating via schematics.

When to avoid Material
- For ultra-lightweight micro-widgets where shipping <1KB matters, consider minimal custom components instead of importing a Material module.

If you want, I can:
- Add a starter `MoviesMaterialModule` file under `movies-app/src/app/material/` with the commonly used imports, or
- Add the SCSS theme snippet into `movies-app/src/styles.scss` and wire it in `angular.json`.
