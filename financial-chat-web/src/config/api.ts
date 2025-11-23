const defaultBaseUrl =
  import.meta.env.MODE === "development"
    ? "http://localhost:5000"
    : "/api";

export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? defaultBaseUrl;
