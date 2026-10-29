import { HttpErrorResponse } from '@angular/common/http';

/**
 * Turns a failed request into a message worth showing to a person.
 *
 * The API answers with `{ "error": "..." }` for the auth endpoints and with
 * RFC 7807 problem details for the rest, and a failure can also be a
 * network error with no body at all. This picks the most specific message
 * available and falls back to something honest rather than "[object Object]".
 */
export function describeHttpError(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as
      | { error?: string; title?: string; detail?: string }
      | string
      | null;

    if (typeof body === 'string') {
      if (body.trim().length > 0) {
        return body;
      }
    } else {
      const message = body?.error ?? body?.title ?? body?.detail;
      if (message) {
        return message;
      }
    }

    if (error.status === 0) {
      return 'The server could not be reached.';
    }

    return `The request failed (${error.status}).`;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return String(error);
}
