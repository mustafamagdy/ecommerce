export interface Storage {
  getItem(key: string): string | null;
  removeItem(key: string): void | Promise<void>;
  setItem(key: string, data: string): void | Promise<void>;
}
