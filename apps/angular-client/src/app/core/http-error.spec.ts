import { HttpErrorResponse } from '@angular/common/http';
import { describeHttpError } from './http-error';

describe('describeHttpError', () => {
  it('should use the "error" property the auth endpoints return', () => {
    const error = new HttpErrorResponse({
      status: 401,
      error: { error: 'Invalid credentials.' },
    });

    expect(describeHttpError(error)).toBe('Invalid credentials.');
  });

  it('should use the "title" of a problem-details response', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { title: 'One or more validation errors occurred.' },
    });

    expect(describeHttpError(error)).toBe('One or more validation errors occurred.');
  });

  it('should fall back to the "detail" of a problem-details response', () => {
    const error = new HttpErrorResponse({
      status: 404,
      error: { detail: 'No movie with that id.' },
    });

    expect(describeHttpError(error)).toBe('No movie with that id.');
  });

  it('should use a plain text body', () => {
    const error = new HttpErrorResponse({ status: 500, error: 'Something broke.' });

    expect(describeHttpError(error)).toBe('Something broke.');
  });

  it('should say the server could not be reached when the status is 0', () => {
    // Status 0 means the request never arrived — a dropped connection or a
    // blocked host — so a "failed with (0)" message would only confuse.
    const error = new HttpErrorResponse({ status: 0, error: null });

    expect(describeHttpError(error)).toBe('The server could not be reached.');
  });

  it('should fall back to the status code when the body says nothing', () => {
    const error = new HttpErrorResponse({ status: 503, error: null });

    expect(describeHttpError(error)).toBe('The request failed (503).');
  });

  it('should ignore an empty text body', () => {
    const error = new HttpErrorResponse({ status: 500, error: '   ' });

    expect(describeHttpError(error)).toBe('The request failed (500).');
  });

  it('should use the message of a plain error', () => {
    expect(describeHttpError(new Error('Boom'))).toBe('Boom');
  });

  it('should stringify anything else', () => {
    expect(describeHttpError('just a string')).toBe('just a string');
  });
});
