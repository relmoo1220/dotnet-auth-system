import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class LocalStorageService {
  private isBrowser: boolean;

  // Use isPlatformBrowser and PLATFORM_ID to handle SSR safely by returning null when not in the browser.
  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  setItem(key: string, value: any): void {
    if (this.isBrowser) {
      try {
        const storedValue = typeof value === 'string' ? value : JSON.stringify(value);

        localStorage.setItem(key, storedValue);
      } catch (error) {
        console.error('Error saving to local storage', error);
      }
    }
  }

  getItem<T>(key: string): T | null {
    if (this.isBrowser) {
      try {
        const value = localStorage.getItem(key);
        if (!value) return null;

        // Handle JSON objects/arrays
        if (value.startsWith('{') || value.startsWith('[')) {
          return JSON.parse(value);
        }

        // Handle raw strings (like JWT tokens)
        return value as unknown as T;
      } catch (error) {
        console.error('Error reading from local storage', error);
        return null;
      }
    }
    return null;
  }

  removeItem(key: string): void {
    if (this.isBrowser) {
      localStorage.removeItem(key);
    }
  }

  clear(): void {
    if (this.isBrowser) {
      localStorage.clear();
    }
  }
}
