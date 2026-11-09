/// <reference types="vite/client" />

interface TuitionDeskConfig {
  apiBaseUrl: string;
  token?: string;
  appVersion?: string;
}

interface Window {
  __TUITIONDESK__?: TuitionDeskConfig;
}
