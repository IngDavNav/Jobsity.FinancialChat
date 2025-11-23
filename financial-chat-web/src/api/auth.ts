import type { User } from "../types/User";
import { API_BASE_URL } from "../config/api";

export async function login(userName: string): Promise<User> {
    const res = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ userName }),
    });

    if (!res.ok) {
        const text = await res.text().catch(() => "");
        throw new Error(`Error al iniciar sesión: ${res.status} ${text}`);
    }

    return res.json();
}