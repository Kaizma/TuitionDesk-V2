import { api } from "../../shared/api/client";

export interface Health {
  status: string;
  version: string;
  schemaVersion: string | null;
  pendingMigrations: string[];
  database: string;
  utcNow: string;
}

export function getHealth() {
  return api<Health>("/api/health");
}
